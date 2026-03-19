using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Orders.Requests;
using Botify.Application.Features.Orders.Responses;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Orders.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.Orders.Interfaces
{
    public interface IOrderService
    {
        Task<Result<OrderResponse>> CreateAsync(
            Guid merchantId,
            CreateOrderRequest request,
            CancellationToken cancellationToken = default);

        Task<Result<OrderResponse>> UpdateStatusAsync(
            Guid orderId,
            OrderStatus newStatus,
            CancellationToken cancellationToken = default);

        Task<Result<OrderResponse>> GetByIdAsync(
            Guid merchantId,
            Guid orderId,
            CancellationToken cancellationToken = default);
        Task<Result<OrderStatsResponse>> GetStatsAsync(
    Guid merchantId,
    CancellationToken cancellationToken = default);

        Task<Result<PaginatedResult<OrderResponse>>> GetOrdersAsync(
         Guid merchantId,
         string? cursor,
         int limit = 10,
         string? search = null,
         OrderStatus? status = null,
         BotPlatform? botPlatform = null,
         CancellationToken cancellationToken = default);
    }
}
