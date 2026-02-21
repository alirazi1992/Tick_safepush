using Ticketing.Backend.Application.DTOs;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Domain.Enums;

namespace Ticketing.Backend.Application.Services;

public class SupervisorService : ISupervisorService
{
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUserRepository _userRepository;
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketTechnicianAssignmentRepository _assignmentRepository;
    private readonly ISupervisorTechnicianLinkRepository _linkRepository;
    private readonly ITicketActivityEventRepository _activityRepository;
    private readonly IUnitOfWork _unitOfWork;

    public SupervisorService(
        ITechnicianRepository technicianRepository,
        IUserRepository userRepository,
        ITicketRepository ticketRepository,
        ITicketTechnicianAssignmentRepository assignmentRepository,
        ISupervisorTechnicianLinkRepository linkRepository,
        ITicketActivityEventRepository activityRepository,
        IUnitOfWork unitOfWork)
    {
        _technicianRepository = technicianRepository;
        _userRepository = userRepository;
        _ticketRepository = ticketRepository;
        _assignmentRepository = assignmentRepository;
        _linkRepository = linkRepository;
        _activityRepository = activityRepository;
        _unitOfWork = unitOfWork;
    }

    private async Task EnsureSupervisorAsync(Guid supervisorUserId)
    {
        var supervisor = await _technicianRepository.GetByUserIdAsync(supervisorUserId);
        if (supervisor == null || !supervisor.IsSupervisor)
        {
            throw new UnauthorizedAccessException("Only supervisor technicians can perform this action.");
        }
    }

    public async Task<IEnumerable<SupervisorTechnicianListItemDto>> GetTechniciansAsync(Guid supervisorUserId)
    {
        await EnsureSupervisorAsync(supervisorUserId);

        var links = await _linkRepository.GetLinksForSupervisorAsync(supervisorUserId);
        var technicianUserIds = links.Select(l => l.TechnicianUserId).Distinct().ToList();
        if (technicianUserIds.Count == 0)
        {
            return Enumerable.Empty<SupervisorTechnicianListItemDto>();
        }

        var technicians = await _userRepository.GetAllAsync();
        var technicianLookup = technicians
            .Where(u => technicianUserIds.Contains(u.Id))
            .ToDictionary(u => u.Id, u => u);

        var results = new List<SupervisorTechnicianListItemDto>();
        foreach (var technicianUserId in technicianUserIds)
        {
            if (!technicianLookup.TryGetValue(technicianUserId, out var technicianUser))
            {
                continue;
            }

            var assignments = await _assignmentRepository.GetActiveTicketsForTechnicianAsync(technicianUserId);
            var total = assignments.Count();
            var left = assignments.Count(a => a.Ticket != null && a.Ticket.Status != TicketStatus.Solved);
            var percent = total == 0 ? 0 : (int)Math.Round((double)left / total * 100);

            results.Add(new SupervisorTechnicianListItemDto
            {
                TechnicianUserId = technicianUserId,
                TechnicianName = technicianUser.FullName,
                InboxTotal = total,
                InboxLeft = left,
                WorkloadPercent = percent
            });
        }

        return results.OrderBy(r => r.TechnicianName).ToList();
    }

    public async Task<IEnumerable<TechnicianResponse>> GetAvailableTechniciansAsync(Guid supervisorUserId)
    {
        await EnsureSupervisorAsync(supervisorUserId);

        // Get all technicians (non-supervisors)
        var allTechnicians = await _technicianRepository.GetAllAsync();
        var technicianUserIds = allTechnicians
            .Where(t => !t.IsSupervisor && t.UserId.HasValue)
            .Select(t => t.UserId!.Value)
            .ToList();

        // Get already linked technicians
        var links = await _linkRepository.GetLinksForSupervisorAsync(supervisorUserId);
        var linkedUserIds = links.Select(l => l.TechnicianUserId).ToHashSet();

        // Get available technicians (not yet linked)
        var availableUserIds = technicianUserIds.Where(id => !linkedUserIds.Contains(id)).ToList();
        
        if (availableUserIds.Count == 0)
        {
            return Enumerable.Empty<TechnicianResponse>();
        }

        var users = await _userRepository.GetAllAsync();
        var results = users
            .Where(u => availableUserIds.Contains(u.Id))
            .Select(u => new TechnicianResponse
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                Phone = u.PhoneNumber,
                Department = u.Department,
                IsActive = true,
                IsSupervisor = false,
                Role = "Technician",
                CreatedAt = u.CreatedAt
            })
            .OrderBy(t => t.FullName)
            .ToList();

