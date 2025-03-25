namespace Airbnb_Clone_App.Dtos
{
    public class UpdateBookingDto
    {
        public class UpdateBookingStatusDto
        {
            public string Status { get; set; } = "Pending"; // "Confirmed" or "Canceled"
        }
    }
}
