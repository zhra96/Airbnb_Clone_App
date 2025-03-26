namespace Airbnb_Clone_App.Dtos
{
    public class ListingDto
    {
        public int ListingId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // ✅ Round price to 2 decimal places when getting value
        private decimal _price;
        public decimal Price
        {
            get => Math.Round(_price, 2);
            set => _price = value;
        }
        public string Location { get; set; } = string.Empty;
        public bool Availability { get; set; }

        // ✅ Include Host details
        public UserDto? Host { get; set; }
    }
}
