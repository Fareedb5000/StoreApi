using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using InventoryItem.Models;

[Authorize]
[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly InventoryItems _context;

    public ProductsController(InventoryItems context)
    {
        _context = context;
    }

    private int GetCurrentUserId()
    {
        var nameIdentifier = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (string.IsNullOrEmpty(nameIdentifier) || !int.TryParse(nameIdentifier, out var userId))
        {
            throw new UnauthorizedAccessException("Invalid token claims.");
        }
        return userId;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] InventoryRecord product)
    {
        product.UserId = GetCurrentUserId();
        _context.Products.Add(product);
        await _context.SaveChangesAsync();
        return CreatedAtAction(nameof(GetById), new { id = product.Id }, product);
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<InventoryRecord>>> GetAll()
    {
        var userId = GetCurrentUserId();
        var products = await _context.Products
            .Where(p => p.UserId == userId)
            .ToListAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<InventoryRecord>> GetById(int id)
    {
        var userId = GetCurrentUserId();
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, [FromBody] InventoryRecord updated)
    {
        var userId = GetCurrentUserId();
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (product == null) return NotFound();

        product.Name = updated.Name;
        product.Category = updated.Category;
        product.Quantity = updated.Quantity;
        product.CostPrice = updated.CostPrice;
        product.SellingPrice = updated.SellingPrice;

        await _context.SaveChangesAsync();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var userId = GetCurrentUserId();
        var product = await _context.Products
            .FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
        if (product == null) return NotFound();

        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
        return NoContent();
    }
}