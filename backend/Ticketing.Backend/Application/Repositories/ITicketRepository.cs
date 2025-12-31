using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Domain.Enums;

namespace Ticketing.Backend.Application.Repositories;

public interface ITicketRepository
{
    Task<Ticket?> GetByIdAsync(Guid id);
    Task<Ticket?> GetByIdWithIncludesAsync(Guid id);
    Task<IEnumerable<Ticket>> QueryAsync(
        UserRole role,
        Guid userId,
        TicketStatus? status = null,
        TicketPriority? priority = null,
        Guid? assignedTo = null,
        Guid? createdBy = null,
        string? search = null);
    Task<IEnumerable<Ticket>> GetCalendarTicketsAsync(DateTime startDate, DateTime endDate);
    Task<int> CountByTechnicianIdAndStatusAsync(Guid technicianId, IEnumerable<TicketStatus> statuses);
    Task<IEnumerable<Ticket>> GetUnassignedTicketsAsync(DateTime? startDate = null, DateTime? endDate = null);
    Task<Ticket> AddAsync(Ticket ticket);
    Task UpdateAsync(Ticket ticket);
}

