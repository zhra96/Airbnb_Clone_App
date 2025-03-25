namespace Airbnb_Clone_App.Dtos
{
    public class UpdateUserDto
    {
        public int UserId { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public string? UserType { get; set; } // Nullable to allow optional updates
    }
}
