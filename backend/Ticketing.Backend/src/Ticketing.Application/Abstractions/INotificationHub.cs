namespace Ticketing.Application.Abstractions;

public interface INotificationHub
{
    Task SendNotificationToUserAsync(Guid userId, string message);
    Task SendNotificationToAllAsync(string message);
    Task SendToGroupAsync(string groupName, string method, object data);
}
