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
            new { Id = SeedIds.ProductHeadphones, Name = "Sony WH-1000XM5", Price = 349.99m, Category = "Audio", Stock = 25, Description = "Industry-leading noise canceling headphones with exceptional sound.", Image = "https://placehold.co/400?text=Sony+Headphones" },
            new { Id = SeedIds.ProductSmartWatch, Name = "Apple Watch Series 9", Price = 399.00m, Category = "Wearables", Stock = 15, Description = "Advanced health features and powerful performance.", Image = "https://placehold.co/400?text=Apple+Watch" },
            new { Id = SeedIds.ProductCoffeeMaker, Name = "Smart Espresso Machine", Price = 599.00m, Category = "Smart Home", Stock = 10, Description = "Wi-Fi enabled coffee maker for the perfect morning brew.", Image = "https://placehold.co/400?text=Smart+Espresso" },
            new { Id = SeedIds.ProductLaptop, Name = "MacBook Pro 14 M3", Price = 1599.00m, Category = "Computers", Stock = 8, Description = "The ultimate pro laptop with extreme performance and battery life.", Image = "https://placehold.co/400?text=MacBook+Pro" },
            new { Id = SeedIds.ProductMonitor, Name = "Samsung Odyssey G7", Price = 699.99m, Category = "Gaming", Stock = 12, Description = "27-inch curved gaming monitor with 240Hz refresh rate.", Image = "https://placehold.co/400?text=Gaming+Monitor" },
            new { Id = SeedIds.ProductKeyboard, Name = "Keychron Q3 Mechanical", Price = 169.00m, Category = "Accessories", Stock = 30, Description = "Fully customizable mechanical keyboard with QMK/VIA support.", Image = "https://placehold.co/400?text=Mechanical+Keyboard" },
            new { Id = SeedIds.ProductDeskChair, Name = "Razer Iskur Gaming Chair", Price = 499.00m, Category = "Gaming", Stock = 5, Description = "Ergonomic gaming chair with built-in lumbar support.", Image = "https://placehold.co/400?text=Gaming+Chair" },
            new { Id = SeedIds.ProductAirFryer, Name = "Xiaomi Smart Air Fryer", Price = 99.00m, Category = "Smart Home", Stock = 20, Description = "Healthier cooking with smart control via App.", Image = "https://placehold.co/400?text=Smart+AirFryer" },
            new { Id = SeedIds.ProductWaterBottle, Name = "Self-Cleaning Smart Bottle", Price = 79.00m, Category = "Gadgets", Stock = 50, Description = "UV-C LED technology cleans your water and bottle.", Image = "https://placehold.co/400?text=Smart+Bottle" },
            new { Id = SeedIds.ProductYogaMat, Name = "Smart Fitness Tracker Mat", Price = 120.00m, Category = "Gadgets", Stock = 20, Description = "Pressure-sensitive mat that tracks your poses and calories.", Image = "https://placehold.co/400?text=Fitness+Mat" }
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