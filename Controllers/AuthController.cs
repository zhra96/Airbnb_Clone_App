using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Airbnb_Clone_Api.Models;
using Airbnb_Clone_Api.Data;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using Airbnb_Clone_App.Dtos;

namespace Airbnb_Clone_Api.Controllers
{
    [ApiController]
    [Route("api/auth")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        //Correct Constructor: Inject both IConfiguration & AppDbContext
        public AuthController(IConfiguration configuration, AppDbContext context)
        {
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
            _context = context ?? throw new ArgumentNullException(nameof(context));
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterModel model)
        {
            if (model.Password != model.ConfirmPassword)
            {
                return BadRequest(new { message = "Passwords do not match" });
            }

            //Check if user already exists
            var existingUser = await _context.Users
                .FirstOrDefaultAsync(u => u.Username == model.Username || u.Email == model.Email);
            if (existingUser != null)
            {
                return BadRequest(new { message = "User already exists" });
            }

            //Hash the password
            string passwordHash = HashPassword(model.Password);

            //Create new user
           
        var newUser = new User
        {
            FirstName = model.FirstName,
            LastName = model.LastName,
            Username = model.Username,
            Email = model.Email,
            PasswordHash = passwordHash,
            UserType = model.UserType  // Guest or Host
        };


            _context.Users.Add(newUser);
            await _context.SaveChangesAsync();

            // ✅ Return DTO instead of full User entity
            var userDto = new UserDto
            {
                UserId = newUser.UserId,
                FirstName = newUser.FirstName,
                LastName = newUser.LastName,
                Username = newUser.Username,
                Email = newUser.Email,
                UserType = newUser.UserType
            };

            return Ok(new
            {
                message = "User registered successfully",
                user = userDto
            });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginModel model)
        {
            //Check if user exists in DB
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Username == model.Username);
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                return Unauthorized(new { message = "Invalid credentials" });
            }

            //Generate JWT Token
            var token = GenerateJwtToken(user.UserId, user.Username,user.UserType);

            //Set JWT as an HttpOnly Cookie
            Response.Cookies.Append("jwt", token, new CookieOptions
            {
                HttpOnly = true,      // Prevents JavaScript access (protects against XSS attacks)
                Secure = true,        // Use Secure flag in production (only send over HTTPS)
                SameSite = SameSiteMode.Strict, // Prevent CSRF attacks
                Expires = DateTime.UtcNow.AddHours(1) // Token expiration
            });
            var userDto = new UserDto
            {
                UserId = user.UserId,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Username = user.Username,
                Email = user.Email,
                UserType = user.UserType
            };
            return Ok(new
            {
                message = "Logged in successfully",
                token,
                user = userDto
            });
        }

        //Secure Password Hashing
        private string HashPassword(string password)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] bytes = Encoding.UTF8.GetBytes(password);
                byte[] hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }

        //Verify Hashed Password
        private bool VerifyPassword(string enteredPassword, string storedHash)
        {
            return HashPassword(enteredPassword) == storedHash;
        }

        // Generate JWT Token
        private string GenerateJwtToken(int userId, string username, string userType)
        {
            string? secretKey = _configuration["Jwt:Key"];
            string? issuer = _configuration["Jwt:Issuer"];
            string? audience = _configuration["Jwt:Audience"];

            if (string.IsNullOrEmpty(secretKey) || string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience))
            {
                throw new InvalidOperationException("JWT configuration values are missing in appsettings.json");
            }

            

            var key = Convert.FromBase64String(secretKey);

            var tokenHandler = new JwtSecurityTokenHandler();
            string role = userType.Equals("Host", StringComparison.OrdinalIgnoreCase) ? "Host" : "Guest";

            var claims = new[]
            {
        new Claim("UserId", userId.ToString()), // ✅ Include userId in JWT
        new Claim(ClaimTypes.Name, username),
        new Claim(ClaimTypes.Role, role)
    };

            var expiration = DateTime.UtcNow.AddHours(1);
            Console.WriteLine($"Generated JWT expires at: {expiration} UTC");

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = expiration,
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(key),
                    SecurityAlgorithms.HmacSha256)
            };

            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

    }
}
