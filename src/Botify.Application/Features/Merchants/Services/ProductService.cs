using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.Products;
using Botify.Domain.ResultPattern;
using Microsoft.EntityFrameworkCore;

namespace Botify.Application.Features.Merchants.Services;

public sealed class ProductService : IProductService
{
    private readonly IApplicationDbContext _db;

    public ProductService(IApplicationDbContext db)
    {
        _db = db;
    }

    #region Product Lifecycle

    public async Task<Result<ProductResponse>> CreateAsync(Guid merchantId, CreateProductRequest request, CancellationToken cancellationToken = default)
    {
        var merchant = await _db.Merchants.FirstOrDefaultAsync(m => m.Id == merchantId, cancellationToken);
        if (merchant is null) return MerchantErrors.NotFound;

        var productResult = request.ToProduct(merchantId);

        if (!productResult.IsSuccess)
            return productResult.Errors;

        var merchanrProductRulesResult = merchant.AddProduct(productResult.Value);

        if (merchanrProductRulesResult.IsError)
            return merchanrProductRulesResult.Errors;

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

    public async Task<Result<Success>> DeleteAsync(Guid merchantId, Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);
        if (product is null) return ProductErrors.NotFound;

        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);

        return Result.Success;
    }

    #endregion

    #region Queries

    public async Task<Result<ProductResponse>> GetByIdAsync(Guid merchantId, Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);
        if (product is null) return ProductErrors.NotFound;

        return ProductResponse.FromDomain(product);
    }

    /// <summary>
    /// Get all products with pagination and optional search query.
    /// </summary>
    public async Task<Result<PaginatedResult<ProductResponse>>> GetProductsAsync(
        Guid merchantId,
        int page = 1,
        int limit = 10,
        string? search = null,
        CancellationToken cancellationToken = default)
    {
        var query = _db.Products.Where(p => p.MerchantId == merchantId);

        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p =>
                p.Name.Contains(search) ||
                p.Category.Contains(search));
        }

        var totalCount = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((page - 1) * limit)
            .Take(limit)
            .ToListAsync(cancellationToken);

        var responses = items.Select(ProductResponse.FromDomain).ToList();

        var paginated = new PaginatedResult<ProductResponse>(responses, page, limit, totalCount);
        return paginated;
    }

    #endregion

    #region Status Management

    /// <summary>
    /// Toggle product active/inactive status.
    /// </summary>
    public async Task<Result<Success>> ToggleStatusAsync(Guid merchantId, Guid productId, CancellationToken cancellationToken = default)
    {
        var product = await _db.Products.FirstOrDefaultAsync(p => p.MerchantId == merchantId && p.Id == productId, cancellationToken);
        if (product is null) return ProductErrors.NotFound;

        var result = product.IsActive ? product.Deactivate() : product.Activate();
        if (!result.IsSuccess) return result;

        await _db.SaveChangesAsync(cancellationToken);
        return result;
    }

    #endregion
}
