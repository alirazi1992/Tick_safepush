using Ticketing.Backend.Application.DTOs;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Domain.Enums;

namespace Ticketing.Backend.Application.Services;

/// <summary>
/// Thrown when a user attempts a status change they don't have permission for
/// </summary>
public class StatusChangeForbiddenException : Exception
{
    public StatusChangeForbiddenException(string message) : base(message) { }
}

public interface ITicketService
{
    Task<IEnumerable<TicketResponse>> GetTicketsAsync(Guid userId, UserRole role, TicketStatus? status, TicketPriority? priority, Guid? assignedTo, Guid? createdBy, string? search);
    Task<TicketResponse?> GetTicketAsync(Guid id, Guid userId, UserRole role);
    Task<TicketResponse?> CreateTicketAsync(Guid userId, TicketCreateRequest request);
    Task<TicketResponse?> UpdateTicketAsync(Guid id, Guid userId, UserRole role, TicketUpdateRequest request);
    Task<TicketResponse?> AssignTicketAsync(Guid id, Guid technicianId);
    Task<IEnumerable<TicketMessageDto>> GetMessagesAsync(Guid ticketId, Guid userId, UserRole role);
    Task<TicketMessageDto?> AddMessageAsync(Guid ticketId, Guid authorId, string message, TicketStatus? status = null);
    Task<IEnumerable<TicketCalendarResponse>> GetCalendarTicketsAsync(DateTime startDate, DateTime endDate);
}

public class TicketService : ITicketService
{
    private readonly ITicketRepository _ticketRepository;
    private readonly ITicketMessageRepository _ticketMessageRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ITechnicianService _technicianService;
    private readonly ISystemSettingsService _systemSettingsService;
    private readonly ISmartAssignmentService _smartAssignmentService;

    public TicketService(
        ITicketRepository ticketRepository,
        ITicketMessageRepository ticketMessageRepository,
        ITechnicianRepository technicianRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        INotificationService notificationService, 
        ITechnicianService technicianService,
        ISystemSettingsService systemSettingsService,
        ISmartAssignmentService smartAssignmentService)
    {
        _ticketRepository = ticketRepository;
        _ticketMessageRepository = ticketMessageRepository;
        _technicianRepository = technicianRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _technicianService = technicianService;
        _systemSettingsService = systemSettingsService;
        _smartAssignmentService = smartAssignmentService;
    }

    public async Task<IEnumerable<TicketResponse>> GetTicketsAsync(Guid userId, UserRole role, TicketStatus? status, TicketPriority? priority, Guid? assignedTo, Guid? createdBy, string? search)
    {
        var tickets = await _ticketRepository.QueryAsync(role, userId, status, priority, assignedTo, createdBy, search);
        return tickets.Select(MapToResponse);
    }

