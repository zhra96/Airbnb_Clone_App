namespace Airbnb_Clone_Api.Models
{
    public class Review
    {
        public int ReviewId { get; set; }
        public required int UserId { get; set; }
        public required int ListingId { get; set; }
        public required int Rating { get; set; } // 1-5
        public required string Comment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation Properties
        public required User User { get; set; }
        public required Listing Listing { get; set; }
    }
}
