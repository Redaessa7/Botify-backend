using Botify.Domain.Models.AI;
using Microsoft.SemanticKernel;

namespace Botify.Infrastructure.AIServices.AIConnectors.Interfaces
{
    public interface IAIConnectorStrategy
    {
        // يجب أن يكون المفتاح هو نفس اسم المنصة المخزن في قاعدة البيانات (مثل "OpenAI")
        string PlatformKey { get; }

        /// <summary>
        /// يقوم بإضافة إعدادات الموصل (مثل Model, API Key) إلى KernelBuilder.
        /// </summary>
        void ConfigureBuilder(IKernelBuilder builder, AIConfiguration config);
    }
}
