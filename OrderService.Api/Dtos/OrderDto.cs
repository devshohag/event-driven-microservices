namespace OrderService.Api.Dtos
{
    public record CreateOrderDto(string CustomerName);
    public record OrderResponseDto(int Id, string CustomerName, string Status, DateTime CreatedAt);
}
