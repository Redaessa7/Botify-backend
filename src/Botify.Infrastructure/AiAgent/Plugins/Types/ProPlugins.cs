using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.Features.Customers.Interfaces;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Orders;
using Botify.Domain.Models.Orders.Enums;
using Botify.Domain.Models.Orders.Helpers;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.AiAgent.Plugins.Types;
using Microsoft.EntityFrameworkCore;
using Microsoft.SemanticKernel;
using System.ComponentModel;

namespace Botify.Infrastructure.AIServices.Plugins.Types;

public class ProPlugins(
    IProductService productService,
    ICustomerService customerService,
    IApplicationDbContext db
) : FreePlugins(productService, customerService, db)
{
    private readonly IApplicationDbContext _db = db;

    [KernelFunction("create_order_items")]
    [Description("Convert product selection into order line items.")]
    public Result<List<OrderItem>> CreateOrderItems(
        Guid orderId,
        List<(Guid productId, string productName, int quantity, decimal unitPrice)> items)
    {
        var orderItems = new List<OrderItem>();
        foreach (var (productId, productName, quantity, unitPrice) in items)
        {
            var result = OrderItem.Create(orderId, productId, productName, quantity, unitPrice);
            if (!result.IsSuccess) return result.Errors;
            orderItems.Add(result.Value);
        }
        return orderItems;
    }

    [KernelFunction("create_order")]
    [Description("Place a new order using merchant currency.")]
    [return: Description("The created order details.")]
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
        var merchant = await _db.Merchants.Include(m => m.ShopSettings)
            .FirstOrDefaultAsync(m => m.Id == merchantId, ct);

        if (merchant?.ShopSettings is null || string.IsNullOrWhiteSpace(merchant.ShopSettings.Currency))
            return Error.Validation("Order.CurrencyMissing", "Currency not configured.");

        var orderResult = Order.Create(
            Guid.NewGuid(),
            OrderUuidGenerator.Generate(merchantId, customerId),
            merchantId,
            customerId,
            customerName,
            customerContact,
            platform,
            items,
            items.Sum(i => i.LineTotal),
            merchant.ShopSettings.Currency,
            OrderStatus.Pending,
            deliveryAddress,
            notes
        );

        if (!orderResult.IsSuccess) return orderResult.Errors;

        _db.Orders.Add(orderResult.Value);
        await _db.SaveChangesAsync(ct);
        return orderResult;
    }
}