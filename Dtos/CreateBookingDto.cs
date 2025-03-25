namespace Airbnb_Clone_App.Dtos
{
    public class CreateBookingDto
    {
        public int ListingId { get; set; }
        public DateTime CheckInDate { get; set; }
        public DateTime CheckOutDate { get; set; }
    }
}
