using AutoMapper;
using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using EventBus.Messages.Events;
using MassTransit;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _repository;
    private readonly DiscountGrpcService _discountGrpcService;
    private readonly IMapper _mapper;
    private readonly IPublishEndpoint _publishEndpoint;

    public BasketController(IBasketRepository repository,
                            DiscountGrpcService discountGrpcService,
                            IMapper mapper,
                            IPublishEndpoint publishEndpoint)
    {
        _repository = repository;
        _discountGrpcService = discountGrpcService;
        _mapper = mapper;
        _publishEndpoint = publishEndpoint;
    }

    [HttpGet("{username}", Name = "GetBasket")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ShoppingCart>> GetBasket(string username)
    {
        var basket = await _repository.GetBasket(username);
        return basket ?? new ShoppingCart(username);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<ShoppingCart?>> UpdateBasket(ShoppingCart basket)
    {
        //Communicate with the discount gRPC and calculate the prices for shopping cart.
        foreach (var item in basket.Items)
        {
            var coupon = await _discountGrpcService.GetDiscount(item.ProductName!);
            item.Price -= coupon.Amount;
        }

        return await _repository.UpdateBasket(basket);
    }

    [HttpDelete("{username}", Name = "DeleteBasket")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteBasket(string username)
    {
        await _repository.DeleteBasket(username);
        return NoContent();
    }

    [Route("[action]")]
    [ProducesResponseType(StatusCodes.Status202Accepted)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [HttpPost]
    public async Task<IActionResult> Checkout(BasketCheckout basketCheckout)
    {
        // Get existing basket with total price
        var basket = await _repository.GetBasket(basketCheckout.UserName);
        if (basket is null)
        {
            return BadRequest();
        }

        // Send checkout event to rabbitmq
        var eventMessage = _mapper.Map<BasketCheckoutEvent>(basketCheckout);
        eventMessage.TotalPrice = basket.TotalPrice;

        await _publishEndpoint.Publish(eventMessage);

        // Remove items from basket
        await _repository.DeleteBasket(basketCheckout.UserName);

        return Accepted();
    }
}