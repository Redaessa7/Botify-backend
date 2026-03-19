using Botify.Application.Features.AI.Interfaces;
using Botify.Application.Features.Customers.Responses;
using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.AiAgent;
using Botify.Infrastructure.AIServices.AIConnectors.Interfaces;
using Botify.Infrastructure.AIServices.Plugins.Types;
using Botify.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using Microsoft.SemanticKernel.Connectors.AzureOpenAI;
using Microsoft.SemanticKernel.Connectors.Google;
using Microsoft.SemanticKernel.Connectors.OpenAI;
using System.Text.Json;

namespace Botify.Infrastructure.AIServices
{
    public class SemanticKernelService : IKernelExecutionService
    {
        private readonly IEnumerable<IAIConnectorStrategy> _connectorStrategies;
        private readonly IServiceProvider _serviceProvider;
        private readonly ApplicationDbContext _db;

        // Root path for dynamic prompt discovery
        private static readonly string PromptRoot = Path.Combine(AppContext.BaseDirectory, "AiAgent", "Prompts");
        public SemanticKernelService(
            IServiceProvider serviceProvider,
            IEnumerable<IAIConnectorStrategy> connectorStrategies,
            ApplicationDbContext db)
        {
            _serviceProvider = serviceProvider;
            _connectorStrategies = connectorStrategies;
            _db = db;
        }

        public async Task<Result<string>> ExecuteAsync(KernelExecutionRequest request, CancellationToken ct)
        {


            var kernel = BuildKernel(request.Config);
            LoadPlugins(kernel, GetPluginNames(request.PluginTier));

            // 1. Session & Memory
            var session = await GetOrCreateSessionAsync(request.Config.MerchantId, request.ChatId, request.Platform, ct);
            var history = BuildChatHistory(request);
            InjectMemorySummary(history, session.CurrentSummary);

            // 2. Dynamic Routing (No Hardcoding)
            string intent = await DetermineIntentAsync(kernel, request.Prompt, ct);
            var expertFunction = await ResolveExpertFunctionAsync(kernel, intent, request.Config, ct);

            // 3. AI Execution
            var result = await kernel.InvokeAsync(expertFunction, new KernelArguments { ["user_input"] = request.Prompt }, ct);
            var response = result.ToString();

            // 4. Background Persistence
            await FinalizeSessionStateAsync(session, history, request, ct);

            return response;
        }

        #region AI Orchestration Helpers (Dynamic Discovery)

        private async Task<KernelFunction> ResolveExpertFunctionAsync(Kernel kernel, string intent, AIConfiguration config, CancellationToken ct)
        {
            // Recursively find the YAML file matching the intent name (e.g., "SALESFLOW.yaml")
            var yamlPath = Directory.GetFiles(PromptRoot, $"{intent}.yaml", SearchOption.AllDirectories)
                                    .FirstOrDefault();

            // Fallback strategy if specific intent file is missing
            if (string.IsNullOrEmpty(yamlPath))
            {
                yamlPath = Path.Combine(PromptRoot, "Info", "MerchantFAQ.yaml");
            }

            var yamlContent = await File.ReadAllTextAsync(yamlPath, ct);
            var promptConfig = KernelFunctionYaml.ToPromptTemplateConfig(yamlContent);

            var settings = GetExecutionSettings(config);
            promptConfig.ExecutionSettings.Clear();
            promptConfig.ExecutionSettings.Add("default", settings);

            return kernel.CreateFunctionFromPrompt(promptConfig);
        }

        private async Task<string> DetermineIntentAsync(Kernel kernel, string input, CancellationToken ct)
        {
            string routerYaml = Path.Combine(PromptRoot, "Router", "IntentRouter.yaml");
            var routerFunc = kernel.CreateFunctionFromPromptYaml(await File.ReadAllTextAsync(routerYaml, ct));

            var result = await kernel.InvokeAsync(routerFunc, new() { ["user_input"] = input }, ct);

            // Clean output to match filenames (e.g., "SalesFlow" -> "SALESFLOW")
            return result.ToString().Trim().Replace(".", "").ToUpper();
        }

        #endregion

        #region Session & Memory Helpers

        private async Task<ChatSession> GetOrCreateSessionAsync(Guid merchantId, string externalChatId, BotPlatform platform, CancellationToken ct)
        {
            var session = await _db.ChatSessions
                .FirstOrDefaultAsync(s => s.ExternalChatId == externalChatId && s.Platform == platform, ct);

            if (session == null)
            {
                session = ChatSession.Create(merchantId, externalChatId, platform);
                _db.ChatSessions.Add(session);
                await _db.SaveChangesAsync(ct);
            }

            return session;
        }

        private void InjectMemorySummary(ChatHistory history, string? summary)
        {
            if (!string.IsNullOrEmpty(summary))
            {
                history.Insert(1, new ChatMessageContent(AuthorRole.System, $"Summary of previous facts: {summary}"));
            }
        }

