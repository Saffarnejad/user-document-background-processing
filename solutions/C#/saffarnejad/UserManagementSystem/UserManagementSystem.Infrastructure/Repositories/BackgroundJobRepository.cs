using Microsoft.EntityFrameworkCore;
using UserManagementSystem.Core.Entities;
using UserManagementSystem.Core.Interfaces.Repositories;
using UserManagementSystem.Infrastructure.Data;

namespace UserManagementSystem.Infrastructure.Repositories
{
    public class BackgroundJobRepository : IBackgroundJobRepository
    {
        private readonly ApplicationDbContext _context;

        public BackgroundJobRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<BackgroundJob> AddAsync(BackgroundJob job)
        {
            _context.BackgroundJobs.Add(job);
            await _context.SaveChangesAsync();
            return job;
        }

        public async Task<BackgroundJob?> GetByJobIdAsync(string jobId)
        {
            return await _context.BackgroundJobs.FirstOrDefaultAsync(j => j.JobId == jobId);
        }

        public async Task<IEnumerable<BackgroundJob>> GetFailedJobsAsync()
        {
            return await _context.BackgroundJobs
                .Where(j => j.Status == JobStatus.Failed)
                .OrderBy(j => j.CreatedAt)
                .ToListAsync();
        }

        public async Task UpdateAsync(BackgroundJob job)
        {
            _context.BackgroundJobs.Update(job);
            await _context.SaveChangesAsync();
        }
    }
}
