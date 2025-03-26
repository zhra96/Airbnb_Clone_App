using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Airbnb_Clone_Api.Data;
using Airbnb_Clone_Api.Models;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;
using Airbnb_Clone_App.Dtos;


namespace Airbnb_Clone_Api.Controllers
{
    [ApiController]
    [Route("api/listings")]
    public class ListingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ListingsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [AllowAnonymous] // Anyone can view listings
        public async Task<IActionResult> GetListings()
        {
            var listings = await _context.Listings
                .Include(l => l.Host)
                .Select(l => new ListingDto
                {
                    ListingId = l.ListingId,
                    Title = l.Title,
                    Description = l.Description,
                    Price = l.Price, // Auto-rounded via DTO
                    Location = l.Location,
                    Availability = l.Availability,
                    Host = new UserDto
                    {
                        UserId = l.Host.UserId,
                        FirstName = l.Host.FirstName,
                        LastName = l.Host.LastName,
                        Username = l.Host.Username,
                        Email = l.Host.Email,
                        UserType = l.Host.UserType
                    }
                })
                .ToListAsync();

            return Ok(listings);
        }


        //  Create a new listing (Only Host can access)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateListing([FromBody] Listing model)
        {
            var userId = GetUserId();
            var userType = GetUserType();

            if (userType != "Host")
            {
                return Forbid(); // Guests cannot create listings
            }

            // ✅ Fetch the Host (user) details
            var host = await _context.Users.FindAsync(userId);
            if (host == null)
            {
                return NotFound(new { message = "Host not found." });
            }

            var listing = new Listing
            {
                HostId = userId,
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Location = model.Location,
                Availability = model.Availability
            };

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

            // ✅ Create a DTO to return
            var listingDto = new ListingDto
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price, // Automatically rounded via DTO
                Location = listing.Location,
                Availability = listing.Availability,
                Host = new UserDto
                {
                    UserId = host.UserId,
                    FirstName = host.FirstName,
                    LastName = host.LastName,
                    Username = host.Username,
                    Email = host.Email,
                    UserType = host.UserType
                }
            };

            return CreatedAtAction(nameof(GetListings), new { id = listing.ListingId }, listingDto);
        }

        //  Update a listing (Only Host can access & must own the listing)
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateListing(int id, [FromBody] UpdateListingDto model)
        {
            var userId = GetUserId();
            var userType = GetUserType();

            if (userType != "Host")
            {
                return Forbid(); // Guests cannot update listings
            }

            var listing = await _context.Listings
                .Include(l => l.Host) // ✅ Ensure Host data is loaded
                .FirstOrDefaultAsync(l => l.ListingId == id);

            if (listing == null || listing.HostId != userId)
            {
                return NotFound(new { message = "Listing not found or not owned by user" });
            }

            // ✅ Apply updates only if values are provided
            if (!string.IsNullOrEmpty(model.Title)) listing.Title = model.Title;
            if (!string.IsNullOrEmpty(model.Description)) listing.Description = model.Description;
            if (!string.IsNullOrEmpty(model.Location)) listing.Location = model.Location;
            if (model.Price.HasValue) listing.Price = model.Price.Value;
            if (model.Availability.HasValue) listing.Availability = model.Availability.Value;

            await _context.SaveChangesAsync();

            // ✅ Create DTO response
            var listingDto = new ListingDto
            {
                ListingId = listing.ListingId,
                Title = listing.Title,
                Description = listing.Description,
                Price = listing.Price,
                Location = listing.Location,
                Availability = listing.Availability,
                Host = listing.Host != null
                    ? new UserDto
                    {
                        UserId = listing.Host.UserId,
                        FirstName = listing.Host.FirstName,
                        LastName = listing.Host.LastName,
                        Username = listing.Host.Username,
                        Email = listing.Host.Email,
                        UserType = listing.Host.UserType
                    }
                    : null
            };

            return Ok(new
            {
                message = "Listing updated successfully",
                listing = listingDto
            });
        }


        //  Delete a listing (Only Host can access & must own the listing)
        [HttpDelete("{id}")]
        [Authorize] // User must be authenticated
        public async Task<IActionResult> DeleteListing(int id)
        {
            var userId = GetUserId();
            var userType = GetUserType();
        
            if (userType != "Host")
            {
                return Forbid(); // Guests cannot delete listings
            }

            var listing = await _context.Listings.FindAsync(id);
            if (listing == null || listing.HostId != userId)
            {
                return NotFound(new { message = "Listing not found or not owned by user" });
            }

            _context.Listings.Remove(listing);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        //  Helper Method: Extract User ID from JWT
        private int GetUserId()
        {
            var userIdClaim = User.FindFirst("UserId"); // Match the exact claim name
            return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
        }


        //  Helper Method: Extract User Type from JWT
        private string GetUserType()
        {
            var userTypeClaim = User.FindFirst(ClaimTypes.Role);
            Console.WriteLine($"Extracted Role from JWT: {userTypeClaim?.Value}");
            return userTypeClaim?.Value ?? "Guest"; // Default to Guest if no role found
        }

    }
}
