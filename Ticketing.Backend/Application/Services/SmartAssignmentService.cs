using Microsoft.EntityFrameworkCore;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Domain.Enums;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Application.Services;

public interface ISmartAssignmentService
{
    Task<Guid?> AssignTechnicianToTicketAsync(Guid ticketId);
    Task<int> AssignUnassignedTicketsAsync(DateTime? startDate = null, DateTime? endDate = null);
}

public class SmartAssignmentService : ISmartAssignmentService
{
    private readonly AppDbContext _context;
    private readonly ITechnicianService _technicianService;

    public SmartAssignmentService(AppDbContext context, ITechnicianService technicianService)
    {
        _context = context;
        _technicianService = technicianService;
    }

    /// <summary>
    /// Assigns a technician to a ticket using least-loaded active technician rule
    /// </summary>
    public async Task<Guid?> AssignTechnicianToTicketAsync(Guid ticketId)
    {
        var ticket = await _context.Tickets
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null || ticket.TechnicianId != null)
        {
            return null; // Ticket not found or already assigned
        }

        // Get all active technicians
        var allTechnicians = await _technicianService.GetAllTechniciansAsync();
        var activeTechnicians = allTechnicians.Where(t => t.IsActive).ToList();

        if (activeTechnicians.Count == 0)
        {
            return null; // No active technicians available
        }

        // Calculate load for each technician (count of open/in-progress tickets)
        var technicianLoads = new List<(Guid TechnicianId, int LoadCount)>();

        foreach (var tech in activeTechnicians)
        {
            var loadCount = await _context.Tickets
                .CountAsync(t => 
                    t.TechnicianId == tech.Id && 
                    (t.Status == TicketStatus.New || t.Status == TicketStatus.InProgress));

            technicianLoads.Add((tech.Id, loadCount));
        }

        // Select least loaded technician (tie-break by earliest created technician)
        var selectedTechnician = technicianLoads
            .OrderBy(t => t.LoadCount)
            .ThenBy(t => t.TechnicianId) // Tie-break by ID (earliest)
            .First();

        // Assign technician to ticket
        ticket.TechnicianId = selectedTechnician.TechnicianId;
        ticket.Status = TicketStatus.InProgress;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return selectedTechnician.TechnicianId;
    }

    /// <summary>
    /// Assigns all unassigned tickets within a date range (or all if no range specified)
    /// </summary>
    public async Task<int> AssignUnassignedTicketsAsync(DateTime? startDate = null, DateTime? endDate = null)
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

        var unassignedTickets = await query.ToListAsync();
        int assignedCount = 0;

        foreach (var ticket in unassignedTickets)
        {
            var assignedTechnicianId = await AssignTechnicianToTicketAsync(ticket.Id);
            if (assignedTechnicianId != null)
            {
                assignedCount++;
            }
        }

        return assignedCount;
    }
}

