namespace Airbnb_Clone_App.Dtos
{
    public class UpdateListingDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public decimal? Price { get; set; }
        public string? Location { get; set; }
        public bool? Availability { get; set; }
    }

}
