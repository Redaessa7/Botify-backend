using Botify.Domain.ResultPattern;
using System;

namespace Botify.Domain.Models.Customers
{
    public sealed class Customer : AuditableEntity
    {
        public string Name { get; private set; } = string.Empty;
        public string Contact { get; private set; } = string.Empty;
        public Guid MerchantId { get; private set; }

        private Customer() { }

        private Customer(Guid id, Guid merchantId, string name, string contact) : base(id)
        {
            MerchantId = merchantId;
            Name = name;
            Contact = contact;
        }

        public static Result<Customer> Create(
            Guid id,
            Guid merchantId,
            string name,
            string contact)
        {
            var validation = ValidateInputs(id, merchantId, name, contact);
            if (!validation.IsSuccess)
                return validation.Errors;

            return new Customer(id, merchantId, name.Trim(), contact.Trim());
        }

        public Result<Updated> Update(string name, string contact)
        {
            if (string.IsNullOrWhiteSpace(name))
                return CustomerErrors.NameRequired;

            if (string.IsNullOrWhiteSpace(contact))
                return CustomerErrors.ContactRequired;

            Name = name.Trim();
            Contact = contact.Trim();

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
