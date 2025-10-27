using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Greggs.Products.Api.Models;

namespace Greggs.Products.Api.DataAccess
{
    /// <summary>
    /// DISCLAIMER: This is only here to help enable the purpose of this exercise, this doesn't reflect the way we work!
    /// </summary>
    public class ProductAccess : IDataAccess<Product>
    {
        private static readonly IEnumerable<Product> ProductDatabase = new List<Product>()
        {
            new() { Name = "Sausage Roll", PriceInPounds = 1m },
            new() { Name = "Vegan Sausage Roll", PriceInPounds = 1.1m },
            new() { Name = "Steak Bake", PriceInPounds = 1.2m },
            new() { Name = "Yum Yum", PriceInPounds = 0.7m },
            new() { Name = "Pink Jammie", PriceInPounds = 0.5m },
            new() { Name = "Mexican Baguette", PriceInPounds = 2.1m },
            new() { Name = "Bacon Sandwich", PriceInPounds = 1.95m },
            new() { Name = "Coca Cola", PriceInPounds = 1.2m }
        };

        /// <summary>
        /// Asynchronously lists products from the "database" with optional pagination.
        /// </summary>
        /// <param name="pageStart">The number of records to skip (0-based).</param>
        /// <param name="pageSize">The maximum number of records to return.</param>
        /// <returns>
        /// A <see cref="Task"/> containing a tuple with the <see cref="IEnumerable{T}"/> of paged products
        /// and the total count of all products.
        /// </returns>
        public Task<(IEnumerable<Product> Products, int TotalCount)> ListAsync(int? pageStart, int? pageSize)
        {
            var queryable = ProductDatabase.AsQueryable();

            // Get the total count before applying pagination
            int totalCount = queryable.Count();

            if (pageStart.HasValue)
                queryable = queryable.Skip(pageStart.Value);

            if (pageSize.HasValue)
                queryable = queryable.Take(pageSize.Value);

            var pagedProducts = queryable.ToList();

            // Wrap the synchronous result in a completed Task
            // to simulate an async I/O operation.
            return Task.FromResult((pagedProducts as IEnumerable<Product>, totalCount));
        }
    }
}