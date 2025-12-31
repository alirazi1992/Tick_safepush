using Ticketing.Backend.Domain.Entities;

namespace Ticketing.Backend.Application.Repositories;

public interface INotificationRepository
{
    Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId);
    Task<Notification?> GetByIdAndUserIdAsync(Guid notificationId, Guid userId);
    Task<Notification> AddAsync(Notification notification);
    Task UpdateAsync(Notification notification);
}

