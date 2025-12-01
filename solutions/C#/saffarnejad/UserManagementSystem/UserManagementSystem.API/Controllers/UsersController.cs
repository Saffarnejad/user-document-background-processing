using Hangfire;
using Microsoft.AspNetCore.Mvc;
using UserManagementSystem.API.Models;
using UserManagementSystem.BackgroundJobs.Jobs;
using UserManagementSystem.Core.Entities;
using UserManagementSystem.Core.Interfaces.Repositories;
using UserManagementSystem.Core.Interfaces.Services;

namespace UserManagementSystem.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IDocumentRepository _documentRepository;
        private readonly IBackgroundJobRepository _backgroundJobRepository;
        private readonly IFileStorageService _fileStorageService;
        private readonly IBackgroundJobClient _backgroundJobClient;
        private readonly ILogger<UsersController> _logger;

        public UsersController(
            IUserRepository userRepository,
            IDocumentRepository documentRepository,
            IBackgroundJobRepository backgroundJobRepository,
            IFileStorageService fileStorageService,
            IBackgroundJobClient backgroundJobClient,
            ILogger<UsersController> logger)
        {
            _userRepository = userRepository;
            _documentRepository = documentRepository;
            _backgroundJobRepository = backgroundJobRepository;
            _fileStorageService = fileStorageService;
            _backgroundJobClient = backgroundJobClient;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromForm] UserRegistrationRequest request)
        {
            try
            {
                _logger.LogInformation("Starting user registration for {Email}", request.Email);

                // Create user
                var user = new User
                {
                    Name = request.Name,
                    Email = request.Email
                };

                user = await _userRepository.AddAsync(user);

                // Handle document upload
                Document document = null;
                if (request.Document != null)
                {
                    var fileName = $"{user.Id}_{Guid.NewGuid()}{Path.GetExtension(request.Document.FileName)}";
                    var filePath = await _fileStorageService.StoreFileAsync(request.Document, fileName);

                    document = new Document
                    {
                        UserId = user.Id,
                        OriginalFileName = request.Document.FileName,
                        StoredFileName = fileName,
                        ContentType = request.Document.ContentType,
                        FileSize = request.Document.Length,
                        Status = DocumentStatus.Pending
                    };

                    document = await _documentRepository.AddAsync(document);
                }

                // Enqueue welcome notification job
                var welcomeJobId = Guid.NewGuid().ToString();
                var welcomeJob = new Core.Entities.BackgroundJob
                {
                    JobId = welcomeJobId,
                    Type = "WelcomeNotification",
                    Status = JobStatus.Pending,
                    MaxRetries = 2,
                    Data = System.Text.Json.JsonSerializer.Serialize(new { userId = user.Id })
                };
                await _backgroundJobRepository.AddAsync(welcomeJob);

                _backgroundJobClient.Enqueue<WelcomeNotificationJob>(j => j.Execute(user.Id, welcomeJobId));

                // Enqueue delayed document processing job (30 seconds)
                if (document != null)
                {
                    var processingJobId = Guid.NewGuid().ToString();
                    var processingJob = new Core.Entities.BackgroundJob
                    {
                        JobId = processingJobId,
                        Type = "DocumentProcessing",
                        Status = JobStatus.Pending,
                        MaxRetries = 2,
                        Data = System.Text.Json.JsonSerializer.Serialize(new { documentId = document.Id })
                    };
                    await _backgroundJobRepository.AddAsync(processingJob);

                    _backgroundJobClient.Schedule<DocumentProcessingJob>(j => j.Execute(document.Id, processingJobId), TimeSpan.FromSeconds(30));
                }

                var response = new UserRegistrationResponse
                {
                    UserId = user.Id,
                    Status = "Registered",
                    Message = "User registered successfully."
                };

                _logger.LogInformation($"User registration completed for {request.Email} with ID {user.Id}");

                return CreatedAtAction(nameof(Register), response);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error during user registration for {request.Email}");
                return StatusCode(500, new { error = "An error occurred during registration." });
            }
        }
    }
}
