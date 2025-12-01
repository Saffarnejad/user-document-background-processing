using Microsoft.Extensions.Logging;
using UserManagementSystem.Core.Interfaces.Services;

namespace UserManagementSystem.Infrastructure.Services
{
    public class NotificationService : INotificationService
    {
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(ILogger<NotificationService> logger)
        {
            _logger = logger;
        }

        public async Task SendProcessingCompleteNotificationAsync(string email, string name, string documentName)
        {
            // In real implementation, this would integrate with email service.

            _logger.LogInformation($"Sending processing complete notification to {email} for document {documentName}");
            await Task.Delay(100); // Simulate network delay
            _logger.LogInformation($"Processing complete notification sent to {email}");
        }

        public async Task SendWelcomeNotificationAsync(string email, string name)
        {
            // In real implementation, this would integrate with email service.

            _logger.LogInformation($"Sending welcome notification to {email} for user {name}");
            await Task.Delay(100); // Simulate network delay
            _logger.LogInformation($"Welcome notification sent to {email}");
        }
    }
}
