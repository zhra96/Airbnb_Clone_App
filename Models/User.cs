using System.Text.Json.Serialization;

namespace Airbnb_Clone_Api.Models
{
    public class User
    {
        public int UserId { get; set; }
        public required string  FirstName { get; set; }
        public required string LastName { get; set; }
        public required string Username { get; set; }  // Ensure this exists
        
        public required string Email { get; set; }

        [JsonIgnore] // ✅ Exclude from JSON response
        public  string? PasswordHash { get; set; }
        public required string UserType { get; set; } // Guest or Host
    }
}
