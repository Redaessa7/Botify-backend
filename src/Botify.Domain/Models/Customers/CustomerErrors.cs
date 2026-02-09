using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Customers
{
    public static class CustomerErrors
    {
        // General Validation
        public static readonly Error IdRequired = Error.Validation(
            "Customer.Id.Required",
            "Customer ID is required.");

        public static readonly Error UuidRequired = Error.Validation(
            "Customer.Uuid.Required",
            "Customer UUID is required.");

        public static readonly Error MerchantIdRequired = Error.Validation(
            "Customer.MerchantId.Required",
            "Merchant ID is required.");

        public static readonly Error NameRequired = Error.Validation(
            "Customer.Name.Required",
            "Customer name is required.");

        public static readonly Error ContactRequired = Error.Validation(
            "Customer.Contact.Required",
            "Customer contact is required.");

        public static readonly Error PhoneRequired = Error.Validation(
            "Customer.Phone.Required",
            "Customer phone number is required.");

        public static readonly Error EmailInvalid = Error.Validation(
            "Customer.Email.Invalid",
            "Customer email format is invalid.");

        // Social Profiles (only a few fields, no growth expected)
        public static readonly Error TelegramChatIdRequired = Error.Validation(
            "Customer.Telegram.ChatId.Required",
            "Telegram chat ID is required when Telegram profile is provided.");

        public static readonly Error WhatsAppChatIdRequired = Error.Validation(
            "Customer.WhatsApp.ChatId.Required",
            "WhatsApp chat ID is required when WhatsApp profile is provided.");

        public static readonly Error MessengerChatIdRequired = Error.Validation(
            "Customer.Messenger.ChatId.Required",
            "Messenger chat ID is required when Messenger profile is provided.");
        // Persistence Errors
        public static readonly Error NotFound = Error.NotFound(
            "Customer.NotFound",
            "The requested customer record was not found in our database.");

        public static readonly Error NotFoundByUuid = Error.NotFound(
            "Customer.NotFoundByUuid",
            "No customer matches the provided unique identifier.");
    }
}
