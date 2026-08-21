using MassTransit;
using Shared.Contracts.Events;

namespace NotificationService.Api.Consumers
{
    /// <summary>
    /// Reacts to OrderCreated events published by OrderService.Api and simulates
    /// sending a customer notification (e.g. an order confirmation email).
    /// </summary>
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
            _logger.LogInformation(
                "Notification sent to {Customer}: your order #{OrderId} was placed on {CreatedAt}.",
                msg.CustomerName, msg.OrderId, msg.CreatedAt);
            return Task.CompletedTask;
        }
    }
}
