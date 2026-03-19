using Botify.Domain.ResultPattern;
using System;
using Botify.Domain.Models.Products;

namespace Botify.Application.Features.Merchants.Requests
{
    public sealed class UpdateProductRequest
    {
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string ImageUrl { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public int StockQuantity { get; init; }

        /// <summary>
        /// Applies this request to an existing product.
        /// </summary>
        public Result<Success> ApplyTo(Product product)
        {
            return product.UpdateDetails(
                Name,
                Description,
                Price,
                ImageUrl,
                Category
            );
        }
    }
}
