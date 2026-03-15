using Botify.Domain.Models.Products;
using Microsoft.EntityFrameworkCore;

namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedProductsExtensions
{
    public static async Task SeedLegacyProductsAsync(this ApplicationDbContext context, Guid merchantId)
    {
        if (await context.Products.AnyAsync(p => p.MerchantId == merchantId))
            return;

        var techProducts = new[]
        {
            new { Id = SeedIds.ProductHeadphones, Name = "Sony WH-1000XM5", Price = 349.99m, Category = "Audio", Stock = 25, Description = "Industry-leading noise canceling headphones with exceptional sound.", Image = "https://vjjx8gr7or.ufs.sh/f/YEh2DAq4MAFcXzQZtZk9IsKP0fD8eqRxEHzB3wGS6dWclgTy" },
            new { Id = SeedIds.ProductSmartWatch, Name = "Apple Watch Series 9", Price = 399.00m, Category = "Wearables", Stock = 15, Description = "Advanced health features and powerful performance.", Image = "https://vjjx8gr7or.ufs.sh/f/YEh2DAq4MAFckM9JYtUZzalh4bQTe6YgUHIA8t37iM0sj1Dv" },
            new { Id = SeedIds.ProductCoffeeMaker, Name = "Smart Espresso Machine", Price = 599.00m, Category = "Smart Home", Stock = 10, Description = "Wi-Fi enabled coffee maker for the perfect morning brew.", Image = "https://vjjx8gr7or.ufs.sh/f/YEh2DAq4MAFcf2mj4l5rXIKohuix4DZ6N829QCTU5stavOgY" },
            new { Id = SeedIds.ProductLaptop, Name = "MacBook Pro 14 M3", Price = 1599.00m, Category = "Computers", Stock = 8, Description = "The ultimate pro laptop with extreme performance and battery life.", Image = "https://vjjx8gr7or.ufs.sh/f/YEh2DAq4MAFcyKYPtW63HWQikyc4pbxfZnSwvP0hLTeJ2EFN" },
            new { Id = SeedIds.ProductMonitor, Name = "Samsung Odyssey G7", Price = 699.99m, Category = "Gaming", Stock = 12, Description = "27-inch curved gaming monitor with 240Hz refresh rate.", Image = "https://vjjx8gr7or.ufs.sh/f/YEh2DAq4MAFcKVBYQCLQ5kurj17aGqDobRenSsJidpX0VZ9f" },
            new { Id = SeedIds.ProductKeyboard, Name = "Keychron Q3 Mechanical", Price = 169.00m, Category = "Accessories", Stock = 30, Description = "Fully customizable mechanical keyboard with QMK/VIA support.", Image = "https://vjjx8gr7or.ufs.sh/f/YEh2DAq4MAFctwQY4PUDNd0SlrR8w3HO9E4p25kCgUIAqYLf" },
            new { Id = SeedIds.ProductDeskChair, Name = "Razer Iskur Gaming Chair", Price = 499.00m, Category = "Gaming", Stock = 5, Description = "Ergonomic gaming chair with built-in lumbar support.", Image = "https://vjjx8gr7or.ufs.sh/f/YEh2DAq4MAFc9bHBnNUQfwJzW17sKEOaicZ0x68gpnQmDFdj" },
            new { Id = SeedIds.ProductAirFryer, Name = "Xiaomi Smart Air Fryer", Price = 99.00m, Category = "Smart Home", Stock = 20, Description = "Healthier cooking with smart control via App.", Image = "https://vjjx8gr7or.ufs.sh/f/YEh2DAq4MAFcTykG0jGECKwmhLkV0UWNHAlEYS786IGxPeac" },
            new { Id = SeedIds.ProductWaterBottle, Name = "Self-Cleaning Smart Bottle", Price = 79.00m, Category = "Gadgets", Stock = 50, Description = "UV-C LED technology cleans your water and bottle.", Image = "https://vjjx8gr7or.ufs.sh/f/YEh2DAq4MAFcvtU0wsFNGEVXCut6yrHmncWDpYjeOiSPQKsh" },
        };

        foreach (var data in techProducts)
        {
            var result = Product.Create(
                data.Id,
                merchantId,
                data.Name,
                data.Price,
                data.Image,
                data.Category,
                data.Stock,
                data.Description
            );

            if (result.IsSuccess)
                context.Products.Add(result.Value);
        }

        await context.SaveChangesAsync();
    }
}