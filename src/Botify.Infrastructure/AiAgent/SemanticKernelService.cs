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
            KernelExecutionRequest request,
            CancellationToken cancellationToken)
        {
            var kernel = BuildKernel(request.Config);

            var pluginTierMapping = new Dictionary<PluginTier, List<string>>
            {
                { PluginTier.Free, new List<string> { "FreePlugins" } },
                { PluginTier.Premium, new List<string> { "FreePlugins", "PremiumPlugins" } },
                { PluginTier.Enterprise, new List<string> { "FreePlugins", "PremiumPlugins", "EnterprisePlugins" } }
            };

            LoadPlugins(kernel, pluginTierMapping[request.PluginTier]);

            var chat = kernel.GetRequiredService<IChatCompletionService>();

            // System prompt must edit and append customer cases
            
            var history = new ChatHistory();
            
            // 1. تحديث الـ System Prompt ليتعامل مع حالة عدم وجود العميل
            string customInstructions = "\n- If the customer is not found (null), politely ask the user for their details (Name, Phone, etc.)." +
                                        "\n- Once you have the details, use the 'CreateCustomer' function to register them.";
    
            history.AddSystemMessage(request.Config.SystemPrompt + customInstructions);

            // 2. محاكاة عملية البحث عن العميل (GetCustomer)
            var callId = $"call_{Guid.NewGuid()}";
    
            // المساعد يحاول البحث أولاً
            history.Add(new ChatMessageContent(AuthorRole.Assistant, items: [ 
                new FunctionCallContent("GetCustomer", "DatabasePlugin", callId, new KernelArguments { { "customerId", request.Customer?.Id } }) 
            ]));

            if (request.Customer != null)
            {
                // حالة الـ Success: العميل موجود
                var toolResult = System.Text.Json.JsonSerializer.Serialize(request.Customer);
                history.Add(new ChatMessageContent(AuthorRole.Tool, items: [ 
                    new FunctionResultContent("GetCustomer", "DatabasePlugin", callId, toolResult) 
                ]));
            }
            else
            {
                // حالة الـ Null: العميل غير موجود
                // نرسل رد "not_found" لكي يفهم الـ LLM ويبدأ في طلب البيانات
                var toolResult = "{\"status\": \"not_found\", \"message\": \"Customer record does not exist in our database.\"}";
                history.Add(new ChatMessageContent(AuthorRole.Tool, items: [ 
                    new FunctionResultContent("GetCustomer", "DatabasePlugin", callId, toolResult) 
                ]));
            }
           
            
            history.AddUserMessage(request.Prompt);
            var settings = GetExecutionSettings(request.Config);

            var result = await chat.GetChatMessageContentAsync(
                history,
                settings,
                kernel: kernel,
                cancellationToken: cancellationToken);

            history.AddAssistantMessage(result.Content ?? string.Empty);
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
