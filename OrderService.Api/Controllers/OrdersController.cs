using MassTransit;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OrderService.Api.Data;
using OrderService.Api.Dtos;
using OrderService.Api.Models;
using Shared.Contracts.Events;

namespace OrderService.Api.Controllers
{
    [ApiController]
    [Route("api/orders")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderDbContext _context;
        private readonly IPublishEndpoint _publishEndpoint;
        public OrdersController(OrderDbContext context, IPublishEndpoint publishEndpoint)
        {
            _context = context;
            _publishEndpoint = publishEndpoint;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<OrderResponseDto>>> GetAll()
        {
            var orders = await _context.Orders
                .Select(o => new OrderResponseDto(o.Id, o.CustomerName, o.Status, o.CreatedAt))
                .ToListAsync();
            return Ok(orders);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<OrderResponseDto>> GetById(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order is null) return NotFound();
            return Ok(new OrderResponseDto(order.Id, order.CustomerName, order.Status, order.CreatedAt));
        }
        [HttpPost]
        public async Task<ActionResult<OrderResponseDto>> Create(CreateOrderDto dto)
        {
            var order = new Order { CustomerName = dto.CustomerName };
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();

            await _publishEndpoint.Publish(new OrderCreated(order.Id, order.CustomerName, order.CreatedAt));

            var response = new OrderResponseDto(order.Id, order.CustomerName, order.Status, order.CreatedAt);
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, response);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateOrderDto dto)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order is null) return NotFound();
            order.CustomerName = dto.CustomerName;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order is null) return NotFound();
            _context.Orders.Remove(order);
            await _context.SaveChangesAsync();
            return NoContent();
        }


    }
}
