using Botify.Domain.Models.Customers;

namespace Botify.Application.Features.Customers.Responses;

public sealed class CustomerDto
{
    public Guid Id { get; init; }
    public string Uuid { get; init; } = string.Empty;
    public string FullName { get; init; } = string.Empty;
    public string PhoneNumber { get; init; } = string.Empty;
    public Guid MerchantId { get; init; }

    // ✅ Audit fields
    public DateTime CreatedAt { get; init; }
    public DateTime? UpdatedAt { get; init; }
 
    public PlatformProfilesDto? Platforms { get; init; }

    public sealed class PlatformProfilesDto
    {
        public TelegramDto? Telegram { get; init; }
        public WhatsAppDto? WhatsApp { get; init; }
        public MessengerDto? Messenger { get; init; }
    }

    public sealed class TelegramDto
    {
        public string? Link { get; init; }
        public string? ChatId { get; init; }
    }

    public sealed class WhatsAppDto
    {
        public string? Link { get; init; }
        public string? ChatId { get; init; }
    }

    public sealed class MessengerDto
    {
        public string? Link { get; init; }
        public string? ChatId { get; init; }
    }

    public static CustomerDto FromDomain(Customer customer) => new()
    {
        Id = customer.Id,
        Uuid = customer.Uuid,
        FullName = customer.Name,
        PhoneNumber = customer.PhoneNumber,
        MerchantId = customer.MerchantId,

        // ✅ Audit mapping
        CreatedAt = customer.CreatedAtUtc,
        UpdatedAt = customer.LastModifiedUtc,
        Platforms = new PlatformProfilesDto
        {
            Telegram = customer.Telegram is null ? null : new TelegramDto
            {
                Link = customer.Telegram.Link,
                ChatId = customer.Telegram.ChatId
            },
            WhatsApp = customer.WhatsApp is null ? null : new WhatsAppDto
            {
                Link = customer.WhatsApp.Link,
                ChatId = customer.WhatsApp.ChatId
            },
            Messenger = customer.Messenger is null ? null : new MessengerDto
            {
                Link = customer.Messenger.Link,
                ChatId = customer.Messenger.ChatId
            }
        }
    };
}
