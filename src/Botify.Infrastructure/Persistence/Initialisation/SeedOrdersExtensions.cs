using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Orders;
using Botify.Domain.Models.Orders.Enums;
using Botify.Domain.Models.Orders.Helpers;
using Microsoft.EntityFrameworkCore;

namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedOrdersExtensions
{
    public static async Task SeedOrdersAsync(this ApplicationDbContext context, Guid merchantId)
    {
        if (await context.Orders.AnyAsync(o => o.MerchantId == merchantId))
            return;

        // Build items for each order explicitly
        var headphonesItem = OrderItem.Create(Guid.NewGuid(), SeedIds.ProductHeadphones, "Wireless Headphones", 1, 199.99m).Value;
        var watchItem = OrderItem.Create(Guid.NewGuid(), SeedIds.ProductSmartWatch, "Smart Watch", 1, 149.99m).Value;
        var coffeeItem = OrderItem.Create(Guid.NewGuid(), SeedIds.ProductCoffeeMaker, "Coffee Maker", 1, 89.99m).Value;
        var headphonesItem2 = OrderItem.Create(Guid.NewGuid(), SeedIds.ProductHeadphones, "Wireless Headphones", 1, 199.99m).Value;

        var orders = new[]
        {
        new {
            Id = SeedIds.OrderPending,
            Status = OrderStatus.Pending,
            Platform = BotPlatform.TELEGRAM,
            CustomerId = SeedIds.CustomerJohnDoe,
            CustomerName = "John Doe",
            CustomerContact = "john@example.com",
            Items = new List<OrderItem> { headphonesItem2 }
        },
        new {
            Id = SeedIds.OrderConfirmed,
            Status = OrderStatus.Confirmed,
            Platform = BotPlatform.WHATSAPP,
            CustomerId = SeedIds.CustomerFatimaAli,
            CustomerName = "Fatima Ali",
            CustomerContact = "fatima@example.com",
            Items = new List<OrderItem> { watchItem }
        },
        new {
            Id = SeedIds.OrderDelivered,
            Status = OrderStatus.Delivered,
            Platform = BotPlatform.MESSENGER,
            CustomerId = SeedIds.CustomerCarlos,
            CustomerName = "Carlos Mendes",
            CustomerContact = "carlos@example.com",
            Items = new List<OrderItem> { coffeeItem }
        },
        new {
            Id = SeedIds.OrderCancelled,
            Status = OrderStatus.Cancelled,
            Platform = BotPlatform.TELEGRAM,
            CustomerId = SeedIds.CustomerJohnDoe,
            CustomerName = "John Doe",
            CustomerContact = "john@example.com",
            Items = new List<OrderItem> { headphonesItem }
        }
    };

        foreach (var data in orders)
        {
            var uuid = OrderUuidGenerator.Generate(merchantId, data.CustomerId);

            var result = Order.Create(
                id: data.Id,
                uuid: uuid, // meaningful UUID
                merchantId: merchantId,
                customerId: data.CustomerId,
                customerName: data.CustomerName,
                customerContact: data.CustomerContact,
                platform: data.Platform,
                items: data.Items,
                totalAmount: data.Items.Sum(i => i.LineTotal),
                currency: "USD",
                status: data.Status,
                deliveryAddress: "123 Test Street",
                notes: "Seeded order"
            );


            if (result.IsSuccess)
                context.Orders.Add(result.Value);
        }
    }

}
