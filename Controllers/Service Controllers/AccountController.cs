using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using StoreAccount.Models;

namespace StoreAccount.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : ControllerBase
    {
        private readonly AccountStore _context;

        public AccountController(AccountStore context)
        {
            _context = context;
        }

        [HttpPost("deposit")]
        public async Task<IActionResult> Deposit([FromBody] decimal amount)
        {
            if (amount <= 0) return BadRequest("Deposit amount must be greater than zero.");

            var userId = GetCurrentUserId();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null)
            {
                account = new AccountRecord 
                { 
                    UserName = User.FindFirstValue(ClaimTypes.Name) ?? "User",
                    Balance = 0,
                    UserId = userId
                };
                _context.Accounts.Add(account);
            }

            account.Balance += amount;
            await _context.SaveChangesAsync();
            return Ok(account);
        }

        [HttpPost("withdraw")]
        public async Task<IActionResult> Withdraw([FromBody] decimal amount)
        {
            if (amount <= 0) return BadRequest("Withdrawal amount must be greater than zero.");

            var userId = GetCurrentUserId();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);

            if (account == null || account.Balance < amount)
            {
                return BadRequest("Insufficient funds or account not found");
            }

            account.Balance -= amount;
            await _context.SaveChangesAsync();
            return Ok(account);
        }

        [HttpGet("balance")]
        public async Task<ActionResult<decimal>> GetBalance()
        {
            var userId = GetCurrentUserId();
            var account = await _context.Accounts.FirstOrDefaultAsync(a => a.UserId == userId);
            
            if (account == null) return Ok(0);
            return Ok(account.Balance);
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