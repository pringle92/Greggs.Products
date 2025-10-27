using System.Collections.Generic;
using System.Linq;

namespace Greggs.Products.Api.Models.Dto
{
    /// <summary>
    /// Provides a generic wrapper for returning paginated API responses.
    /// This model includes metadata about the data set, such as the total count
    /// and page information, which is essential for client-side pagination.
    /// </summary>
    /// <typeparam name="T">The type of the items in the paged list.</typeparam>
    public class PagedResult<T>
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the collection of items for the current page.
        /// </summary>
        public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();

        /// <summary>
        /// Gets or sets the total number of records available in the full dataset.
        /// </summary>
        /// <example>8</example>
        public int TotalCount { get; set; }

        /// <summary>
        /// Gets or sets the 0-based index of the page being returned.
        /// </summary>
        /// <example>0</example>
        public int PageStart { get; set; }

        /// <summary>
        /// Gets or sets the number of items requested for the page.
        /// </summary>
        /// <example>5</example>
        public int PageSize { get; set; }

        #endregion
    }
}