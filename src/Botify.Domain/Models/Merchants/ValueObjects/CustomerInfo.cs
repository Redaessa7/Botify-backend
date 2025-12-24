using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.ValueObjects
{
    public sealed class CustomerInfo
    {
        public string Name { get; private set; } = string.Empty;
        public string? Email { get; private set; }
        public string? Telegram { get; private set; }
        public string? Whatsapp { get; private set; }
        public string? Messenger { get; private set; }

        private CustomerInfo() { }

        private CustomerInfo(string name, string? email, string? telegram, string? whatsapp, string? messenger)
        {
            Name = name;
            Email = email;
            Telegram = telegram;
            Whatsapp = whatsapp;
            Messenger = messenger;
        }

        #region Factory

        public static Result<CustomerInfo> Create(string name, string? email, string? telegram, string? whatsapp, string? messenger)
        {
            var validationResult = ValidateInputs(name, email, telegram, whatsapp, messenger);
            if (!validationResult.IsSuccess)
                return validationResult.Errors;

            var info = new CustomerInfo(name, email, telegram, whatsapp, messenger);
            return info;
        }

        #endregion

        #region Validation

        private static Result<Success> ValidateInputs(string name, string? email, string? telegram, string? whatsapp, string? messenger)
        {
            if (string.IsNullOrWhiteSpace(name))
                return CustomerInfoErrors.NameRequired;

            if (name.Length > 200)
                return CustomerInfoErrors.NameTooLong;

            if (!string.IsNullOrWhiteSpace(email) && email.Length > 200)
                return CustomerInfoErrors.EmailTooLong;

            if (!string.IsNullOrWhiteSpace(telegram) && telegram.Length > 100)
                return CustomerInfoErrors.TelegramTooLong;

            if (!string.IsNullOrWhiteSpace(whatsapp) && whatsapp.Length > 100)
                return CustomerInfoErrors.WhatsappTooLong;

            if (!string.IsNullOrWhiteSpace(messenger) && messenger.Length > 100)
                return CustomerInfoErrors.MessengerTooLong;

            return Result.Success;
        }

        #endregion
    }

    public static class CustomerInfoErrors
    {
        public static Error NameRequired =>
            Error.Validation("CustomerInfo.General.Name.Required", "Customer name is required.");

        public static Error NameTooLong =>
            Error.Validation("CustomerInfo.General.Name.TooLong", "Customer name cannot exceed 200 characters.");

        public static Error EmailTooLong =>
            Error.Validation("CustomerInfo.General.Email.TooLong", "Email cannot exceed 200 characters.");

        public static Error TelegramTooLong =>
            Error.Validation("CustomerInfo.General.Telegram.TooLong", "Telegram contact cannot exceed 100 characters.");

        public static Error WhatsappTooLong =>
            Error.Validation("CustomerInfo.General.Whatsapp.TooLong", "WhatsApp contact cannot exceed 100 characters.");

        public static Error MessengerTooLong =>
            Error.Validation("CustomerInfo.General.Messenger.TooLong", "Messenger contact cannot exceed 100 characters.");
    }
}
