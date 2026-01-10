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

namespace Botify.Application.Features.Orders.Services;

public sealed class OrderService : IOrderService
{
    private readonly IApplicationDbContext _db;
    private readonly PaginationOptions _paginationOptions;

    public OrderService(IApplicationDbContext db, IOptions<PaginationOptions> options)
    {
        _db = db;
        _paginationOptions = options.Value;
    }

    #region Order Lifecycle

    public async Task<Result<OrderResponse>> CreateAsync(
        Guid merchantId,
        CreateOrderRequest request,
        CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants
            .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

        if (merchant is null)
            return MerchantErrors.NotFound;

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
        });

        var orderResult = Order.Create(
            request.OrderId,
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
            return orderResult.Errors;

        var order = orderResult.Value;
        _db.Orders.Add(order);

        await _db.SaveChangesAsync(cancellationToken);

        return OrderResponse.FromDomain(order);
    }

    public async Task<Result<OrderResponse>> UpdateStatusAsync(
        Guid orderId,
        OrderStatus newStatus,
        CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .FirstOrDefaultAsync(o => o.Id == orderId, cancellationToken);

        if (order is null)
            return OrderErrors.IdRequired;

        var result = order.UpdateStatus(newStatus);
        if (!result.IsSuccess)
            return result.Errors;

        await _db.SaveChangesAsync(cancellationToken);

        return OrderResponse.FromDomain(order);
    }

    #endregion

    #region Queries
    public async Task<Result<OrderStatsResponse>> GetStatsAsync(
    Guid merchantId,
    CancellationToken cancellationToken = default)
    {
        var query = _db.Orders.AsNoTracking()
            .Where(o => o.MerchantId == merchantId);

        var total = await query.CountAsync(cancellationToken);
        var pending = await query.CountAsync(o => o.Status == OrderStatus.Pending, cancellationToken);
        var preparing = await query.CountAsync(o => o.Status == OrderStatus.Preparing, cancellationToken);
        var delivered = await query.CountAsync(o => o.Status == OrderStatus.Delivered, cancellationToken);

        var response = new OrderStatsResponse(total, pending, preparing, delivered);
        return response;
    }

    public async Task<Result<OrderResponse>> GetByIdAsync(
        Guid merchantId,
        Guid orderId,
        CancellationToken cancellationToken = default)
    {
        var order = await _db.Orders
            .Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.MerchantId == merchantId && o.Id == orderId, cancellationToken);

        if (order is null)
            return OrderErrors.IdRequired;

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
        // enforce limits
        if (limit <= 0) limit = _paginationOptions.DefaultLimit;
        limit = Math.Min(limit, _paginationOptions.MaxLimit);

        // decode cursor if provided
        CursorHelper.CursorPayload? payload = null;
        if (!string.IsNullOrEmpty(cursor))
        {
            payload = CursorHelper.Decode(cursor);
            if (payload == null)
                return Error.Validation("Invalid cursor");
        }

        // base query
        var query = _db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Where(o => o.MerchantId == merchantId);

        // optional search filter (customer name/contact)
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(o =>
                o.CustomerName.Contains(s) ||
                o.CustomerContact.Contains(s));
        }

        // optional status filter
        if (status.HasValue)
        {
            query = query.Where(o => o.Status == status.Value);
        }

        // optional platform filter
        if (botPlatform.HasValue)
        {
            query = query.Where(o => o.Platform == botPlatform.Value);
        }

        // order newest-first
        query = query.OrderByDescending(o => o.CreatedAtUtc)
                     .ThenByDescending(o => o.Id);

        // total count for metadata
        var totalCount = await query.CountAsync(cancellationToken);

        // apply cursor filter
        if (payload != null)
        {
            query = query.Where(o =>
                o.CreatedAtUtc < payload.CreatedAt ||
                (o.CreatedAtUtc == payload.CreatedAt && o.Id.CompareTo(payload.Id) < 0));
        }

        // fetch one extra to detect next page
        var items = await query
            .Take(limit + 1)
            .Select(o => OrderResponse.FromDomain(o))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        // build next cursor
        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var last = items.Last();
            var nextPayload = new CursorHelper.CursorPayload(last.Id, last.CreatedAtUtc);
            nextCursor = CursorHelper.Encode(nextPayload);
        }

        var result = new PaginatedResult<OrderResponse>(items, limit, totalCount, nextCursor);
        return result;
    }

    #endregion
}
