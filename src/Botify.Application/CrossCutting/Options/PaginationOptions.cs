namespace Botify.Application.CrossCutting.Options
{
    /// <summary>
    /// Options for cursor-based pagination.
    /// </summary>
    public sealed class PaginationOptions
    {
        /// <summary>
        /// Secret key used to sign/verify cursors (HMAC).
        /// </summary>
        public string CursorHmacSecret { get; init; }

        /// <summary>
        /// Default number of items per page when no limit is specified.
        /// </summary>
        public int DefaultLimit { get; init; } 

        /// <summary>
        /// Maximum number of items allowed per page.
        /// </summary>
        public int MaxLimit { get; init; }
    }
}
