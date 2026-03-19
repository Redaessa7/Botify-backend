using Botify.Application.Features.Customers.Responses;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Orders;
using Botify.Domain.Models.Orders.Enums;

namespace Botify.Application.Features.Orders.Responses
{
    public sealed class OrderResponse
    {
        public Guid Id { get; init; }
        public CustomerDto? Customer { get; init; } 
        public string Uuid { get; init; } = string.Empty;
        public BotPlatform Platform { get; init; }
        public decimal TotalAmount { get; init; }
        public string Currency { get; init; } = string.Empty;
        public OrderStatus Status { get; init; }
        public string? DeliveryAddress { get; init; }
        public string? Notes { get; init; }
        public DateTime CreatedAtUtc { get; init; }
        public DateTime UpdatedAtUtc { get; init; }
        public List<OrderItemResponse> Items { get; init; } = new();

        public static OrderResponse FromDomain(Order order) =>
            new()
            {
                Id = order.Id,
                Uuid = order.Uuid,
                Platform = order.Platform,
                TotalAmount = order.TotalAmount,
                Currency = order.Currency,
                Status = order.Status,
                DeliveryAddress = order.DeliveryAddress,
                Notes = order.Notes,
                CreatedAtUtc = order.CreatedAtUtc,
                UpdatedAtUtc = order.LastModifiedUtc,
                Items = order.Items.Select(OrderItemResponse.FromDomain).ToList(),
                Customer = order?.Customer is not null ? CustomerDto.FromDomain(order.Customer) : null
            };
    }
}
