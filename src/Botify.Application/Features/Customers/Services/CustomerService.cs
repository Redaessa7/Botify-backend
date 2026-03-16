using Botify.Application.CrossCutting.Helpers;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Options;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Customers.Interfaces;
using Botify.Application.Features.Customers.Responses;
using Botify.Domain.Models.Customers;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Botify.Application.Features.Customers.Services;

public sealed class CustomerService : ICustomerService
{
    private readonly IApplicationDbContext _db;
    private readonly IUser _user;
    private readonly PaginationOptions _paginationOptions;
    private readonly ILogger<CustomerService> _logger;

    public CustomerService(IApplicationDbContext db, IUser user, IOptions<PaginationOptions> options, ILogger<CustomerService> logger)
    {
        _db = db;
        _user = user;
        _paginationOptions = options.Value;
        _logger = logger;
    }

    /// <summary>
    /// Get all customers for the current merchant with cursor-based pagination and optional search.
    /// </summary>
    public async Task<Result<PaginatedResult<CustomerDto>>> GetAllAsync(
        string? cursor,
        int limit = 10,
        string? search = null,
        BotPlatform? botPlatform = null,
        CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching customers for UserId={UserId} Limit={Limit} Cursor={Cursor} Search={Search} Platform={Platform}", 
            _user.Id, limit, cursor, search, botPlatform);

        if (limit <= 0) limit = _paginationOptions.DefaultLimit;
        limit = Math.Min(limit, _paginationOptions.MaxLimit);

        var merchant = await _db.Merchants.Include(m => m.Profile)
            .FirstOrDefaultAsync(x => x.Profile.IdentityUserId == _user.Id, cancellationToken);

        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for UserId={UserId}", _user.Id);
            return MerchantErrors.NotFound;
        }

        CursorHelper.CursorPayload? payload = null;
        if (!string.IsNullOrEmpty(cursor))
        {
            payload = CursorHelper.Decode(cursor);
            if (payload == null)
            {
                _logger.LogWarning("Invalid cursor provided for MerchantId={MerchantId}", merchant.Id);
                return Error.Validation("Invalid cursor");
            }
        }

        var query = _db.Customers.AsNoTracking().Where(c => c.MerchantId == merchant.Id);

        if (botPlatform.HasValue)
        {
            query = botPlatform.Value switch
            {
                BotPlatform.TELEGRAM => query.Where(c => c.Telegram != null),
                BotPlatform.WHATSAPP => query.Where(c => c.WhatsApp != null),
                BotPlatform.MESSENGER => query.Where(c => c.Messenger != null),
                _ => query
            };
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim().ToLower();
            query = query.Where(c =>
                c.Name.ToLower().Contains(s) ||
                (c.Email != null && c.Email.ToLower().Contains(s)) ||
                c.PhoneNumber.Contains(s));
        }

        query = query.OrderByDescending(c => c.CreatedAtUtc).ThenByDescending(c => c.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        if (payload != null)
        {
            query = query.Where(c =>
                c.CreatedAtUtc < payload.CreatedAt ||
                (c.CreatedAtUtc == payload.CreatedAt && c.Id.CompareTo(payload.Id) < 0));
        }

        var items = await query.Take(limit + 1)
            .Select(x => CustomerDto.FromDomain(x))
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore) items.RemoveAt(items.Count - 1);

        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var last = items.Last();
            var nextPayload = new CursorHelper.CursorPayload(last.Id, last.CreatedAt);
            nextCursor = CursorHelper.Encode(nextPayload);
        }

        _logger.LogInformation("Customers retrieved for MerchantId={MerchantId}. Count={Count} Total={TotalCount} HasMore={HasMore}", merchant.Id, items.Count, totalCount, hasMore);

        return new PaginatedResult<CustomerDto>(items, limit, totalCount, nextCursor);
    }

    /// <summary>
    /// Get a single customer by ID for the current merchant.
    /// </summary>
    public async Task<Result<CustomerDto>> GetByIdAsync(Guid customerId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching customer CustomerId={CustomerId} for UserId={UserId}", customerId, _user.Id);

        var merchant = await _db.Merchants.Include(m => m.Profile)
            .FirstOrDefaultAsync(x => x.Profile.IdentityUserId == _user.Id, cancellationToken);

        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for UserId={UserId}", _user.Id);
            return MerchantErrors.NotFound;
        }

        var customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.MerchantId == merchant.Id && c.Id == customerId, cancellationToken);

        if (customer is null)
        {
            _logger.LogWarning("Customer not found CustomerId={CustomerId} MerchantId={MerchantId}", customerId, merchant.Id);
            return CustomerErrors.IdRequired;
        }

        _logger.LogInformation("Customer retrieved successfully CustomerId={CustomerId} MerchantId={MerchantId}", customerId, merchant.Id);
        return CustomerDto.FromDomain(customer);
    }

    public async Task<Result<CustomersStatsResponse>> GetStatsAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching customer stats for UserId={UserId}", _user.Id);

        var merchant = await _db.Merchants.Include(m => m.Profile)
            .FirstOrDefaultAsync(x => x.Profile.IdentityUserId == _user.Id, cancellationToken);

        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for UserId={UserId}", _user.Id);
            return MerchantErrors.NotFound;
        }

        var query = _db.Customers.AsNoTracking().Where(c => c.MerchantId == merchant.Id);

        var total = await query.CountAsync(cancellationToken);
        var telegram = await query.CountAsync(c => c.Telegram != null, cancellationToken);
        var whatsapp = await query.CountAsync(c => c.WhatsApp != null, cancellationToken);
        var messenger = await query.CountAsync(c => c.Messenger != null, cancellationToken);

        _logger.LogInformation("Customer stats retrieved for MerchantId={MerchantId}. Total={Total}, Telegram={Telegram}, WhatsApp={WhatsApp}, Messenger={Messenger}",
            merchant.Id, total, telegram, whatsapp, messenger);

        return new CustomersStatsResponse
        {
            Total = total,
            Telegram = telegram,
            WhatsApp = whatsapp,
            Messenger = messenger
        };
    }
}
