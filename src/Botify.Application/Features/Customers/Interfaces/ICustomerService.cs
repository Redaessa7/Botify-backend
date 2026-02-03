using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Customers.Responses;
using Botify.Domain.ResultPattern;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace Botify.Application.Features.Customers.Interfaces
{
    public interface ICustomerService
    {
        /// <summary>
        /// Get all customers for a merchant with cursor-based pagination and optional search.
        /// </summary>
        Task<Result<PaginatedResult<CustomerDto>>> GetAllAsync(
            string? cursor,
            int limit = 10,
            string? search = null,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Get a single customer by ID.
        /// </summary>
        Task<Result<CustomerDto>> GetByIdAsync(
            Guid customerId,
            CancellationToken cancellationToken = default);
    }
}
