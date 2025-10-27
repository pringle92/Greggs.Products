using Greggs.Products.Api.Common;
using Greggs.Products.Api.Configuration;
using Microsoft.Extensions.Options;
using System;

namespace Greggs.Products.Api.Services.CurrencyConversion
{
    /// <summary>
    /// Implements the <see cref="ICurrencyConverter"/> strategy for EUR.
    /// </summary>
    public class EurConverter : ICurrencyConverter
    {
        private readonly CurrencySettings _currencySettings;

        /// <summary>
        /// Gets the currency code "EUR".
        /// </summary>
        public string CurrencyCode => CurrencyConstants.EUR;

        /// <summary>
        /// Initialises a new instance of the <see cref="EurConverter"/> class.
        /// </summary>
        /// <param name="currencySettings">The currency settings, injected via DI.</param>
        public EurConverter(IOptions<CurrencySettings> currencySettings)
        {
            ArgumentNullException.ThrowIfNull(currencySettings);
            ArgumentNullException.ThrowIfNull(currencySettings.Value);
            _currencySettings = currencySettings.Value;
        }

        /// <summary>
        /// Converts the base GBP price to EUR using the configured exchange rate.
        /// </summary>
        /// <param name="basePriceInPounds">The base price in GBP.</param>
        /// <returns>The converted price in EUR, rounded to 2 decimal places.</returns>
        public decimal Convert(decimal basePriceInPounds)
        {
            decimal convertedPrice = basePriceInPounds * _currencySettings.EurExchangeRate;
            return Math.Round(convertedPrice, 2, MidpointRounding.AwayFromZero);
        }
    }
}