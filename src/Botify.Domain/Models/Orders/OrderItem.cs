using Botify.Domain.Models.Merchants.Products;
using Botify.Domain.ResultPattern;
using System.ComponentModel.DataAnnotations.Schema;

namespace Botify.Domain.Models.Orders
{
    public sealed class OrderItem
    {
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor.
        private OrderItem() { }
#pragma warning restore CS8618

        private OrderItem(Guid orderId, Guid productId, string productName, int quantity, decimal unitPrice)
        {
            OrderId = orderId;
            ProductId = productId;
            ProductName = productName;
            Quantity = quantity;
            UnitPrice = unitPrice;
        }

        public Guid OrderId { get; }
        public Guid ProductId { get; }
        public string ProductName { get; }
        public int Quantity { get; }
        public decimal UnitPrice { get; }

        [NotMapped]
        public decimal LineTotal => Quantity * UnitPrice;
        public static Result<OrderItem> Create(
            Guid orderId,
            Guid productId,
            string productName,
            int quantity,
            decimal unitPrice)
        {
            if (orderId == Guid.Empty)
                return OrderItemErrors.OrderIdRequired;

            if (productId == Guid.Empty)
                return OrderItemErrors.ProductIdRequired;

            if (string.IsNullOrWhiteSpace(productName))
                return OrderItemErrors.ProductNameRequired;

            if (quantity <= 0)
                return OrderItemErrors.QuantityInvalid;

            if (unitPrice <= 0)
                return OrderItemErrors.UnitPriceInvalid;

            return new OrderItem(orderId, productId, productName.Trim(), quantity, unitPrice);
        }
    }
}
