using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Orders
{
    public static class OrderErrors
    {
        public static readonly Error IdRequired = Error.Validation(
            "Order.IdRequired",
            "Order ID is required.");

        public static readonly Error MerchantIdRequired = Error.Validation(
            "Order.MerchantIdRequired",
            "Merchant ID is required.");

        public static readonly Error CustomerIdRequired = Error.Validation(
            "Order.CustomerIdRequired",
            "Customer ID is required.");

        public static readonly Error CustomerNameRequired = Error.Validation(
            "Order.CustomerNameRequired",
            "Customer name is required.");

        public static readonly Error CustomerContactRequired = Error.Validation(
            "Order.CustomerContactRequired",
            "Customer contact is required.");

        public static readonly Error PlatformInvalid = Error.Validation(
            "Order.PlatformInvalid",
            "Platform is invalid or unsupported.");

        public static readonly Error ItemsRequired = Error.Validation(
            "Order.ItemsRequired",
            "At least one order item is required.");

        public static readonly Error TotalAmountInvalid = Error.Validation(
            "Order.TotalAmountInvalid",
            "Total amount must be greater than zero.");

        public static readonly Error CurrencyRequired = Error.Validation(
            "Order.CurrencyRequired",
            "Currency is required.");

        public static readonly Error StatusInvalid = Error.Validation(
            "Order.StatusInvalid",
            "Order status is invalid.");
    }
}
