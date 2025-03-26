using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Airbnb_Clone_Api.Data;
using Airbnb_Clone_Api.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Airbnb_Clone_App.Dtos;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace Airbnb_Clone_Api.Controllers
{
    [Route("api/users")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly AppDbContext _context;

        public UserController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/users
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            var users = await _context.Users
                .Select(user => new UserDto
                {
                    UserId = user.UserId,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Username = user.Username,
                    Email = user.Email,
                    UserType = user.UserType
                })
                .ToListAsync();

            if (!users.Any())
            {
                return NotFound(new { message = "No users found." });
            }

            return Ok(users);
        }


        // GET: api/users/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<object>> GetUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

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
               
                user = userDto
            });
        }

        [HttpPut("{id}")]
        [Authorize] // Ensures only authenticated users can update profiles
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            var requestingUserId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");
            var requestingUserRole = User.FindFirst(ClaimTypes.Role)?.Value ?? "";

            //  Ensure the user can only update their own details OR must be an admin
            if (requestingUserId != id && requestingUserRole != "Admin")
            {
                return Forbid(); // 403 Forbidden
            }

            // Only update fields if provided
            if (!string.IsNullOrEmpty(updateDto.FirstName))
                user.FirstName = updateDto.FirstName;

            if (!string.IsNullOrEmpty(updateDto.LastName))
                user.LastName = updateDto.LastName;

            if (!string.IsNullOrEmpty(updateDto.Username))
                user.Username = updateDto.Username;

            if (!string.IsNullOrEmpty(updateDto.Email))
                user.Email = updateDto.Email;

            // 🚨 Prevent role updates unless user is Admin
            if (!string.IsNullOrEmpty(updateDto.UserType) && requestingUserRole == "Admin")
                user.UserType = updateDto.UserType;

            await _context.SaveChangesAsync();

            // ✅ Return updated user details using DTO
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
                message = "User updated successfully",
                user = userDto
            });
        }


        // DELETE: api/users/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            _context.Users.Remove(user);
            await _context.SaveChangesAsync();

            return Ok(new { message = "User deleted successfully." });
        }
    }
}
