using System.ComponentModel.DataAnnotations;

namespace Greggs.Products.Api.Configuration
{
    /// <summary>
    /// Represents the configuration settings for currency conversions.
    /// </summary>
    /// <remarks>
    /// This class is designed to be bound from the application's configuration (e.g., appsettings.json)
    /// and validated at startup using Data Annotations.
    /// </remarks>
    public class CurrencySettings
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the exchange rate for converting GBP to EUR.
        /// </summary>
        /// <example>1.11</example>
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "EurExchangeRate must be a positive value greater than 0.01.")]
        public decimal EurExchangeRate { get; set; }
        

        #endregion
    }
}
