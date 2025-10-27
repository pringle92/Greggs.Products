using System.ComponentModel.DataAnnotations;

namespace Greggs.Products.Api.Models.DTO
{
    /// <summary>
    /// Represents a product data transfer object (DTO) for API responses.
    /// This model defines the structure of product data sent to clients,
    /// allowing for flexibility in currency and presentation independent
    /// of the internal domain model.
    /// </summary>
    public class ProductDto
    {
        #region Public Properties

        /// <summary>
        /// Gets or sets the name of the product.
        /// </summary>
        /// <example>Sausage Roll</example>
        [Required] // Ensures the name is always present in the response.
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the price of the product in the specified currency.
        /// </summary>
        /// <example>1.11</example>
        [Required]
        [Range(0.01, (double)decimal.MaxValue, ErrorMessage = "Price must be greater than zero.")]
        public decimal Price { get; set; }

        /// <summary>
        /// Gets or sets the currency code for the price.
        /// </summary>
        /// <example>EUR</example>
        [Required]
        [StringLength(3, MinimumLength = 3, ErrorMessage = "Currency must be a 3-letter ISO code.")]
        public string Currency { get; set; }

        #endregion
    }
}
