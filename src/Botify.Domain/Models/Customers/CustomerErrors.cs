using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Customers
{
    public static class CustomerErrors
    {
        public static readonly Error IdRequired = Error.Validation(
            "Customer.IdRequired",
            "Customer ID is required.");

        public static readonly Error MerchantIdRequired = Error.Validation(
            "Customer.MerchantIdRequired",
            "Merchant ID is required.");

        public static readonly Error NameRequired = Error.Validation(
            "Customer.NameRequired",
            "Customer name is required.");

        public static readonly Error ContactRequired = Error.Validation(
            "Customer.ContactRequired",
            "Customer contact is required.");
    }
}
