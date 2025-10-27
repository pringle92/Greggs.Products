using Greggs.Products.Api.Configuration;
using Greggs.Products.Api.Services.CurrencyConversion;
using Microsoft.Extensions.Options;
using Moq;
using System;
using System.ComponentModel;
using Xunit;

namespace Greggs.Products.UnitTests.Services.CurrencyConversion
{
    /// <summary>
    /// Unit tests for the <see cref="EurConverter"/> strategy.
    /// </summary>
    public class EurConverterTests
    {
        private readonly Mock<IOptions<CurrencySettings>> _mockSettings;
        private readonly EurConverter _converter;

        public EurConverterTests()
        {
            _mockSettings = new Mock<IOptions<CurrencySettings>>();
            _mockSettings.Setup(s => s.Value)
                         .Returns(new CurrencySettings { EurExchangeRate = 1.11m });

            _converter = new EurConverter(_mockSettings.Object);
        }

        [Fact]
        public void CurrencyCode_IsAlways_EUR()
        {
            // Assert
            Assert.Equal("EUR", _converter.CurrencyCode);
        }

        [Fact]
        public void Convert_WithIntegerPrice_ConvertsCorrectly()
        {
            // Arrange
            decimal basePrice = 1.0m;
            decimal expected = 1.11m; // 1.0 * 1.11

            // Act
            var result = _converter.Convert(basePrice);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Convert_WithPriceNeedingRounding_RoundsUpCorrectly()
        {
            // Arrange
            decimal basePrice = 0.7m;
            decimal expected = 0.78m; // 0.7 * 1.11 = 0.777

            // Act
            var result = _converter.Convert(basePrice);

            // Assert
            Assert.Equal(expected, result);
        }

        [Fact]
        public void Constructor_WithNullOptions_ThrowsArgumentNullException()
        {
            // Assert
            Assert.Throws<ArgumentNullException>(() => new EurConverter(null));
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

    }
}