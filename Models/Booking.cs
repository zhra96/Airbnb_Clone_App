namespace Airbnb_Clone_Api.Models
{
    public enum BookingStatus
    {
        Pending,
        Confirmed,
        Canceled
    }
    public class Booking
    {
        public int BookingId { get; set; }
        public required int GuestId { get; set; }
        public required int ListingId { get; set; }
        public required DateTime CheckInDate { get; set; }
        public required DateTime CheckOutDate { get; set; }
        public required BookingStatus Status { get; set; } = BookingStatus.Pending; // Default value

        // Navigation Properties
        public required User Guest { get; set; }
        public required Listing Listing { get; set; }
    }
}
