using Microsoft.EntityFrameworkCore;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Domain.Enums;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Infrastructure.Data.Repositories;

public class TicketRepository : ITicketRepository
{
    private readonly AppDbContext _context;

    public TicketRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Ticket?> GetByIdAsync(Guid id)
    {
        return await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Ticket?> GetByIdWithIncludesAsync(Guid id)
    {
        return await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Technician)
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<IEnumerable<Ticket>> QueryAsync(
        UserRole role,
        Guid userId,
        TicketStatus? status = null,
        TicketPriority? priority = null,
        Guid? assignedTo = null,
        Guid? createdBy = null,
        string? search = null)
    {
        var query = _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.Subcategory)
            .Include(t => t.CreatedByUser)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Technician)
            .AsQueryable();

        // Restrict tickets based on role
        query = role switch
        {
            UserRole.Client => query.Where(t => t.CreatedByUserId == userId),
            UserRole.Technician => query.Where(t => t.TechnicianId == userId || t.AssignedToUserId == userId),
            _ => query
        };

        if (status.HasValue)
        {
            query = query.Where(t => t.Status == status.Value);
        }
        if (priority.HasValue)
        {
            query = query.Where(t => t.Priority == priority.Value);
        }
        if (assignedTo.HasValue)
        {
            query = query.Where(t => t.AssignedToUserId == assignedTo.Value);
        }
        if (createdBy.HasValue)
        {
            query = query.Where(t => t.CreatedByUserId == createdBy.Value);
        }
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(t => t.Title.Contains(search) || t.Description.Contains(search));
        }

        return await query.OrderByDescending(t => t.CreatedAt).ToListAsync();
    }

    public async Task<IEnumerable<Ticket>> GetCalendarTicketsAsync(DateTime startDate, DateTime endDate)
    {
        return await _context.Tickets
            .Include(t => t.Category)
            .Include(t => t.AssignedToUser)
            .Include(t => t.Technician)
            .Where(t => t.CreatedAt >= startDate && t.CreatedAt <= endDate)
            .OrderBy(t => t.CreatedAt)
            .ToListAsync();
    }

    public async Task<int> CountByTechnicianIdAndStatusAsync(Guid technicianId, IEnumerable<TicketStatus> statuses)
    {
        return await _context.Tickets
            .CountAsync(t => 
                t.TechnicianId == technicianId && 
                statuses.Contains(t.Status));
    }

    public async Task<IEnumerable<Ticket>> GetUnassignedTicketsAsync(DateTime? startDate = null, DateTime? endDate = null)
    {
        var query = _context.Tickets
            .Where(t => t.TechnicianId == null)
            .AsQueryable();

        if (startDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt >= startDate.Value);
        }

        if (endDate.HasValue)
        {
            query = query.Where(t => t.CreatedAt <= endDate.Value);
        }

        return await query.ToListAsync();
    }

    public async Task<Ticket> AddAsync(Ticket ticket)
    {
        await _context.Tickets.AddAsync(ticket);
        return ticket;
    }

    public Task UpdateAsync(Ticket ticket)
    {
        _context.Tickets.Update(ticket);
        return Task.CompletedTask;
    }
}
