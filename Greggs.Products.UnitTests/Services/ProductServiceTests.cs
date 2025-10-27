using Greggs.Products.Api.DataAccess;
using Greggs.Products.Api.Models;
using Greggs.Products.Api.Models.Dto;
using Greggs.Products.Api.Services;
using Greggs.Products.Api.Services.CurrencyConversion;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Greggs.Products.UnitTests.Services
{
    /// <summary>
    /// Unit tests for the <see cref="ProductService"/> class.
    /// These tests use a <see cref="Mock{IMemoryCache}"/> for isolation.
    /// </summary>
    public class ProductServiceTests
    {
        #region Fields

        // Mocks for dependencies
        private readonly Mock<IDataAccess<Product>> _mockDataAccess;
        private readonly Mock<ILogger<ProductService>> _mockLogger;
        private readonly Mock<ICurrencyConverter> _mockGbpConverter;
        private readonly Mock<ICurrencyConverter> _mockEurConverter;
        private readonly IEnumerable<ICurrencyConverter> _converters;
        private readonly Mock<IMemoryCache> _mockCache;
        private readonly ProductService _productService; 

        // Data to be returned by the mock data access
        private readonly (IEnumerable<Product> Products, int TotalCount) _dataAccessResult;
        private readonly List<Product> _sampleProducts;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialises the mocks that are shared across all tests.
        /// </summary>
        public ProductServiceTests()
        {
            _mockDataAccess = new Mock<IDataAccess<Product>>();
            _mockLogger = new Mock<ILogger<ProductService>>();
            _mockGbpConverter = new Mock<ICurrencyConverter>();
            _mockEurConverter = new Mock<ICurrencyConverter>();
            _mockCache = new Mock<IMemoryCache>(); // Instantiate the mock cache

            _sampleProducts = new List<Product>
            {
                new() { Name = "Sausage Roll", PriceInPounds = 1.0m }
            };

            _dataAccessResult = (_sampleProducts, _sampleProducts.Count);

            // Setup Data Access
            _mockDataAccess.Setup(da => da.ListAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                           .ReturnsAsync(_dataAccessResult);

            // Setup Converters
            _mockGbpConverter.Setup(c => c.CurrencyCode).Returns("GBP");
            _mockGbpConverter.Setup(c => c.Convert(It.IsAny<decimal>())).Returns((decimal d) => Math.Round(d, 2)); // 1:1

            _mockEurConverter.Setup(c => c.CurrencyCode).Returns("EUR");
            _mockEurConverter.Setup(c => c.Convert(It.IsAny<decimal>())).Returns((decimal d) => Math.Round(d * 1.11m, 2));

            _converters = new[] { _mockGbpConverter.Object, _mockEurConverter.Object };

            // Instantiate the service once for all tests
            _productService = new ProductService(
                _mockDataAccess.Object,
                _converters,
                _mockCache.Object,
                _mockLogger.Object
            );
        }

        #endregion

        #region Cache Mock Helpers

        /// <summary>
        /// Sets up the IMemoryCache mock to simulate a "cache miss".
        /// This ensures the service's factory logic is executed.
        /// </summary>
        private void SetupCacheMiss()
        {
            // 1. Setup TryGetValue to return false (cache miss)
            object ignored;
            _mockCache.Setup(m => m.TryGetValue(It.IsAny<object>(), out ignored))
                      .Returns(false);

            // 2. Setup CreateEntry to return a mock ICacheEntry
            var mockCacheEntry = new Mock<ICacheEntry>();
            _mockCache.Setup(m => m.CreateEntry(It.IsAny<object>()))
                      .Returns(mockCacheEntry.Object);

            // 3. Mock the Value property (get and set) to allow the
            // GetOrCreateAsync extension method to store and retrieve the factory result.
            object cacheValue = null;
            mockCacheEntry.SetupSet(e => e.Value = It.IsAny<object>())
                          .Callback<object>(v => cacheValue = v); // Store the value from the factory
            mockCacheEntry.SetupGet(e => e.Value)
                          .Returns(() => cacheValue); // Return the stored value
        }

        /// <summary>
        /// Sets up the IMemoryCache mock to simulate a "cache hit".
        /// </summary>
        /// <param name="cachedResult">The data to return from the cache.</param>
        private void SetupCacheHit((IEnumerable<Product> Products, int TotalCount) cachedResult)
        {
            // Setup TryGetValue to return true and set the out parameter
            // to the desired cached value.
            object cacheHitValue = cachedResult;
            _mockCache.Setup(m => m.TryGetValue(It.IsAny<object>(), out cacheHitValue))
                      .Returns(true);
        }

        #endregion


        #region Tests

        /// <summary>
        /// Verifies that when the cache is empty (a miss),
        /// the service correctly calls the data access layer.
        /// </summary>
        [Fact]
        public async Task GetProductsAsync_WhenCacheMiss_CallsDataAccess()
        {
            // Arrange
            SetupCacheMiss(); // Use the cache miss helper

            // Act
            await _productService.GetProductsAsync(0, 5, "GBP");

            // Assert
            _mockDataAccess.Verify(da => da.ListAsync(0, 5), Times.Once());
            _mockCache.Verify(c => c.CreateEntry(It.IsAny<object>()), Times.Once());
        }

        /// <summary>
        /// Verifies that when the cache contains data (a hit),
        /// the service returns the cached data and does *not*
        /// call the data access layer.
        /// </summary>
        [Fact]
        public async Task GetProductsAsync_WhenCacheHit_SkipsDataAccess()
        {
            // Arrange
            // Create a distinct cache result to prove it's being used
            var cachedProducts = new List<Product> { new() { Name = "Cached Roll", PriceInPounds = 2.0m } };
            (IEnumerable<Product> Products, int TotalCount) cacheHitResult = (cachedProducts, 1);

            SetupCacheHit(cacheHitResult); // Use the cache hit helper

            // Act
            var result = await _productService.GetProductsAsync(0, 5, "GBP");

            // Assert
            _mockDataAccess.Verify(da => da.ListAsync(It.IsAny<int?>(), It.IsAny<int?>()), Times.Never());
            Assert.Equal("Cached Roll", result.Items.First().Name);
        }

        /// <summary>
        /// Verifies that a request for GBP currency
        /// correctly invokes the GbpConverter strategy.
        /// </summary>
        [Fact]
        public async Task GetProductsAsync_WithGBP_CallsGbpConverter()
        {
            // Arrange
            SetupCacheMiss();

            // Act
            var result = await _productService.GetProductsAsync(0, 5, "GBP");

            // Assert
            // We must check the result first to force the LINQ .Select() to execute.
            Assert.Equal(1.0m, result.Items.First().Price);

            // Now we can verify the mock was called as expected.
            _mockGbpConverter.Verify(c => c.Convert(1.0m), Times.Once());
            _mockEurConverter.Verify(c => c.Convert(It.IsAny<decimal>()), Times.Never());
        }

        /// <summary>
        /// Verifies that a request for EUR currency
        /// correctly invokes the EurConverter strategy.
        /// </summary>
        [Fact]
        public async Task GetProductsAsync_WithEUR_CallsEurConverter()
        {
            // Arrange
            SetupCacheMiss();

            // Act
            var result = await _productService.GetProductsAsync(0, 5, "EUR");

            // Assert
            // We must check the result first to force the LINQ .Select() to execute.
            Assert.Equal(1.11m, result.Items.First().Price);

            // Now we can verify the mock was called as expected.
            _mockEurConverter.Verify(c => c.Convert(1.0m), Times.Once());
            _mockGbpConverter.Verify(c => c.Convert(It.IsAny<decimal>()), Times.Never());
        }

        /// <summary>
        /// Verifies that the returned PagedResult object contains
        /// the correct pagination metadata.
        /// </summary>
        [Fact]
        public async Task GetProductsAsync_Always_ReturnsCorrectPagedMetadata()
        {
            // Arrange
            SetupCacheMiss();

            // Act
            var result = await _productService.GetProductsAsync(0, 5, "GBP");

            // Assert
            Assert.Equal(_sampleProducts.Count, result.TotalCount);
            Assert.Equal(0, result.PageStart);
            Assert.Equal(5, result.PageSize);
            Assert.Single(result.Items);
        }

        /// <summary>
        /// Verifies that if the data access layer returns an empty list,
        /// the service returns a correct, empty PagedResult.
        /// </summary>
        [Fact]
        public async Task GetProductsAsync_WhenDataAccessReturnsEmpty_ReturnsEmptyPagedResult()
        {
            // Arrange
            // 1. Setup data access to return an empty list and 0 count
            var emptyResult = (new List<Product>(), 0);
            _mockDataAccess.Setup(da => da.ListAsync(It.IsAny<int?>(), It.IsAny<int?>()))
                           .ReturnsAsync(emptyResult);

            // 2. Setup a cache miss so the data access layer is called
            SetupCacheMiss();

            // Act
            var result = await _productService.GetProductsAsync(0, 5, "GBP");

            // Assert
            // 1. Verify the result is not null
            Assert.NotNull(result);

            // 2. Verify the items list is empty
            Assert.NotNull(result.Items);
            Assert.Empty(result.Items);

            // 3. Verify the metadata is correct
            Assert.Equal(0, result.TotalCount);
            Assert.Equal(0, result.PageStart);
            Assert.Equal(5, result.PageSize);
        }

        #endregion
    }
}

