using Hangfire;
using Microsoft.Extensions.Logging;
using UserManagementSystem.Core.Entities;
using UserManagementSystem.Core.Interfaces.Repositories;
using UserManagementSystem.Core.Interfaces.Services;

namespace UserManagementSystem.BackgroundJobs.Jobs
{
    public class WelcomeNotificationJob
    {
        private readonly INotificationService _notificationService;
        private readonly IUserRepository _userRepository;
        private readonly IBackgroundJobRepository _backgroundJobRepository;
        private readonly ILogger<WelcomeNotificationJob> _logger;

        public WelcomeNotificationJob(
            INotificationService notificationService,
            IUserRepository userRepository,
            IBackgroundJobRepository backgroundJobRepository,
            ILogger<WelcomeNotificationJob> logger)
        {
            _notificationService = notificationService;
            _userRepository = userRepository;
            _backgroundJobRepository = backgroundJobRepository;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 300, 600 })] // 5 min, 10 min
        public async Task Execute(Guid userId, string jobId)
        {
            try
            {
                _logger.LogInformation($"Starting welcome notification job {jobId} for user {userId}");

                var job = await _backgroundJobRepository.GetByJobIdAsync(jobId);
                if (job == null)
                {
                    _logger.LogWarning($"Job {jobId} not found");
                    return;
                }

                job.Status = JobStatus.Running;
                job.LastAttempt = DateTime.UtcNow;
                await _backgroundJobRepository.UpdateAsync(job);

                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    throw new Exception($"User {userId} not found");
                }

                await _notificationService.SendWelcomeNotificationAsync(user.Email, user.Name);

                job.Status = JobStatus.Completed;
                await _backgroundJobRepository.UpdateAsync(job);

                _logger.LogInformation($"Welcome notification job {jobId} completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in welcome notification job {jobId}");

                var job = await _backgroundJobRepository.GetByJobIdAsync(jobId);
                if (job != null)
                {
                    job.Status = JobStatus.Failed;
                    job.ErrorMessage = ex.Message;
                    job.RetryCount++;
                    await _backgroundJobRepository.UpdateAsync(job);
                }

                throw; // Let Hangfire handle retry
            }
        }
    }
}
