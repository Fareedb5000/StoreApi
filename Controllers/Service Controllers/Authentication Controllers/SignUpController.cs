using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using UserSignUp.Models;
using static BCrypt.Net.BCrypt;

namespace UserSignUp.Controllers
{
    public class LoginRequest
    {
        public required string Email { get; set; }
        public required string Password { get; set; }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class SignUpController : ControllerBase
    {
        private readonly UserSignUpStore _context;
        private readonly IConfiguration _configuration;

        public SignUpController(UserSignUpStore context, IConfiguration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        [HttpPost]
        public async Task<IActionResult> Create([FromBody] UserSignUpRecord dto)
        {
            var existingUser = await _context.UserSignUps
                .FirstOrDefaultAsync(x => x.UserName == dto.UserName || x.Email == dto.Email);
                
            if (existingUser != null) 
            {
                return BadRequest("Username or email already exists.");
            }

            var newUser = new UserSignUpRecord
            {
                UserName = dto.UserName,
                PasswordHash = HashPassword(dto.PasswordHash), 
                Email = dto.Email,
                FirstName = dto.FirstName,
                LastName = dto.LastName
            };

            _context.UserSignUps.Add(newUser);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User registered successfully" });
        }

        [HttpPost("login")]
        public async Task<ActionResult<string>> Login([FromBody] LoginRequest request)
        {
            var user = await _context.UserSignUps
                .FirstOrDefaultAsync(u => u.Email == request.Email);

            if (user == null)
            {
                return BadRequest("Invalid email or password.");
            }

            bool isPasswordValid = Verify(request.Password, user.PasswordHash);

            if (!isPasswordValid)
            {
                return BadRequest("Invalid email or password.");
            }

            var token = GenerateJwtToken(user);
            return Ok(new { token });
        }

        private string GenerateJwtToken(UserSignUpRecord user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName),
                new Claim(ClaimTypes.Email, user.Email)
            };

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.UtcNow.AddHours(2),
                signingCredentials: credentials);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}