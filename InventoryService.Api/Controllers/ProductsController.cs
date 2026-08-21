using InventoryService.Api.Data;
using InventoryService.Api.Dtos;
using InventoryService.Api.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static InventoryService.Api.Dtos.ProductDtos;

namespace InventoryService.Api.Controllers
{

    [ApiController]
    [Route("api/product")]
    public class ProductsController : ControllerBase
    {
        private readonly InventoryDbContext _context;

        public ProductsController(InventoryDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProductResponseDto>>> GetAll()
        {
            var products = await _context.Products
                .Select(o => new ProductResponseDto(o.Id, o.Name, o.QuantityAvailable))
                .ToListAsync();
            return Ok(products);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ProductResponseDto>> GetById(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return NotFound();
            return Ok(new ProductResponseDto(product.Id, product.Name, product.QuantityAvailable));
        }

        [HttpPost]
        public async Task<ActionResult<ProductResponseDto>> Create(CreateProductDtos dto)
        {
            var product = new Product { Name = dto.Name,QuantityAvailable=dto.QuantityAvailable };
            _context.Products.Add(product);
            await _context.SaveChangesAsync();

            var response = new ProductResponseDto(product.Id, product.Name, product.QuantityAvailable);
            return CreatedAtAction(nameof(GetById), new { id = product.Id }, response);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, CreateProductDtos dto)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return NotFound();
            product.Name = dto.Name;
            product.QuantityAvailable = dto.QuantityAvailable;
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product is null) return NotFound();
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
