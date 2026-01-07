using System;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.Shops
{
    public sealed class ShopSettings : AuditableEntity
    {
        public Guid MerchantId { get; private set; }
        public string Name { get; private set; }
        public string? IndustryName { get; private set; }
        public string Currency { get; private set; }
        public string Language { get; private set; }
        public string Address { get; private set; }
        public string? BusinessEmail { get; private set; }
        public string? BusinessPhoneNumber { get; private set; }
        public SocialMediaLinks? SocialMediaLinks { get; private set; }
        public string? LogoImageUrl { get; private set; }
        public string? CoverImageUrl { get; private set; }
        public decimal? TaxRate { get; private set; }
        public string? Description { get; private set; }
        public string? Timezone { get; private set; }
        public bool DeliveryEnabled { get; private set; }
        public bool PickupEnabled { get; private set; }
        public decimal? MinimumOrder { get; private set; }
        public decimal? DeliveryFee { get; private set; }
        public bool IsOpen { get; private set; }
        public OpeningHours OpeningHours { get; private set; }

        private ShopSettings() { }

        private ShopSettings(
            Guid id,
            Guid merchantId,
            string name,
            string currency,
            string language,
            string address)
            : base(id)
        {
            MerchantId = merchantId;
            Name = name;
            Currency = currency;
            Language = language;
            Address = address;
        }

        public static Result<ShopSettings> Set(
            Guid id,
            Guid merchantId,
            string name,
            string currency,
            string language,
            string address,
            string? industryName = null,
            string? businessEmail = null,
            string? businessPhoneNumber = null,
            SocialMediaLinks? socialMediaLinks = null,
            string? logoImageUrl = null,
            string? coverImageUrl = null,
            decimal? taxRate = null,
            string? description = null,
            string? timezone = null,
            bool deliveryEnabled = false,
            bool pickupEnabled = false,
            decimal? minimumOrder = null,
            decimal? deliveryFee = null,
            bool isOpen = false,
            OpeningHours? openingHours = null)
        {
            if (id == Guid.Empty) return ShopErrors.IdRequired;
            if (merchantId == Guid.Empty) return ShopErrors.MerchantIdRequired;
            if (string.IsNullOrWhiteSpace(name)) return ShopErrors.NameRequired;
            if (string.IsNullOrWhiteSpace(currency)) return ShopErrors.CurrencyRequired;
            if (string.IsNullOrWhiteSpace(language)) return ShopErrors.LanguageRequired;
            if (string.IsNullOrWhiteSpace(address)) return ShopErrors.AddressRequired;

            var shopSettings = new ShopSettings(id, merchantId, name, currency, language, address)
            {
                IndustryName = industryName,
                BusinessEmail = businessEmail,
                BusinessPhoneNumber = businessPhoneNumber,
                SocialMediaLinks = socialMediaLinks,
                LogoImageUrl = logoImageUrl,
                CoverImageUrl = coverImageUrl,
                TaxRate = taxRate,
                Description = description,
                Timezone = timezone,
                DeliveryEnabled = deliveryEnabled,
                PickupEnabled = pickupEnabled,
                MinimumOrder = minimumOrder,
                DeliveryFee = deliveryFee,
                IsOpen = isOpen,
                OpeningHours = openingHours
            };

            return shopSettings;
        }

        public bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(Name) &&
                   !string.IsNullOrWhiteSpace(Currency) &&
                   !string.IsNullOrWhiteSpace(Language) &&
                   !string.IsNullOrWhiteSpace(Address);
        }

        public Result<Success> Set(
            string? name = null,
            string? currency = null,
            string? language = null,
            string? address = null,
            string? industryName = null,
            string? businessEmail = null,
            string? businessPhoneNumber = null,
            SocialMediaLinks? socialMediaLinks = null,
            string? logoImageUrl = null,
            string? coverImageUrl = null,
            decimal? taxRate = null,
            string? description = null,
            string? timezone = null,
            bool? deliveryEnabled = null,
            bool? pickupEnabled = null,
            decimal? minimumOrder = null,
            decimal? deliveryFee = null,
            bool? isOpen = null,
            OpeningHours? openingHours = null)
        {
            if (name is not null)
            {
                if (string.IsNullOrWhiteSpace(name)) return ShopErrors.NameRequired;
                Name = name;
            }
            if (currency is not null)
            {
                if (string.IsNullOrWhiteSpace(currency)) return ShopErrors.CurrencyRequired;
                Currency = currency;
            }
            if (language is not null)
            {
                if (string.IsNullOrWhiteSpace(language)) return ShopErrors.LanguageRequired;
                Language = language;
            }
            if (address is not null)
            {
                if (string.IsNullOrWhiteSpace(address)) return ShopErrors.AddressRequired;
                Address = address;
            }

            IndustryName = industryName ?? IndustryName;
            BusinessEmail = businessEmail ?? BusinessEmail;
            BusinessPhoneNumber = businessPhoneNumber ?? BusinessPhoneNumber;
            SocialMediaLinks = socialMediaLinks ?? SocialMediaLinks;
            LogoImageUrl = logoImageUrl ?? LogoImageUrl;
            CoverImageUrl = coverImageUrl ?? CoverImageUrl;
            TaxRate = taxRate ?? TaxRate;
            Description = description ?? Description;
            Timezone = timezone ?? Timezone;
            DeliveryEnabled = deliveryEnabled ?? DeliveryEnabled;
            PickupEnabled = pickupEnabled ?? PickupEnabled;
            MinimumOrder = minimumOrder ?? MinimumOrder;
            DeliveryFee = deliveryFee ?? DeliveryFee;
            IsOpen = isOpen ?? IsOpen;
            OpeningHours = openingHours ?? OpeningHours;

            return Result.Success;
        }
 public static Result<ShopSettings> Create( Guid merchantId, string name, string currency, string language, string address, string? industryName = null, string? businessEmail = null, string? businessPhoneNumber = null, SocialMediaLinks? socialMediaLinks = null, string? logoImageUrl = null, string? coverImageUrl = null, decimal? taxRate = null, string? description = null, string? timezone = null, bool deliveryEnabled = false, bool pickupEnabled = false, decimal? minimumOrder = null, decimal? deliveryFee = null, bool isOpen = false, OpeningHours? openingHours = null)
        { 
            if (merchantId == Guid.Empty)
                return ShopErrors.MerchantIdRequired;
            if (string.IsNullOrWhiteSpace(name)) 
                return ShopErrors.NameRequired; 
            if (string.IsNullOrWhiteSpace(currency))
                return ShopErrors.CurrencyRequired; 
            if (string.IsNullOrWhiteSpace(language)) 
                return ShopErrors.LanguageRequired;
            if (string.IsNullOrWhiteSpace(address)) 
                return ShopErrors.AddressRequired;
            var shopSettings = new ShopSettings(Guid.NewGuid(), merchantId, name, currency, language, address) { IndustryName = industryName, BusinessEmail = businessEmail, BusinessPhoneNumber = businessPhoneNumber, SocialMediaLinks = socialMediaLinks, LogoImageUrl = logoImageUrl, CoverImageUrl = coverImageUrl, TaxRate = taxRate, Description = description, Timezone = timezone, DeliveryEnabled = deliveryEnabled, PickupEnabled = pickupEnabled, MinimumOrder = minimumOrder, DeliveryFee = deliveryFee, IsOpen = isOpen, OpeningHours = openingHours };
            
            return shopSettings; 
        }

        public Result<Success> Update(
            string? name = null,
            string? currency = null,
            string? language = null,
            string? address = null,
            string? industryName = null,
            string? businessEmail = null,
            string? businessPhoneNumber = null,
            SocialMediaLinks? socialMediaLinks = null,
            string? logoImageUrl = null,
            string? coverImageUrl = null,
            decimal? taxRate = null,
            string? description = null,
            string? timezone = null,
            bool? deliveryEnabled = null,
            bool? pickupEnabled = null,
            decimal? minimumOrder = null,
            decimal? deliveryFee = null,
            bool? isOpen = null,
            OpeningHours? openingHours = null)
        {
            // Validate required fields only if they are being updated
            if (name is not null && string.IsNullOrWhiteSpace(name))
                return ShopErrors.NameRequired;
            if (currency is not null && string.IsNullOrWhiteSpace(currency))
                return ShopErrors.CurrencyRequired;
            if (language is not null && string.IsNullOrWhiteSpace(language))
                return ShopErrors.LanguageRequired;
            if (address is not null && string.IsNullOrWhiteSpace(address))
                return ShopErrors.AddressRequired;

            // Apply updates
            Name = name ?? Name;
            Currency = currency ?? Currency;
            Language = language ?? Language;
            Address = address ?? Address;
            IndustryName = industryName ?? IndustryName;
            BusinessEmail = businessEmail ?? BusinessEmail;
            BusinessPhoneNumber = businessPhoneNumber ?? BusinessPhoneNumber;
            SocialMediaLinks = socialMediaLinks ?? SocialMediaLinks;
            LogoImageUrl = logoImageUrl ?? LogoImageUrl;
            CoverImageUrl = coverImageUrl ?? CoverImageUrl;
            TaxRate = taxRate ?? TaxRate;
            Description = description ?? Description;
            Timezone = timezone ?? Timezone;
            DeliveryEnabled = deliveryEnabled ?? DeliveryEnabled;
            PickupEnabled = pickupEnabled ?? PickupEnabled;
            MinimumOrder = minimumOrder ?? MinimumOrder;
            DeliveryFee = deliveryFee ?? DeliveryFee;
            IsOpen = isOpen ?? IsOpen;
            OpeningHours = openingHours ?? OpeningHours;

            return Result.Success;
        }

        public static ShopSettings CreateDefault(Guid merchantId)
        {
            // Define default day opening hours
            var monday = DayOpeningHours.Create(new TimeOnly(9, 0), new TimeOnly(17, 0), true);
            var tuesday = DayOpeningHours.Create(new TimeOnly(9, 0), new TimeOnly(17, 0), true);
            var wednesday = DayOpeningHours.Create(new TimeOnly(9, 0), new TimeOnly(17, 0), true);
            var thursday = DayOpeningHours.Create(new TimeOnly(9, 0), new TimeOnly(17, 0), true);
            var friday = DayOpeningHours.Create(new TimeOnly(9, 0), new TimeOnly(17, 0), true);
            var saturday = DayOpeningHours.Create(new TimeOnly(10, 0), new TimeOnly(14, 0), true);
            var sunday = DayOpeningHours.Create(new TimeOnly(0, 0), new TimeOnly(0, 0), false);

            // Build OpeningHours value object
            var defaultOpeningHours = OpeningHours.Create(
                monday,
                tuesday,
                wednesday,
                thursday,
                friday,
                saturday,
                sunday
            );

            // Return a ShopSettings with default values
            return new ShopSettings(Guid.NewGuid(), merchantId, "Default Shop", "USD", "en", "123 Default Street")
            {
                IndustryName = "General",
                BusinessEmail = "default@shop.com",
                BusinessPhoneNumber = "+10000000000",
                SocialMediaLinks = new SocialMediaLinks
                {
                    Facebook = "https://facebook.com/defaultshop",
                    Instagram = "https://instagram.com/defaultshop",
                    Twitter = "https://twitter.com/defaultshop",
                    Website = "https://defaultshop.com",
                    Whatsapp = "https://wa.me/10000000000",
                    LinkedIn = "https://linkedin.com/company/defaultshop"
                },
                LogoImageUrl = "https://cdn.defaultshop.com/logo.png",
                CoverImageUrl = "https://cdn.defaultshop.com/cover.png",
                TaxRate = 0.15m,
                Description = "This is a default shop description.",
                Timezone = "UTC",
                DeliveryEnabled = true,
                PickupEnabled = true,
                MinimumOrder = 10,
                DeliveryFee = 5,
                IsOpen = true,
                OpeningHours = defaultOpeningHours
            };
        }

    }
}
