namespace Ticketing.Application.Services;

public interface INotificationService
{
    Task NotifyActivityToAssignedTechniciansAsync(Guid ticketId, Guid technicianUserId, string message);
    Task NotifyTicketMessageAsync(Guid ticketId, Guid authorUserId, string message);
    Task NotifyTicketMessageAsync(Guid ticketId, Guid authorUserId, string ticketTitle, Guid? assignedToUserId, Guid createdByUserId);
    Task NotifyTicketClosedAsync(Guid ticketId, Guid userId);
    Task NotifyTicketClosedAsync(Guid ticketId, Guid userId, string ticketTitle, Ticketing.Domain.Enums.TicketStatus status);
    Task NotifyTicketAssignedAsync(Guid ticketId, Guid technicianUserId);
    Task NotifyTicketAssignedAsync(Guid ticketId, string ticketTitle, List<Guid> technicianUserIds);
    Task NotifyTicketCreatedAsync(Guid ticketId, string ticketTitle, Guid createdByUserId, int categoryId, int? subcategoryId);
}
