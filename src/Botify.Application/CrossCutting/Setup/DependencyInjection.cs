using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Services;
using Botify.Application.Features.Webhooks;
using Botify.Domain.Events;
using Botify.Domain.Events.Merchants;
using Microsoft.Extensions.Configuration;
namespace Microsoft.Extensions.DependencyInjection;


public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Register domain event handlers
        DomainEventDispatcher.Register(MerchantEventHandlers.Handle);
        services.AddScoped<IMerchantService, MerchantService>();
        services.AddScoped<IWebhookService, WebhookService>();
        return services;
    }


}

