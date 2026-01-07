using Botify.Application.Features.AI.Interfaces;
using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.AIServices.AIConnectors.Interfaces;
using Botify.Infrastructure.AIServices.Plugins.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Botify.Infrastructure.AIServices
{
    public class SemanticKernelService : IKernelExecutionService
    {
        private readonly IEnumerable<IAIConnectorStrategy> _connectorStrategies;
        private readonly IServiceProvider _serviceProvider;

        public SemanticKernelService(IServiceProvider serviceProvider, IEnumerable<IAIConnectorStrategy> connectorStrategies)
        {
            _serviceProvider = serviceProvider;
            _connectorStrategies = connectorStrategies;
        }

        public async Task<Result<string>> ExecuteAsync(
            string prompt,
            AIConfiguration config,
            PluginTier pluginTier,
            CancellationToken cancellationToken)
        {
            var kernel = BuildKernel(config);

            var pluginTierMapping = new Dictionary<PluginTier, List<string>>
            {
                { PluginTier.Free, new List<string> { "FreePlugins" } },
                { PluginTier.Premium, new List<string> { "FreePlugins", "PremiumPlugins" } },
                { PluginTier.Enterprise, new List<string> { "FreePlugins", "PremiumPlugins", "EnterprisePlugins" } }
            };

            LoadPlugins(kernel, pluginTierMapping[pluginTier]);

            var chat = kernel.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory();
            history.AddSystemMessage(config.SystemPrompt);
            history.AddUserMessage(prompt);

            var settings = GetExecutionSettings(config);

            var result = await chat.GetChatMessageContentAsync(
                history,
                settings,
                kernel: kernel,
                cancellationToken: cancellationToken);

            return result.Content;
        }

        private void LoadPlugins(Kernel kernel, IEnumerable<string> requiredPluginNames)
        {
            foreach (var pluginName in requiredPluginNames)
            {
                using var scope = _serviceProvider.CreateScope();
                var pluginInstance = scope.ServiceProvider.GetKeyedService<IPlugin>(pluginName);

                if (pluginInstance != null)
                {
                    kernel.ImportPluginFromObject(pluginInstance, pluginName);
                }
                else
                {
                    Console.WriteLine($"Error: Keyed Plugin '{pluginName}' not found or resolved.");
                }
            }
        }

        private Kernel BuildKernel(AIConfiguration config)
        {
            var builder = Kernel.CreateBuilder();

            var connectorStrategy = _connectorStrategies
                .FirstOrDefault(s => s.PlatformKey.Equals(config.Platform.ToString(), StringComparison.OrdinalIgnoreCase));

            if (connectorStrategy == null)
            {
                throw new NotSupportedException($"AI Platform '{config.Platform}' is not supported by any registered connector strategy.");
            }

            connectorStrategy.ConfigureBuilder(builder, config);

            return builder.Build();
        }

        private PromptExecutionSettings GetExecutionSettings(AIConfiguration config)
        {
            return config.Platform switch
            {
                AIPlatform.OpenAI => new OpenAIPromptExecutionSettings
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                    Temperature = config.Temperature,
                    MaxTokens = config.MaxTokens
                },
                AIPlatform.AzureOpenAI => new AzureOpenAIPromptExecutionSettings
                {
                    ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions,
                    Temperature = config.Temperature,
                    MaxTokens = config.MaxTokens
                },
                AIPlatform.GooglePaLM => new GeminiPromptExecutionSettings
                {
                    ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions,
                    Temperature = config.Temperature,
                    MaxTokens = config.MaxTokens
                },
                _ => throw new NotSupportedException(
                    $"AI Platform '{config.Platform}' is not supported for execution settings.")
            };
        }
    }
}
