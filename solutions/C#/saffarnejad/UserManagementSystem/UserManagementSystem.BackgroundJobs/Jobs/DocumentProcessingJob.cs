using Hangfire;
using Microsoft.Extensions.Logging;
using UserManagementSystem.Core.Entities;
using UserManagementSystem.Core.Interfaces.Repositories;
using UserManagementSystem.Core.Interfaces.Services;

namespace UserManagementSystem.BackgroundJobs.Jobs
{
    public class DocumentProcessingJob
    {
        private readonly IDocumentRepository _documentRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IDocumentProcessingService _documentProcessingService;
        private readonly IBackgroundJobRepository _backgroundJobRepository;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<DocumentProcessingJob> _logger;

        public DocumentProcessingJob(
            IDocumentRepository documentRepository,
            IFileStorageService fileStorageService,
            IDocumentProcessingService documentProcessingService,
            IBackgroundJobRepository backgroundJobRepository,
            IBackgroundJobClient backgroundJobClient,
            ILogger<DocumentProcessingJob> logger)
        {
            _documentRepository = documentRepository;
            _fileStorageService = fileStorageService;
            _documentProcessingService = documentProcessingService;
            _backgroundJobRepository = backgroundJobRepository;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        [AutomaticRetry(Attempts = 2, DelaysInSeconds = new[] { 300, 600 })] // 5 min, 10 min
        public async Task Execute(Guid documentId, string jobId)
        {
            try
            {
                _logger.LogInformation($"Starting document processing job {jobId} for document {documentId}");

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

                document.Status = DocumentStatus.Processing;
                await _documentRepository.UpdateAsync(document);

                // Read original file
                var fileData = await _fileStorageService.GetFileAsync(document.StoredFileName);

                // Convert to PDF
                var pdfData = await _documentProcessingService.ConvertToPdfAsync(fileData, document.ContentType);

                // Store PDF
                var pdfFileName = $"{Path.GetFileNameWithoutExtension(document.StoredFileName)}.pdf";
                await _fileStorageService.StorePdfAsync(pdfData, pdfFileName);

                // Update document status
                document.PdfFileName = pdfFileName;
                document.Status = DocumentStatus.Completed;
                document.ProcessedAt = DateTime.UtcNow;
                await _documentRepository.UpdateAsync(document);

                job.Status = JobStatus.Completed;
                await _backgroundJobRepository.UpdateAsync(job);

                // Enqueue completion notification
                var completionJobId = Guid.NewGuid().ToString();
                var completionJob = new Core.Entities.BackgroundJob
                {
                    JobId = completionJobId,
                    Type = "CompletionNotification",
                    Status = JobStatus.Pending,
                    MaxRetries = 2,
                    Data = System.Text.Json.JsonSerializer.Serialize(new { documentId })
                };
                await _backgroundJobRepository.AddAsync(completionJob);

                _backgroundJobClient.Enqueue<CompletionNotificationJob>(j => j.Execute(documentId, completionJobId));

                _logger.LogInformation($"Document processing job {jobId} completed successfully");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error in document processing job {jobId}");

                var job = await _backgroundJobRepository.GetByJobIdAsync(jobId);
                if (job != null)
                {
                    job.Status = JobStatus.Failed;
                    job.ErrorMessage = ex.Message;
                    job.RetryCount++;
                    await _backgroundJobRepository.UpdateAsync(job);
                }

                // Also update document status
                try
                {
                    var document = await _documentRepository.GetByIdAsync(documentId);
                    if (document != null)
                    {
                        document.Status = DocumentStatus.Failed;
                        await _documentRepository.UpdateAsync(document);
                    }
                }
                catch (Exception updateEx)
                {
                    _logger.LogError(updateEx, $"Failed to update document status for {documentId}");
                }

                throw; // Let Hangfire handle retry
            }
        }
    }
}
