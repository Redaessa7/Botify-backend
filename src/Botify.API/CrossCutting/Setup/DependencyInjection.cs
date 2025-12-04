using Botify.API.CrossCutting.Constants;
using Botify.API.CrossCutting.Extensions;
using Botify.API.CrossCutting.Filters;
using Botify.API.Validators;
using Botify.Application.CrossCutting.Interfaces;
using Botify.Application.CrossCutting.Options;
using FluentValidation;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Options;
using System.Text.Json.Serialization;
namespace Microsoft.Extensions.DependencyInjection;

internal static class DependencyInjection
{
    /// <summary>
    /// Registers all Presentation (API) layer services.
    /// </summary>
    public static IServiceCollection AddPresentationServices(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        return services
            .AddOptionsConfiguration(configuration)
            .AddControllersServices()
            .AddCurrentUserService()
            .AddProblemDetailsExceptionHandling()
            .AddFluentValidationServices()
            .AddCorsConfiguration(env);
    }

    private static IServiceCollection AddControllersServices(this IServiceCollection services)
    {
        services.AddControllers(options =>
        {
            options.Filters.Add<EnvelopeResultFilter>();
        }).AddJsonOptions(options =>
        {
            options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }

    private static IServiceCollection AddCurrentUserService(this IServiceCollection services)
    {
        services.AddScoped<IUser, CurrentUser>();
        services.AddHttpContextAccessor();
        return services;
    }

    private static IServiceCollection AddProblemDetailsExceptionHandling(this IServiceCollection services)
    {
        services.AddProblemDetails(options =>
        {
            options.CustomizeProblemDetails = context =>
            {
                context.ProblemDetails.Instance = $"{context.HttpContext.Request.Method} {context.HttpContext.Request.Path}";
                context.ProblemDetails.Extensions[LoggingConstants.TraceId] = context.HttpContext.TraceIdentifier;
            };
        });

        return services;
    }

    private static IServiceCollection AddFluentValidationServices(this IServiceCollection services)
    {
        services.AddFluentValidationAutoValidation();
        services.AddValidatorsFromAssemblyContaining<RegisterRequestValidator>();
        return services;
    }

    private static IServiceCollection AddOptionsConfiguration(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtConstants.OptionsSection));
        services.Configure<CorsOptions>(configuration.GetSection(CorsConstants.OptionsSection));
        return services;
    }

    private static IServiceCollection AddCorsConfiguration(this IServiceCollection services,IWebHostEnvironment env)
    {
        using var sp = services.BuildServiceProvider();
        var corsOptions = sp.GetRequiredService<IOptions<CorsOptions>>().Value;

        var policyName = corsOptions.PolicyName ??
                (env.IsDevelopment() ? CorsConstants.DevPolicy : CorsConstants.ProdPolicy);
        var allowedOrigins = corsOptions.AllowedOrigins ?? Array.Empty<string>();


        services.AddCors(options =>
        {
            options.AddPolicy(policyName, b =>
            {
                if (env.IsDevelopment())
                {
                    b.WithOrigins(allowedOrigins)
                     .AllowAnyHeader()
                     .AllowAnyMethod()
                     .AllowCredentials();
                }
                else
                {
                    b.WithOrigins(allowedOrigins)
                     .AllowAnyHeader()
                     .WithMethods(HttpMethods.Get, HttpMethods.Post, HttpMethods.Put, HttpMethods.Delete)
                     .AllowCredentials()
                     .SetPreflightMaxAge(TimeSpan.FromHours(1));
                }
            });
        });

        services.AddSingleton(new CorsPolicyName(policyName));
        return services;
    }

}