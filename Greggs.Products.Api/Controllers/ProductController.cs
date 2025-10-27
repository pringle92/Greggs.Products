using Greggs.Products.Api.Common;
using Greggs.Products.Api.Configuration;
using Greggs.Products.Api.Models.Dto;
using Greggs.Products.Api.Models.DTO;
using Greggs.Products.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Greggs.Products.Api.Controllers
{
    /// <summary>
    /// API controller for managing products.
    /// This controller delegates business logic
    /// to the IProductService and focusing solely on request handling and validation.
    /// </summary>
    [ApiController]
    [Route("[controller]")]
    public class ProductController : ControllerBase
    {
        #region Fields

        private readonly IProductService _productService;
        private readonly PaginationSettings _paginationSettings;
        private readonly ILogger<ProductController> _logger;

        /// <summary>
        /// A static, read-only set of supported currency codes for efficient validation.
        /// Using a HashSet for look-up performance.
        /// </summary>
        private static readonly ISet<string> _supportedCurrencies = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            CurrencyConstants.GBP,
            CurrencyConstants.EUR
        };

        #endregion

        #region Constructor

        /// <summary>
        /// Initialises a new instance of the <see cref="ProductController"/> class.
        /// Dependencies are injected via the constructor.
        /// </summary>
        /// <param name="productService">The product service implementation.</param>
        /// <param name="paginationSettings">The pagination configuration settings.</param>
        /// <param name="logger">The logger instance for this controller.</param>
        /// <exception cref="ArgumentNullException">
        /// Thrown if injected dependencies are null.
        /// </exception>
        public ProductController(
            IProductService productService,
            IOptions<PaginationSettings> paginationSettings,
            ILogger<ProductController> logger)
        {
            // Constructor guards ensure the controller cannot be instantiated
            // in an invalid state.
            ArgumentNullException.ThrowIfNull(productService);
            ArgumentNullException.ThrowIfNull(paginationSettings);
            ArgumentNullException.ThrowIfNull(paginationSettings.Value);
            ArgumentNullException.ThrowIfNull(logger);

            _productService = productService;
            _paginationSettings = paginationSettings.Value;
            _logger = logger;
        }

        #endregion

        #region Public Methods

        /// <summary>
        /// Retrieves a paginated list of products, optionally in a specified currency.
        /// </summary>
        /// <param name="pageStart">The zero-based page index. Defaults to 0.</param>
        /// <param name="pageSize">The size of the page. Defaults to 5.</param>
        /// <param name="currency">The target currency (GBP or EUR). Defaults to GBP.</param>
        /// <returns>
        /// An <see cref="IActionResult"/> containing the <see cref="PagedResult{T}"/> of products
        /// or a <see cref="BadRequestObjectResult"/> if validation fails.
        /// </Treturns>
        [HttpGet]
        [ProducesResponseType(typeof(PagedResult<ProductDto>), 200)] // Success
        [ProducesResponseType(typeof(string), 400)] // Validation error
        [ProducesResponseType(500)] // Internal server error
        public async Task<IActionResult> Get([FromQuery] int pageStart = 0, [FromQuery] int pageSize = 5, [FromQuery] string currency = CurrencyConstants.GBP)
        {
            // Use structured logging to include request parameters for better context.
            using (_logger.BeginScope(new Dictionary<string, object>
            {
                [nameof(pageStart)] = pageStart,
                [nameof(pageSize)] = pageSize,
                [nameof(currency)] = currency
            }))
            {
                #region Input Validation

                if (pageStart < 0)
                {
                    const string error = "pageStart must be a non-negative number.";
                    _logger.LogWarning("Validation failed: {ValidationErrorMessage}", error);
                    return BadRequest(error);
                }

                int maxPageSize = _paginationSettings.MaxPageSize;
                if (pageSize <= 0 || pageSize > maxPageSize)
                {
                    string error = $"pageSize must be a positive number and cannot exceed {maxPageSize}.";
                    _logger.LogWarning("Validation failed: {ValidationErrorMessage} (Received: {PageSize})", error, pageSize);
                    return BadRequest(error);
                }

                var targetCurrency = currency.ToUpperInvariant();
                if (!_supportedCurrencies.Contains(targetCurrency))
                {
                    var supported = string.Join(", ", _supportedCurrencies);
                    string error = $"Unsupported currency. Supported values are: {supported}.";
                    _logger.LogWarning("Validation failed: {ValidationErrorMessage} (Received: {Currency})", error, currency);
                    return BadRequest(error);
                }
                #endregion

                #region Service Delegation

                _logger.LogInformation("Validation passed. Delegating product retrieval to service.");

                // Delegate to the product service to fetch the paginated products.
                var pagedProducts = await _productService.GetProductsAsync(pageStart, pageSize, targetCurrency);

                //  // Return the paginated products wrapped in an Ok response.
                return Ok(pagedProducts);

                #endregion
            }
        }

        #endregion
    }
}
