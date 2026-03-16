using System.Security.Claims;
using System.Threading.RateLimiting;
using Botify.API.CrossCutting.Policies;
using Botify.Infrastructure.CrossCutting.Settings;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace Botify.API.CrossCutting.Extensions;
 
/// <summary>
/// Provides extension methods for configuring application-wide rate limiting.
/// 
/// Notes:
/// - Reads configuration from <c>appsettings.json</c> under the "RateLimiting" section.
/// - Registers a global fallback limiter (applies to all requests if no specific policy is attached).
/// - Registers a file concurrency policy to protect I/O heavy endpoints (like file download/upload).
/// - Configures unified rejection handling to return a <see cref="ProblemDetails"/> response with HTTP 429.
/// 
/// Usage:
/// - Call <c>builder.Services.AddAppRateLimiting(builder.Configuration)</c> in Program.cs.
/// - Apply policies on controllers/endpoints using <c>[EnableRateLimiting(RateLimitPolicies.FileConcurrency)]</c>.
/// </summary>
public static class RateLimitingExtensions
{
    /// <summary>
    /// Adds and configures rate limiting policies for the application.
    /// </summary>
    /// <param name="services">The service collection to register services into.</param>
    /// <param name="configuration">The application configuration (used to bind settings).</param>
    /// <returns>The updated <see cref="IServiceCollection"/>.</returns>
    public static IServiceCollection AddAppRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        // Bind & validate settings from appsettings.json
        // If the section is missing, throw an exception to fail fast.
        var settings = configuration.GetSection("RateLimiting").Get<RateLimitSettings>()
            ?? throw new InvalidOperationException("RateLimiting section is missing in configuration.");

        services.AddRateLimiter(options =>
        {
            //  Global fallback limiter
            // - Applies to all requests if no specific policy is attached.
            // - Protects the API from abuse by limiting requests per user/IP.
            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
            {
                var partitionKey = GetPartitionKey(httpContext);

                return RateLimitPartition.GetSlidingWindowLimiter(
                    partitionKey,
                    _ => new SlidingWindowRateLimiterOptions
                    {
                        PermitLimit = settings.Global.PermitLimit,          // Max requests per window
                        Window = TimeSpan.FromSeconds(settings.Global.WindowInSeconds), // Window size
                        SegmentsPerWindow = settings.Global.SegmentsPerWindow,          // Granularity
                        QueueLimit = settings.Global.QueueLimit,            // Allow queued requests
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    });
            });

            // FileConcurrencyPolicy
            // - Designed for file-heavy endpoints (download/upload).
            // - Prevents too many concurrent file operations that could overload I/O.
            options.AddPolicy(RateLimitPolicies.FileConcurrency, context =>
                RateLimitPartition.GetConcurrencyLimiter(
                    GetPartitionKey(context),
                    _ => new ConcurrencyLimiterOptions
                    {
                        PermitLimit = settings.FileConcurrency.PermitLimit, // Max concurrent requests
                        QueueLimit = settings.FileConcurrency.QueueLimit,   // Allow queued requests
                        QueueProcessingOrder = QueueProcessingOrder.OldestFirst
                    }));

            //  Unified rejection handling
            // - Returns HTTP 429 Too Many Requests when limits are exceeded.
            // - Provides a ProblemDetails JSON response with retry guidance.
            options.OnRejected = async (context, token) =>
            {
                var retryAfter = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var timeSpan)
                    ? ((int)timeSpan.TotalSeconds).ToString()
                    : settings.Global.WindowInSeconds.ToString();

                context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
                context.HttpContext.Response.ContentType = "application/problem+json";
                context.HttpContext.Response.Headers["Retry-After"] = retryAfter;

                var problemFactory = context.HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
                var problem = problemFactory.CreateProblemDetails(
                    context.HttpContext,
                    statusCode: StatusCodes.Status429TooManyRequests,
                    title: "Too Many Requests",
                    detail: "You have exceeded the allowed request rate. Please retry later.",
                    type: "https://httpstatuses.com/429"
                );

                await context.HttpContext.Response.WriteAsJsonAsync(problem, token);
            };
        });

        return services;
    }

    /// <summary>
    /// Determines the partition key for rate limiting.
    /// 
    /// Notes:
    /// - If the user is authenticated, use their unique identifier (NameIdentifier claim or Name).
    /// - If anonymous, fallback to the client IP address.
    /// - Ensures fair distribution of rate limits per user/IP.
    /// </summary>
    private static string GetPartitionKey(HttpContext context)
    {
        if (context.User.Identity?.IsAuthenticated == true)
            return context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value
                   ?? context.User.Identity?.Name
                   ?? "anonymous";

        return context.Connection.RemoteIpAddress?.ToString() ?? "anonymous";
    }
}
