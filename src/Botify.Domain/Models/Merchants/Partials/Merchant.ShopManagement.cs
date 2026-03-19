using Botify.Domain.Models.Shops;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public sealed partial class Merchant
    {
        public ShopSettings? ShopSettings { get; private set; }

        public Result<Success> AssignShop(ShopSettings settings)
        {
            ShopSettings = settings;
            return Result.Success;
        }
    }
}
