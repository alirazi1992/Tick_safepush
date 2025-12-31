using Microsoft.EntityFrameworkCore;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Infrastructure.Data.Repositories;

public class NotificationRepository : INotificationRepository
{
    private readonly AppDbContext _context;

    public NotificationRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Notification>> GetByUserIdAsync(Guid userId)
    {
        return await _context.Notifications
            .Where(n => n.UserId == userId)
            .OrderByDescending(n => n.CreatedAt)
            .ToListAsync();
    }

    public async Task<Notification?> GetByIdAndUserIdAsync(Guid notificationId, Guid userId)
    {
        return await _context.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);
    }

    public async Task<Notification> AddAsync(Notification notification)
    {
        await _context.Notifications.AddAsync(notification);
        return notification;
    }

    public Task UpdateAsync(Notification notification)
    {
        _context.Notifications.Update(notification);
        return Task.CompletedTask;
    }
}

