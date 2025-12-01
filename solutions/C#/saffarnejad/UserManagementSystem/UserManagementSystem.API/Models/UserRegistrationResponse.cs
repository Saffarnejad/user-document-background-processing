namespace UserManagementSystem.API.Models
{
    public class UserRegistrationResponse
    {
        public Guid UserId { get; set; }
        public string Status { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
    }
}
