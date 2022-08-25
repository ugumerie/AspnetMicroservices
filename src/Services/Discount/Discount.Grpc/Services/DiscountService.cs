using AutoMapper;
using Discount.API.Repositories;
using Discount.Grpc.Entities;
using Discount.Grpc.Protos;
using Grpc.Core;

namespace Discount.Grpc.Services;
public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
{
    private readonly IDiscountRepository _repository;
    private readonly ILogger<DiscountService> _logger;
    private readonly IMapper _mapper;

    public DiscountService(IDiscountRepository repository, ILogger<DiscountService> logger, IMapper mapper)
    {
        _repository = repository;
        _logger = logger;
        _mapper = mapper;
    }

    public override async Task<CouponModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
    {
        var coupon = await _repository.GetDiscount(request.ProductName);
        if (coupon is null)
            throw new RpcException(new Status(StatusCode.NotFound, $"Discount with product name = {request.ProductName} is not found."));

        _logger.LogInformation(
            "Discount is retrieved for product name: {productName}, amount: {amount}",
            coupon.ProductName,
            coupon.Amount);

        var couponModel = _mapper.Map<CouponModel>(coupon);

        return couponModel;
    }

    public override async Task<CouponModel> CreateDiscount(CreateDiscountRequest request, ServerCallContext context)
    {
        var coupon = _mapper.Map<Coupon>(request.Coupon);

        await _repository.CreateDiscount(coupon);
        _logger.LogInformation("Discount is successfully created. Product name: {productName}", coupon.ProductName);

        var couponModel = _mapper.Map<CouponModel>(coupon);

        return couponModel;
    }

    public override async Task<CouponModel> UpdateDiscount(UpdateDiscountRequest request, ServerCallContext context)
    {
         var coupon = _mapper.Map<Coupon>(request.Coupon);
         await _repository.UpdateDiscount(coupon);

        _logger.LogInformation("Discount is successfully updated. Product name: {productName}", coupon.ProductName);

        var couponModel = _mapper.Map<CouponModel>(coupon);

        return couponModel;
    }

    public override async Task<DeleteDiscountResponse> DeleteDiscount(DeleteDiscountRequest request, ServerCallContext context)
    {
        var deleted = await _repository.DeleteDiscount(request.ProductName);

        return new DeleteDiscountResponse
        {
            Success = deleted
        };
    }
}