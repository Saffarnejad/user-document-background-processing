using UserManagementSystem.Core.Entities;

namespace UserManagementSystem.Core.Interfaces.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<User> AddAsync(User user);
        Task UpdateAsync(User user);
    }
}
