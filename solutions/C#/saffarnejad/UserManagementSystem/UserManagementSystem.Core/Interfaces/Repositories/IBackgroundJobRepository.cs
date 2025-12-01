using UserManagementSystem.Core.Entities;

namespace UserManagementSystem.Core.Interfaces.Repositories
{
    public interface IBackgroundJobRepository
    {
        Task<BackgroundJob?> GetByJobIdAsync(string jobId);
        Task<BackgroundJob> AddAsync(BackgroundJob job);
        Task UpdateAsync(BackgroundJob job);
        Task<IEnumerable<BackgroundJob>> GetFailedJobsAsync();
    }
}
