namespace UserManagementSystem.Core.Interfaces.Services
{
    public interface IDocumentProcessingService
    {
        Task<byte[]> ConvertToPdfAsync(byte[] fileData, string contentType);
    }
}
