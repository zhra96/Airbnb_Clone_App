namespace Airbnb_Clone_App.Dtos
{
    public class BookingDto
    {
        public int BookingId { get; set; }
        public int GuestId { get; set; }
        public int ListingId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
        public string Status { get; set; } // ✅ Return Status as string

        // ✅ Include Guest and Listing details
        public UserDto? Guest { get; set; }
        public ListingDto? Listing { get; set; }
    }

}

