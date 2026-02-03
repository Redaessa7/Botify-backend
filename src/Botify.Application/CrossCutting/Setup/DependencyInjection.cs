using Botify.Application.Features.AI.Interfaces;
using Botify.Application.Features.AI.Services;
using Botify.Application.Features.AI.Strategies;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Merchants.Services;
using Botify.Application.Features.Orders.Interfaces;
using Botify.Application.Features.Orders.Services;
using Botify.Application.Features.Webhooks;
using Botify.Domain.Events;
using Botify.Domain.Events.Merchants;
using Botify.Domain.Models.AI.Enums;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddApplicationServices(this IServiceCollection services, IConfiguration configuration)
        {
            // Register domain event handlers
            DomainEventDispatcher.Register(MerchantEventHandlers.Handle);

            // Merchant services
            services.AddScoped<IMerchantService, MerchantService>();
            services.AddScoped<IWebhookService, WebhookService>();
            services.AddScoped<IProductService, ProductService>();
            services.AddScoped<ISupportRequestService, SupportRequestService>();
            services.AddScoped<IAIConfigurationService, AIConfigurationService>();
            services.AddScoped<IOrderService, OrderService>();

            return services;
        }
    }
}
