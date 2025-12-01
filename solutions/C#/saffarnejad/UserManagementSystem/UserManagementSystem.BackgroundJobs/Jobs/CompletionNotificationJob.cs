using Hangfire;
using Microsoft.Extensions.Logging;
using UserManagementSystem.Core.Entities;
using UserManagementSystem.Core.Interfaces.Repositories;
using UserManagementSystem.Core.Interfaces.Services;

namespace UserManagementSystem.BackgroundJobs.Jobs
{
    public class CompletionNotificationJob
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IUserRepository _userRepository;
        private readonly INotificationService _notificationService;
        private readonly IBackgroundJobRepository _backgroundJobRepository;
        private readonly ILogger<CompletionNotificationJob> _logger;

        public CompletionNotificationJob(
            IDocumentRepository documentRepository,
            IUserRepository userRepository,
            INotificationService notificationService,
            IBackgroundJobRepository backgroundJobRepository,
            ILogger<CompletionNotificationJob> logger)
        {
            _documentRepository = documentRepository;
            _userRepository = userRepository;
            _notificationService = notificationService;
            _backgroundJobRepository = backgroundJobRepository;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 300, 600 })] // 5 min, 10 min
        public async Task Execute(Guid documentId, string jobId)
        {
            try
            {
                _logger.LogInformation($"Starting completion notification job {jobId} for document {documentId}");

                var job = await _backgroundJobRepository.GetByJobIdAsync(jobId);
                if (job == null)
                {
                    _logger.LogWarning($"Job {jobId} not found");
                    return;
                }

                job.Status = JobStatus.Running;
                job.LastAttempt = DateTime.UtcNow;
                await _backgroundJobRepository.UpdateAsync(job);

                var document = await _documentRepository.GetByIdAsync(documentId);
                if (document == null)
                {
                    throw new Exception($"Document {documentId} not found");
                }

                var user = await _userRepository.GetByIdAsync(document.UserId);
                if (user == null)
                {
                    throw new Exception($"User {document.UserId} not found");
                }

                await _notificationService.SendProcessingCompleteNotificationAsync(user.Email, user.Name, document.OriginalFileName);

                job.Status = JobStatus.Completed;
                await _backgroundJobRepository.UpdateAsync(job);

                _logger.LogInformation($"Completion notification job {jobId} completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in completion notification job {jobId}");

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
