using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Orders.Enums;

namespace Botify.Application.Features.Orders.Requests
{
    public sealed class CreateOrderRequest
    {
        public Guid OrderId { get; init; }
        public Guid CustomerId { get; init; }
        public string CustomerName { get; init; } = string.Empty;
        public string CustomerContact { get; init; } = string.Empty;
        public BotPlatform Platform { get; init; }
        public List<OrderItemRequest> Items { get; init; } = new();
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public OrderStatus Status { get; init; }
        public string? DeliveryAddress { get; init; }
        public string? Notes { get; init; }
    }
}
