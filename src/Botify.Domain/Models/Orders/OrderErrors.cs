using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Orders
{
    /// <summary>
    /// Contains validation and domain error definitions for the Order model.
    /// </summary>
    public static class OrderErrors
    {
        // Validation Errors
        public static readonly Error IdRequired = Error.Validation(
            "Order.IdRequired",
            "Order ID is required.");

        public static readonly Error UuidRequired = Error.Validation(
            "Order.UuidRequired",
            "Order UUID is required.");   // <-- Added for UUID validation

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

        // Domain Errors
        public static readonly Error NotFound = Error.NotFound(
            "Order.NotFound",
            "The requested order could not be found.");

        public static readonly Error AlreadyExists = Error.Conflict(
            "Order.AlreadyExists",
            "An order with the same identifier already exists.");

        public static readonly Error UpdateFailed = Error.Failure(
            "Order.UpdateFailed",
            "Failed to update the order.");

        public static readonly Error DeleteFailed = Error.Failure(
            "Order.DeleteFailed",
            "Failed to delete the order.");
    }
}
