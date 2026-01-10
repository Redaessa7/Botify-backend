using Botify.Domain.Models.Orders;

namespace Botify.Application.Features.Orders.Responses
{
    public sealed class OrderItemResponse
    {
        public Guid ProductId { get; init; }
        public string ProductName { get; init; } = string.Empty;
        public int Quantity { get; init; }
        public decimal UnitPrice { get; init; }
        public decimal LineTotal { get; init; }

        public static OrderItemResponse FromDomain(OrderItem item) =>
            new()
            {
                ProductId = item.ProductId,
                ProductName = item.ProductName,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                LineTotal = item.LineTotal
            };
    }
}
