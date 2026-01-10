using Botify.API.CrossCutting.Controllers;
using Botify.Application.Features.Orders.Interfaces;
using Botify.Application.Features.Orders.Requests;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Orders.Enums;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Merchants;

[ApiVersion("1.0")]
[Route("api/merchants/{merchantId:guid}/orders")]
public class OrderController(IOrderService orderService) : ApiController
{
    private readonly IOrderService _orderService = orderService;

    #region Order Lifecycle


    /// <summary>
    /// Updates the status of an existing order.
    /// </summary>
    [HttpPatch("{orderId:guid}/status")]
    [Authorize]
    public async Task<IActionResult> UpdateStatusAsync(Guid merchantId, Guid orderId,[FromQuery]OrderStatus newStatus, CancellationToken ct = default)
    {
        var result = await _orderService.UpdateStatusAsync(orderId, newStatus, ct);
        return result.Match(_ => Ok(new { }), Problem);
    }

    #endregion

    #region Queries
    /// <summary>
    /// Retrieves order statistics (total, pending, preparing, delivered).
    /// </summary>
    [HttpGet("stats")]
    [Authorize]
    public async Task<IActionResult> GetOrderStatsAsync(Guid merchantId, CancellationToken ct = default)
    {
        var result = await _orderService.GetStatsAsync(merchantId, ct);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Retrieves an order by ID.
    /// </summary>
    [HttpGet("{orderId:guid}")]
    [Authorize]
    public async Task<IActionResult> GetByIdAsync(Guid merchantId, Guid orderId, CancellationToken ct = default)
    {
        var result = await _orderService.GetByIdAsync(merchantId, orderId, ct);
        return result.Match(Ok, Problem);
    }

    /// <summary>
    /// Retrieves all orders with cursor-based pagination and optional filters.
    /// </summary>
    [HttpGet]
    [Authorize]
    public async Task<IActionResult> GetOrdersAsync(
        Guid merchantId,
        [FromQuery] string? cursor,
        [FromQuery] int limit = 10,
        [FromQuery] string? search = null,
        [FromQuery] OrderStatus? status = null,
        [FromQuery] BotPlatform? botPlatform = null,
        CancellationToken ct = default)
    {
        var result = await _orderService.GetOrdersAsync(merchantId, cursor, limit, search, status, botPlatform, ct);
        return result.Match(Ok, Problem);
    }

    #endregion
}
