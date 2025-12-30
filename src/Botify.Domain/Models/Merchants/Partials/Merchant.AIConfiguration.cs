using Botify.Domain.Models.AI;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Models.Merchants
{
    public sealed partial class Merchant
    {
        public AIConfiguration? AIConfiguration { get; private set; }

        public Result<Created> AddConfigureAI(AIConfiguration aiConfiguration)
        {
            if (aiConfiguration is null)
                return AIConfigurationErrors.NotFound;
            AIConfiguration = aiConfiguration;
            return Result.Created;
        }
    }
}
