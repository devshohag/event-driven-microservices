using MassTransit;
using Shared.Contracts.Events;

namespace InventoryService.Api.Consumers
{
    public class OrderCreatedConsumer : IConsumer<OrderCreated>
    {
        private readonly ILogger<OrderCreatedConsumer> _logger;

        public OrderCreatedConsumer(ILogger<OrderCreatedConsumer> logger)
        {
            _logger = logger;
        }

        public Task Consume(ConsumeContext<OrderCreated> context)
        {
            var msg = context.Message;
            _logger.LogInformation("Received OrderCreated: OrderId={OrderId}, Customer={Customer}",
                msg.OrderId, msg.CustomerName);
            return Task.CompletedTask;
        }
    }
}
