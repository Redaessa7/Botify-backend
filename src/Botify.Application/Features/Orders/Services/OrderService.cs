using Botify.Application.CrossCutting.Helpers;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Options;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Orders.Interfaces;
using Botify.Application.Features.Orders.Requests;
using Botify.Application.Features.Orders.Responses;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Orders;
using Botify.Domain.Models.Orders.Enums;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Botify.Application.Features.Orders.Services;

public sealed class OrderService : IOrderService
{
    private readonly IApplicationDbContext _db;
    private readonly PaginationOptions _paginationOptions;
    private readonly ILogger<OrderService> _logger;

    public OrderService(IApplicationDbContext db, IOptions<PaginationOptions> options, ILogger<OrderService> logger)
    {
        _db = db;
        _paginationOptions = options.Value;
        _logger = logger;
    }

    #region Order Lifecycle

    public async Task<Result<OrderResponse>> CreateAsync(Guid merchantId, CreateOrderRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating order OrderId={OrderId} for MerchantId={MerchantId}", request.OrderId, merchantId);

        var merchant = await _db.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var items = new List<OrderItem>();
        request.Items.ForEach(itemRequest =>
        {
            var itemResult = OrderItem.Create(
                request.OrderId,
                itemRequest.ProductId,
                itemRequest.ProductName,
                itemRequest.Quantity,
                itemRequest.UnitPrice);

            if (itemResult.IsSuccess)
            {
                items.Add(itemResult.Value);
            }
            else
            {
                _logger.LogWarning("Failed to create order item ProductId={ProductId} for OrderId={OrderId}", itemRequest.ProductId, request.OrderId);
            }
        });

        var orderResult = Order.Create(
            request.OrderId,
            "",
            merchantId,
            request.CustomerId,
            request.CustomerName,
            request.CustomerContact,
            request.Platform,
            items,
            request.TotalAmount,
            request.Currency,
            request.Status,
            request.DeliveryAddress,
            request.Notes);

        if (!orderResult.IsSuccess)
        {
            _logger.LogWarning("Failed to create order OrderId={OrderId} MerchantId={MerchantId}. Errors={Errors}", request.OrderId, merchantId, orderResult.Errors);
            return orderResult.Errors;
        }

        var order = orderResult.Value;
        _db.Orders.Add(order);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Order created successfully OrderId={OrderId} MerchantId={MerchantId}", order.Id, merchantId);
        return OrderResponse.FromDomain(order);
    }

    public async Task<Result<OrderResponse>> UpdateStatusAsync(Guid orderId, OrderStatus newStatus, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating order status OrderId={OrderId} NewStatus={NewStatus}", orderId, newStatus);

        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Order not found OrderId={OrderId}", orderId);
            return OrderErrors.IdRequired;
        }

        var result = order.UpdateStatus(newStatus);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update order status OrderId={OrderId}. Errors={Errors}", orderId, result.Errors);
            return result.Errors;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Order status updated successfully OrderId={OrderId} NewStatus={NewStatus}", orderId, newStatus);

        return OrderResponse.FromDomain(order);
    }

    #endregion

    #region Queries

    public async Task<Result<OrderStatsResponse>> GetStatsAsync(Guid merchantId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching order stats for MerchantId={MerchantId}", merchantId);

        var query = _db.Orders.AsNoTracking().Where(o => o.MerchantId == merchantId);

        var total = await query.CountAsync(cancellationToken);
        var pending = await query.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var preparing = await query.CountAsync(o => o.Status == OrderStatus.Preparing, cancellationToken);
        var delivered = await query.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);

        var response = new OrderStatsResponse(total, pending, preparing, delivered);

        _logger.LogInformation("Order stats retrieved MerchantId={MerchantId}. Total={Total}, Pending={Pending}, Preparing={Preparing}, Delivered={Delivered}",
            merchantId, total, pending, preparing, delivered);

        return response;
    }

    public async Task<Result<OrderResponse>> GetByIdAsync(Guid merchantId, Guid orderId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching order OrderId={OrderId} MerchantId={MerchantId}", orderId, merchantId);

        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.MerchantId == merchantId && o.Id == orderId, cancellationToken);
        if (order is null)
        {
            _logger.LogWarning("Order not found OrderId={OrderId} MerchantId={MerchantId}", orderId, merchantId);
            return OrderErrors.IdRequired;
        }

        _logger.LogInformation("Order retrieved successfully OrderId={OrderId} MerchantId={MerchantId}", orderId, merchantId);
        return OrderResponse.FromDomain(order);
    }

    public async Task<Result<PaginatedResult<OrderResponse>>> GetOrdersAsync(
        Guid merchantId,
        string? cursor,
        int limit = 10,
        string? search = null,
        OrderStatus? status = null,
        BotPlatform? botPlatform = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching orders for MerchantId={MerchantId} Limit={Limit} Cursor={Cursor} Search={Search} Status={Status} Platform={Platform}",
            merchantId, limit, cursor, search, status, botPlatform);

        if (limit <= 0) limit = _paginationOptions.DefaultLimit;
        limit = Math.Min(limit, _paginationOptions.MaxLimit);

        CursorHelper.CursorPayload? payload = null;
        if (!string.IsNullOrEmpty(cursor))
        {
            payload = CursorHelper.Decode(cursor);
            if (payload == null)
            {
                _logger.LogWarning("Invalid cursor provided for MerchantId={MerchantId}", merchantId);
                return Error.Validation("Invalid cursor");
            }
        }

        var query = _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Customer)
            .Where(o => o.MerchantId == merchantId);

        if (status.HasValue) query = query.Where(o => o.Status == status.Value);
        if (botPlatform.HasValue) query = query.Where(o => o.Platform == botPlatform.Value);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower().Trim();
            query = query.Where(o =>
                o.Uuid.ToLower().Contains(s) ||
                o.CustomerName.ToLower().Contains(s) ||
                o.CustomerContact.ToLower().Contains(s));
        }

        query = query.OrderByDescending(o => o.CreatedAtUtc).ThenByDescending(o => o.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        if (payload != null)
        {
            query = query.Where(o =>
                o.CreatedAtUtc < payload.CreatedAt ||
                (o.CreatedAtUtc == payload.CreatedAt && o.Id.CompareTo(payload.Id) < 0));
        }


        // fetch one extra to detect next page
        var items = await query
            .Take(limit + 1)
            .Select(q => OrderResponse.FromDomain(q))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore) items.RemoveAt(items.Count - 1);

        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var last = items.Last();
            var nextPayload = new CursorHelper.CursorPayload(last.Id, last.CreatedAtUtc);
            nextCursor = CursorHelper.Encode(nextPayload);
        }

        _logger.LogInformation("Orders retrieved for MerchantId={MerchantId}. Count={Count} Total={TotalCount} HasMore={HasMore}", merchantId, items.Count, totalCount, hasMore);

        var result = new PaginatedResult<OrderResponse>(items, limit, totalCount, nextCursor);
        return result;
    }

    #endregion
}
