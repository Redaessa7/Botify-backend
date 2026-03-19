using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.SupportRequests.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.Merchants.Interfaces
{
    public interface ISupportRequestService
    {
        Task<Result<Success>> DeleteAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default);

        Task<Result<SupportRequestDto>> GetByIdAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default);

        Task<Result<PaginatedResult<SupportRequestDto>>> GetAllAsync(
               Guid merchantId,
               string? cursor,
               int limit = 10,
               string? search = null,
                bool? isResolve = null,
                BotPlatform? botPlatform = null,
                RequestReason? reason = null,
                SupportRequestType? type = null,
               CancellationToken cancellationToken = default);
        Task<Result<SupportRequestStatsResponse>> GetStatsAsync(Guid merchantId, CancellationToken ct = default);
        Task<Result<Success>> ToggleStatusAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default);

    }
}
