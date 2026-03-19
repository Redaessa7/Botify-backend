using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Orders
{
    public static class OrderItemErrors
    {
        public static readonly Error OrderIdRequired = Error.Validation(
            "OrderItem.OrderIdRequired",
            "Order ID is required.");

        public static readonly Error ProductIdRequired = Error.Validation(
            "OrderItem.ProductIdRequired",
            "Product ID is required.");

        public static readonly Error ProductNameRequired = Error.Validation(
            "OrderItem.ProductNameRequired",
            "Product name is required.");

        public static readonly Error QuantityInvalid = Error.Validation(
            "OrderItem.QuantityInvalid",
            "Quantity must be greater than zero.");

        public static readonly Error UnitPriceInvalid = Error.Validation(
            "OrderItem.UnitPriceInvalid",
            "Unit price must be greater than zero.");
    }
}
