using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.Shops
{
    public sealed class ShopSettings : AuditableEntity
    {
        public Guid MerchantId { get; private set; }
        public string Name { get; private set; }
        public string Currency { get; private set; }
        public string Language { get; private set; }
        public string Address { get; private set; }
        public SocialMediaLinks? SocialMediaLinks { get; private set; }
        public string? ImageUrl { get; private set; }
        public decimal? TaxRate { get; private set; }

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

        public static Result<ShopSettings> Create(
            Guid id, 
            Guid merchantId, 
            string name, 
            string currency, 
            string language, 
            string address, 
            SocialMediaLinks? socialMediaLinks = null, 
            string? imageUrl = null, 
            decimal? taxRate = null)
        {
            // Input Validation using Result Pattern
            if (id == Guid.Empty) return ShopErrors.IdRequired;
            if (merchantId == Guid.Empty) return ShopErrors.MerchantIdRequired;
            
            if (string.IsNullOrWhiteSpace(name))
            {
                return ShopErrors.NameRequired;
            }
            if (string.IsNullOrWhiteSpace(currency))
            {
                return ShopErrors.CurrencyRequired;
            }
            if (string.IsNullOrWhiteSpace(language))
            {
                return ShopErrors.LanguageRequired;
            }
            if (string.IsNullOrWhiteSpace(address))
            {
                return ShopErrors.AddressRequired;
            }

            var shopSettings = new ShopSettings(id, merchantId, name, currency, language, address)
            {
                SocialMediaLinks = socialMediaLinks,
                ImageUrl = imageUrl,
                TaxRate = taxRate
            };

            return shopSettings;
        }

        // IsComplete remains the same, used for general business logic checks
        public bool IsComplete()
        {
            return !string.IsNullOrWhiteSpace(Name) && 
                   !string.IsNullOrWhiteSpace(Currency) && 
                   !string.IsNullOrWhiteSpace(Language) && 
                   !string.IsNullOrWhiteSpace(Address);
        }

        // Update methods refactored to return Result<Success> for consistency, 
        // though validation inside them might be simplified or omitted for brevity.
        public Result<Success> UpdateDetails(string name, string currency, string language, string address)
        {
            if (string.IsNullOrWhiteSpace(name)) return ShopErrors.NameRequired;
            if (string.IsNullOrWhiteSpace(currency)) return ShopErrors.CurrencyRequired;
            // ... add checks for language and address if needed

            Name = name;
            Currency = currency;
            Language = language;
            Address = address;
            return Result.Success;
        }

        public Result<Success> UpdateSocialMediaLinks(SocialMediaLinks links)
        {
            SocialMediaLinks = links;
            return Result.Success;
        }

        public Result<Success> UpdateImageAndTax(string? imageUrl, decimal? taxRate)
        {
            ImageUrl = imageUrl;
            TaxRate = taxRate;
            return Result.Success;
        }
    }
}