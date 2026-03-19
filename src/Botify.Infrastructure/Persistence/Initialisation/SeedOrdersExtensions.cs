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
        // منع التكرار
        if (await context.Orders.AnyAsync(o => o.MerchantId == merchantId))
            return;

        // 1. بناء عناصر الطلبات (OrderItem)
        // ملاحظة: تأكد أن ProductIds موجودة فعلياً في جدول Products قبل تشغيل هذا الجزء
        var headphonesItem = OrderItem.Create(Guid.NewGuid(), SeedIds.ProductHeadphones, "Wireless Headphones", 1, 199.99m).Value;
        var watchItem = OrderItem.Create(Guid.NewGuid(), SeedIds.ProductSmartWatch, "Smart Watch", 1, 149.99m).Value;
        var coffeeItem = OrderItem.Create(Guid.NewGuid(), SeedIds.ProductCoffeeMaker, "Coffee Maker", 1, 89.99m).Value;
        var headphonesItem2 = OrderItem.Create(Guid.NewGuid(), SeedIds.ProductHeadphones, "Wireless Headphones", 1, 199.99m).Value;

        var ordersData = new[]
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
                Platform = BotPlatform.TELEGRAM,
                CustomerId = SeedIds.CustomerFatimaAli,
                CustomerName = "Fatima Ali",
                CustomerContact = "fatima@example.com",
                Items = new List<OrderItem> { watchItem }
            },
            new {
                Id = SeedIds.OrderDelivered,
                Status = OrderStatus.Delivered,
                Platform = BotPlatform.TELEGRAM,
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

        foreach (var data in ordersData)
        {
            var uuid = OrderUuidGenerator.Generate(merchantId, data.CustomerId);

            var result = Order.Create(
                id: data.Id,
                uuid: uuid,
                merchantId: merchantId,
                customerId: data.CustomerId,
                customerName: data.CustomerName,
                customerContact: data.CustomerContact,
                platform: data.Platform,
                items: data.Items,
                totalAmount: data.Items.Sum(i => i.UnitPrice * i.Quantity),
                currency: "USD",
                status: data.Status,
                deliveryAddress: "123 Test Street",
                notes: "Seeded order"
            );

            if (result.IsSuccess)
            {
                context.Orders.Add(result.Value);
            }
        }

        // حفظ جميع الطلبات دفعة واحدة لضمان الـ Transaction
        await context.SaveChangesAsync();
    }
}