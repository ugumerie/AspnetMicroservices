using Basket.API.Entities;
using Basket.API.GrpcServices;
using Basket.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Basket.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketRepository _repository;
    private readonly DiscountGrpcService _discountGrpcService;

    public BasketController(IBasketRepository repository, DiscountGrpcService discountGrpcService)
    {
        _repository = repository;
        _discountGrpcService = discountGrpcService;
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
}