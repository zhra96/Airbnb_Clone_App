namespace Airbnb_Clone_Api.Models
{
    public class Payment
    {
        public int PaymentId { get; set; }
        public required int BookingId { get; set; }
        public decimal Amount { get; set; }
        public required string PaymentMethod { get; set; } // Credit Card, PayPal, etc.
        public required string Status { get; set; } // Paid, Pending, Failed
        public required DateTime TransactionDate { get; set; } = DateTime.UtcNow;

        // Navigation Property
        public required Booking Booking { get; set; }
    }
}