        return results;
    }

    public async Task<SupervisorTechnicianSummaryDto?> GetTechnicianSummaryAsync(Guid supervisorUserId, Guid technicianUserId)
    {
        await EnsureSupervisorAsync(supervisorUserId);
        if (!await _linkRepository.IsLinkedAsync(supervisorUserId, technicianUserId))
        {
            return null;
        }

        var technicianUser = await _userRepository.GetByIdAsync(technicianUserId);
        if (technicianUser == null)
        {
            return null;
        }

        var allAssignments = await _assignmentRepository.GetTicketsForTechnicianAsync(technicianUserId);
        var activeAssignments = await _assignmentRepository.GetActiveTicketsForTechnicianAsync(technicianUserId);

        var archiveTickets = allAssignments
            .Where(a => a.Ticket != null && a.Ticket.Status == TicketStatus.Solved)
            .Select(a => MapTicketSummary(a.Ticket!))
            .DistinctBy(t => t.Id)
            .ToList();

        var activeTickets = activeAssignments
            .Where(a => a.Ticket != null && a.Ticket.Status != TicketStatus.Solved)
            .Select(a => MapTicketSummary(a.Ticket!))
            .DistinctBy(t => t.Id)
            .ToList();

        return new SupervisorTechnicianSummaryDto
        {
            TechnicianUserId = technicianUserId,
            TechnicianName = technicianUser.FullName,
            TechnicianEmail = technicianUser.Email,
            ArchiveTickets = archiveTickets,
            ActiveTickets = activeTickets
        };
    }

    public async Task<List<TicketSummaryDto>> GetAvailableTicketsAsync(Guid supervisorUserId)
    {
        await EnsureSupervisorAsync(supervisorUserId);
        var assignments = await _assignmentRepository.GetActiveTicketsForTechnicianAsync(supervisorUserId);
        return assignments
            .Where(a => a.Ticket != null && a.Ticket.Status != TicketStatus.Solved)
            .Select(a => MapTicketSummary(a.Ticket!))
            .DistinctBy(t => t.Id)
            .ToList();
    }

    public async Task<bool> LinkTechnicianAsync(Guid supervisorUserId, Guid technicianUserId)
    {
        await EnsureSupervisorAsync(supervisorUserId);

        var technician = await _technicianRepository.GetByUserIdAsync(technicianUserId);
        if (technician == null || technician.IsSupervisor)
        {
            throw new InvalidOperationException("Technician not found or is a supervisor.");
        }

        if (await _linkRepository.IsLinkedAsync(supervisorUserId, technicianUserId))
        {
            return true;
        }

        await _linkRepository.AddAsync(new SupervisorTechnicianLink
        {
            SupervisorUserId = supervisorUserId,
            TechnicianUserId = technicianUserId
        });

        return true;
    }

    public async Task<bool> UnlinkTechnicianAsync(Guid supervisorUserId, Guid technicianUserId)
    {
        await EnsureSupervisorAsync(supervisorUserId);
        return await _linkRepository.RemoveAsync(supervisorUserId, technicianUserId);
    }

    public async Task<bool> AssignTicketAsync(Guid supervisorUserId, Guid technicianUserId, Guid ticketId)
    {
        await EnsureSupervisorAsync(supervisorUserId);
        if (!await _linkRepository.IsLinkedAsync(supervisorUserId, technicianUserId))
        {
            return false;
        }

        var supervisorAssignment = await _assignmentRepository.GetActiveAssignmentAsync(ticketId, supervisorUserId);
        if (supervisorAssignment == null)
        {
            return false;
        }

        var existingAssignment = await _assignmentRepository.GetActiveAssignmentAsync(ticketId, technicianUserId);
        if (existingAssignment != null)
        {
            return true;
        }

        var assignment = new TicketTechnicianAssignment
        {
            TicketId = ticketId,
            TechnicianUserId = technicianUserId,
            AssignedAt = DateTime.UtcNow,
            AssignedByUserId = supervisorUserId,
            IsActive = true,
            Role = "Collaborator"
        };
        await _assignmentRepository.AddAsync(assignment);

        var ticket = await _ticketRepository.GetByIdWithIncludesAsync(ticketId);
        if (ticket == null)
        {
            return false;
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await _ticketRepository.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        await _activityRepository.AddEventAsync(
            ticketId,
            supervisorUserId,
            "Supervisor",
            "SupervisorAssigned",
            null,
            ticket.Status.ToString(),
            System.Text.Json.JsonSerializer.Serialize(new { technicianUserId }));

        return true;
    }

    public async Task<bool> RemoveAssignmentAsync(Guid supervisorUserId, Guid technicianUserId, Guid ticketId)
    {
        await EnsureSupervisorAsync(supervisorUserId);
        if (!await _linkRepository.IsLinkedAsync(supervisorUserId, technicianUserId))
        {
            return false;
        }

        var assignment = await _assignmentRepository.GetActiveAssignmentAsync(ticketId, technicianUserId);
        if (assignment == null || assignment.AssignedByUserId != supervisorUserId)
        {
            return false;
        }

        assignment.IsActive = false;
        assignment.UpdatedAt = DateTime.UtcNow;
        await _assignmentRepository.UpdateAsync(assignment);

        var ticket = await _ticketRepository.GetByIdWithIncludesAsync(ticketId);
        if (ticket == null)
        {
            return false;
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await _ticketRepository.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        await _activityRepository.AddEventAsync(
            ticketId,
            supervisorUserId,
            "Supervisor",
            "SupervisorUnassigned",
            null,
            ticket.Status.ToString(),
            System.Text.Json.JsonSerializer.Serialize(new { technicianUserId }));

        return true;
    }

    private static TicketSummaryDto MapTicketSummary(Ticket ticket)
    {
        // Supervisors are technicians - they can see all statuses including Redo
        return new TicketSummaryDto
        {
            Id = ticket.Id,
            Title = ticket.Title,
            CanonicalStatus = ticket.Status,
            DisplayStatus = StatusMappingService.MapStatusForRole(ticket.Status, UserRole.Technician),
            ClientName = ticket.CreatedByUser?.FullName ?? string.Empty,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt
        };
    }

}

