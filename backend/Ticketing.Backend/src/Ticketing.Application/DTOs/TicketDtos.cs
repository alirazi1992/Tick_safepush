using Ticketing.Domain.Enums;

namespace Ticketing.Application.DTOs;

public class TicketDynamicFieldRequest
{
    public int FieldDefinitionId { get; set; }
    public string Value { get; set; } = string.Empty;
}

public class TicketCreateRequest
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public TicketPriority Priority { get; set; }
    public List<TicketDynamicFieldRequest>? DynamicFields { get; set; }
}

public class TicketUpdateRequest
{
    public TicketStatus? Status { get; set; }
    public TicketPriority? Priority { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public DateTime? DueDate { get; set; }
    public string? Description { get; set; }
}

public class TicketResponse
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public int? SubcategoryId { get; set; }
    public string? SubcategoryName { get; set; }
    public TicketPriority Priority { get; set; }
    public List<TicketDynamicFieldResponse>? DynamicFields { get; set; }
    public TicketStatus Status { get; set; }
    public Guid CreatedByUserId { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public string CreatedByEmail { get; set; } = string.Empty;
    public string? CreatedByPhoneNumber { get; set; }
    public string? CreatedByDepartment { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedToName { get; set; }
    public string? AssignedToEmail { get; set; }
    public string? AssignedToPhoneNumber { get; set; }
    public string? AssignedTechnicianName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}

public class TicketMessageRequest
{
    public string Message { get; set; } = string.Empty;
    public TicketStatus? Status { get; set; }
}

public class TicketMessageDto
{
    public Guid Id { get; set; }
    public Guid AuthorUserId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorEmail { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public TicketStatus? Status { get; set; }
}

public class TicketCalendarResponse
{
    public Guid Id { get; set; }
    public string TicketNumber { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? AssignedTechnicianName { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
}

public class AssignTechnicianRequest
{
    public Guid TechnicianId { get; set; }
}

public class AssignTechniciansRequest
{
    public List<Guid> TechnicianIds { get; set; } = new();
    public Guid? LeadTechnicianId { get; set; }
}

public class UpdateTechnicianStateRequest
{
    public TicketTechnicianState State { get; set; }
}

public class SetResponsibleTechnicianRequest
{
    public Guid ResponsibleTechnicianId { get; set; }
}

public class TicketTechnicianDto
{
    public Guid TechnicianId { get; set; }
    public Guid TechnicianUserId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
    public string TechnicianEmail { get; set; } = string.Empty;
    public bool IsLead { get; set; }
    public TicketTechnicianState State { get; set; }
    public DateTime AssignedAt { get; set; }
}

public class TicketActivityDto
{
    public Guid Id { get; set; }
    public Guid TicketId { get; set; }
    public Guid ActorUserId { get; set; }
    public string ActorName { get; set; } = string.Empty;
    public string ActorEmail { get; set; } = string.Empty;
    public TicketActivityType Type { get; set; }
    public string Message { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

public class UpdateWorkSessionRequest
{
    public string WorkingOn { get; set; } = string.Empty;
    public string? Note { get; set; }
    public TicketTechnicianState State { get; set; }
}

public class TicketCollaborationResponse
{
    public Guid TicketId { get; set; }
    public TicketStatus Status { get; set; }
    public TicketActivityDto? LastActivity { get; set; }
    public List<TicketActivityDto> RecentActivities { get; set; } = new();
    public List<ActiveTechnicianDto> ActiveTechnicians { get; set; } = new();
}

public class ActiveTechnicianDto
{
    public Guid TechnicianId { get; set; }
    public Guid TechnicianUserId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string WorkingOn { get; set; } = string.Empty;
    public string? Note { get; set; }
    public TicketTechnicianState State { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class TicketSummaryDto
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? SubcategoryName { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public Guid? AssignedToUserId { get; set; }
    public string? AssignedTechnicianName { get; set; }
}

public class TicketDynamicFieldResponse
{
    public int FieldDefinitionId { get; set; }
    public string Key { get; set; } = string.Empty;
    public string Label { get; set; } = string.Empty;
    public FieldType Type { get; set; }
    public string Value { get; set; } = string.Empty;
    public bool IsRequired { get; set; }
}

public class AssignmentQueueResponse
{
    public List<TicketSummaryDto>? Tickets { get; set; }
    public List<TicketSummaryDto>? Tasks { get; set; }
    public List<AssignmentQueueItem> Items { get; set; } = new();
    public int TotalCount { get; set; }
}

public class AssignmentQueueItem
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public TicketStatus Status { get; set; }
    public TicketPriority Priority { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? DueDate { get; set; }
    public string CategoryName { get; set; } = string.Empty;
    public string? SubcategoryName { get; set; }
    public string CreatedByName { get; set; } = string.Empty;
}
