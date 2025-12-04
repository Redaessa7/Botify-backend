namespace Botify.API.CrossCutting.Extensions;

public static class CurrentCorsPolicyExtensions
{
    public static CorsPolicyName CurrentCorsPolicy(this IApplicationBuilder application)
    {
        return application.ApplicationServices.GetRequiredService<CorsPolicyName>();
    }
}

/// <summary>
/// Holds the resolved CORS policy name for runtime.
/// </summary>
public record CorsPolicyName(string Name);