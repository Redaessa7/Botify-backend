using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants.Products;

public static class ProductErrors
{
    // General Validation
    public static Error IdRequired =>
        Error.Validation("Product.General.Id.Required", "Product ID is required.");

    public static Error MerchantIdRequired =>
        Error.Validation("Product.General.MerchantId.Required", "Merchant ID is required for the product.");

    public static Error NameInvalid =>
        Error.Validation("Product.General.Name.Invalid", "Product name is required and cannot be empty.");

    public static Error NameTooLong =>
        Error.Validation("Product.General.Name.TooLong", "Product name cannot exceed 200 characters.");

    public static Error DescriptionTooLong =>
        Error.Validation("Product.General.Description.TooLong", "Product description cannot exceed 1000 characters.");

    public static Error PriceInvalid =>
        Error.Validation("Product.General.Price.Invalid", "Price must be greater than or equal to zero.");

    public static Error ImageUrlInvalid =>
        Error.Validation("Product.General.ImageUrl.Invalid", "Image URL must be a valid URL.");

    public static Error CategoryRequired =>
        Error.Validation("Product.General.Category.Required", "Category is required.");

    public static Error CategoryTooLong =>
        Error.Validation("Product.General.Category.TooLong", "Category cannot exceed 100 characters.");

    public static Error StockQuantityInvalid =>
        Error.Validation("Product.General.StockQuantity.Invalid", "Stock quantity cannot be negative.");

    // Status
    public static Error NotActive =>
        Error.Forbidden("Product.Status.NotActive", "Product is inactive and cannot perform this action.");

    public static Error AlreadyActive =>
        Error.Conflict("Product.Status.AlreadyActive", "Product is already active.");

    public static Error AlreadyInactive =>
        Error.Conflict("Product.Status.AlreadyInactive", "Product is already inactive.");

    public static Error AlreadyDiscontinued =>
        Error.Conflict("Product.Status.AlreadyDiscontinued", "Product has already been discontinued.");

    // Lookup
    public static Error NotFound =>
        Error.NotFound("Product.Lookup.NotFound", "The specified product could not be found.");
}
