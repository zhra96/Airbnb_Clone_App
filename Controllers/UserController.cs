using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Airbnb_Clone_Api.Data;
using Airbnb_Clone_Api.Models;
using System.Threading.Tasks;
using System.Collections.Generic;
using Airbnb_Clone_App.Dtos;

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
        public async Task<ActionResult<IEnumerable<object>>> GetUsers()
        {
            var users = await _context.Users
                .Select(user => new
                {
                    user.UserId,
                    user.FirstName,
                    user.LastName,
                    user.Username,
                    user.Email,
                    user.UserType
                })
                .ToListAsync();

            if (users.Count == 0)
            {
                return NotFound(new { message = "No users found." });
            }

            return users;
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

            return new
            {
                user.UserId,
                user.FirstName,
                user.LastName,
                user.Username,
                user.Email,
                user.UserType
            };
        }

        // PUT: api/users/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateUser(int id, UpdateUserDto updateDto)
        {
            var user = await _context.Users.FindAsync(id);
            if (user == null)
            {
                return NotFound(new { message = "User not found." });
            }

            // Only update fields that are provided in the request
            if (!string.IsNullOrEmpty(updateDto.FirstName))
                user.FirstName = updateDto.FirstName;

            if (!string.IsNullOrEmpty(updateDto.LastName))
                user.LastName = updateDto.LastName;

            if (!string.IsNullOrEmpty(updateDto.Username))
                user.Username = updateDto.Username;

            if (!string.IsNullOrEmpty(updateDto.Email))
                user.Email = updateDto.Email;

            if (!string.IsNullOrEmpty(updateDto.UserType))
                user.UserType = updateDto.UserType;

            // Ensure password is not modified
            _context.Entry(user).Property(x => x.PasswordHash).IsModified = false;

            await _context.SaveChangesAsync();

            // Return the updated user object without PasswordHash
            return Ok(new
            {
                message = "User updated successfully.",
                user = new
                {
                    user.UserId,
                    user.FirstName,
                    user.LastName,
                    user.Username,
                    user.Email,
                    user.UserType
                }
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
