using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Products
{
    public sealed class Product : AuditableEntity
    {
        public Guid MerchantId { get; private set; }
        public string Name { get; private set; }
        public string Description { get; private set; } = string.Empty;
        public decimal Price { get; private set; }
        public string ImageUrl { get; private set; } = string.Empty;
        public string Category { get; private set; } = string.Empty;
        public int StockQuantity { get; private set; } = 0;
        public bool IsActive { get; private set; } = true;

        private Product() { }

        private Product(Guid id, Guid merchantId, string name, string description, decimal price, string imageUrl, string category, int stockQuantity, bool isActive)
            : base(id)
        {
            MerchantId = merchantId;
            Name = name;
            Description = description;
            Price = price;
            ImageUrl = imageUrl;
            Category = category;
            StockQuantity = stockQuantity;
            IsActive = isActive;
        }

        #region Factory

        public static Result<Product> Create(
            Guid id,
            Guid merchantId,
            string name,
            decimal price,
            string imageUrl,
            string category,
            int stockQuantity,
            string description = "")
        {
            var validationResult = ValidateInputs(id, merchantId, name, description, price, category, stockQuantity);
            if (!validationResult.IsSuccess)
                return validationResult.Errors;

            var product = new Product(
                id,
                merchantId,
                name,
                description,
                price,
                imageUrl,
                category,
                stockQuantity,
                isActive: true);

            return product;
        }

        #endregion

        #region Domain Operations

        public Result<Success> Activate()
        {
            if (IsActive)
                return ProductErrors.AlreadyActive;

            IsActive = true;
            return Result.Success;
        }
        public Result<Success> UpdateDetails(
    string name,
    string description,
    decimal price,
    string imageUrl,
    string category)
        {
            var validationResult = ValidateInputs(
                Id,
                MerchantId,
                name,
                description,
                price,
                category,
                StockQuantity
            );

            if (!validationResult.IsSuccess)
                return validationResult.Errors;

            Name = name;
            Description = description;
            Price = price;
            ImageUrl = imageUrl;
            Category = category;

            return Result.Success;
        }

        public Result<Success> Deactivate()
        {
            if (!IsActive)
                return ProductErrors.AlreadyInactive;

            IsActive = false;
            return Result.Success;
        }

        public Result<Success> UpdateStock(int quantity)
        {
            if (quantity < 0)
                return ProductErrors.StockQuantityInvalid;

            StockQuantity = quantity;
            return Result.Success;
        }



        #endregion

        #region Validation

        private static Result<Success> ValidateInputs(
            Guid id,
            Guid merchantId,
            string name,
            string description,
            decimal price,
            string category,
            int stockQuantity)
        {
            if (id == Guid.Empty)
                return ProductErrors.IdRequired;

            if (merchantId == Guid.Empty)
                return ProductErrors.MerchantIdRequired;

            if (string.IsNullOrWhiteSpace(name))
                return ProductErrors.NameInvalid;

            if (name.Length > 200)
                return ProductErrors.NameTooLong;

            if (description?.Length > 1000)
                return ProductErrors.DescriptionTooLong;

            if (price < 0)
                return ProductErrors.PriceInvalid;

            if (stockQuantity < 0)
                return ProductErrors.StockQuantityInvalid;

            if (string.IsNullOrWhiteSpace(category))
                return ProductErrors.CategoryRequired;

            if (category.Length > 100)
                return ProductErrors.CategoryTooLong;

            return Result.Success;
        }

        #endregion
    }
}
