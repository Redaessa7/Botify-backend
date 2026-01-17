using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;
using System;

namespace Botify.Domain.Models.Customers
{
    public sealed class Customer : AuditableEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Contact { get; private set; } = string.Empty;

        public BotPlatform Platform { get; private set; }
        public string PhoneNumber { get; private set; } = string.Empty;
        public Guid MerchantId { get; private set; }
 
        public string? Email { get; private set; }

        private Customer() { }

        private Customer(Guid id, Guid merchantId, string name, string contact, string? email = null)
            : base(id)
        {
            MerchantId = merchantId;
            Name = name;
            Contact = contact;
            Email = email;
        }

        public static Result<Customer> Create(
            Guid id,
            Guid merchantId,
            string name,
            string contact,
            string? email = null)
        {
            var validation = ValidateInputs(id, merchantId, name, contact);
            if (!validation.IsSuccess)
                return validation.Errors;

            return new Customer(id, merchantId, name.Trim(), contact.Trim(), email?.Trim());
        }

        public Result<Updated> Update(string name, string contact, string? email = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                return CustomerErrors.NameRequired;

            if (string.IsNullOrWhiteSpace(contact))
                return CustomerErrors.ContactRequired;

            Name = name.Trim();
            Contact = contact.Trim();
            Email = string.IsNullOrWhiteSpace(email) ? null : email.Trim();

            return Result.Updated;
        }

        #region Validation

        private static Result<Success> ValidateInputs(Guid id, Guid merchantId, string name, string contact)
        {
            if (id == Guid.Empty)
                return CustomerErrors.IdRequired;

            if (merchantId == Guid.Empty)
                return CustomerErrors.MerchantIdRequired;

            if (string.IsNullOrWhiteSpace(name))
                return CustomerErrors.NameRequired;

            if (string.IsNullOrWhiteSpace(contact))
                return CustomerErrors.ContactRequired;

            return Result.Success;
        }

        #endregion
    }
}
