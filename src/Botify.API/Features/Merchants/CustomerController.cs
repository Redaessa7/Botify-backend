using Botify.API.CrossCutting.Controllers;
using Botify.Application.Features.Customers.Interfaces;
using Botify.Domain.Models.Merchants.ValueObjects;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.Features.Merchants
{
    [Route("api/customers")]
    [Authorize]
    public sealed class CustomerController : ApiController
    {
        private readonly ICustomerService _customerService;

        public CustomerController(ICustomerService customerService)
        {
            _customerService = customerService;
        }

        /// <summary>
        /// Get all customers for the current merchant with optional search and pagination.
        /// </summary>
        [HttpGet]
        public async Task<IActionResult> GetAllAsync(
            [FromQuery] string? cursor,
            [FromQuery] int limit = 10,
            [FromQuery] string? search = null,
            [FromQuery] BotPlatform? botPlatform = null,
            CancellationToken cancellationToken = default)
        {
            var result = await _customerService.GetAllAsync(cursor, limit, search, botPlatform, cancellationToken);
            return result.Match(Ok, Problem);
        }
        [HttpGet("stats")]
        public async Task<IActionResult> GetStatsAsync(
    CancellationToken cancellationToken = default)
        {
            var result = await _customerService.GetStatsAsync(cancellationToken);
            return result.Match(Ok, Problem);
        }

        /// <summary>
        /// Get a single customer by ID for the current merchant.
        /// </summary>
        [HttpGet("{customerId:guid}")]
        public async Task<IActionResult> GetByIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default)
        {
            var result = await _customerService.GetByIdAsync(customerId, cancellationToken);
            return result.Match(Ok, Problem);
        }
    }
}
