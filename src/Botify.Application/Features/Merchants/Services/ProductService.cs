using Botify.Application.CrossCutting.Helpers;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Options;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Products;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;

namespace Botify.Application.Features.Merchants.Services;

public sealed class ProductService : IProductService
{
    private readonly IApplicationDbContext _db;
    private readonly PaginationOptions _paginationOptions;
    private readonly ILogger<ProductService> _logger;

    public ProductService(IApplicationDbContext db, IOptions<PaginationOptions> options, ILogger<ProductService> logger)
    {
        _db = db;
        _paginationOptions = options.Value;
        _logger = logger;
    }

    #region Product Lifecycle

    public async Task<Result<ProductResponse>> CreateAsync(Guid merchantId, CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Creating product for MerchantId={MerchantId}", merchantId);

        var merchant = await _db.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
        if (merchant is null)
        {
            _logger.LogWarning("Merchant not found for MerchantId={MerchantId}", merchantId);
            return MerchantErrors.NotFound;
        }

        var productResult = request.ToProduct(merchantId);
        if (!productResult.IsSuccess)
        {
            _logger.LogWarning("Failed to create product for MerchantId={MerchantId}. Errors={Errors}", merchantId, productResult.Errors);
            return productResult.Errors;
        }

        var rulesResult = merchant.AddProduct(productResult.Value);
        if (rulesResult.IsError)
        {
            _logger.LogWarning("Business rules prevented product creation for MerchantId={MerchantId}. Errors={Errors}", merchantId, rulesResult.Errors);
            return rulesResult.Errors;
        }

        var product = productResult.Value;
        _db.Products.Add(product);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product created successfully ProductId={ProductId} MerchantId={MerchantId}", product.Id, merchantId);
        return ProductResponse.FromDomain(product);
    }

    public async Task<Result<Success>> UpdateAsync(Guid merchantId, Guid productId, UpdateProductRequest request, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Updating product ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);

        var product = await _db.Products.FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);
        if (product is null)
        {
            _logger.LogWarning("Product not found ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);
            return ProductErrors.NotFound;
        }

        var result = request.ApplyTo(product);
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to update product ProductId={ProductId} MerchantId={MerchantId}. Errors={Errors}", productId, merchantId, result.Errors);
            return result;
        }

        product.UpdateStock(request.StockQuantity);
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product updated successfully ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);
        return result;
    }

    public async Task<Result<Success>> DeleteAsync(Guid merchantId, Guid productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Deleting product ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);

        var product = await _db.Products.FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);
        if (product is null)
        {
            _logger.LogWarning("Product not found ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);
            return ProductErrors.NotFound;
        }

        product.SoftDelete();
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Product deleted successfully ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);
        return Result.Success;
    }

    #endregion

    #region Queries

    public async Task<Result<ProductResponse>> GetByIdAsync(Guid merchantId, Guid productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching product ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);

        var product = await _db.Products.FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);
        if (product is null)
        {
            _logger.LogWarning("Product not found ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);
            return ProductErrors.NotFound;
        }

        _logger.LogInformation("Product retrieved successfully ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);
        return ProductResponse.FromDomain(product);
    }

    public async Task<Result<PaginatedResult<ProductResponse>>> GetProductsAsync(Guid merchantId, string? cursor, int limit = 10, string? search = null, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Fetching products for MerchantId={MerchantId} with Limit={Limit} Cursor={Cursor} Search={Search}", merchantId, limit, cursor, search);

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

        var query = _db.Products.AsNoTracking().Where(p => p.MerchantId == merchantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            var s = search.ToLower().Trim();
            query = query.Where(p => p.Name.ToLower().Contains(s));
        }

        query = query.OrderByDescending(q => q.CreatedAtUtc).ThenByDescending(q => q.Id);

        var totalCount = await query.CountAsync(cancellationToken);

        if (payload != null)
        {
            query = query.Where(q =>
                q.CreatedAtUtc < payload.CreatedAt ||
                (q.CreatedAtUtc == payload.CreatedAt && q.Id.CompareTo(payload.Id) < 0));
        }

        var items = await query.Take(limit + 1).Select(p => new ProductResponse
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
        }).ToListAsync(cancellationToken);

        var hasMore = items.Count > limit;
        if (hasMore) items.RemoveAt(items.Count - 1);

        string? nextCursor = null;
        if (hasMore && items.Any())
        {
            var last = items.Last();
            var nextPayload = new CursorHelper.CursorPayload(last.Id, last.CreateAt);
            nextCursor = CursorHelper.Encode(nextPayload);
        }

        _logger.LogInformation("Products retrieved successfully for MerchantId={MerchantId}. Count={Count} Total={TotalCount} HasMore={HasMore}", merchantId, items.Count, totalCount, hasMore);

        var result = new PaginatedResult<ProductResponse>(items, limit, totalCount, nextCursor);
        return result;
    }

    #endregion

    #region Status Management
    public async Task<Result<Success>> ToggleStatusAsync(Guid merchantId, Guid productId, CancellationToken cancellationToken = default)
    {
        _logger.LogInformation("Toggling status for ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);

        var product = await _db.Products.FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);
        if (product is null)
        {
            _logger.LogWarning("Product not found ProductId={ProductId} MerchantId={MerchantId}", productId, merchantId);
            return ProductErrors.NotFound;
        }

        var result = product.IsActive ? product.Deactivate() : product.Activate();
        if (!result.IsSuccess)
        {
            _logger.LogWarning("Failed to toggle status for ProductId={ProductId} MerchantId={MerchantId}. Errors={Errors}", productId, merchantId, result.Errors);
            return result;
        }

        await _db.SaveChangesAsync(cancellationToken);
        _logger.LogInformation("Product status toggled successfully ProductId={ProductId} MerchantId={MerchantId} NewStatus={Status}", productId, merchantId, product.IsActive);

        return result;
    }
    public async Task<Result<ProductStatsResponse>> GetStatsAsync(Guid merchantId, CancellationToken ct = default)
    {
        _logger.LogInformation("Fetching product stats for MerchantId={MerchantId}", merchantId);

        var total = await _db.Products.CountAsync(p => p.MerchantId == merchantId, ct);
        var active = await _db.Products.CountAsync(p => p.MerchantId == merchantId && p.IsActive, ct);
        var outOfStock = await _db.Products.CountAsync(p => p.MerchantId == merchantId && p.StockQuantity == 0, ct);

        var stats = new ProductStatsResponse
        {
            Total = total,
            Active = active,
            OutOfStock = outOfStock
        };

        _logger.LogInformation("Product stats retrieved for MerchantId={MerchantId}. Total={Total}, Active={Active}, OutOfStock={OutOfStock}",
            merchantId, total, active, outOfStock);

        return stats;
    }

    #endregion
}
