using Hangfire;
using Microsoft.Extensions.Logging;
using UserManagementSystem.Core.Entities;
using UserManagementSystem.Core.Interfaces.Repositories;
using UserManagementSystem.Core.Interfaces.Services;

namespace UserManagementSystem.BackgroundJobs.Jobs
{
    public class NightlyCleanupJob
    {
        private readonly IFileStorageService _fileStorageService;
        private readonly IDocumentRepository _documentRepository;
        private readonly IBackgroundJobRepository _backgroundJobRepository;
        private readonly ILogger<NightlyCleanupJob> _logger;

        public NightlyCleanupJob(
            IFileStorageService fileStorageService,
            IDocumentRepository documentRepository,
            IBackgroundJobRepository backgroundJobRepository,
            ILogger<NightlyCleanupJob> logger)
        {
            _fileStorageService = fileStorageService;
            _documentRepository = documentRepository;
            _backgroundJobRepository = backgroundJobRepository;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 300, 600 })] // 5 min, 10 min
        public async Task Execute(string jobId)
        {
            try
            {
                _logger.LogInformation($"Starting nightly cleanup job {jobId}");

                var job = await _backgroundJobRepository.GetByJobIdAsync(jobId);
                if (job == null)
                {
                    _logger.LogWarning($"Job {jobId} not found");
                    return;
                }

                job.Status = JobStatus.Running;
                job.LastAttempt = DateTime.UtcNow;
                await _backgroundJobRepository.UpdateAsync(job);

                // Find stale documents (older than 7 days and not completed)
                var threshold = DateTime.UtcNow.AddDays(-7);
                var staleDocuments = await _documentRepository.GetStaleDocumentsAsync(threshold);

                int cleanedCount = 0;
                foreach (var document in staleDocuments)
                {
                    try
                    {
                        // Delete original file
                        if (await _fileStorageService.FileExistsAsync(document.StoredFileName))
                        {
                            await _fileStorageService.DeleteFileAsync(document.StoredFileName);
                        }

                        // Delete PDF file if exists
                        if (!string.IsNullOrEmpty(document.PdfFileName) && await _fileStorageService.FileExistsAsync(document.PdfFileName))
                        {
                            await _fileStorageService.DeleteFileAsync(document.PdfFileName);
                        }

                        // Mark document for deletion or update status
                        document.Status = DocumentStatus.Failed; // Or you could delete from DB
                        await _documentRepository.UpdateAsync(document);

                        cleanedCount++;
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, $"Error cleaning up document {document.Id}");
                    }
                }

                job.Status = JobStatus.Completed;
                await _backgroundJobRepository.UpdateAsync(job);

                _logger.LogInformation($"Nightly cleanup job {jobId} completed. Cleaned {cleanedCount} documents.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in nightly cleanup job {jobId}");

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
