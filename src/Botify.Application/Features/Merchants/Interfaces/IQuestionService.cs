using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.Merchants.Interfaces
{
    public interface IQuestionService
    {
        Task<Result<QuestionResponse>> CreateAsync(Guid merchantId, CreateQuestionRequest request, CancellationToken cancellationToken = default);


        Task<Result<Success>> DeleteAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default);

        Task<Result<QuestionResponse>> GetByIdAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default);

        Task<Result<PaginatedResult<QuestionResponse>>> GetQuestionsAsync(
               Guid merchantId,
               string? cursor,
               int limit = 10,
               string? search = null,
                bool? isResolve = null,
                BotPlatform? botPlatform = null,
               CancellationToken cancellationToken = default);
        Task<Result<QuestionStatsResponse>> GetStatsAsync(Guid merchantId, CancellationToken ct = default);
        Task<Result<Success>> ToggleStatusAsync(Guid merchantId, Guid questionId, CancellationToken cancellationToken = default);

    }
}
