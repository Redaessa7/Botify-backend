using Botify.Domain.Models.Customers;
using Botify.Domain.Models.Customers.ValueObjects;
using Botify.Domain.Models.SupportRequests;
using Botify.Domain.Models.SupportRequests.Enums;
using Botify.Domain.Models.Merchants.ValueObjects;
using Microsoft.EntityFrameworkCore;

namespace Botify.Infrastructure.Persistence.Initialisation;

public static class SeedCustomersExtensions
{
    public static async Task SeedCustomersAsync(this ApplicationDbContext context, Guid merchantId)
    {
        if (await context.Customers.AnyAsync(c => c.MerchantId == merchantId))
            return;

        var customers = new[]
        {
            new
            {
                Id = SeedIds.CustomerJohnDoe,
                Uuid = "uuid-john-doe",
                Name = "John Doe",
                PhoneNumber = "+1555000001",
                Email = "john@example.com",
                Telegram = new TelegramProfile { Link = "https://t.me/john_doe", ChatId = "123456" }
            },
            new
            {
                Id = SeedIds.CustomerFatimaAli,
                Uuid = "uuid-fatima-ali",
                Name = "Fatima Ali",
                PhoneNumber = "+1555000002",
                Email = "fatima@example.com",
                Telegram = new TelegramProfile { Link = "https://t.me/fatima_ali", ChatId = "654321" }
            },
            new
            {
                Id = SeedIds.CustomerCarlos,
                Uuid = "uuid-carlos",
                Name = "Carlos Mendes",
                PhoneNumber = "+1555000003",
                Email = "carlos@example.com",
                Telegram = new TelegramProfile { Link = "https://t.me/carlos_mendes", ChatId = "789012" }
            }
        };

        foreach (var data in customers)
        {
            var customerResult = Customer.Create(
                data.Id,
                merchantId,
                data.Uuid,
                data.Name,
                data.PhoneNumber,
                data.Email,
                telegram: data.Telegram
            );

            if (customerResult.IsSuccess)
            {
                var customer = customerResult.Value;
                context.Customers.Add(customer);

                // ✅ Add a SupportRequest for each customer
                var supportRequestResult = SupportRequest.Create(
                    Guid.NewGuid(),                // SupportRequest ID
                    merchantId,                    // MerchantId
                    SeedIds.BotDefaultTelegram,            // Example BotId (replace with your actual bot seed ID)
                    "Default Bot",                 // BotName
                    BotPlatform.TELEGRAM,          // Platform (example: Telegram)
                    data.Telegram?.ChatId ?? "",   // PlatformContact
                    data.Name,                     // CustomerName
                    SupportRequestType.GENERAL,    // Type
                    RequestReason.CLIENT_REQUEST,   // Reason
                    data.Id,                       // CustomerId
                    $"Initial support request for {data.Name}" // Message
                );

                if (supportRequestResult.IsSuccess)
                    context.SupportRequests.Add(supportRequestResult.Value);
            }
        }
    }
}
