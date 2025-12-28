using System;
using System.Collections.Generic;
using System.Linq;

namespace Botify.Application.CrossCutting.Responses
{
    /// <summary>
    /// Represents a paginated result set using cursor-based pagination.
    /// </summary>
    /// <typeparam name="T">The type of items in the result set.</typeparam>
    public sealed class PaginatedResult<T>
    {
        /// <summary>
        /// The items for the current page.
        /// </summary>
        public IReadOnlyList<T> Items { get; }

        /// <summary>
        /// The number of items requested per page.
        /// </summary>
        public int Limit { get; }

        /// <summary>
        /// The total number of items across the entire dataset.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// The opaque cursor string pointing to the next page, or null if there is no next page.
        /// </summary>
        public string? NextCursor { get; }
        public bool HasNext => !string.IsNullOrEmpty(NextCursor);
        public PaginatedResult(
            IEnumerable<T> items,
            int limit,
            int totalCount,
            string? nextCursor)
        {
            Items = items.ToList().AsReadOnly();
            Limit = limit;
            TotalCount = totalCount;
            NextCursor = nextCursor;
        }

        /// <summary>
        /// Creates a paginated result from a source collection.
        /// Note: This is only for in-memory collections; in production use EF queries with cursor filters.
        /// </summary>
        public static PaginatedResult<T> Create(
            IEnumerable<T> source,
            int limit,
            string? nextCursor)
        {
            var items = source.Take(limit).ToList();
            var totalCount = source.Count();
            return new PaginatedResult<T>(items, limit, totalCount, nextCursor);
        }
    }
}
