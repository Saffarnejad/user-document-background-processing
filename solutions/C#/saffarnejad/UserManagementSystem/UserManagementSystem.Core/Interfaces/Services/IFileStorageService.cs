using Microsoft.AspNetCore.Http;

namespace UserManagementSystem.Core.Interfaces.Services
{
    public interface IFileStorageService
    {
        Task<string> StoreFileAsync(IFormFile file, string fileName);
        Task<string> StorePdfAsync(byte[] pdfData, string fileName);
        Task<byte[]> GetFileAsync(string fileName);
        Task DeleteFileAsync(string fileName);
        Task<bool> FileExistsAsync(string fileName);
    }
}
