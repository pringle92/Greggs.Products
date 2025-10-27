using Greggs.Products.Api.Services.CurrencyConversion;
using System;
using Xunit;

namespace Greggs.Products.UnitTests.Services.CurrencyConversion
{
    /// <summary>
    /// Unit tests for the <see cref="GbpConverter"/> strategy.
    /// These tests verify the simple 1:1 conversion and rounding.
    /// </summary>
    public class GbpConverterTests
    {
        #region Fields

        private readonly GbpConverter _converter;

        #endregion

        #region Constructor

        /// <summary>
        /// Initialises a new instance of the <see cref="GbpConverterTests"/> class.
        /// </summary>
        public GbpConverterTests()
        {
            _converter = new GbpConverter();
        }

        #endregion

        #region Tests

        /// <summary>
        /// Verifies that the converter's currency code is always "GBP".
        /// </summary>
        [Fact]
        public void CurrencyCode_IsAlways_GBP()
        {
            // Assert
            Assert.Equal("GBP", _converter.CurrencyCode);
        }

        /// <summary>
        /// Verifies that the convert method returns the same price
        /// for a standard 2-decimal-place input.
        /// </summary>
        [Fact]
        public void Convert_WithStandardPrice_ReturnsSamePrice()
        {
            // Arrange
            decimal basePrice = 1.23m;

            // Act
            var result = _converter.Convert(basePrice);

            // Assert
            Assert.Equal(1.23m, result);
        }

        /// <summary>
        /// Verifies that the convert method correctly rounds down
        /// a price with more than 2 decimal places.
        /// </summary>
        [Fact]
        public void Convert_WithPriceNeedingRoundingDown_RoundsCorrectly()
        {
            // Arrange
            decimal basePrice = 1.234m;

            // Act
            var result = _converter.Convert(basePrice);

            // Assert
            Assert.Equal(1.23m, result);
        }

        /// <summary>
        /// Verifies that the convert method correctly rounds up
        /// a price with more than 2 decimal places.
        /// </summary>
        [Fact]
        public void Convert_WithPriceNeedingRoundingUp_RoundsCorrectly()
        {
            // Arrange
            decimal basePrice = 1.235m;

            // Act
            var result = _converter.Convert(basePrice);

            // Assert
            // Using MidpointRounding.AwayFromZero
            Assert.Equal(1.24m, result);
        }

        /// <summary>
        /// Verifies that a price of zero is handled correctly.
        /// </summary>
        [Fact]
        public void Convert_WithZeroPrice_ReturnsZero()
        {
            // Arrange
            decimal basePrice = 0m;

            // Act
            var result = _converter.Convert(basePrice);

            // Assert
            Assert.Equal(0m, result);
        }

        #endregion
    }
}