        private async Task FinalizeSessionStateAsync(ChatSession session, ChatHistory history, KernelExecutionRequest request, CancellationToken ct)
        {
            session.MessageCount++;
            session.LastInteractionAt = DateTime.UtcNow;
            await _db.SaveChangesAsync(ct);

            if (session.MessageCount >= 10)
            {
                _ = Task.Run(async () =>
                {
                    using var scope = _serviceProvider.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

                    var kernelService = new SemanticKernelService(
                        scope.ServiceProvider,
                        scope.ServiceProvider.GetServices<IAIConnectorStrategy>(),
                        db
                    );

                    await kernelService.UpdateSummaryAsync(
                        request.ChatId,
                        request.Platform,
                        history,
                        session.CurrentSummary,
                        request.Config,
                        CancellationToken.None
                    );
                });

            }
        }

        #endregion

        #region Core Kernel & Plugin Logic

        private Kernel BuildKernel(AIConfiguration config)
        {
            var builder = Kernel.CreateBuilder();
            var strategy = _connectorStrategies.FirstOrDefault(s => s.PlatformKey.Equals(config.Platform));

            if (strategy == null) throw new NotSupportedException($"Platform '{config.Platform}' not supported.");

            strategy.ConfigureBuilder(builder, config);
            return builder.Build();
        }

        private void LoadPlugins(Kernel kernel, IEnumerable<string> pluginNames)
        {
            using var scope = _serviceProvider.CreateScope();
            foreach (var name in pluginNames)
            {
                var plugin = scope.ServiceProvider.GetKeyedService<IPlugin>(name);
                if (plugin != null) kernel.ImportPluginFromObject(plugin, name);
            }
        }

        private static IEnumerable<string> GetPluginNames(PluginTier tier) => tier switch
        {
            PluginTier.Free => ["FreePlugins"],
            PluginTier.Pro => ["ProPlugins"],
            _ => []
        };

        #endregion

        #region Static Builders

        private static ChatHistory BuildChatHistory(KernelExecutionRequest request)
        {
            var history = new ChatHistory();
            var instructions = "\n- Specialized Context: Sales/Support." +
                               "\n- Mandatory: Use 'get_customer_by_uuid' results only. If Not Found, ask for details.";

            history.AddSystemMessage(request.Config.SystemPrompt + instructions);

            // Simulated Turn for Contextual Awareness
            var callId = $"call_{Guid.NewGuid():N}";
            history.Add(new ChatMessageContent(AuthorRole.Assistant, [
                new FunctionCallContent("get_customer_by_uuid", "FreePlugins", callId, new() { ["uuid"] = request.ChatId })
            ]));

            string toolResult = request.Customer != null
                ? JsonSerializer.Serialize(CustomerDto.FromDomain(request.Customer))
                : JsonSerializer.Serialize(new { code = "Customer.NotFound", message = "Not registered.", type = 4 });

            history.Add(new ChatMessageContent(AuthorRole.Tool, [
                new FunctionResultContent("get_customer_by_uuid", "FreePlugins", callId, toolResult)
            ]));

            history.AddUserMessage(request.Prompt);
            return history;
        }

        private static PromptExecutionSettings GetExecutionSettings(AIConfiguration config) => config.Platform switch
        {
            //AIPlatform.OpenAI => new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions, Temperature = config.Temperature, MaxTokens = config.MaxTokens },
            //AIPlatform.AzureOpenAI => new AzureOpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions, Temperature = config.Temperature, MaxTokens = config.MaxTokens },
            AIPlatform.GooglePaLM => new GeminiPromptExecutionSettings { ToolCallBehavior = GeminiToolCallBehavior.AutoInvokeKernelFunctions, Temperature = config.Temperature, MaxTokens = config.MaxTokens },
            AIPlatform.GitHub => new OpenAIPromptExecutionSettings { ToolCallBehavior = ToolCallBehavior.AutoInvokeKernelFunctions, Temperature = config.Temperature, MaxTokens = config.MaxTokens },
            _ => throw new NotSupportedException()
        };

        #endregion

        #region Background Tasks

        private async Task UpdateSummaryAsync(string chatId, BotPlatform platform, ChatHistory history, string? existingSummary, AIConfiguration config, CancellationToken ct)
        {
  
                using var scope = _serviceProvider.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
                var kernel = BuildKernel(config);

                string yamlPath = Path.Combine(PromptRoot, "System", "Summarizer.yaml");
                var dialogue = string.Join("\n", history.Where(m => m.Role != AuthorRole.System).TakeLast(10).Select(m => $"{m.Role}: {m.Content}"));

                var result = await kernel.InvokeAsync(kernel.CreateFunctionFromPromptYaml(await File.ReadAllTextAsync(yamlPath, ct)), new KernelArguments
                {
                    ["current_summary"] = existingSummary ?? "No history.",
                    ["recent_history"] = dialogue
                }, ct);

                var session = await db.ChatSessions.FirstOrDefaultAsync(s => s.ExternalChatId == chatId && s.Platform == platform, ct);
                if (session != null)
                {
                    session.CurrentSummary = result.ToString();
                    session.MessageCount = 0;
                    await db.SaveChangesAsync(ct);
                }
        }

        #endregion
    }
}