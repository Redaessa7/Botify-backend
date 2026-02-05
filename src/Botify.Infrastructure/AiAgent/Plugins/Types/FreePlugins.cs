using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Customers.Interfaces;
using Botify.Application.Features.Customers.Responses;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Customers;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Orders;
using Botify.Domain.Models.SupportRequests;
using Botify.Domain.Models.SupportRequests.Enums;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.AIServices.Plugins.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Botify.Infrastructure.AiAgent.Plugins.Types
{
    /// <summary>
    /// Free plan AI plugin exposing limited functions:
    /// - Get Products
    /// - Create Customer
    /// - Get Customer
    /// - Get Merchant Details
    /// - Get Order Details
    /// - Create Support Request
    /// </summary>
    public class FreePlugins : IPlugin
    {
        private readonly IProductService _productService;
        private readonly ICustomerService _customerService;
        private readonly IApplicationDbContext _db;

        public FreePlugins(
            IProductService productService,
            ICustomerService customerService,
            IApplicationDbContext db)
        {
            _productService = productService;
            _customerService = customerService;
            _db = db;
        }

        /// <summary>
        /// Retrieve paginated list of products for a merchant with optional search and cursor pagination.
        /// </summary>
        [KernelFunction("get_products")]
        [Description("Retrieve paginated list of products for a merchant with optional search and cursor pagination.")]
        public async Task<Result<PaginatedResult<ProductResponse>>> GetProductsAsync(
            Guid merchantId,
            string? cursor = null,
            int limit = 10,
            string? search = null,
            CancellationToken ct = default)
        {
            return await _productService.GetProductsAsync(merchantId, cursor, limit, search, ct);
        }

        /// <summary>
        /// Create a new support request for a merchant and customer.
        /// Requires merchant, bot, customer details, type, reason, and message.
        /// </summary>
        [KernelFunction("create_support_request")]
        [Description("Create a new support request for a merchant and customer. Requires merchant, bot, customer details, type, reason, and message.")]
        public async Task<Result<SupportRequest>> CreateSupportRequestAsync(
            Guid merchantId,
            Guid botId,
            string botName,
            BotPlatform platform,
            string platformContact,
            string customerName,
            Guid customerId,
            SupportRequestType type,
            RequestReason reason,
            string message,
            CancellationToken ct = default)
        {
            var supportRequestResult = SupportRequest.Create(
                Guid.NewGuid(),
                merchantId,
                botId,
                botName,
                platform,
                platformContact,
                customerName,
                type,
                reason,
                customerId,
                message
            );

            if (!supportRequestResult.IsSuccess)
                return supportRequestResult.Errors;

            _db.SupportRequests.Add(supportRequestResult.Value);
            await _db.SaveChangesAsync(ct);

            return supportRequestResult;
        }

        /// <summary>
        /// Create a new customer for the current merchant.
        /// Requires name, phone number, and optional email/social profiles.
        /// </summary>
        [KernelFunction("create_customer")]
        [Description("Create a new customer for the current merchant. Requires name, phone number, and optional email/social profiles.")]
        public async Task<Result<CustomerDto>> CreateCustomerAsync(
            Guid merchantId,
            string name,
            string phoneNumber,
            string? email = null,
            CancellationToken ct = default)
        {
            var uuid = Guid.NewGuid().ToString();
            var customerResult = Customer.Create(Guid.NewGuid(), merchantId, uuid, name, phoneNumber, email);

            if (!customerResult.IsSuccess)
                return customerResult.Errors;

            _db.Customers.Add(customerResult.Value);
            await _db.SaveChangesAsync(ct);

            return CustomerDto.FromDomain(customerResult.Value);
        }

        /// <summary>
        /// Provide the customer with full details about their order.
        /// Includes items, total amount, currency, status, and delivery info.
        /// </summary>
        [KernelFunction("get_order_details")]
        [Description("Generate a detailed message for the customer about their order, including items, total, currency, status, and delivery information.")]
        public async Task<Result<string>> GetOrderDetailsAsync(
            Guid merchantId,
            Guid orderId,
            CancellationToken ct = default)
        {
            var order = await _db.Orders
                .Include(o => o.Customer)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.MerchantId == merchantId && o.Id == orderId, ct);

            if (order is null)
                return OrderErrors.NotFound;

            var message = BuildOrderDetailsMessage(order);
            return message;
        }

        /// <summary>
        /// Retrieve a single customer by ID for the current merchant.
        /// </summary>
        [KernelFunction("get_customer")]
        [Description("Retrieve a single customer by ID for the current merchant.")]
        public async Task<Result<CustomerDto>> GetCustomerAsync(
            Guid merchantId,
            Guid customerId,
            CancellationToken ct = default)
        {
            return await _customerService.GetByIdAsync(customerId, ct);
        }

        /// <summary>
        /// Retrieve merchant details.
        /// </summary>
        [KernelFunction("get_merchant_details")]
        [Description("Retrieve merchant details.")]
        public async Task<Result<CurrentMerchantResponse>> GetMerchantDetailsAsync(
            Guid merchantId,
            CancellationToken ct = default)
        {
            var merchant = await _db.Merchants
                .Include(m => m.ShopSettings)
                .FirstOrDefaultAsync(m => m.Id == merchantId, ct);

            if (merchant is null)
                return MerchantErrors.NotFound;

            return merchant.ToCurrentResponse();
        }

        #region Private Helpers

        private static string BuildOrderDetailsMessage(Order order)
        {
            var itemsList = string.Join(", ", order.Items.Select(i => $"{i.ProductName} (x{i.Quantity})"));
            var total = $"{order.TotalAmount} {order.Currency}";
            var status = order.Status.ToString();
            var delivery = string.IsNullOrEmpty(order.DeliveryAddress)
                ? "No delivery address provided"
                : order.DeliveryAddress;

            var notes = string.IsNullOrEmpty(order.Notes) ? "" : $"- Notes: {order.Notes}\n";

            return
                $"Hello {order.CustomerName}, here are the details of your order {order.Id}:\n" +
                $"- Items: {itemsList}\n" +
                $"- Total: {total}\n" +
                $"- Status: {status}\n" +
                $"- Delivery Address: {delivery}\n" +
                notes +
                "Thank you for shopping with us!";
        }

        #endregion
    }
}
