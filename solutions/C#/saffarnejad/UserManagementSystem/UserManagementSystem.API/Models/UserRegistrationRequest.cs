namespace UserManagementSystem.API.Models
{
    public class UserRegistrationRequest
    {
        public string Name { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IFormFile? Document { get; set; }
    }
}
