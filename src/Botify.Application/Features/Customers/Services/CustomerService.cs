using Botify.Application.CrossCutting.Helpers;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Options;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Customers.Interfaces;
using Botify.Application.Features.Customers.Responses;
using Botify.Domain.Models.Customers;
using Botify.Domain.Models.Merchants;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Botify.Application.Features.Customers.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _db;
    private readonly IUser _user;
    private readonly PaginationOptions _paginationOptions;

    public CustomerService(IApplicationDbContext db, IUser user, IOptions<PaginationOptions> options)
    {
        _db = db;
        _user = user;
        _paginationOptions = options.Value;
    }

    /// <summary>
    /// Get all customers for the current merchant with cursor-based pagination and optional search.
    /// </summary>
    public async Task<Result<PaginatedResult<CustomerDto>>> GetAllAsync(
        string? cursor,
        int limit = 10,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        // enforce limits
        if (limit <= 0) limit = _paginationOptions.DefaultLimit;
        limit = Math.Min(limit, _paginationOptions.MaxLimit);

        // get current merchant
        var merchant = await _db.Merchants
            .Include(m => m.Profile)
            .FirstOrDefaultAsync(x => x.Profile.IdentityUserId == _user.Id, cancellationToken);

        if (merchant is null) return MerchantErrors.NotFound;

        // decode cursor if provided
        CursorHelper.CursorPayload? payload = null;
        if (!string.IsNullOrEmpty(cursor))
        {
            payload = CursorHelper.Decode(cursor);
            if (payload == null)
                return Error.Validation("Invalid cursor");
        }

        // base query
        var query = _db.Customers.AsNoTracking()
            .Where(c => c.MerchantId == merchant.Id);

        // optional search filter
        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(s) ||
                (c.Email != null && c.Email.ToLower().Contains(s)) ||
                c.PhoneNumber.Contains(s));
        }

        // order newest-first
        query = query.OrderByDescending(c => c.CreatedAtUtc)
                     .ThenByDescending(c => c.Id);

        // total count for metadata
        var totalCount = await query.CountAsync(cancellationToken);

        if (payload != null)
        {
            query = query.Where(c =>
                c.CreatedAtUtc < payload.CreatedAt ||
                (c.CreatedAtUtc == payload.CreatedAt && c.Id.CompareTo(payload.Id) < 0));
        }

        // fetch one extra to detect next page
        var items = await query
            .Take(limit + 1)
            .Select(c => CustomerDto.FromDomain(c))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        // build next cursor
        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var last = items.Last();
            var nextPayload = new CursorHelper.CursorPayload(last.Id, last.CreatedAt);
            nextCursor = CursorHelper.Encode(nextPayload);
        }

        var result = new PaginatedResult<CustomerDto>(items, limit, totalCount, nextCursor);
        return result;
    }

    /// <summary>
    /// Get a single customer by ID for the current merchant.
    /// </summary>
    public async Task<Result<CustomerDto>> GetByIdAsync(
        Guid customerId,
        CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants
            .Include(m => m.Profile)
            .FirstOrDefaultAsync(x => x.Profile.IdentityUserId == _user.Id, cancellationToken);

        if (merchant is null) return MerchantErrors.NotFound;

        var customer = await _db.Customers
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.MerchantId == merchant.Id && c.Id == customerId, cancellationToken);

        if (customer is null) return CustomerErrors.IdRequired;

        return CustomerDto.FromDomain(customer);
    }
}
