using Botify.Application.Features.AI.Interfaces;
using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.ResultPattern;
using Botify.Infrastructure.AIServices.AIConnectors.Interfaces;
using Botify.Infrastructure.AIServices.Plugins.Types;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.SemanticKernel;
using Microsoft.SemanticKernel.ChatCompletion;
using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Infrastructure.AIServices
{
    public class SemanticKernelService(IServiceProvider serviceProvider, IEnumerable<IAIConnectorStrategy> connectorStrategies) : IKernelExecutionService
    {
        private readonly IEnumerable<IAIConnectorStrategy> _connectorStrategies = connectorStrategies;
        private readonly IServiceProvider _serviceProvider = serviceProvider;
        public virtual async Task<Result<string>> ExecuteAsync(string prompt, AIConfiguration config, PluginTier pluginTier, CancellationToken cancellationToken)
        {
            Console.WriteLine("In AiModel ,Enter prompt");

            var kernel = BuildKernel(config);

            Console.WriteLine("In AiModel ,Create Kernal Success");

            var pluginTierMapping = new Dictionary<PluginTier, List<string>>
            {
                { PluginTier.Free, new List<string> { "FreePlugins" } },
                { PluginTier.Premium, new List<string> { "FreePlugins", "PremiumPlugins" } },
                { PluginTier.Enterprise, new List<string> { "FreePlugins", "PremiumPlugins", "EnterprisePlugins" } }
            };

            LoadPlugins(kernel, pluginTierMapping[pluginTier]);

            Console.WriteLine("In AiModel ,Load Plugins success");

            // 3. تنفيذ Prompt (قد تحتاج لعمل Prompt مسبق أو تنفيذه مباشرة كـ Input)
            var chat = kernel.GetRequiredService<IChatCompletionService>();

            var history = new ChatHistory();
            history.AddSystemMessage(config.SystemPrompt);
            history.AddUserMessage(prompt);

            var result = await chat.GetChatMessageContentAsync(
                history,
                cancellationToken: cancellationToken
            );

            Console.WriteLine("In AiModel ,ai result");
            return result.Content;

            // return result.GetValue<string>();

        }

       private void LoadPlugins(Kernel kernel, List<string> requiredPluginNames)
    {
        foreach (var pluginName in requiredPluginNames)
        {
                // 1. محاولة استرجاع نسخة الـ Plugin من الـ DI Container باستخدام المفتاح (Key)

                // بما أنك سجلتها بـ Service Type هو IPlugin، سنستخدم هذا النوع
                using var scope = _serviceProvider.CreateScope(); 

                var pluginInstance = scope.ServiceProvider.GetKeyedService<IPlugin>(pluginName);
                // 2. التحقق من وجود النسخة المحملة
                if (pluginInstance != null)
            {
                // 3. تحميل الـ Plugin في Semantic Kernel
                // يجب التأكد من أن pluginInstance هو Type مناسب للاستخدام مع ImportPlugin

                // SK تتطلب نسخة الكلاس (Instance) واسم مجموعة الـ Plugin
               if (pluginInstance != null)
                {
                    kernel.ImportPluginFromObject(pluginInstance, pluginName);
                    Console.WriteLine($"Keyed Plugin '{pluginName}' loaded successfully.");
                }

            }
            else
            {
                // إذا لم يتم العثور على المفتاح، قد يعني ذلك أن التاجر طلب Plugin غير مسجلة
                Console.WriteLine($"Error: Keyed Plugin '{pluginName}' not found or resolved.");
            }
        }
    }


        private Kernel BuildKernel(AIConfiguration config)
        {
            var builder = Kernel.CreateBuilder();
            ChatHistory history = new ChatHistory();

            history.AddSystemMessage(config.SystemPrompt);

            var connectorStrategy = _connectorStrategies
                        .FirstOrDefault(s => s.PlatformKey.Equals(config.Platform.ToString(), StringComparison.OrdinalIgnoreCase));

            if (connectorStrategy == null)
            {
                throw new NotSupportedException($"AI Platform '{config.Platform}' is not supported by any registered connector strategy.");
            }

            connectorStrategy.ConfigureBuilder(builder, config);

            return builder.Build();
        }
    }
}
