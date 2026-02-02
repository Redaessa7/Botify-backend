using Botify.Domain.Models.Products;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedProductsExtensions
{
    public static async Task SeedLegacyProductsAsync(this ApplicationDbContext context, Guid merchantId)
    {
        if (await context.Products.AnyAsync(p => p.MerchantId == merchantId))
            return;

        var products = new[]
        {
        new { Id = SeedIds.ProductHeadphones, Name = "Wireless Headphones", Price = 199.99m, ImageUrl = "https://example.com/images/headphones.png", Category = "Electronics", Stock = 50, Description = "Noise-cancelling headphones" },
        new { Id = SeedIds.ProductSmartWatch, Name = "Smart Watch", Price = 149.99m, ImageUrl = "https://example.com/images/smartwatch.png", Category = "Wearables", Stock = 30, Description = "Fitness tracking smartwatch" },
        new { Id = SeedIds.ProductCoffeeMaker, Name = "Coffee Maker", Price = 89.99m, ImageUrl = "https://example.com/images/coffeemaker.png", Category = "Home Appliances", Stock = 20, Description = "Automatic drip coffee maker" }
    };

        foreach (var data in products)
        {
            var result = Product.Create(data.Id, merchantId, data.Name, data.Price, data.ImageUrl, data.Category, data.Stock, data.Description);
            if (result.IsSuccess)
                context.Products.Add(result.Value);
        }
    }

}
