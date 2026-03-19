using Botify.Application.CrossCutting.Responses;
using Botify.Application.Features.AI.Requests;
using Botify.Application.Features.AI.Responses;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.AI.Interfaces
{
    public interface IAIConfigurationService
    {
        #region AIConfiguration Lifecycle

        Task<Result<AIConfigurationResponse>> CreateAsync(
            Guid merchantId,
            CreateAIConfigurationRequest request,
            CancellationToken cancellationToken = default);

        Task<Result<AIConfigurationResponse>> UpdateAsync(
            Guid merchantId,
            SetAIConfigurationRequest request,
            CancellationToken cancellationToken = default);
        #endregion

        #region Queries

        Task<Result<AIConfigurationResponse>> GetByIdAsync(
            Guid merchantId,
            Guid configId,
            CancellationToken cancellationToken = default);

        Task<Result<AIConfigurationResponse>> GetConfigurationsAsync(
            Guid merchantId,
            CancellationToken cancellationToken = default);
        Task<Result<bool>> IsAIConfigExistsAsync(
        Guid merchantId,
        CancellationToken cancellationToken = default);
        #endregion
    }
}
