using Ticketing.Domain.Entities;

namespace Ticketing.Application.Repositories;

public interface INotificationRepository
{
    Task<Notification?> GetByIdAsync(Guid id);
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<Notification> AddAsync(Notification notification);
    Task<Notification> UpdateAsync(Notification notification);
    Task<bool> DeleteAsync(Guid id);
}
