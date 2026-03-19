using Botify.Domain.Models.AI;
using Botify.Domain.Models.AI.Enums;
using Microsoft.SemanticKernel;

namespace Botify.Infrastructure.AIServices.AIConnectors.Interfaces
{
    public interface IAIConnectorStrategy
    {
        // يجب أن يكون المفتاح هو نفس اسم المنصة المخزن في قاعدة البيانات (مثل "OpenAI")
        AIPlatform PlatformKey { get; }

        /// <summary>
        /// يقوم بإضافة إعدادات الموصل (مثل Model, API Key) إلى KernelBuilder.
        /// </summary>
        Task ConfigureBuilder(IKernelBuilder builder, AIConfiguration config);
    }
}
