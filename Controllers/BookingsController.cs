using Airbnb_Clone_Api.Data;
using Airbnb_Clone_App.Dtos;
using Airbnb_Clone_Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static Airbnb_Clone_App.Dtos.UpdateBookingDto;
using System.Security.Claims;

namespace Airbnb_Clone_Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize] // Require authentication
    public class BookingsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BookingsController(AppDbContext context)
        {
            _context = context;
        }

        // ✅ GET: api/bookings (Guest sees their bookings, Host sees bookings for their listings)
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<Booking>>> GetBookings()
        {
            var userId = GetUserId();
            var userType = GetUserType();

            List<Booking> bookings;

            if (userType == "Guest")
            {
                // ✅ Guests can only see their own bookings
                bookings = await _context.Bookings
                    .Where(b => b.GuestId == userId)
                    .ToListAsync();
            }
            else if (userType == "Host")
            {
                // ✅ Hosts can see bookings for their listings
                bookings = await _context.Bookings
                    .Where(b => b.Listing.HostId == userId)
                    .Include(b => b.Listing) // Ensure Listing data is included
                    .ToListAsync();
            }
            else
            {
                return Forbid(); // Prevent unauthorized access
            }

            if (!bookings.Any())
            {
                return NotFound(new { message = "No bookings found." });
            }

            return Ok(bookings);
        }


        // ✅ GET: api/bookings/{id} (Get a single booking)
        [HttpGet("{id}")]
        public async Task<ActionResult<Booking>> GetBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            return booking;
        }

        // ✅ POST: api/bookings (Guest creates a booking request)
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<Booking>> CreateBooking([FromBody] CreateBookingDto model)
        {
            var userId = GetUserId();  // ✅ Securely fetch the User ID from JWT
            var userType = GetUserType();

            if (userType != "Guest")
            {
                return Forbid(); // Only Guests can create bookings
            }


            // 🛑 Check listing availability
            bool isAvailable = !await _context.Bookings
                .AnyAsync(b => b.ListingId == model.ListingId
                    && b.Status == BookingStatus.Confirmed
                    && model.CheckInDate < b.CheckOutDate
                    && model.CheckOutDate > b.CheckInDate);

            if (!isAvailable)
            {
                return BadRequest(new { message = "Listing is not available for the selected dates." });
            }
            // Fetch required data from DB
            var guest = await _context.Users.FindAsync(userId);
            var listing = await _context.Listings.FindAsync(model.ListingId);

            if (guest == null || listing == null)
            {
                return NotFound(new { message = "Guest or Listing not found." });
            }

            var booking = new Booking
            {
                GuestId = userId,
                ListingId = model.ListingId,
                CheckInDate = model.CheckInDate,
                CheckOutDate = model.CheckOutDate,
                Status = BookingStatus.Pending, // ✅ Use enum value instead of a string
                Guest = guest, // ✅ Set required navigation property
                Listing = listing
            };
          

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetBooking), new { id = booking.BookingId }, booking);
        }

        // ✅ PUT: api/bookings/{id}/status (Host approves or declines a booking)
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Host")]
        public async Task<IActionResult> UpdateBookingStatus(int id, [FromBody] UpdateBookingStatusDto model)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            var hostId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            // 🛑 Ensure the host owns the listing
            var listing = await _context.Listings.FindAsync(booking.ListingId);
            if (listing == null || listing.HostId != hostId)
            {
                return Forbid(); // Unauthorized action
            }

            if (!Enum.TryParse(model.Status, out BookingStatus newStatus) ||
                (newStatus != BookingStatus.Confirmed && newStatus != BookingStatus.Canceled))
            {
                return BadRequest(new { message = "Invalid status. Use 'Confirmed' or 'Canceled'." });
            }

            booking.Status = newStatus;
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // ✅ DELETE: api/bookings/{id} (Guest cancels their booking)
        [HttpDelete("{id}")]
        [Authorize(Roles = "Guest")]
        public async Task<IActionResult> CancelBooking(int id)
        {
            var booking = await _context.Bookings.FindAsync(id);
            if (booking == null)
                return NotFound();

            var userId = int.Parse(User.FindFirst("UserId")?.Value ?? "0");

            // 🛑 Ensure only the guest who made the booking can cancel it
            if (booking.GuestId != userId)
            {
                return Forbid(); // Unauthorized
            }

            booking.Status = BookingStatus.Canceled;
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
