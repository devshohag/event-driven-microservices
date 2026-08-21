namespace SearchService.Api.Models
{
    /// <summary>
    /// Lightweight read-model exposed by the search index. In a full implementation this
    /// would be populated by consuming events from InventoryService.Api (e.g. ProductCreated,
    /// ProductUpdated) via MassTransit and stored in a real search engine (Elasticsearch,
    /// Postgres full-text, etc). It's seeded with sample data here to keep the demo self-contained.
    /// </summary>
    public class SearchableProduct
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal QuantityAvailable { get; set; }
    }
}
