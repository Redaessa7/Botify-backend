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

namespace Botify.Infrastructure.AiAgent.Plugins.Types;

public class FreePlugins(
    IProductService productService,
    ICustomerService customerService,
    IApplicationDbContext db) : IPlugin
{
    private readonly IProductService _productService = productService;
    private readonly ICustomerService _customerService = customerService;
    private readonly IApplicationDbContext _db = db;

    [KernelFunction("get_products")]
    [Description("Get merchant products with search and pagination.")]
    public async Task<Result<PaginatedResult<ProductResponse>>> GetProductsAsync(
        Guid merchantId,
        string? cursor = null,
        int limit = 10,
        string? search = null,
        CancellationToken ct = default) =>
        await _productService.GetProductsAsync(merchantId, cursor, limit, search, ct);

    [KernelFunction("create_support_request")]
    [Description("Create support ticket for a customer.")]
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
        var result = SupportRequest.Create(Guid.NewGuid(), merchantId, botId, botName, platform, platformContact, customerName, type, reason, customerId, message);
        if (!result.IsSuccess) return result.Errors;

        _db.SupportRequests.Add(result.Value);
        await _db.SaveChangesAsync(ct);
        return result;
    }

    [KernelFunction("get_customer_by_uuid")]
    [Description("Find customer by UUID string.")]
    [return: Description("Customer profile data.")]
    public async Task<Result<CustomerDto>> GetCustomerByUuidAsync(
        Guid merchantId,
        string uuid,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(uuid)) return CustomerErrors.UuidRequired;

        var customer = await _db.Customers.AsNoTracking()
            .FirstOrDefaultAsync(c => c.MerchantId == merchantId && c.Uuid == uuid, ct);

        return customer is null ? CustomerErrors.NotFound : CustomerDto.FromDomain(customer);
    }

    [KernelFunction("create_customer")]
    [Description("Register new customer for merchant.")]
    public async Task<Result<CustomerDto>> CreateCustomerAsync(
        Guid merchantId,
        string name,
        string phoneNumber,
        string? email = null,
        CancellationToken ct = default)
    {
        var customerResult = Customer.Create(Guid.NewGuid(), merchantId, Guid.NewGuid().ToString(), name, phoneNumber, email);
        if (!customerResult.IsSuccess) return customerResult.Errors;

        _db.Customers.Add(customerResult.Value);
        await _db.SaveChangesAsync(ct);
        return CustomerDto.FromDomain(customerResult.Value);
    }

    [KernelFunction("get_order_details")]
    [Description("Get full order info, items, and status.")]
    public async Task<Result<string>> GetOrderDetailsAsync(Guid merchantId, Guid orderId, CancellationToken ct = default)
    {
        var order = await _db.Orders.Include(o => o.Customer).Include(o => o.Items)
            .FirstOrDefaultAsync(o => o.MerchantId == merchantId && o.Id == orderId, ct);

        return order is null ? OrderErrors.NotFound : BuildOrderDetailsMessage(order);
    }

    [KernelFunction("get_customer")]
    [Description("Get customer details by Guid ID.")]
    public async Task<Result<CustomerDto>> GetCustomerAsync(Guid merchantId, Guid customerId, CancellationToken ct = default) =>
        await _customerService.GetByIdAsync(customerId, ct);

    [KernelFunction("get_merchant_details")]
    [Description("Get merchant profile, settings, and plan.")]
    [return: Description("Merchant preferences and subscription status.")]
    public async Task<Result<CurrentMerchantResponse>> GetMerchantDetailsAsync(Guid merchantId, CancellationToken ct = default)
    {
        var merchant = await _db.Merchants.Include(m => m.ShopSettings)
            .FirstOrDefaultAsync(m => m.Id == merchantId, ct);

        return merchant is null ? MerchantErrors.NotFound : merchant.ToCurrentResponse();
    }

    private static string BuildOrderDetailsMessage(Order order)
    {
        var items = string.Join(", ", order.Items.Select(i => $"{i.ProductName} (x{i.Quantity})"));
        return $"Order {order.Id} for {order.CustomerName}:\n- Items: {items}\n- Total: {order.TotalAmount} {order.Currency}\n- Status: {order.Status}\n- Delivery: {order.DeliveryAddress ?? "N/A"}";
    }
}