using System;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Shops;

namespace Botify.Application.Features.Merchants.Responses
{
    /// <summary>
    /// Response DTO for shops by merchant.
    /// </summary>
    public sealed class ShopResponse
    {
        public Guid Id { get; init; }
        public string ShopName { get; init; } = string.Empty;
        public string? IndustryName { get; init; }
        public string Currency { get; init; } = string.Empty;
        public string Language { get; init; } = string.Empty;
        public string? Address { get; init; }
        public string? BusinessEmail { get; init; }
        public string? BusinessPhoneNumber { get; init; }
        public SocialMediaLinks SocialMediaLinks { get; init; } = new SocialMediaLinks
        { Facebook = "", Instagram = "", LinkedIn = "", Twitter = "", Website = "", Whatsapp = "" };
        public string? LogoImageUrl { get; init; }
        public string? CoverImageUrl { get; init; }
        public decimal TaxRate { get; init; }
        public string? Description { get; init; }
        public string? Timezone { get; init; }
        public bool DeliveryEnabled { get; init; }
        public bool PickupEnabled { get; init; }
        public decimal? MinimumOrder { get; init; }
        public decimal? DeliveryFee { get; init; }
        public bool IsOpen { get; init; }
        public OpeningHours? OpeningHours { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime LastUpdated { get; init; }

        // Factory method to map from domain ShopSettings entity
        public static ShopResponse FromDomain(ShopSettings shop) => new()
        {
            Id = shop.Id,
            ShopName = shop.Name,
            IndustryName = shop.IndustryName,
            Currency = shop.Currency,
            Language = shop.Language,
            Address = shop.Address,
            BusinessEmail = shop.BusinessEmail,
            BusinessPhoneNumber = shop.BusinessPhoneNumber,
            SocialMediaLinks = shop.SocialMediaLinks ?? new SocialMediaLinks
            { Facebook = "", Instagram = "", LinkedIn = "", Twitter = "", Website = "", Whatsapp = "" },
            LogoImageUrl = shop.LogoImageUrl,
            CoverImageUrl = shop.CoverImageUrl,
            TaxRate = shop.TaxRate ?? 0,
            Description = shop.Description,
            Timezone = shop.Timezone,
            DeliveryEnabled = shop.DeliveryEnabled,
            PickupEnabled = shop.PickupEnabled,
            MinimumOrder = shop.MinimumOrder,
            DeliveryFee = shop.DeliveryFee,
            IsOpen = shop.IsOpen,
            OpeningHours = shop.OpeningHours,
            CreatedAt = shop.CreatedAtUtc,
            LastUpdated = shop.LastModifiedUtc
        };
    }
}
