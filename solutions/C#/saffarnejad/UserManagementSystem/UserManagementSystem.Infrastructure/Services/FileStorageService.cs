using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using UserManagementSystem.Core.Interfaces.Services;

namespace UserManagementSystem.Infrastructure.Services
{
    public class FileStorageService : IFileStorageService
    {
        private readonly string _storagePath;
        private readonly string _pdfStoragePath;

        public FileStorageService(IConfiguration configuration)
        {
            _storagePath = configuration["FileStorage:Path"] ?? "FileStorage/Uploads";
            _pdfStoragePath = configuration["FileStorage:PdfPath"] ?? "FileStorage/Pdfs";

            Directory.CreateDirectory(_storagePath);
            Directory.CreateDirectory(_pdfStoragePath);
        }

        public async Task DeleteFileAsync(string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }
        }

        public async Task<bool> FileExistsAsync(string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            return File.Exists(filePath);
        }

        public async Task<byte[]> GetFileAsync(string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            return await File.ReadAllBytesAsync(filePath);
        }

        public async Task<string> StoreFileAsync(IFormFile file, string fileName)
        {
            var filePath = Path.Combine(_storagePath, fileName);
            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);
            return filePath;
        }

        public async Task<string> StorePdfAsync(byte[] pdfData, string fileName)
        {
            var filePath = Path.Combine(_pdfStoragePath, fileName);
            await File.WriteAllBytesAsync(filePath, pdfData);
            return filePath;
        }
    }
}
