namespace UserManagementSystem.Core.Entities
{
    public class BackgroundJob
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string JobId { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public JobStatus Status { get; set; } = JobStatus.Pending;
        public int RetryCount { get; set; }
        public int MaxRetries { get; set; } = 2;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastAttempt { get; set; }
        public DateTime? NextRetry { get; set; }
        public string? ErrorMessage { get; set; }
        public string Data { get; set; } = string.Empty;
    }

    public enum JobStatus
    {
        Pending,
        Running,
        Completed,
        Failed,
        Retrying
    }
}
