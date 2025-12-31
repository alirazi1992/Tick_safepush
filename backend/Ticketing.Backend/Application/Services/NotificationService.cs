using Ticketing.Backend.Application.DTOs;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;

namespace Ticketing.Backend.Application.Services;

public interface INotificationService
{
    Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid userId);
    Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId);
    Task CreateNotificationAsync(Guid userId, string message);
}

public class NotificationService : INotificationService
{
    private readonly INotificationRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public NotificationService(INotificationRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<NotificationDto>> GetNotificationsAsync(Guid userId)
    {
        var notifications = await _repository.GetByUserIdAsync(userId);
        return notifications.Select(n => new NotificationDto
        {
            Id = n.Id,
            Message = n.Message,
            IsRead = n.IsRead,
            CreatedAt = n.CreatedAt
        });
    }

    public async Task<bool> MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var notification = await _repository.GetByIdAndUserIdAsync(notificationId, userId);
        if (notification == null)
        {
            return false;
        }

        notification.IsRead = true;
        await _repository.UpdateAsync(notification);
        await _unitOfWork.SaveChangesAsync();
        return true;
    }

    public async Task CreateNotificationAsync(Guid userId, string message)
    {
        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Message = message,
            IsRead = false,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddAsync(notification);
        await _unitOfWork.SaveChangesAsync();
    }
}
