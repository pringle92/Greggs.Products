namespace Greggs.Products.Api.Services.CurrencyConversion
{
    /// <summary>
    /// Defines the contract for a currency conversion strategy.
    /// Each implementation is responsible for a single currency.
    /// </summary>
    public interface ICurrencyConverter
    {
        /// <summary>
        /// Gets the 3-letter ISO currency code that this converter supports.
        /// </summary>
        /// <example>GBP</example>
        string CurrencyCode { get; }

        /// <summary>
        /// Converts a base price (GBP) into this converter's target currency.
        /// </summary>
        /// <param name="basePriceInPounds">The base price in GBP.</param>
        /// <returns>The converted price, rounded for currency.</returns>
        decimal Convert(decimal basePriceInPounds);
    }
}