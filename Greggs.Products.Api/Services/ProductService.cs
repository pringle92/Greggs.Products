using Greggs.Products.Api.Common;
using Greggs.Products.Api.Configuration;
using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Models.Dto;
using Greggs.Products.Api.Models.DTO;
using Greggs.Products.Api.Services.CurrencyConversion;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Greggs.Products.Api.Services
{
    /// <summary>
    /// Implements the <see cref="IProductService"/> interface.
    /// This class contains the core business logic for handling products.
    /// </summary>
    public class ProductService : IProductService
    {
        #region Fields

        /// <summary>
        /// Provides access to product data through the specified data access implementation.
        /// </summary>
        /// <remarks>This field is intended for internal use to interact with product-related data storage
        /// or retrieval operations. The specific behavior depends on the implementation of the IDataAccess<Product>
        /// interface.</remarks>
        private readonly IDataAccess<Product> _productAccess;

        /// <summary>
        /// A dictionary of all available currency converters, keyed by their currency code.
        /// </summary>
        private readonly IDictionary<string, ICurrencyConverter> _converters;

        /// <summary>
        /// A memory cache instance for caching frequently accessed data.
        /// </summary>
        private readonly IMemoryCache _cache;

        /// <summary>
        /// Logger instance for logging operations within the ProductService.
        /// </summary>
        private readonly ILogger<ProductService> _logger;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialises a new instance of the <see cref="ProductService"/> class.
        /// </summary>
        /// <param name="converters">An enumerable of all registered <see cref="ICurrencyConverter"/> strategies.</param>
        /// <param name="cache">The in-memory cache service.</param>
        /// <param name="logger">The logger service.</param>
        /// An enumerable of all registered <see cref="ICurrencyConverter"/> strategies, injected by DI.
        /// </param>
        public ProductService(IDataAccess<Product> productAccess, IEnumerable<ICurrencyConverter> converters, IMemoryCache cache, ILogger<ProductService> logger)
        {
            // Use null-checking for robust constructor guards.
            ArgumentNullException.ThrowIfNull(productAccess);
            ArgumentNullException.ThrowIfNull(converters);
            ArgumentNullException.ThrowIfNull(cache);
            ArgumentNullException.ThrowIfNull(logger);

            _productAccess = productAccess;
            _cache = cache;
            _logger = logger;

            // Build a dictionary from all injected strategies for near instant look-up.
            _converters = converters.ToDictionary(
                c => c.CurrencyCode,
                c => c,
                StringComparer.OrdinalIgnoreCase // Ensure look-up is case-insensitive
            );
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves a paginated list of products, converted to the specified currency.
        /// </summary>
        /// <param name="pageStart">The zero-based index of the page to retrieve.</param>
        /// <param name="pageSize">The number of items to retrieve for the page.</param>
        /// <param name="currency">The target currency code. Assumed to be validated by the controller.</param>
        /// <returns>
        /// A <see cref="Task"/> containing a <see cref="PagedResult{T}"/> of <see cref="ProductDto"/>.
        /// containing the processed product data.
        /// </returns>
        public async Task<PagedResult<ProductDto>> GetProductsAsync(int pageStart, int pageSize, string currency)
        {
            // 1. Create a unique cache key based on the data access parameters
            string cacheKey = $"Products:PageStart={pageStart}:PageSize={pageSize}";

            // 2. Asynchronously get or create the item in the cache
            // This ensures the ListAsync method is only called if the item
            // is not already in the cache.
            var (products, totalCount) = await _cache.GetOrCreateAsync(
                cacheKey,
                async (cacheEntry) =>
                {
                    // Set cache options (e.g., expire after 60 seconds)
                    cacheEntry.AbsoluteExpirationRelativeToNow = TimeSpan.FromSeconds(60);
                    _logger.LogInformation("Cache miss. Fetching from data access for key: {CacheKey}", cacheKey);
                    // This is the "expensive" operation we are caching
                    return await _productAccess.ListAsync(pageStart, pageSize);
                });

            // 3. Find the correct conversion strategy.
            // The controller has already validated the currency, so we can use a direct look-up.
            var converter = _converters[currency];

            // 4. Handle the case where the data source returns no items. (this can come from cache or data access)
            if (products == null || !products.Any())
            {
                return new PagedResult<ProductDto>
                {
                    Items = Enumerable.Empty<ProductDto>(),
                    TotalCount = 0,
                    PageStart = pageStart,
                    PageSize = pageSize
                };
            }


            // 5. Map the domain models to DTOs, delegating conversion to the strategy. (this part is cheap and not cached)
            var productDtos = products.Select(product => new ProductDto
            {
                Name = product.Name,
                Currency = currency,
                // The service doesn't need to know how conversion works.
                Price = converter.Convert(product.PriceInPounds)
            });

            // 6. Return the complete, metadata-rich PagedResult.
            return new PagedResult<ProductDto>
            {
                Items = productDtos,
                TotalCount = totalCount,
                PageStart = pageStart,
                PageSize = pageSize
            };
        }

        #endregion
    }
}
