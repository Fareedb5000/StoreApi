using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using InventoryItem.Models;
using StoreAccount.Models;

namespace InventoryItem.Controllers
{
    public class TransactionRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; }
    }

    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class TransactionController : ControllerBase
    {
        private readonly InventoryItems _inventoryContext;
        private readonly AccountStore _accountContext;

        public TransactionController(InventoryItems inventoryContext, AccountStore accountContext)
        {
            _inventoryContext = inventoryContext;
            _accountContext = accountContext;
        }

        [HttpPost("buy")]
        public async Task<IActionResult> BuyStock([FromBody] TransactionRequest request)
        {
            var userId = GetCurrentUserId();

            var product = await _inventoryContext.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.UserId == userId);
            if (product == null) return NotFound("Product not found");

            decimal totalCost = product.CostPrice * request.Quantity;

            // Fetch account specifically tied to the authenticated user
            var account = await _accountContext.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null || account.Balance < totalCost)
            {
                return BadRequest("Insufficient funds or account not found");
            }

            account.Balance -= totalCost;
            product.Quantity += request.Quantity;

            await _accountContext.SaveChangesAsync();
            await _inventoryContext.SaveChangesAsync();

            return Ok(new { message = "Stock purchased successfully", newBalance = account.Balance, updatedQuantity = product.Quantity });
        }

        [HttpPost("sell")]
        public async Task<IActionResult> SellStock([FromBody] TransactionRequest request)
        {
            var userId = GetCurrentUserId();

            var product = await _inventoryContext.Products
                .FirstOrDefaultAsync(p => p.Id == request.ProductId && p.UserId == userId);
            if (product == null) return NotFound("Product not found");

            if (product.Quantity < request.Quantity)
            {
                return BadRequest("Not enough stock available");
            }

            decimal totalRevenue = product.SellingPrice * request.Quantity;

            var account = await _accountContext.Accounts
                .FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
            {
                // Initialize AccountRecord with the required UserName and UserId
                account = new AccountRecord 
                { 
                    UserName = User.FindFirstValue(ClaimTypes.Name) ?? "User",
                    Balance = 0,
                    UserId = userId
                };
                _accountContext.Accounts.Add(account);
            }

            account.Balance += totalRevenue;
            product.Quantity -= request.Quantity;

            await _accountContext.SaveChangesAsync();
            await _inventoryContext.SaveChangesAsync();

            return Ok(new { message = "Stock sold successfully", newBalance = account.Balance, remainingQuantity = product.Quantity });
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
    }
}