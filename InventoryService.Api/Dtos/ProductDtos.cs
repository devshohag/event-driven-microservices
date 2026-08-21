namespace InventoryService.Api.Dtos
{
    public class ProductDtos
    {
        
        public record CreateProductDtos(string Name,decimal QuantityAvailable);
        public record ProductResponseDto(int Id, string Name, decimal QuantityAvailable);
    }
}
