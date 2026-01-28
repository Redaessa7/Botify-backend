using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Shops
{
    public static class ShopErrors
    {
        // Business Logic Error
        public static Error ShopIncomplete =>
            Error.Validation("Shop.Incomplete", "Shop settings must be complete before adding or activating bots.");

        // Validation Errors used by AssignShopRequestValidator
        public static readonly Error NameRequired =
            Error.Validation("Shop.NameRequired", "Shop name is required.");

        public static readonly Error NameTooLong =
            Error.Validation("Shop.NameTooLong", "Shop name must not exceed the maximum length.");

        public static readonly Error CurrencyRequired =
            Error.Validation("Shop.CurrencyRequired", "Currency is required.");

        public static readonly Error CurrencyInvalid =
            Error.Validation("Shop.CurrencyInvalid", "Currency must be a valid ISO 3-letter code.");

        public static readonly Error LanguageRequired =
            Error.Validation("Shop.LanguageRequired", "Language is required.");

        public static readonly Error LanguageInvalid =
            Error.Validation("Shop.LanguageInvalid", "Language must be a valid language code.");

        public static readonly Error AddressRequired =
            Error.Validation("Shop.AddressRequired", "Address is required.");

        public static readonly Error AddressTooLong =
            Error.Validation("Shop.AddressTooLong", "Address exceeds the maximum allowed length.");

        public static readonly Error SocialMediaLinksInvalid =
            Error.Validation("Shop.SocialMediaLinksInvalid", "One or more social media links are invalid.");

        public static readonly Error ImageInvalid =
            Error.Validation("Shop.ImageInvalid", "Uploaded image is invalid or not supported.");

        public static readonly Error ImageTooLarge =
            Error.Validation("Shop.ImageTooLarge", "Uploaded image exceeds the maximum allowed size.");

        public static readonly Error TaxRateInvalid =
            Error.Validation("Shop.TaxRateInvalid", "Tax rate must be between 0 and 1.");

        public static readonly Error IdRequired =
            Error.Validation("Shop.IdRequired", "Shop ID cannot be empty.");

        public static readonly Error MerchantIdRequired =
            Error.Validation("Shop.MerchantIdRequired", "Merchant ID is required.");

        // Not found / state errors
        public static readonly Error NotFound =
            Error.NotFound("Shop.NotFound", "Shop not found.");

        public static readonly Error AlreadyExists =
            Error.Conflict("Shop.AlreadyExists", "A shop with the same identifier already exists.");
    }
}
