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

        //  Get all listings (Guest & Host can access)
        [HttpGet]
        [AllowAnonymous] // Anyone can view listings
        public async Task<IActionResult> GetListings()
        {
            var listings = await _context.Listings.Include(l => l.Host).ToListAsync();
            return Ok(listings);
        }

        //  Create a new listing (Only Host can access)
        [HttpPost]
        [Authorize]
        public async Task<IActionResult> CreateListing([FromBody] Listing model)
        {
            // Log the Authorization header to verify if it's present in the controller
            //if (Request.Headers.TryGetValue("Authorization", out var authHeader))
            //{
            //    Console.WriteLine($"🛠 Authorization Header Received in Controller: {authHeader}");
            //}
            //else
            //{
            //    Console.WriteLine("❌ No Authorization Header Found in Controller!");
            //}

            //// 🔎 Debug: List all claims in User.Claims
            //Console.WriteLine("🔎 Listing all claims in User.Claims:");
            //foreach (var claim in User.Claims)
            //{
            //    Console.WriteLine($"➡ Claim Type: {claim.Type}, Value: {claim.Value}");
            //}

            var userId = GetUserId();
            var userType = GetUserType();
          

            if (userType != "Host")
            {
                return Forbid(); // Guests cannot create listings
            }

            // ✅ Fetch the Host (user) details from the database
            var host = await _context.Users
                .Where(u => u.UserId == userId)
                .Select(u => new
                {
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Username,
                    u.Email,
                    u.UserType
                })
                .FirstOrDefaultAsync();

            var listing = new Listing
            {
                HostId = userId, // ✅ Required field
                Title = model.Title,
                Description = model.Description,
                Price = model.Price,
                Location = model.Location,
                Availability = model.Availability,
                
            };

            _context.Listings.Add(listing);
            await _context.SaveChangesAsync();

          

            // ✅ Return the listing along with the host object
            return CreatedAtAction(nameof(GetListings), new { id = listing.ListingId }, new
            {
                listing.ListingId,
                listing.HostId,
                listing.Title,
                listing.Description,
                listing.Price,
                listing.Location,
                listing.Availability,
                Host = host // ✅ Includes host in response
            });
        }


        //  Update a listing (Only Host can access & must own the listing)
        [HttpPut("{id}")]
        [Authorize] // User must be authenticated
        public async Task<IActionResult> UpdateListing(int id, [FromBody] UpdateListingDto model)
        {
            var userId = GetUserId();
            var userType = GetUserType();


            if (userType != "Host")
            {
                return Forbid(); // Guests cannot update listings
            }

            var listing = await _context.Listings
    .Include(l => l.Host) // ✅ Ensures Host data is loaded
    .FirstOrDefaultAsync(l => l.ListingId == id);

            if (listing == null || listing.HostId != userId)
            {
                return NotFound(new { message = "Listing not found or not owned by user" });
            }

            // Apply updates only if values are provided

            if (model.Title != null) listing.Title = model.Title;
            if (model.Description != null) listing.Description = model.Description;

            if (model.Location != null) listing.Location = model.Location;

            if (model.Price is { } price) listing.Price = price;
            if (model.Availability is { } availability) listing.Availability = availability;

            await _context.SaveChangesAsync();
            return Ok(new
            {
                listing.ListingId,
                listing.HostId,
                listing.Title,
                listing.Description,
                listing.Price,
                listing.Location,
                listing.Availability,
                Host = listing.Host != null
        ? new UpdateUserDto
        {
            UserId = listing.Host.UserId,
            FirstName = listing.Host.FirstName,
            LastName = listing.Host.LastName,
            Username = listing.Host.Username,
            Email = listing.Host.Email,
            UserType = listing.Host.UserType
        }
        : null
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
