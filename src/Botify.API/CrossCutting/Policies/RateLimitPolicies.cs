namespace Botify.API.CrossCutting.Policies;
/// <summary>
/// Contains constants for named rate limiting policies.
///
/// Usage:
/// - Apply on endpoints using <c>[EnableRateLimiting(RateLimitPolicies.FileConcurrency)]</c>.
/// </summary>
public static class RateLimitPolicies
{
    /// <summary>
    /// Policy that limits concurrent file operations (download/upload).
    /// </summary>
    public const string FileConcurrency = "FileConcurrencyPolicy";
}
