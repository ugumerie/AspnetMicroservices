using Microsoft.Extensions.Logging;
using Ordering.Domain.Entities;

namespace Ordering.Infrastructure.Persistence;

public class OrderContextSeed
{
    public static async Task SeedAsync(OrderContext orderContext, ILogger<OrderContextSeed> logger)
    {
        if (!orderContext.Orders!.Any())
        {
            orderContext.Orders?.AddRange(GetPreConfiguredOrders());
            await orderContext.SaveChangesAsync();
            logger.LogInformation("Seed database associated with context {DbContextName}", typeof(OrderContext).Name);
        }
    }

    private static IEnumerable<Order> GetPreConfiguredOrders()
    {
        return new List<Order>
        {
            new Order 
            {
                UserName = "swn",
                FirstName = "Ugochukwu",
                LastName = "Umerie",
                EmailAddress = "umerieugochukwu@gmail.com",
                AddressLine = "Ziks Avenue",
                Country = "Nigeria",
                TotalPrice = 350
            }
        };
    }
}