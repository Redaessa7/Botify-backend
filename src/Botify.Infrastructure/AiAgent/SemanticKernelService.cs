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
using System.Text.Json;

namespace Botify.Infrastructure.AIServices
{
    /// <summary>
    /// Executes AI requests using Semantic Kernel with plugin support.
    /// Handles Free and Pro tiers, connector strategies, and customer lookup logic.
    /// </summary>
    public class SemanticKernelService : IKernelExecutionService
    {
        private readonly IEnumerable<IAIConnectorStrategy> _connectorStrategies;
        private readonly IServiceProvider _serviceProvider;

        public SemanticKernelService(
            IServiceProvider serviceProvider,
            IEnumerable<IAIConnectorStrategy> connectorStrategies)
        {
            _serviceProvider = serviceProvider;
            _connectorStrategies = connectorStrategies;
        }

        /// <summary>
        /// Executes an AI request with Semantic Kernel, loading appropriate plugins and handling customer lookup.
        /// </summary>
        public async Task<Result<string>> ExecuteAsync(
            KernelExecutionRequest request,
            CancellationToken cancellationToken)
        {
            var kernel = BuildKernel(request.Config);
            LoadPlugins(kernel, GetPluginNames(request.PluginTier));

            var chatService = kernel.GetRequiredService<IChatCompletionService>();
            var history = BuildChatHistory(request);

            var settings = GetExecutionSettings(request.Config);

            var result = await chatService.GetChatMessageContentAsync(
                history,
                settings,
                kernel: kernel,
                cancellationToken: cancellationToken);

            history.AddAssistantMessage(result.Content ?? string.Empty);

            return result.Content;
        }

        #region Private Helpers

        private static IEnumerable<string> GetPluginNames(PluginTier tier) =>
            tier switch
            {
                PluginTier.Free => new[] { "FreePlugins" },
                PluginTier.Pro => new[] { "ProPlugins" },
                _ => Array.Empty<string>()
            };

        private void LoadPlugins(Kernel kernel, IEnumerable<string> pluginNames)
        {
            foreach (var pluginName in pluginNames)
            {
                using var scope = _serviceProvider.CreateScope();
                var pluginInstance = scope.ServiceProvider.GetKeyedService<IPlugin>(pluginName);

                if (pluginInstance != null)
                {
                    kernel.ImportPluginFromObject(pluginInstance, pluginName);
                }
                else
                {
                    Console.WriteLine($"Error: Plugin '{pluginName}' not found or resolved.");
                }
            }
        }

        private Kernel BuildKernel(AIConfiguration config)
        {
            var builder = Kernel.CreateBuilder();

            var connectorStrategy = _connectorStrategies
                .FirstOrDefault(s => s.PlatformKey.Equals(config.Platform.ToString(), StringComparison.OrdinalIgnoreCase));

            if (connectorStrategy == null)
                throw new NotSupportedException($"AI Platform '{config.Platform}' is not supported.");

            connectorStrategy.ConfigureBuilder(builder, config);
            return builder.Build();
        }

        private static ChatHistory BuildChatHistory(KernelExecutionRequest request)
        {
            var history = new ChatHistory();

            // Extend system prompt with customer handling instructions
            var customInstructions =
                "\n- If the customer is not found (null), politely ask the user for their details (Name, Phone, etc.)." +
                "\n- Once you have the details, use the 'CreateCustomer' function to register them.";

            history.AddSystemMessage(request.Config.SystemPrompt + customInstructions);

            // Simulate customer lookup
            var callId = $"call_{Guid.NewGuid()}";
            history.Add(new ChatMessageContent(
                AuthorRole.Assistant,
                items: [
                    new FunctionCallContent(
                        "GetCustomer",
                        "DatabasePlugin",
                        callId,
                        new KernelArguments { { "customerId", request.Customer?.Id } })
                ]));

            var toolResult = request.Customer != null
                ? JsonSerializer.Serialize(request.Customer)
                : "{\"status\": \"not_found\", \"message\": \"Customer record does not exist in our database.\"}";

            history.Add(new ChatMessageContent(
                AuthorRole.Tool,
                items: [
                    new FunctionResultContent("GetCustomer", "DatabasePlugin", callId, toolResult)
                ]));

            // Add user prompt
            history.AddUserMessage(request.Prompt);

            return history;
        }

        private static PromptExecutionSettings GetExecutionSettings(AIConfiguration config) =>
            config.Platform switch
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

        #endregion
    }
}
