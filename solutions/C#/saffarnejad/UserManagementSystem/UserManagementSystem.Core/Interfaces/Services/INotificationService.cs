namespace UserManagementSystem.Core.Interfaces.Services
{
    public interface INotificationService
    {
        Task SendWelcomeNotificationAsync(string email, string name);
        Task SendProcessingCompleteNotificationAsync(string email, string name, string documentName);
    }
}
