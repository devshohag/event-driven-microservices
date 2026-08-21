namespace InventoryService.Api.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public decimal QuantityAvailable { get; set; }
    }
}
