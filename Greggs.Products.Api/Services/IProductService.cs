using Greggs.Products.Api.Models.Dto;
using Greggs.Products.Api.Models.DTO;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Greggs.Products.Api.Services
{
    /// <summary>
    /// Defines the contract for the product service.
    /// This service is responsible for retrieving and processing product data,
    /// including applying business logic such as currency conversions.
    /// </summary>
    public interface IProductService
    {
        /// <summary>
        /// Asynchronously retrieves a paginated list of products, converted to the specified currency.
        /// </summary>
        /// <param name="pageStart">The zero-based index of the page to retrieve.</param>
        /// <param name="pageSize">The number of items to retrieve for the page.</param>
        /// <param name="currency">The target currency code (e.g., "GBP" or "EUR") for the prices.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// The task result contains a <see cref="PagedResult{T}"/> of <see cref="ProductDto"/>
        /// with the processed product data and pagination metadata.
        /// </returns>
        Task<PagedResult<ProductDto>> GetProductsAsync(int pageStart, int pageSize, string currency);
    }
}