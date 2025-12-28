using Ticketing.Domain.Enums;

namespace Ticketing.Domain.Entities;

public class Ticket
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public int CategoryId { get; set; }
    public int? SubcategoryId { get; set; }
    public TicketPriority Priority { get; set; }
    public TicketStatus Status { get; set; }
    public Guid CreatedByUserId { get; set; }

    /// <summary>
    /// Backwards-compat single assignee (lead technician's UserId).
    /// For new code, use AssignedTechnicians join table.
    /// </summary>
    public Guid? AssignedToUserId { get; set; }

    /// <summary>
    /// Backwards-compat single Technician entity link (lead technician).
    /// For new code, use AssignedTechnicians join table.
    /// </summary>
    public Guid? TechnicianId { get; set; }

    /// <summary>
    /// Responsible technician for this ticket (one active at a time).
    /// Must be one of the assigned technicians.
    /// </summary>
    public Guid? ResponsibleTechnicianId { get; set; }

    /// <summary>
    /// Identity UserId of the responsible technician (User.Id / JWT sub).
    /// Denormalized for fast filtering.
    /// </summary>
    public Guid? ResponsibleUserId { get; set; }

    /// <summary>
    /// User who set the responsible technician (for audit).
    /// </summary>
    public Guid? ResponsibleSetByUserId { get; set; }

    /// <summary>
    /// When the responsible technician was last set.
    /// </summary>
    public DateTime? ResponsibleSetAt { get; set; }

    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? DueDate { get; set; }

    public Category? Category { get; set; }
    public Subcategory? Subcategory { get; set; }
    public User? CreatedByUser { get; set; }
    public User? AssignedToUser { get; set; }
    public Technician? Technician { get; set; }
    
    /// <summary>
    /// Navigation to the responsible technician entity.
    /// </summary>
    public Technician? ResponsibleTechnician { get; set; }
    
    /// <summary>
    /// Navigation to the user who set the responsible technician.
    /// </summary>
    public User? ResponsibleSetByUser { get; set; }
    
    public ICollection<TicketMessage> Messages { get; set; } = new List<TicketMessage>();
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<TicketFieldValue> FieldValues { get; set; } = new List<TicketFieldValue>();

    /// <summary>
    /// New many-to-many relation: all technicians assigned to this ticket.
    /// </summary>
    public ICollection<TicketTechnician> AssignedTechnicians { get; set; } = new List<TicketTechnician>();

    /// <summary>
    /// Activity/audit log entries for this ticket.
    /// </summary>
    public ICollection<TicketActivity> Activities { get; set; } = new List<TicketActivity>();
}
