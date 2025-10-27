using Greggs.Products.Api.Configuration;
using Greggs.Products.Api.Controllers;
using Greggs.Products.Api.Models.Dto;
using Greggs.Products.Api.Models.DTO;
using Greggs.Products.Api.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Greggs.Products.UnitTests.Controllers
{
    /// <summary>
    /// Unit tests for the <see cref="ProductController"/> class.
    /// These tests ensure all input validation logic is robust and that
    /// the controller correctly delegates valid requests to the service layer.
    /// </summary>
    public class ProductControllerTests
    {
        #region Fields

        private readonly Mock<IProductService> _mockProductService;
        private readonly Mock<IOptions<PaginationSettings>> _mockPaginationSettings;
        private readonly Mock<ILogger<ProductController>> _mockLogger;
        private readonly ProductController _controller;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialises a new instance of the <see cref="ProductControllerTests"/> class.
        /// This constructor sets up all the required mocks and the controller instance
        /// that will be tested.
        /// </summary>
        public ProductControllerTests()
        {
            // Initialise mocks for all injected dependencies
            _mockProductService = new Mock<IProductService>();
            _mockPaginationSettings = new Mock<IOptions<PaginationSettings>>();
            _mockLogger = new Mock<ILogger<ProductController>>();

            // Setup the mock PaginationSettings to return a specific max page size
            _mockPaginationSettings.Setup(ps => ps.Value)
                                   .Returns(new PaginationSettings { MaxPageSize = 100 });

            // Setup the mock ProductService to return a default (empty) async paged result
            _mockProductService.Setup(s => s.GetProductsAsync(It.IsAny<int>(), It.IsAny<int>(), It.IsAny<string>()))
                               .ReturnsAsync(new PagedResult<ProductDto>());

            // Create the controller instance to be tested, injecting the mocks
            _controller = new ProductController(
                _mockProductService.Object,
                _mockPaginationSettings.Object,
                _mockLogger.Object
            );
        }

        #endregion

        #region Tests

        #region Valid Requests

        /// <summary>
        /// Tests that a valid request returns an OK (200) status code.
        /// </summary>
        [Fact]
        public async Task Get_WithValidRequest_ReturnsOkObjectResult()
        {
            // Act
            // Await the asynchronous controller action
            var result = await _controller.Get(0, 5, "GBP");

            // Assert
            // Verify that the result is of type OkObjectResult
            Assert.IsType<OkObjectResult>(result);
        }

        /// <summary>
        /// Tests that a valid request returns the expected PagedResult<T> model.
        /// </summary>
        [Fact]
        public async Task Get_WithValidRequest_ReturnsPagedResultOfProductDto()
        {
            // Act
            var result = await _controller.Get(0, 5, "GBP");

            // Assert
            // Cast the result to check its value
            var okResult = Assert.IsType<OkObjectResult>(result);
            // Verify the value is the correct paged result type
            Assert.IsType<PagedResult<ProductDto>>(okResult.Value);
        }

        /// <summary>
        /// Tests that a valid request correctly calls the service layer with
        /// the exact parameters it received.
        /// </summary>
        [Fact]
        public async Task Get_WithValidRequest_CallsServiceWithCorrectParameters()
        {
            // Arrange
            int expectedPageStart = 10;
            int expectedPageSize = 20;
            string expectedCurrency = "EUR";

            // Act
            await _controller.Get(expectedPageStart, expectedPageSize, expectedCurrency);

            // Assert
            // Verify that the GetProductsAsync method was called exactly once
            // with the specified parameters.
            _mockProductService.Verify(
                s => s.GetProductsAsync(expectedPageStart, expectedPageSize, expectedCurrency),
                Times.Once()
            );
        }

        /// <summary>
        /// Tests that the controller normalises lowercase currency input
        /// to uppercase before passing it to the service.
        /// </summary>
        [Fact]
        public async Task Get_WithLowercaseCurrency_CallsServiceWithUppercaseCurrency()
        {
            // Act
            await _controller.Get(0, 5, "eur");

            // Assert
            // Verifies that the currency was correctly normalised to "EUR"
            // before being passed to the service.
            _mockProductService.Verify(
                s => s.GetProductsAsync(0, 5, "EUR"),
                Times.Once()
            );
        }

        #endregion

        #region Invalid Parameter Tests

        /// <summary>
        /// Tests that a negative pageStart value results in a BadRequest.
        /// </summary>
        [Fact]
        public async Task Get_WithInvalidPageStart_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Get(-1, 5, "GBP");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("pageStart must be a non-negative number.", badRequestResult.Value);
        }

        /// <summary>
        /// Tests that a pageSize of zero results in a BadRequest.
        /// </summary>
        [Fact]
        public async Task Get_WithPageSizeZero_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Get(0, 0, "GBP");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("pageSize must be a positive number", badRequestResult.Value.ToString());
        }

        /// <summary>
        /// Tests that a negative pageSize results in a BadRequest.
        /// </summary>
        [Fact]
        public async Task Get_WithNegativePageSize_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Get(0, -5, "GBP");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("pageSize must be a positive number", badRequestResult.Value.ToString());
        }

        /// <summary>
        /// Tests that a pageSize exceeding the configured maximum results in a BadRequest.
        /// </summary>
        [Fact]
        public async Task Get_WithPageSizeExceedingMax_ReturnsBadRequest()
        {
            // Arrange
            // MaxPageSize is 100 (from setup in constructor)
            int excessivePageSize = 101;

            // Act
            var result = await _controller.Get(0, excessivePageSize, "GBP");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("cannot exceed 100", badRequestResult.Value.ToString());
        }

        /// <summary>
        /// Tests that an unsupported currency code results in a BadRequest.
        /// </summary>
        [Fact]
        public async Task Get_WithUnsupportedCurrency_ReturnsBadRequest()
        {
            // Act
            var result = await _controller.Get(0, 5, "USD");

            // Assert
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Contains("Unsupported currency", badRequestResult.Value.ToString());
        }

        #endregion

        #endregion
    }
}
