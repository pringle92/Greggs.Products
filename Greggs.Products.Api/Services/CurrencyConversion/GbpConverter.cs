using Greggs.Products.Api.Common;
using System;

namespace Greggs.Products.Api.Services.CurrencyConversion
{
    /// <summary>
    /// Implements the <see cref="ICurrencyConverter"/> strategy for GBP.
    /// </summary>
    public class GbpConverter : ICurrencyConverter
    {
        /// <summary>
        /// Gets the currency code "GBP".
        /// </summary>
        public string CurrencyCode => CurrencyConstants.GBP;

        /// <summary>
        /// Converts the base GBP price to GBP (1:1 conversion).
        /// </summary>
        /// <param name="basePriceInPounds">The base price in GBP.</param>
        /// <returns>The same price, rounded to 2 decimal places.</returns>
        public decimal Convert(decimal basePriceInPounds)
        {
            // No conversion, just apply standard rounding
            return Math.Round(basePriceInPounds, 2, MidpointRounding.AwayFromZero);
        }
    }
}