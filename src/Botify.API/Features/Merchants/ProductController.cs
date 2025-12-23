using Botify.API.CrossCutting.Controllers;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Merchants;

[ApiVersion("1.0")]
[Route("api/merchants/{merchantId:guid}/products")]
public class ProductController(IProductService productService) : ApiController
{
    private readonly IProductService _productService = productService;

    #region Product Lifecycle

    /// <summary>
    /// Creates a new product for the specified merchant.
    /// </summary>
    [HttpPost]
    [Authorize]
    public async Task<IActionResult> CreateAsync(Guid merchantId, CreateProductRequest request, CancellationToken ct = default)
    {
        var result = await _productService.CreateAsync(merchantId, request, ct);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Updates an existing product.
    /// </summary>
    [HttpPut("{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> UpdateAsync(Guid merchantId, Guid productId, UpdateProductRequest request, CancellationToken ct = default)
    {
        var result = await _productService.UpdateAsync(merchantId, productId, request, ct);
        return result.Match(_ => Ok(new {}), Problem);
    }

    /// <summary>
    /// Deletes a product.
    /// </summary>
    [HttpDelete("{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> DeleteAsync(Guid merchantId, Guid productId, CancellationToken ct = default)
    {
        var result = await _productService.DeleteAsync(merchantId, productId, ct);
        return result.Match(_ => Ok(new { }), Problem);
    }

    #endregion

    #region Queries

    /// <summary>
    /// Retrieves a product by ID.
    /// </summary>
    [HttpGet("{productId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetByIdAsync(Guid merchantId, Guid productId, CancellationToken ct = default)
    {
        var result = await _productService.GetByIdAsync(merchantId, productId, ct);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Retrieves all products with pagination and optional search.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetProductsAsync(Guid merchantId, [FromQuery]int page = 1,[FromQuery] int limit = 10,[FromQuery] string? search = null, CancellationToken ct = default)
    {
        var result = await _productService.GetProductsAsync(merchantId, page, limit, search, ct);
        return result.Match(Ok, Problem);
    }

    #endregion

    #region Status

    /// <summary>
    /// Toggles product active/inactive status.
    /// </summary>
    [HttpPatch("{productId:guid}/toggle")]
    [Authorize]
    public async Task<IActionResult> ToggleStatusAsync(Guid merchantId, Guid productId, CancellationToken ct = default)
    {
        var result = await _productService.ToggleStatusAsync(merchantId, productId, ct);
        return result.Match(_ => Ok(new { }), Problem);
    }

    #endregion
}
