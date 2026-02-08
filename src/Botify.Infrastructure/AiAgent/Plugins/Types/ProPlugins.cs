using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.Features.Customers.Interfaces;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Orders;
using Botify.Domain.Models.Orders.Enums;
using Botify.Domain.Models.Orders.Helpers;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.AiAgent.Plugins.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Botify.Infrastructure.AIServices.Plugins.Types
{
    /// <summary>
    /// Pro plan AI plugin exposing advanced functions:
    /// - Create Order Items
    /// - Create Orders (currency from merchant shop settings)
    /// </summary>
    public class ProPlugins : FreePlugins
    {
        private readonly IApplicationDbContext _db;

        public ProPlugins(
            IProductService productService,
            ICustomerService customerService,
            IApplicationDbContext db
        ) : base(productService, customerService, db)
        {
            _db = db;
        }

        /// <summary>
        /// Create a list of order items for a given order.
        /// Each item requires product ID, product name, quantity, and unit price.
        /// </summary>
        [KernelFunction("create_order_items")]
        [Description("Create a list of order items for a given order. Each item requires product ID, product name, quantity, and unit price.")]
        public Result<List<OrderItem>> CreateOrderItems(
            Guid orderId,
            List<(Guid productId, string productName, int quantity, decimal unitPrice)> items)
        {
            var orderItems = new List<OrderItem>();

            foreach (var item in items)
            {
                var result = OrderItem.Create(orderId, item.productId, item.productName, item.quantity, item.unitPrice);

                if (!result.IsSuccess)
                    return result.Errors; // return validation errors immediately

                orderItems.Add(result.Value);
            }

            return orderItems;
        }

        /// <summary>
        /// Create a new order for a merchant and customer.
        /// Currency is automatically retrieved from the merchant's shop settings.
        /// </summary>
        [KernelFunction("create_order")]
        [Description("Create a new order for a merchant and customer with items and optional delivery address. Currency is taken from merchant shop settings.")]
        public async Task<Result<Order>> CreateOrderAsync(
            Guid merchantId,
            Guid customerId,
            string customerName,
            string customerContact,
            List<OrderItem> items,
            BotPlatform platform,
            string? deliveryAddress = null,
            string? notes = null,
            CancellationToken ct = default)
        {
            // Load merchant with shop settings
            var merchant = await _db.Merchants
                .Include(m => m.ShopSettings)
                .FirstOrDefaultAsync(m => m.Id == merchantId, ct);

            if (merchant is null)
                return MerchantErrors.NotFound;

            if (merchant.ShopSettings is null || string.IsNullOrWhiteSpace(merchant.ShopSettings.Currency))
                return Error.Validation("Order.CurrencyMissing", "Merchant shop settings do not define a currency.");

            var currency = merchant.ShopSettings.Currency;
            var totalAmount = items.Sum(i => i.LineTotal);

            // Create order using domain factory
            var orderResult = Order.Create(
                Guid.NewGuid(),
                OrderUuidGenerator.Generate(merchantId,customerId),
                merchantId,
                customerId,
                customerName,
                customerContact,
                platform,
                items,
                totalAmount,
                currency,
                OrderStatus.Pending,
                deliveryAddress,
                notes
            );

            if (!orderResult.IsSuccess)
                return orderResult.Errors;

            _db.Orders.Add(orderResult.Value);
            await _db.SaveChangesAsync(ct);

            return orderResult;
        }
    }
}