    public async Task<TicketResponse?> GetTicketAsync(Guid id, Guid userId, UserRole role)
    {
        var ticket = await _ticketRepository.GetByIdWithIncludesAsync(id);

        if (ticket == null)
        {
            return null;
        }

        if (role == UserRole.Client && ticket.CreatedByUserId != userId)
        {
            return null;
        }

        if (role == UserRole.Technician && ticket.TechnicianId != userId && ticket.AssignedToUserId != userId)
        {
            return null;
        }

        // Auto-set Viewed when technician/admin opens ticket detail (if status is Submitted and viewer is not the creator)
        if (ticket.Status == TicketStatus.Submitted && 
            ticket.CreatedByUserId != userId && 
            (role == UserRole.Technician || role == UserRole.Admin))
        {
            ticket.Status = TicketStatus.Viewed;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _ticketRepository.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();
        }

        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> CreateTicketAsync(Guid userId, TicketCreateRequest request)
    {
        // Clients create tickets for themselves; the role check happens in the controller
        var ticket = new Ticket
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            CategoryId = request.CategoryId,
            SubcategoryId = request.SubcategoryId,
            Priority = request.Priority,
            Status = TicketStatus.Submitted,
            CreatedByUserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        await _ticketRepository.AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        // NOTE: Auto-assignment on ticket creation is DISABLED by design.
        // Tickets are always created as Submitted + unassigned.
        // Smart Assignment runs manually via POST /api/admin/assignment/smart/run
        // or can be scheduled externally. This ensures predictable ticket state.

        ticket = await _ticketRepository.GetByIdWithIncludesAsync(ticket.Id);
        if (ticket == null)
        {
            return null;
        }

        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> UpdateTicketAsync(Guid id, Guid userId, UserRole role, TicketUpdateRequest request)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
        {
            return null;
        }

        // Validate permission rules
        if (role == UserRole.Client && ticket.CreatedByUserId != userId)
        {
            return null;
        }
        if (role == UserRole.Technician && ticket.TechnicianId != userId && ticket.AssignedToUserId != userId)
        {
            return null;
        }

        if (request.Description != null && role != UserRole.Technician)
        {
            ticket.Description = request.Description;
        }

        if (request.Priority.HasValue && role != UserRole.Technician)
        {
            ticket.Priority = request.Priority.Value;
        }

        if (request.Status.HasValue)
        {
            var newStatus = request.Status.Value;
            
            // Validation: Only Admin can set Closed
            if (newStatus == TicketStatus.Closed && role != UserRole.Admin)
            {
                return null; // Forbid - return null to indicate permission denied
            }
            
            // Client restrictions: Cannot set InProgress, Resolved, or Closed
            if (role == UserRole.Client)
            {
                if (newStatus == TicketStatus.InProgress || 
                    newStatus == TicketStatus.Resolved || 
                    newStatus == TicketStatus.Closed)
                {
                    return null; // Forbid
                }
            }
            
            // Technician can set Open, InProgress, Resolved (but not Closed - only Admin)
            // Admin can set any status including Closed
            ticket.Status = newStatus;
        }

        if (role == UserRole.Admin)
        {
            if (request.AssignedToUserId.HasValue)
            {
                ticket.AssignedToUserId = request.AssignedToUserId.Value;
            }
            ticket.DueDate = request.DueDate;
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await _ticketRepository.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return await GetTicketAsync(id, userId, role);
    }

    public async Task<TicketResponse?> AssignTicketAsync(Guid id, Guid technicianId)
    {
        var ticket = await _ticketRepository.GetByIdAsync(id);
        if (ticket == null)
        {
            return null;
        }

        // Load technician to get UserId (required for AssignedToUserId foreign key)
        var technician = await _technicianRepository.GetByIdAsync(technicianId);
        
        if (technician == null || !technician.IsActive)
        {
            return null; // Technician not found or inactive
        }

        // Set both TechnicianId (for display/navigation) and AssignedToUserId (for filtering/queries)
        ticket.TechnicianId = technicianId;
        ticket.AssignedToUserId = technician.UserId; // CRITICAL: Set to Technician.UserId (User.Id), not null
        // When assigning, set status to Open (not InProgress) - technician will change to InProgress when they start working
        ticket.Status = TicketStatus.Open;
        ticket.UpdatedAt = DateTime.UtcNow;
        await _ticketRepository.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        return await GetTicketAsync(id, Guid.Empty, UserRole.Admin);
    }

    public async Task<IEnumerable<TicketMessageDto>> GetMessagesAsync(Guid ticketId, Guid userId, UserRole role)
    {
        var ticket = await GetTicketAsync(ticketId, userId, role);
        if (ticket == null)
        {
            return Enumerable.Empty<TicketMessageDto>();
        }

        var messages = await _ticketMessageRepository.GetByTicketIdAsync(ticketId);
        return messages.Select(m => new TicketMessageDto
        {
            Id = m.Id,
            AuthorUserId = m.AuthorUserId,
            AuthorName = m.AuthorUser!.FullName,
            AuthorEmail = m.AuthorUser.Email,
            Message = m.Message,
            CreatedAt = m.CreatedAt,
            Status = m.Status
        });
    }

    public async Task<TicketMessageDto?> AddMessageAsync(Guid ticketId, Guid authorId, string message, TicketStatus? status = null)
    {
        var ticket = await _ticketRepository.GetByIdAsync(ticketId);
        if (ticket == null)
        {
            return null;
        }

        var author = await _userRepository.GetByIdAsync(authorId);
        if (author == null)
        {
            return null;
        }

        // Access control: Client can only access their own tickets
        if (author.Role == UserRole.Client && ticket.CreatedByUserId != authorId)
        {
            return null;
        }

        // Access control: Technician can only access assigned tickets
        if (author.Role == UserRole.Technician && ticket.TechnicianId != authorId && ticket.AssignedToUserId != authorId)
        {
            return null;
        }

        // ═══════════════════════════════════════════════════════════════════════════════
        // STATUS CHANGE PERMISSION RULES (SECURITY-CRITICAL)
        // ═══════════════════════════════════════════════════════════════════════════════
        // CLOSE (Closed): Admin ONLY
        // Resolved: Technician & Admin ONLY - Client FORBIDDEN
        // InProgress: Technician & Admin ONLY - Client FORBIDDEN
        // Other status changes: Allowed based on role
        // ═══════════════════════════════════════════════════════════════════════════════
        if (status.HasValue)
        {
            var newStatus = status.Value;
            
            // Only Admin can set Closed
            if (newStatus == TicketStatus.Closed && author.Role != UserRole.Admin)
            {
                throw new StatusChangeForbiddenException("Only Admins can close tickets.");
            }
            
            if (author.Role == UserRole.Client)
            {
                // Client cannot set InProgress, Resolved, or Closed
                if (newStatus == TicketStatus.InProgress || 
                    newStatus == TicketStatus.Resolved || 
                    newStatus == TicketStatus.Closed)
                {
                    throw new StatusChangeForbiddenException("Clients cannot set status to InProgress, Resolved, or Closed.");
                }
                // Client can set Submitted, Viewed, Open
                ticket.Status = newStatus;
            }
            else if (author.Role == UserRole.Technician)
            {
                // Technician can set Open, InProgress, Resolved (but not Closed)
                if (newStatus == TicketStatus.Closed)
                {
                    throw new StatusChangeForbiddenException("Only Admins can close tickets.");
                }
                ticket.Status = newStatus;
            }
            else
            {
                // Admin can set any status
                ticket.Status = newStatus;
            }
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await _ticketRepository.UpdateAsync(ticket);

        var ticketMessage = new TicketMessage
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            AuthorUserId = authorId,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            Status = status ?? ticket.Status
        };

        await _ticketMessageRepository.AddAsync(ticketMessage);
        await _unitOfWork.SaveChangesAsync();

        // Notify opposite participant
        var notifyUserId = ticket.AssignedToUserId == authorId ? ticket.CreatedByUserId : ticket.AssignedToUserId ?? ticket.CreatedByUserId;
        await _notificationService.CreateNotificationAsync(notifyUserId, $"New message on ticket '{ticket.Title}'");

        var createdMessage = await _ticketMessageRepository.GetByIdWithAuthorAsync(ticketMessage.Id);
        if (createdMessage == null)
        {
            return null;
        }

        return new TicketMessageDto
        {
            Id = createdMessage.Id,
            AuthorUserId = createdMessage.AuthorUserId,
            AuthorName = createdMessage.AuthorUser!.FullName,
            AuthorEmail = createdMessage.AuthorUser.Email,
            Message = createdMessage.Message,
            CreatedAt = createdMessage.CreatedAt,
            Status = createdMessage.Status
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // MANUAL TEST CHECKLIST (Swagger):
    // 1. POST /api/Tickets → status=Submitted, assignedToUserId=null, assignedToName/email/phone=null
    // 2. POST /api/admin/assignment/smart/run → assignedCount > 0 (if eligible unassigned tickets exist)
    // 3. GET /api/technician/tickets (as assigned tech) → ticket appears in list
    // ═══════════════════════════════════════════════════════════════════════════════
    private static TicketResponse MapToResponse(Ticket ticket)
    {
        // SECURITY-CRITICAL: Only show assigned technician info when ticket is truly assigned
        // "Truly assigned" = AssignedToUserId is not null (the authoritative field for filtering/queries)
        var isAssigned = ticket.AssignedToUserId != null;
        
        return new TicketResponse
        {
            Id = ticket.Id,
            Title = ticket.Title,
            Description = ticket.Description,
            CategoryId = ticket.CategoryId,
            CategoryName = ticket.Category?.Name ?? string.Empty,
            SubcategoryId = ticket.SubcategoryId,
            SubcategoryName = ticket.Subcategory?.Name,
            Priority = ticket.Priority,
            Status = ticket.Status,
            CreatedByUserId = ticket.CreatedByUserId,
            CreatedByName = ticket.CreatedByUser?.FullName ?? string.Empty,
            CreatedByEmail = ticket.CreatedByUser?.Email ?? string.Empty,
            CreatedByPhoneNumber = ticket.CreatedByUser?.PhoneNumber,
            CreatedByDepartment = ticket.CreatedByUser?.Department,
            AssignedToUserId = ticket.AssignedToUserId,
            // Only populate assigned fields when truly assigned
            AssignedToName = isAssigned ? (ticket.Technician?.FullName ?? ticket.AssignedToUser?.FullName) : null,
            AssignedToEmail = isAssigned ? (ticket.Technician?.Email ?? ticket.AssignedToUser?.Email) : null,
            AssignedToPhoneNumber = isAssigned ? (ticket.Technician?.Phone ?? ticket.AssignedToUser?.PhoneNumber) : null,
            AssignedTechnicianName = isAssigned ? (ticket.Technician?.FullName ?? ticket.AssignedToUser?.FullName) : null,
            CreatedAt = ticket.CreatedAt,
            UpdatedAt = ticket.UpdatedAt,
            DueDate = ticket.DueDate
        };
    }

    public async Task<IEnumerable<TicketCalendarResponse>> GetCalendarTicketsAsync(DateTime startDate, DateTime endDate)
    {
        // Get all tickets within the date range (Admin only - no role filtering)
        var tickets = await _ticketRepository.GetCalendarTicketsAsync(startDate, endDate);

        return tickets.Select(t => new TicketCalendarResponse
        {
            Id = t.Id,
            TicketNumber = $"T-{t.Id.ToString("N").Substring(0, 8).ToUpper()}",
            Title = t.Title,
            Status = t.Status,
            Priority = t.Priority,
            CategoryName = t.Category?.Name ?? string.Empty,
            // Only show technician name when truly assigned (AssignedToUserId != null)
            AssignedTechnicianName = t.AssignedToUserId != null ? (t.Technician?.FullName ?? t.AssignedToUser?.FullName) : null,
            CreatedAt = t.CreatedAt,
            DueDate = t.DueDate
        });
    }
}

