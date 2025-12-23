using Botify.Domain.ResultPattern;
using Botify.Domain.Models.Merchants.Products;
using System;

namespace Botify.Application.Features.Merchants.Requests
{
    public sealed class CreateProductRequest
    {
        public string Name { get; init; } = string.Empty;

        public string Description { get; init; } = string.Empty;

        public decimal Price { get; init; }

        public string ImageUrl { get; init; } = string.Empty;

        public string Category { get; init; } = string.Empty;

        public int StockQuantity { get; init; }

        /// <summary>
        /// Converts this request into a domain Product using the factory method.
        /// The MerchantId is supplied externally (from the aggregate route).
        /// </summary>
        public Result<Product> ToProduct(Guid merchantId)
        {
            return Product.Create(
                Guid.NewGuid(),   // generate new Product Id
                merchantId,       // supplied from aggregate route
                Name,
                Price,
                ImageUrl,
                Category,
                StockQuantity,
                Description
            );
        }
    }
}
