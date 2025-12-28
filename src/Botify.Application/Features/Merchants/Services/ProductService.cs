using Botify.Application.CrossCutting.Helpers;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Options;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Products;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Botify.Application.Features.Merchants.Services;

public sealed class ProductService : IProductService
{
    private readonly IApplicationDbContext _db;
    private readonly PaginationOptions _paginationOptions;

    public ProductService(IApplicationDbContext db, IOptions<PaginationOptions> options)
    {
        _db = db;
        _paginationOptions = options.Value;
    }

    #region Product Lifecycle

    public async Task<Result<ProductResponse>> CreateAsync(
        Guid merchantId,
        CreateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants
            .FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);

        if (merchant is null)
            return MerchantErrors.NotFound;

        var productResult = request.ToProduct(merchantId);
        if (!productResult.IsSuccess)
            return productResult.Errors;

        var rulesResult = merchant.AddProduct(productResult.Value);
        if (rulesResult.IsError)
            return rulesResult.Errors;

        var product = productResult.Value;
        _db.Products.Add(product);

        await _db.SaveChangesAsync(cancellationToken);

        return ProductResponse.FromDomain(product);
    }

    public async Task<Result<Success>> UpdateAsync(
        Guid merchantId,
        Guid productId,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);

        if (product is null)
            return ProductErrors.NotFound;

        var result = request.ApplyTo(product);
        if (!result.IsSuccess)
            return result;

        product.UpdateStock(request.StockQuantity);

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    public async Task<Result<Success>> DeleteAsync(
        Guid merchantId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);

        if (product is null)
            return ProductErrors.NotFound;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    #endregion

    #region Queries

    public async Task<Result<ProductResponse>> GetByIdAsync(
        Guid merchantId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);

        if (product is null)
            return ProductErrors.NotFound;

        return ProductResponse.FromDomain(product);
    }

    public async Task<Result<PaginatedResult<ProductResponse>>> GetProductsAsync(
        Guid merchantId,
        string? cursor,
        int limit = 10,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        // enforce sane limits
        if (limit <= 0) limit = _paginationOptions.DefaultLimit;
        limit = Math.Min(limit, _paginationOptions.MaxLimit);

        // decode cursor if provided
        CursorHelper.CursorPayload? payload = null;
        if (!string.IsNullOrEmpty(cursor))
        {
            payload = CursorHelper.Decode(cursor, _paginationOptions.CursorHmacSecret);
            if (payload == null)
                return Error.Validation("Invalid cursor");
        }

        // base query
        var query = _db.Products.AsNoTracking()
            .Where(p => p.MerchantId == merchantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.Trim();
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{s}%"));
        }

        // order newest-first
        query = query.OrderByDescending(p => p.CreatedAtUtc)
                     .ThenByDescending(p => p.Id);
        // total count for metadata
        var totalCount = await query.CountAsync(cancellationToken);

        // apply cursor filter
        if (payload != null)
        {
            query = query.Where(p =>
                p.CreatedAtUtc < payload.CreatedAt ||
                (p.CreatedAtUtc == payload.CreatedAt && p.Id.CompareTo(payload.Id) < 0));
        }

        // fetch one extra to detect next page
        var items = await query
            .Take(limit + 1)
            .Select(p => new ProductResponse
            {
                Id = p.Id,
                MerchantId = p.MerchantId,
                Name = p.Name,
                Description = p.Description,
                Price = p.Price,
                ImageUrl = p.ImageUrl,
                Category = p.Category,
                StockQuantity = p.StockQuantity,
                IsActive = p.IsActive,
                CreateAt = p.CreatedAtUtc,
                UpdateAt = p.LastModifiedUtc
            })
            .ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore)
            items.RemoveAt(items.Count - 1);

        // build next cursor
        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var last = items.Last();
            var nextPayload = new CursorHelper.CursorPayload(last.Id, last.CreateAt);
            nextCursor = CursorHelper.Encode(nextPayload, _paginationOptions.CursorHmacSecret);
        }

        var result = new PaginatedResult<ProductResponse>(items, limit, totalCount, nextCursor);
        return result;
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle product active/inactive status.
    /// </summary>
    public async Task<Result<Success>> ToggleStatusAsync(
        Guid merchantId,
        Guid productId,
        CancellationToken cancellationToken = default)
    {
        var product = await _db.Products
            .FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);

        if (product is null)
            return ProductErrors.NotFound;

        var result = product.IsActive ? product.Deactivate() : product.Activate();
        if (!result.IsSuccess)
            return result;

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    #endregion
}
