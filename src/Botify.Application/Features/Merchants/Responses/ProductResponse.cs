using Botify.Domain.Models.Products;

namespace Botify.Application.Features.Merchants.Responses
{
    public sealed class ProductResponse
    {
        public Guid Id { get; init; }
        public Guid MerchantId { get; init; }
        public string Name { get; init; } = string.Empty;
        public string Description { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public string ImageUrl { get; init; } = string.Empty;
        public string Category { get; init; } = string.Empty;
        public int StockQuantity { get; init; }
        public bool IsActive { get; init; }

        public DateTime CreateAt { get; init; }
        public DateTime UpdateAt { get; init; }

        public static ProductResponse FromDomain(Product product)
        {
            return new ProductResponse
            {
                Id = product.Id,
                MerchantId = product.MerchantId,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                ImageUrl = product.ImageUrl,
                Category = product.Category,
                StockQuantity = product.StockQuantity,
                IsActive = product.IsActive,
                CreateAt = product.CreatedAtUtc,
                UpdateAt = product.LastModifiedUtc
            };
        }
    }

    public sealed class ProductListResponse
    {
        public Guid Id { get; init; }
        public string Name { get; init; } = string.Empty;
        public decimal Price { get; init; }
        public bool IsActive { get; init; }

        public static ProductListResponse FromDomain(Product product)
        {
            return new ProductListResponse
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                IsActive = product.IsActive
            };
        }
    }
}
