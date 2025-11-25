using Botify.Application.Common.Interfaces;
using Botify.Domain.Events;
using Botify.Domain.Events.Merchants;
using Microsoft.Extensions.Configuration;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class ApplicationRegisterServices
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register domain event handlers
            DomainEventDispatcher.Register(MerchantEventHandlers.Handle);
            return services;
        }


    }
}
