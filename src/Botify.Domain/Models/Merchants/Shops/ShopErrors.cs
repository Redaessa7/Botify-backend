

using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.Shops;

public static class ShopErrors
{
    public static Error ShopIncomplete =>
        Error.Validation("Shop.Incomplete", "Shop settings must be complete before adding or activating bots.");
}
