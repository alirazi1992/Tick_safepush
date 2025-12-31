using Ticketing.Backend.Domain.Entities;

namespace Ticketing.Backend.Application.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id);
    Task<User?> GetByEmailAsync(string email);
    Task<IEnumerable<User>> GetAllAsync();
    Task<IEnumerable<User>> GetByRoleAsync(string role);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByEmailExcludingIdAsync(string email, Guid excludeId);
    Task<User> AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> AnyAsync();
}

