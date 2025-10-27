using System.Collections.Generic;
using System.Threading.Tasks;

namespace Greggs.Products.Api.DataAccess
{
    /// <summary>
    /// Defines a generic contract for asynchronous data access.
    /// </summary>
    /// <typeparam name="T">The type of the entity to retrieve.</typeparam>
    public interface IDataAccess<T>
    {
        /// <summary>
        /// Asynchronously lists entities from the data source with optional pagination.
        /// </summary>
        /// <param name="pageStart">The number of records to skip (0-based).</param>
        /// <param name="pageSize">The maximum number of records to return.</param>
        /// <returns>
        /// A <see cref="Task"/> representing the asynchronous operation.
        /// The task result contains a tuple with the <see cref="IEnumerable{T}"/> of paged products
        /// and the total count of all products (pre-pagination).
        /// </returns>
        Task<(IEnumerable<T> Products, int TotalCount)> ListAsync(int? pageStart, int? pageSize);
    }
}