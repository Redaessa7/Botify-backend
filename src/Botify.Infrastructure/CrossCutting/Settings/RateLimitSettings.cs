namespace Botify.Infrastructure.CrossCutting.Settings;
/// <summary>
/// Strongly typed configuration for rate limiting policies.
/// Bound from appsettings.json: "RateLimiting"
/// </summary>
public class RateLimitSettings
{
    /// <summary>
    /// Global fallback limiter (applies if no specific policy is attached).
    /// </summary>
    public SlidingWindowOptions Global { get; set; } = new();

    /// <summary>
    /// Concurrency limiter for file operations (to prevent I/O overload).
    /// </summary>
    public ConcurrencyOptions FileConcurrency { get; set; } = new();
}

/// <summary>
/// Sliding window limiter options (rate per time window).
/// </summary>
public class SlidingWindowOptions
{
    /// <summary>
    /// Maximum number of requests allowed per window.
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Window size in seconds.
    /// </summary>
    public int WindowInSeconds { get; set; }

    /// <summary>
    /// Number of segments per window (granularity).
    /// </summary>
    public int SegmentsPerWindow { get; set; }

    /// <summary>
    /// Maximum number of queued requests when limit is reached.
    /// </summary>
    public int QueueLimit { get; set; }
}

/// <summary>
/// Concurrency limiter options (simultaneous requests).
/// </summary>
public class ConcurrencyOptions
{
    /// <summary>
    /// Maximum number of concurrent requests allowed.
    /// </summary>
    public int PermitLimit { get; set; }

    /// <summary>
    /// Maximum number of queued requests when concurrency limit is reached.
    /// </summary>
    public int QueueLimit { get; set; }
}
