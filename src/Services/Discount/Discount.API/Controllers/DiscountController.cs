using Discount.API.Entities;
using Discount.API.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace Discount.API.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class DiscountController : ControllerBase
{
    private readonly IDiscountRepository _repository;

    public DiscountController(IDiscountRepository repository)
    {
        _repository = repository;
    }

    [HttpGet("{productName}", Name = "GetDiscount")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<Coupon>> GetDiscount(string productName)
    {
        return await _repository.GetDiscount(productName);
    }

    [HttpPost]
    [ProducesResponseType(StatusCodes.Status201Created)]
    public async Task<ActionResult<Coupon>> CreateDiscount(Coupon coupon)
    {
        await _repository.CreateDiscount(coupon);
        return CreatedAtRoute("GetDiscount", new { productName = coupon.ProductName }, coupon);
    }

    [HttpPut]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult<Coupon>> UpdateDiscount(Coupon coupon)
    {
        await _repository.UpdateDiscount(coupon);
        return NoContent();
    }

    [HttpDelete("{productName}", Name = "DeleteDiscount")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> DeleteDiscount(string productName)
    {
        await _repository.DeleteDiscount(productName);
        return NoContent();
    }
}