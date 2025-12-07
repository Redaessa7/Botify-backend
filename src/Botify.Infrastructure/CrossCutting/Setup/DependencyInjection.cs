using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.Features.Merchants.Interfaces;
using Botify.Application.Services.Auths;
using Botify.Domain.Models.Merchants.ValueObjects;
using Botify.Infrastructure.CrossCutting.Services.BotPlatformServices.Telegram;
using Botify.Infrastructure.CrossCutting.Services.Commons;
using Botify.Infrastructure.CrossCutting.Services.FileServices;
using Botify.Infrastructure.Identity;
using Botify.Infrastructure.Identity.Authentication;
using Botify.Infrastructure.Persistence;
using Botify.Infrastructure.Persistence.Interceptors;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.IdentityModel.Tokens;
using System.Text;
namespace Microsoft.Extensions.DependencyInjection;


public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(this IServiceCollection services, IConfiguration configuration)
    {
        // Persistence
        services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();
        services.AddDbContext<ApplicationDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseSqlite(configuration.GetConnectionString("DefaultConnection"));
        });
        services.AddScoped<IApplicationDbContext, ApplicationDbContext>();

        // Identity
        services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

        // Authentication
        services.AddAuthenticationJwtToken(configuration);
        services.AddScoped<ITokenProvider, JwtTokenProvider>();
        services.AddScoped<IAuthServices, AuthService>();

        // File services
        services.AddFileService();

        // Bot platform services
        services.AddBotsPlatformServices(configuration);

        return services;
    }

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
    private static IServiceCollection AddFileService(this IServiceCollection services)
    {
        services.AddSingleton<IFileValidator, FileValidator>();
        services.AddScoped<IFileService>(sp =>
            new LocalFileService(
                rootPath: "uploads",
                validator: sp.GetRequiredService<IFileValidator>(),
                logger: sp.GetRequiredService<ILogger<LocalFileService>>()
            )
        );
        return services;
    }
    #endregion

    #region Bot Platform Services
    private static IServiceCollection AddBotsPlatformServices(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddSingleton<IBotSecretGenerator, BotSecretGenerator>();

        // Register keyed services for each platform
        services.AddHttpClient<TelegramBotService>();
        services.AddKeyedScoped<IBotPlatformService, TelegramBotService>(BotPlatform.TELEGRAM.AsString());
        services.Configure<WebhookOptions>(configuration.GetSection("WebhookOptions"));
        // Future platforms can be added here:
        // services.AddHttpClient<WhatsAppBotService>();
        // services.AddKeyedScoped<IBotPlatformService, WhatsAppBotService>("whatsapp");

        return services;
    }
    #endregion

    #region Seed Roles
    public static async Task SeedRolesAsync(this IServiceProvider serviceProvider)
    {
        var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
        string[] roles = { "MERCHANT", "ADMIN", "USER" };

        foreach (var role in roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
            {
                await roleManager.CreateAsync(new IdentityRole(role));
            }
        }
    }
    #endregion
}
