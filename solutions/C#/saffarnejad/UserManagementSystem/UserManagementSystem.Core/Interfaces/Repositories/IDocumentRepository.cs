using UserManagementSystem.Core.Entities;

namespace UserManagementSystem.Core.Interfaces.Repositories
{
    public interface IDocumentRepository
    {
        Task<Document?> GetByIdAsync(Guid id);
        Task<Document> AddAsync(Document document);
        Task UpdateAsync(Document document);
        Task<IEnumerable<Document>> GetPendingDocumentsAsync();
        Task<IEnumerable<Document>> GetStaleDocumentsAsync(DateTime threshold);
    }
}
