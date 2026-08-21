using SearchService.Api.Models;

namespace SearchService.Api.GraphQL
{
    public class Query
    {
        private static readonly List<SearchableProduct> SampleIndex = new()
        {
            new SearchableProduct { Id = 1, Name = "Wireless Mouse", QuantityAvailable = 120 },
            new SearchableProduct { Id = 2, Name = "Mechanical Keyboard", QuantityAvailable = 45 },
            new SearchableProduct { Id = 3, Name = "USB-C Hub", QuantityAvailable = 200 },
            new SearchableProduct { Id = 4, Name = "27\" Monitor", QuantityAvailable = 30 },
        };

        /// <summary>
        /// Case-insensitive search over the product index by name.
        /// Leave "term" empty/null to return the full index.
        /// </summary>
        public IEnumerable<SearchableProduct> Products(string? term)
        {
            if (string.IsNullOrWhiteSpace(term))
            {
                return SampleIndex;
            }

            return SampleIndex.Where(p =>
                p.Name.Contains(term, StringComparison.OrdinalIgnoreCase));
        }
    }
}
