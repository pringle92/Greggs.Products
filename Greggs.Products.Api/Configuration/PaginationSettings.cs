using System.ComponentModel.DataAnnotations;

namespace Greggs.Products.Api.Configuration
{
    /// <summary>
    /// Represents the configuration settings for pagination.
    /// </summary>
    /// <remarks>
    /// This class is designed to be bound from the application's configuration (e.g., appsettings.json)
    /// and validated at startup using Data Annotations.
    /// </remarks>
    public class PaginationSettings
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the maximum allowed page size for product queries.
        /// This helps protect the API from excessively large requests.
        /// </summary>
        /// <example>100</example>
        [Range(1, 1000, ErrorMessage = "MaxPageSize must be between 1 and 1000.")]
        public int MaxPageSize { get; set; }

        #endregion
    }
}
