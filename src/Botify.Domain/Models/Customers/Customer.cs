using Botify.Domain.Models.Customers.ValueObjects;
using Botify.Domain.ResultPattern;
using System;

namespace Botify.Domain.Models.Customers
{
    public sealed class Customer : AuditableEntity
    {
        public string Uuid { get; private set; } = string.Empty; // external UUID

        // Core Info
        public string Name { get; private set; } = string.Empty;
        public string PhoneNumber { get; private set; } = string.Empty;
        public Guid MerchantId { get; private set; }
        public string? Email { get; private set; }

        // Social Profiles (optional)
        public TelegramProfile? Telegram { get; private set; }
        public WhatsAppProfile? WhatsApp { get; private set; }
        public MessengerProfile? Messenger { get; private set; }

        private Customer() { }

        private Customer(
            Guid id,
            Guid merchantId,
            string uuid,
            string name,
            string phoneNumber,
            string? email = null,
            TelegramProfile? telegram = null,
            WhatsAppProfile? whatsapp = null,
            MessengerProfile? messenger = null)
            : base(id)
        {
            MerchantId = merchantId;
            Uuid = uuid;
            Name = name;
            PhoneNumber = phoneNumber;
            Email = email;
            Telegram = telegram;
            WhatsApp = whatsapp;
            Messenger = messenger;
        }

        public static Result<Customer> Create(
            Guid id,
            Guid merchantId,
            string uuid,
            string name,
            string phoneNumber,
            string? email = null,
            TelegramProfile? telegram = null,
            WhatsAppProfile? whatsapp = null,
            MessengerProfile? messenger = null)
        {
            var validation = ValidateInputs(id, merchantId, uuid, name, phoneNumber);
            if (!validation.IsSuccess)
                return validation.Errors;

            return new Customer(id, merchantId, uuid.Trim(), name.Trim(), phoneNumber.Trim(), email?.Trim(), telegram, whatsapp, messenger);
        }

        #region Validation

        private static Result<Success> ValidateInputs(Guid id, Guid merchantId, string uuid, string name, string phoneNumber)
        {
            if (id == Guid.Empty)
                return CustomerErrors.IdRequired;

            if (merchantId == Guid.Empty)
                return CustomerErrors.MerchantIdRequired;

            if (string.IsNullOrWhiteSpace(uuid))
                return CustomerErrors.UuidRequired;

            if (string.IsNullOrWhiteSpace(name))
                return CustomerErrors.NameRequired;

            if (string.IsNullOrWhiteSpace(phoneNumber))
                return CustomerErrors.PhoneRequired;

            return Result.Success;
        }

        #endregion
    }

}
