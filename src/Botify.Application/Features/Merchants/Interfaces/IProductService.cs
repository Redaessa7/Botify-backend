using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.Merchants.Interfaces
{
    public interface IProductService
    {
        /// <summary>
        /// Creates a new product for the specified merchant.
        /// </summary>
        Task<Result<ProductResponse>> CreateAsync(Guid merchantId, CreateProductRequest request, CancellationToken cancellationToken = default);
       Task<Result<Success>> UpdateAsync(
        Guid merchantId,
        Guid productId,
        UpdateProductRequest request,
        CancellationToken cancellationToken = default);

        /// <summary>
        /// Deletes a product by merchant and product ID.
        /// </summary>
        Task<Result<Success>> DeleteAsync(Guid merchantId, Guid productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves a product by merchant and product ID.
        /// </summary>
        Task<Result<ProductResponse>> GetByIdAsync(Guid merchantId, Guid productId, CancellationToken cancellationToken = default);

        /// <summary>
        /// Retrieves all products for a merchant with pagination and optional search query.
        /// </summary>
        Task<Result<PaginatedResult<ProductResponse>>> GetProductsAsync(
            Guid merchantId,
            string? cursor,
            int limit = 10,
            string? search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Toggles product active/inactive status.
        /// </summary>
        Task<Result<Success>> ToggleStatusAsync(Guid merchantId, Guid productId, CancellationToken cancellationToken = default);
    }
}
