using System;
using System.Collections.Generic;
using System.Linq;

namespace Botify.Application.CrossCutting.Responses
{
    /// <summary>
    /// Represents a paginated result set with metadata about pagination.
    /// </summary>
    /// <typeparam name="T">The type of items in the result set.</typeparam>
    public sealed class PaginatedResult<T>
    {
        /// <summary>
        /// The items for the current page.
        /// </summary>
        public IReadOnlyList<T> Items { get; }

        /// <summary>
        /// The current page number (1-based).
        /// </summary>
        public int Page { get; }

        /// <summary>
        /// The number of items per page.
        /// </summary>
        public int Limit { get; }

        /// <summary>
        /// The total number of items across all pages.
        /// </summary>
        public int TotalCount { get; }

        /// <summary>
        /// The total number of pages.
        /// </summary>
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / Limit);

        /// <summary>
        /// Indicates whether there is a previous page.
        /// </summary>
        public bool HasPreviousPage => Page > 1;

        /// <summary>
        /// Indicates whether there is a next page.
        /// </summary>
        public bool HasNextPage => Page < TotalPages;

        public PaginatedResult(IEnumerable<T> items, int page, int limit, int totalCount)
        {
            Items = items.ToList().AsReadOnly();
            Page = page;
            Limit = limit;
            TotalCount = totalCount;
        }

        /// <summary>
        /// Creates a paginated result from a source collection.
        /// </summary>
        public static PaginatedResult<T> Create(IEnumerable<T> source, int page, int limit)
        {
            var totalCount = source.Count();
            var items = source.Skip((page - 1) * limit).Take(limit).ToList();
            return new PaginatedResult<T>(items, page, limit, totalCount);
        }
    }
}
