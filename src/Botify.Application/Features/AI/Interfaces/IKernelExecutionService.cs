using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.ResultPattern;

namespace Botify.Application.Features.AI.Interfaces
{

    public interface IKernelExecutionService
    {
        Task<Result<string>> ExecuteAsync(string prompt,AIConfiguration config,PluginTier pluginTier, CancellationToken cancellationToken);
    }
}
