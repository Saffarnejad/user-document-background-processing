namespace UserManagementSystem.Core.Entities
{
    public class Document
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string OriginalFileName { get; set; } = string.Empty;
        public string StoredFileName { get; set; } = string.Empty;
        public string ContentType { get; set; } = string.Empty;
        public long FileSize { get; set; }
        public DocumentStatus Status { get; set; } = DocumentStatus.Pending;
        public string? PdfFileName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ProcessedAt { get; set; }
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
    }

    public enum DocumentStatus
    {
        Pending,
        Processing,
        Completed,
        Failed
    }
}
