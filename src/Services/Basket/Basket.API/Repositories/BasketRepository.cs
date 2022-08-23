using System.Text.Json;
using Basket.API.Entities;
using Microsoft.Extensions.Caching.Distributed;

namespace Basket.API.Repositories;
public class BasketRepository : IBasketRepository
{
    private readonly IDistributedCache _redisCache;

    public BasketRepository(IDistributedCache redisCache)
    {
        _redisCache = redisCache;
    }

    public async Task DeleteBasket(string username)
    {
        await _redisCache.RemoveAsync(username);
    }

    public async Task<ShoppingCart?> GetBasket(string username)
    {
        var basket = await _redisCache.GetStringAsync(username);
        
        if (string.IsNullOrWhiteSpace(basket)) return null;

        return JsonSerializer.Deserialize<ShoppingCart>(basket);
    }

    public async Task<ShoppingCart?> UpdateBasket(ShoppingCart basket)
    {
        await _redisCache.SetStringAsync(basket.UserName, JsonSerializer.Serialize(basket));
        return await GetBasket(basket.UserName!);
    }
}