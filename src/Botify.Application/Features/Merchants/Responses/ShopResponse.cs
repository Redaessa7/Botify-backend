using Botify.Domain.Models.Merchants.Shops;
using Botify.Domain.Models.Merchants.ValueObjects;

namespace Botify.Application.Features.Merchants.Responses
{
    /// <summary>
    /// Response DTO for shops by merchant.
    /// </summary>
    public sealed class ShopResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Currency { get; init; } = string.Empty;
        public string Language { get; init; } = string.Empty;
        public string? Address { get; init; }
        public SocialMediaLinks SocialMediaLinks { get; init; } = new SocialMediaLinks
        { Facebook = "", Instagram = "", LinkedIn = "", Twitter = "", Website = "", Whatsapp = ""};
        public string? ImageUrl { get; init; }
        public decimal TaxRate { get; init; }
        public DateTimeOffset CreatedAt { get; init; }
        // Factory method to map from domain ShopSettings (or Shop) entity
        public static ShopResponse FromDomain(ShopSettings shop) => new()
        {
            Id = shop.MerchantId,
            Name = shop.Name,
            Currency = shop.Currency,
            Language = shop.Language,
            Address = shop.Address,
            SocialMediaLinks = shop.SocialMediaLinks ?? new SocialMediaLinks
            { Facebook = "", Instagram = "", LinkedIn = "", Twitter = "", Website = "", Whatsapp = ""},
            ImageUrl = shop.ImageUrl,
            TaxRate = shop.TaxRate ?? 0,
            CreatedAt = shop.CreatedAtUtc,
        };
    }
}
