using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.Features.AI.Interfaces;
using Botify.Application.Features.AI.Strategies;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Features.Webhooks;
using Botify.Application.Services.Auths;
using Botify.Domain.Models.AI.Enums;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Domain.Models.Shops;
using Botify.Infrastructure.AiAgent.Plugins.Types;
using Botify.Infrastructure.AIServices;
using Botify.Infrastructure.AIServices.AIConnectors.Connectors;
using Botify.Infrastructure.AIServices.AIConnectors.Interfaces;
using Botify.Infrastructure.AIServices.Plugins.Types;
using Botify.Infrastructure.AIServices.Strategies;
using Botify.Infrastructure.CrossCutting.BackgroundServices.Email;
using Botify.Infrastructure.CrossCutting.Services.BotPlatformServices.Telegram;
using Botify.Infrastructure.CrossCutting.Services.Commons;
using Botify.Infrastructure.CrossCutting.Services.EmailSenders;
using Botify.Infrastructure.CrossCutting.Services.FileServices;
using Botify.Infrastructure.Identity;
using Botify.Infrastructure.Identity.Authentication;
using Botify.Infrastructure.Persistence;
using Botify.Infrastructure.Persistence.Initialisation;
using Botify.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Reflection;
using System.Text;

namespace Microsoft.Extensions.DependencyInjection
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddPersistence(configuration);
            services.AddIdentityServices();
            services.AddAuthenticationJwtToken(configuration);
            services.AddFileServices();
            services.AddBotPlatformServices(configuration);
            services.AddAIServices();
            services.AddRedis(configuration)
                    .AddEmailServices(configuration);
            return services;
        }

        #region Persistence
        private static IServiceCollection AddPersistence(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
            services.AddDbContext<ApplicationDbContext>((sp, options) =>
            {
                options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
                options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
            });
            services.AddScoped<IApplicationDbContext, ApplicationDbContext>();
            services.AddScoped<ApplicationDbContextInitializer>();
            return services;
        }
        #endregion

        #region Identity
        private static IServiceCollection AddIdentityServices(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();
            services.AddScoped<IAuthServices, AuthService>();
            services.AddScoped<ITokenProvider, JwtTokenProvider>();
            return services;
        }
        #endregion

        #region Authentication
        private static IServiceCollection AddAuthenticationJwtToken(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.Events = new JwtBearerEvents
                {
                    OnMessageReceived = context =>
                    {
                        var token = context.Request.Cookies["access_token"];
                        if (!string.IsNullOrEmpty(token))
                        {
                            context.Token = token;
                        }
                        return Task.CompletedTask;
                    }
                };

                var jwtOptions = configuration.GetSection("JWTOptions");
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions["Issuer"],
                    ValidAudience = jwtOptions["Audience"],
                    IssuerSigningKey = new SymmetricSecurityKey(
                        Encoding.UTF8.GetBytes(jwtOptions["SecurityKey"]!)
                    )
                };
            });

            return services;
        }
        #endregion

        #region File Services
        private static IServiceCollection AddFileServices(this IServiceCollection services)
        {
            services.AddSingleton<IFileValidator, FileValidator>();
            services.AddScoped<IFileService>(sp =>
                new LocalFileService(
                    rootPath: "wwwroot/uploads",
                    validator: sp.GetRequiredService<IFileValidator>(),
                    logger: sp.GetRequiredService<ILogger<LocalFileService>>()
                )
            );
            return services;
        }
        #endregion

        #region Bot Platform Services
        private static IServiceCollection AddBotPlatformServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IBotSecretGenerator, BotSecretGenerator>();

            // Register keyed services for each platform
            services.AddHttpClient<TelegramBotService>();
            services.AddKeyedScoped<IBotPlatformService, TelegramBotService>(BotPlatform.TELEGRAM.AsKey());

            services.Configure<WebhookOptions>(configuration.GetSection("WebhookOptions"));

            // Future platforms can be added here:
            // services.AddHttpClient<WhatsAppBotService>();
            // services.AddKeyedScoped<IBotPlatformService, WhatsAppBotService>("whatsapp");

            return services;
        }
        #endregion

        #region AI Services
        private static IServiceCollection AddSemanticKernelPlugins(
        this IServiceCollection services)
        {
            // var assembly = typeof(IPlugin).Assembly;

            // var pluginTypes = assembly.GetTypes()
            //     .Where(t => typeof(IPlugin).IsAssignableFrom(t) && !t.IsInterface && !t.IsAbstract);

            // foreach (var type in pluginTypes)
            // {
            //     services.AddKeyedScoped(typeof(IPlugin), type.Name, type);
            // }
            services.AddKeyedTransient<IPlugin,FreePlugins>("FreePlugins");
            services.AddKeyedTransient<IPlugin,ProPlugins>("ProPlugins");

            return services;
        }
        private static IServiceCollection AddAIServices(this IServiceCollection services)
        {
            // 1. Setup Logging BEFORE the Kernel
            services.AddLogging(builder =>
            {
                // Console output
                builder.AddConsole();

                // Minimum level: Debug to capture detailed kernel activity
                builder.SetMinimumLevel(LogLevel.Debug);

                // Optional filters to reduce noise from other libraries
                builder.AddFilter("Microsoft.SemanticKernel", LogLevel.Information);
                builder.AddFilter("Microsoft.SemanticKernel.Connectors", LogLevel.Debug);
            });
            // Semantic Kernel setup
            services.AddKernel();
            services.AddSemanticKernelPlugins();
            //services.AddScoped<IAIConnectorStrategy, OpenAIConnectorStrategy>();
            services.AddScoped<IAIConnectorStrategy, GoogleConnectorStrategy>();
            services.AddScoped<IAIConnectorStrategy, GitHubConnectorStrategy>();
            services.AddScoped<IKernelExecutionService, SemanticKernelService>();

            // AI Platform Strategies
            services.AddHttpClient<GeminiStrategy>();
            services.AddHttpClient<GitHubInferenceStrategy>();

            services.AddKeyedScoped<IAIPlatformStrategy, GeminiStrategy>(
                AIPlatform.GooglePaLM.AsKey()
            );

            services.AddKeyedScoped<IAIPlatformStrategy, GitHubInferenceStrategy>(
                AIPlatform.GitHub.AsKey()
            );

            return services;
        }

        #endregion
        #region Redeis
        private static IServiceCollection AddRedis(this IServiceCollection services, IConfiguration configuration)
        {
            // Example of adding Redis caching
            services.AddStackExchangeRedisCache(options =>
            {
                string con = configuration.GetConnectionString("Redis") ?? "localhost:6379";
                options.Configuration = con;
            });


            return services;
        }
        #endregion
        #region Email Services
        private static IServiceCollection AddEmailServices(this IServiceCollection services, IConfiguration configuration)
        {
            services.AddSingleton<IEmailQueue, EmailQueue>();
            services.AddHostedService<EmailBackgroundService>();

            services.Configure<SmtpSettings>(
                configuration.GetSection("SmtpSettings"));

            services.AddTransient<IEmailSender, SmtpEmailSender>();
            return services;
        }
        #endregion
    }
}
