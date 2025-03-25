namespace Airbnb_Clone_Api.Models
{
    public class Listing
    {
        public int ListingId { get; set; }

        public int HostId { get; set; } //Required foreign key

        public required string Title { get; set; }
        public required string Description { get; set; }
        public required decimal Price { get; set; }
        public required string Location { get; set; }
        public required bool Availability { get; set; }

        public User? Host { get; set; }  //Nullable navigation property (fixes CS9035)
    }
}

