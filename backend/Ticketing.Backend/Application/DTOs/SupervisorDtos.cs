using Ticketing.Backend.Domain.Enums;

namespace Ticketing.Backend.Application.DTOs;

public class SupervisorTechnicianListItemDto
{
    public Guid TechnicianUserId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
    public int InboxTotal { get; set; }
    public int InboxLeft { get; set; }
    public int WorkloadPercent { get; set; }
}

public class SupervisorTechnicianSummaryDto
{
    public Guid TechnicianUserId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
    public string TechnicianEmail { get; set; } = string.Empty;
    public List<TicketSummaryDto> ArchiveTickets { get; set; } = new();
    public List<TicketSummaryDto> ActiveTickets { get; set; } = new();
}

public class TicketSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    
    /// <summary>Canonical status from database</summary>
    public TicketStatus CanonicalStatus { get; set; }
    
    /// <summary>Display status mapped for the requester's role</summary>
    public TicketStatus DisplayStatus { get; set; }
    
    /// <summary>Legacy property for backward compatibility</summary>
    [Obsolete("Use DisplayStatus for UI")]
    public TicketStatus Status => DisplayStatus;
    
    public string ClientName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SupervisorTicketAssignmentRequest
{
    public Guid TicketId { get; set; }
}

