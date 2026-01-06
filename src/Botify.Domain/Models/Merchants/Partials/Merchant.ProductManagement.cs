using Botify.Domain.Models.Merchants.Products;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public sealed partial class Merchant
    {
        private readonly List<Product> _products = new();
        public IReadOnlyCollection<Product> Products => _products.AsReadOnly();

        public Result<Product> AddProduct(Product product)
        {
            _products.Add(product);
            return product;
        }

        public Result<Success> ActivateProduct(Guid productId)
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return ProductErrors.NotFound;

            product.Activate();
            return Result.Success;
        }

        public Result<Success> DeactivateProduct(Guid productId)
        {
            var product = _products.FirstOrDefault(p => p.Id == productId);
            if (product == null)
                return ProductErrors.NotFound;

            product.Deactivate();
            return Result.Success;
        }
    }
}
