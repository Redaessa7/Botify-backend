using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.Models.Customers;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.AI.Interfaces
{

    public interface IKernelExecutionService
    {
        Task<Result<string>> ExecuteAsync(KernelExecutionRequest request, CancellationToken cancellationToken);
    }

    public record KernelExecutionRequest(
        string Prompt,
        AIConfiguration Config,
        PluginTier PluginTier,
        Customer? Customer,
        BotPlatform Platform,
        string ChatId);
}
