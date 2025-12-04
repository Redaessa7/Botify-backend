using Botify.API.Validators;
using Botify.Application.Common.Interfaces;
using FluentValidation;
using FluentValidation.AspNetCore;
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
            .AddCurrentUserService()
            .AddProblemDetailsExceptionHandling()
            .AddFluentValidationServices()
            .AddCorsConfiguration(configuration, env);
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

    /// <summary>
    /// Registers JSON converters for MVC.
    /// </summary>
    public static IMvcBuilder ConfigureJsonConverters(this IMvcBuilder builder)
    {
        builder.AddJsonOptions(opts =>
        {
            opts.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });
        return builder;
    }

    private static IServiceCollection AddCorsConfiguration(
        this IServiceCollection services,
        IConfiguration configuration,
        IWebHostEnvironment env)
    {
        //
        var section = configuration.GetSection("AppSettings");
        var policyName = section["CorsPolicyName"]
                         ?? (env.IsDevelopment() ? "DevCors" : "ProdCorsPolicy");

        var allowedOrigins = section
            .GetSection("AllowedOrigins")
            .Get<string[]>() ?? Array.Empty<string>();

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

    /// <summary>
    /// Configures the middleware pipeline.
    /// </summary>
    public static IApplicationBuilder UseCoreMiddlewares(
        this IApplicationBuilder app,
        IConfiguration configuration)
    {
        app.UseExceptionHandler();
        app.UseHttpsRedirection();
        app.UseStaticFiles();


        var policyName = app.ApplicationServices
            .GetRequiredService<CorsPolicyName>().Name;
        app.UseCors(policyName);

        app.UseAuthentication();
        app.UseAuthorization();

        return app;
    }
}

/// <summary>
/// Holds the resolved CORS policy name for runtime.
/// </summary>
public record CorsPolicyName(string Name);