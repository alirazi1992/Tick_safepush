using Microsoft.Extensions.Logging;
using Ticketing.Application.Abstractions;
using Ticketing.Application.DTOs;
using Ticketing.Application.Repositories;
using Ticketing.Application.Services;
using Ticketing.Domain.Entities;
using Ticketing.Domain.Enums;

using Ticketing.Application.Exceptions;

namespace Ticketing.Infrastructure.Services;

public class TicketService : ITicketService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly INotificationService _notificationService;
    private readonly ITechnicianService _technicianService;
    private readonly ISystemSettingsService _systemSettingsService;
    private readonly ISmartAssignmentService _smartAssignmentService;
    private readonly INotificationHub _notificationHub;
    private readonly ILogger<TicketService> _logger;

    public TicketService(
        IUnitOfWork unitOfWork, 
        INotificationService notificationService, 
        ITechnicianService technicianService,
        ISystemSettingsService systemSettingsService,
        ISmartAssignmentService smartAssignmentService,
        INotificationHub notificationHub,
        ILogger<TicketService> logger)
    {
        _unitOfWork = unitOfWork;
        _notificationService = notificationService;
        _technicianService = technicianService;
        _systemSettingsService = systemSettingsService;
        _smartAssignmentService = smartAssignmentService;
        _notificationHub = notificationHub;
        _logger = logger;
    }

    public async Task<IEnumerable<TicketResponse>> GetTicketsAsync(Guid userId, UserRole role, TicketStatus? status, TicketPriority? priority, Guid? assignedTo, Guid? createdBy, string? search)
    {
        try
        {
            _logger.LogInformation("GetTicketsAsync START: Role={Role}, UserId={UserId}, Status={Status}, Priority={Priority}, AssignedTo={AssignedTo}, CreatedBy={CreatedBy}, Search={Search}",
                role, userId, status, priority, assignedTo, createdBy, search);
            
            var tickets = await _unitOfWork.Tickets.GetTicketsAsync(role, userId, status, priority, assignedTo, createdBy, search);
            
            var ticketsList = tickets.ToList();
            _logger.LogInformation("GetTicketsAsync: Query executed successfully. Role={Role}, UserId={UserId}, Count={Count}",
                role, userId, ticketsList.Count);
            
            var responses = new List<TicketResponse>();
            foreach (var ticket in ticketsList)
            {
                try
                {
                    var response = MapToResponse(ticket);
                    responses.Add(response);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "GetTicketsAsync: Failed to map ticket {TicketId} to response. Role={Role}, UserId={UserId}", 
                        ticket.Id, role, userId);
                    throw; // Re-throw to surface the error
                }
            }
            
            _logger.LogInformation("GetTicketsAsync SUCCESS: Role={Role}, UserId={UserId}, ResponseCount={Count}",
                role, userId, responses.Count);
            
            return responses;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTicketsAsync FAILED: Role={Role}, UserId={UserId}, ExceptionType={ExceptionType}, Message={Message}",
                role, userId, ex.GetType().Name, ex.Message);
            throw; // Re-throw to let controller handle it
        }
    }

    public async Task<TicketResponse?> GetTicketAsync(Guid id, Guid userId, UserRole role)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(id);

        if (ticket == null)
        {
            return null;
        }

        if (role == UserRole.Client && ticket.CreatedByUserId != userId)
        {
            return null;
        }

        if (role == UserRole.Technician)
        {
            // Check both old single assignment and new multi-technician assignment
            var isAssigned = ticket.TechnicianId == userId || 
                            ticket.AssignedToUserId == userId ||
                            ticket.AssignedTechnicians.Any(tt => tt.TechnicianUserId == userId);
            if (!isAssigned)
            {
                return null;
            }
        }

        // Auto-set Viewed when technician/admin opens ticket detail (if status is Submitted and viewer is not creator)
        if (ticket.Status == TicketStatus.Submitted && 
            role != UserRole.Client && 
            ticket.CreatedByUserId != userId)
        {
            ticket.Status = TicketStatus.Viewed;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Tickets.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();

            // Create activity log entry if TicketActivity exists
            try
            {
                var actor = await _unitOfWork.Users.GetByIdAsync(userId);
                var actorName = actor?.FullName ?? "Unknown";
                await CreateActivityAsync(id, userId, TicketActivityType.StatusChanged,
                    $"{actorName} viewed the ticket. Status changed from Submitted to Viewed");
            }
            catch (Exception ex)
            {
                // If activity creation fails, log but don't fail the request
                _logger.LogWarning(ex, "Failed to create activity log for status change to Viewed");
            }
        }

        return MapToResponse(ticket);
    }

    public async Task<TicketResponse?> CreateTicketAsync(Guid userId, TicketCreateRequest request)
    {
        // Validate userId is valid (not empty)
        if (userId == Guid.Empty)
        {
            _logger.LogError("CreateTicketAsync: Invalid userId (Guid.Empty)");
            throw new ArgumentException("User ID cannot be empty", nameof(userId));
        }

        // Verify user exists
        var userExists = await _unitOfWork.Users.ExistsAsync(userId);
        if (!userExists)
        {
            _logger.LogError("CreateTicketAsync: User not found (UserId={UserId})", userId);
            throw new ArgumentException($"User with ID {userId} does not exist", nameof(userId));
        }

        _logger.LogInformation("CreateTicketAsync START: UserId={UserId}, CategoryId={CategoryId}, SubcategoryId={SubcategoryId}, Title={Title}",
            userId, request.CategoryId, request.SubcategoryId, request.Title);

        // ═══════════════════════════════════════════════════════════════════════════════════
        // VALIDATE CATEGORY AND SUBCATEGORY
        // ═══════════════════════════════════════════════════════════════════════════════════
        
        _logger.LogDebug("Validating ticket creation request: CategoryId={CategoryId}, SubcategoryId={SubcategoryId}, UserId={UserId}",
            request.CategoryId, request.SubcategoryId, userId);

        // Validate category exists and is active
        var category = await _unitOfWork.Categories.GetByIdAsync(request.CategoryId);
        if (category == null)
        {
            _logger.LogWarning("Ticket creation rejected: Category not found (CategoryId={CategoryId}, UserId={UserId})",
                request.CategoryId, userId);
            throw new TicketValidationException(
                "INVALID_CATEGORY",
                $"Category with ID {request.CategoryId} does not exist",
                request.CategoryId,
                request.SubcategoryId);
        }
        if (!category.IsActive)
        {
            _logger.LogWarning("Ticket creation rejected: Category is inactive (CategoryId={CategoryId}, CategoryName={CategoryName}, UserId={UserId})",
                request.CategoryId, category.Name, userId);
            throw new TicketValidationException(
                "CATEGORY_INACTIVE",
                $"Category '{category.Name}' (ID {request.CategoryId}) is inactive and cannot be used for new tickets",
                request.CategoryId,
                request.SubcategoryId);
        }

        // Validate subcategory (if provided)
        if (request.SubcategoryId.HasValue)
        {
            var subcategory = await _unitOfWork.Categories.GetSubcategoryByIdAsync(request.SubcategoryId.Value);
            if (subcategory == null)
            {
                _logger.LogWarning("Ticket creation rejected: Subcategory not found (SubcategoryId={SubcategoryId}, CategoryId={CategoryId}, UserId={UserId})",
                    request.SubcategoryId.Value, request.CategoryId, userId);
                throw new TicketValidationException(
                    "INVALID_SUBCATEGORY",
                    $"Subcategory with ID {request.SubcategoryId.Value} does not exist",
                    request.CategoryId,
                    request.SubcategoryId);
            }
            if (subcategory.CategoryId != request.CategoryId)
            {
                _logger.LogWarning("Ticket creation rejected: Subcategory belongs to different category (SubcategoryId={SubcategoryId}, SubcategoryName={SubcategoryName}, SubcategoryCategoryId={SubcategoryCategoryId}, RequestCategoryId={CategoryId}, UserId={UserId})",
                    request.SubcategoryId.Value, subcategory.Name, subcategory.CategoryId, request.CategoryId, userId);
                throw new TicketValidationException(
                    "SUBCATEGORY_MISMATCH",
                    $"Subcategory '{subcategory.Name}' (ID {request.SubcategoryId.Value}) does not belong to the selected category '{category.Name}' (ID {request.CategoryId})",
                    request.CategoryId,
                    request.SubcategoryId);
            }
            if (!subcategory.IsActive)
            {
                _logger.LogWarning("Ticket creation rejected: Subcategory is inactive (SubcategoryId={SubcategoryId}, SubcategoryName={SubcategoryName}, CategoryId={CategoryId}, UserId={UserId})",
                    request.SubcategoryId.Value, subcategory.Name, request.CategoryId, userId);
                throw new TicketValidationException(
                    "SUBCATEGORY_INACTIVE",
                    $"Subcategory '{subcategory.Name}' (ID {request.SubcategoryId.Value}) is inactive and cannot be used for new tickets",
                    request.CategoryId,
                    request.SubcategoryId);
            }
        }

        _logger.LogDebug("Category/Subcategory validation passed: CategoryId={CategoryId}, CategoryName={CategoryName}, SubcategoryId={SubcategoryId}",
            request.CategoryId, category.Name, request.SubcategoryId);

        // ═══════════════════════════════════════════════════════════════════════════════════
        // VALIDATE DYNAMIC FIELDS (if subcategory is provided)
        // ═══════════════════════════════════════════════════════════════════════════════════
        
        List<SubcategoryFieldDefinition>? fieldDefinitions = null;
        if (request.SubcategoryId.HasValue)
        {
            var fieldDefs = await _unitOfWork.FieldDefinitions.GetBySubcategoryIdAsync(request.SubcategoryId.Value, includeInactive: false);
            fieldDefinitions = fieldDefs.ToList();

            // Validate dynamic fields if definitions exist
            if (fieldDefinitions.Any() && (request.DynamicFields == null || !request.DynamicFields.Any()))
            {
                var requiredFields = fieldDefinitions.Where(f => f.IsRequired).ToList();
                if (requiredFields.Any())
                {
                    var missingFields = string.Join(", ", requiredFields.Select(f => f.Label));
                    _logger.LogWarning("Ticket creation rejected: Required dynamic fields missing (SubcategoryId={SubcategoryId}, UserId={UserId}, MissingFields={MissingFields})",
                        request.SubcategoryId.Value, userId, missingFields);
                    throw new TicketValidationException(
                        "MISSING_REQUIRED_FIELDS",
                        $"The following required fields are missing: {missingFields}",
                        request.CategoryId,
                        request.SubcategoryId);
                }
            }

            // Validate provided field values
            if (request.DynamicFields != null && request.DynamicFields.Any() && fieldDefinitions.Any())
            {
                var validationErrors = new List<string>();
                var providedFieldIds = request.DynamicFields.Select(f => f.FieldDefinitionId).ToHashSet();
                
                foreach (var fieldDef in fieldDefinitions.Where(f => f.IsRequired))
                {
                    var fieldValue = request.DynamicFields.FirstOrDefault(f => f.FieldDefinitionId == fieldDef.Id);
                    if (fieldValue == null || string.IsNullOrWhiteSpace(fieldValue.Value))
                    {
                        validationErrors.Add($"{fieldDef.Label} (required)");
                    }
                }

                foreach (var fieldValue in request.DynamicFields)
                {
                    var fieldDef = fieldDefinitions.FirstOrDefault(f => f.Id == fieldValue.FieldDefinitionId);
                    if (fieldDef == null)
                    {
                        validationErrors.Add($"Field with ID {fieldValue.FieldDefinitionId} does not exist for this subcategory");
                        continue;
                    }

                    // Validate Select field values
                    if (fieldDef.Type == FieldType.Select && !string.IsNullOrWhiteSpace(fieldValue.Value))
                    {
                        if (!string.IsNullOrEmpty(fieldDef.OptionsJson))
                        {
                            try
                            {
                                var options = System.Text.Json.JsonSerializer.Deserialize<List<FieldOption>>(fieldDef.OptionsJson);
                                if (options != null && !options.Any(o => o.Value == fieldValue.Value))
                                {
                                    validationErrors.Add($"{fieldDef.Label}: Invalid option value '{fieldValue.Value}'");
                                }
                            }
                            catch
                            {
                                // If JSON parse fails, skip validation
                            }
                        }
                    }

                    // Validate Number field values
                    if (fieldDef.Type == FieldType.Number && !string.IsNullOrWhiteSpace(fieldValue.Value))
                    {
                        if (!double.TryParse(fieldValue.Value, out double numValue))
                        {
                            validationErrors.Add($"{fieldDef.Label}: Must be a valid number");
                        }
                        else
                        {
                            if (fieldDef.Min.HasValue && numValue < fieldDef.Min.Value)
                            {
                                validationErrors.Add($"{fieldDef.Label}: Must be at least {fieldDef.Min.Value}");
                            }
                            if (fieldDef.Max.HasValue && numValue > fieldDef.Max.Value)
                            {
                                validationErrors.Add($"{fieldDef.Label}: Must be at most {fieldDef.Max.Value}");
                            }
                        }
                    }

                    // Validate Email field values
                    if (fieldDef.Type == FieldType.Email && !string.IsNullOrWhiteSpace(fieldValue.Value))
                    {
                        if (!fieldValue.Value.Contains('@') || !fieldValue.Value.Contains('.'))
                        {
                            validationErrors.Add($"{fieldDef.Label}: Must be a valid email address");
                        }
                    }
                }

                if (validationErrors.Any())
                {
                    var errorMessage = string.Join("; ", validationErrors);
                    _logger.LogWarning("Ticket creation rejected: Dynamic field validation failed (SubcategoryId={SubcategoryId}, UserId={UserId}, Errors={Errors})",
                        request.SubcategoryId.Value, userId, errorMessage);
                    throw new TicketValidationException(
                        "INVALID_FIELD_VALUES",
                        errorMessage,
                        request.CategoryId,
                        request.SubcategoryId);
                }
            }
        }

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

        _logger.LogInformation("Creating ticket: TicketId={TicketId}, CreatedByUserId={CreatedByUserId}, Status={Status}, CategoryId={CategoryId}, SubcategoryId={SubcategoryId}",
            ticket.Id, ticket.CreatedByUserId, ticket.Status, ticket.CategoryId, ticket.SubcategoryId);

        await _unitOfWork.Tickets.AddAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Ticket saved to database: TicketId={TicketId}, CreatedByUserId={CreatedByUserId}",
            ticket.Id, ticket.CreatedByUserId);

        // DEBUG: Immediately query DB to verify ticket exists
        var verifyTicket = await _unitOfWork.Tickets.GetBasicByIdAsync(ticket.Id);
        
        if (verifyTicket == null)
        {
            _logger.LogError("CRITICAL: Ticket {TicketId} was saved but cannot be found in database!", ticket.Id);
        }
        else
        {
            _logger.LogInformation("DEBUG VERIFY: Ticket {TicketId} confirmed in DB - CreatedByUserId={CreatedByUserId}, Status={Status}, CategoryId={CategoryId}, SubcategoryId={SubcategoryId}",
                verifyTicket.Id, verifyTicket.CreatedByUserId, verifyTicket.Status, verifyTicket.CategoryId, verifyTicket.SubcategoryId);
        }

        // Save dynamic field values
        if (request.DynamicFields != null && request.DynamicFields.Any() && fieldDefinitions != null && fieldDefinitions.Any())
        {
            // Reload ticket to get the navigation property
            ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticket.Id) ?? ticket;
            
            foreach (var fieldValue in request.DynamicFields)
            {
                var fieldDef = fieldDefinitions.FirstOrDefault(f => f.Id == fieldValue.FieldDefinitionId);
                if (fieldDef != null)
                {
                    var ticketFieldValue = new TicketFieldValue
                    {
                        TicketId = ticket.Id,
                        FieldDefinitionId = fieldValue.FieldDefinitionId,
                        Value = fieldValue.Value.Trim(),
                        CreatedAt = DateTime.UtcNow
                    };
                    // Add to ticket's FieldValues collection
                    ticket.FieldValues.Add(ticketFieldValue);
                }
            }
            await _unitOfWork.Tickets.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // AUTO-ASSIGNMENT: Assign ticket to technician based on subcategory permissions
        // ═══════════════════════════════════════════════════════════════════════════════════
        
        if (request.SubcategoryId.HasValue)
        {
            _logger.LogDebug("Attempting auto-assignment for ticket {TicketId} with SubcategoryId={SubcategoryId}",
                ticket.Id, request.SubcategoryId.Value);

            // Find technicians who have permission for this subcategory
            var eligibleTechnicianUserIds = await _unitOfWork.Technicians.GetTechnicianUserIdsBySubcategoryAsync(request.SubcategoryId.Value);
            var allTechnicians = await _unitOfWork.Technicians.GetAllAsync();
            var eligibleTechnicians = allTechnicians
                .Where(t => t.IsActive && t.UserId != null && eligibleTechnicianUserIds.Contains(t.UserId.Value))
                .ToList();

            if (eligibleTechnicians.Any())
            {
                _logger.LogDebug("Found {Count} eligible technicians for SubcategoryId={SubcategoryId}",
                    eligibleTechnicians.Count, request.SubcategoryId.Value);

                // Get active ticket counts for each technician (for load balancing)
                var technicianLoads = new List<(Technician tech, int activeTicketCount)>();
                
                foreach (var tech in eligibleTechnicians)
                {
                    // Count tickets assigned to this technician that are not closed/completed
                    var allTickets = await _unitOfWork.Tickets.GetTicketsAsync(UserRole.Admin, Guid.Empty);
                    var activeTicketCount = allTickets.Count(t => 
                        (t.TechnicianId == tech.Id || (tech.UserId != null && t.AssignedToUserId == tech.UserId))
                        && t.Status != TicketStatus.Closed 
                        && t.Status != TicketStatus.Resolved);

                    technicianLoads.Add((tech, activeTicketCount));
                }

                // Select technician with least active tickets (load balancing)
                var selectedTechnician = technicianLoads
                    .OrderBy(t => t.activeTicketCount)
                    .First().tech;

                // Assign ticket and set status to Open (not InProgress yet - technician will set that)
                ticket = await _unitOfWork.Tickets.GetByIdAsync(ticket.Id) ?? ticket;
                ticket.TechnicianId = selectedTechnician.Id;
                ticket.AssignedToUserId = selectedTechnician.UserId; // CRITICAL: Set to Technician.UserId
                ticket.Status = TicketStatus.Open; // Changed from InProgress to Open
                ticket.UpdatedAt = DateTime.UtcNow;

                _logger.LogInformation("Auto-assigning ticket: TicketId={TicketId}, TechnicianId={TechnicianId}, AssignedToUserId={AssignedToUserId}, Status={Status}",
                    ticket.Id, selectedTechnician.Id, selectedTechnician.UserId, ticket.Status);

                await _unitOfWork.Tickets.UpdateAsync(ticket);
                await _unitOfWork.SaveChangesAsync();

                // DEBUG: Verify assignment in DB
                var verifyAssigned = await _unitOfWork.Tickets.GetBasicByIdAsync(ticket.Id);
                
                _logger.LogInformation("Auto-assigned ticket {TicketId} to technician {TechnicianId} ({TechnicianName}, UserId={UserId}, ActiveTickets={ActiveTicketCount})",
                    ticket.Id, selectedTechnician.Id, selectedTechnician.FullName, selectedTechnician.UserId, 
                    technicianLoads.First(t => t.tech.Id == selectedTechnician.Id).activeTicketCount);
                
                if (verifyAssigned != null)
                {
                    _logger.LogInformation("DEBUG VERIFY ASSIGNMENT: Ticket {TicketId} - AssignedToUserId={AssignedToUserId}, TechnicianId={TechnicianId}, Status={Status}",
                        verifyAssigned.Id, verifyAssigned.AssignedToUserId, verifyAssigned.TechnicianId, verifyAssigned.Status);
                }

                // Notify assigned Technician when ticket is assigned (if setting enabled)
                if (selectedTechnician.UserId.HasValue)
                {
                    await _notificationService.NotifyTicketAssignedAsync(ticket.Id, ticket.Title, new List<Guid> { selectedTechnician.UserId.Value });
                }
            }
            else
            {
                _logger.LogInformation("No eligible technicians found for SubcategoryId={SubcategoryId}. Ticket {TicketId} remains unassigned.",
                    request.SubcategoryId.Value, ticket.Id);
            }
        }
        else
        {
            _logger.LogDebug("Ticket {TicketId} created without SubcategoryId. Skipping auto-assignment.",
                ticket.Id);
        }

        // Notify Admins when ticket is created (if setting enabled)
        await _notificationService.NotifyTicketCreatedAsync(ticket.Id, ticket.Title, ticket.CreatedByUserId, ticket.CategoryId, ticket.SubcategoryId);

        // Reload ticket with all relationships for response
        var finalTicket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticket.Id);
        if (finalTicket == null)
        {
            _logger.LogError("CRITICAL: Ticket {TicketId} not found after creation!", ticket.Id);
            return null;
        }

        // DEBUG: Final verification query
        var debugTicket = await _unitOfWork.Tickets.GetBasicByIdAsync(finalTicket.Id);
        
        if (debugTicket != null)
        {
            _logger.LogInformation("DEBUG FINAL VERIFY: Ticket {TicketId} - CreatedByUserId={CreatedByUserId}, AssignedToUserId={AssignedToUserId}, TechnicianId={TechnicianId}, Status={Status}",
                debugTicket.Id, debugTicket.CreatedByUserId, debugTicket.AssignedToUserId, debugTicket.TechnicianId, debugTicket.Status);
        }

        _logger.LogInformation("CreateTicketAsync COMPLETE: TicketId={TicketId}, CreatedByUserId={CreatedByUserId}, AssignedToUserId={AssignedToUserId}, TechnicianId={TechnicianId}, Status={Status}",
            finalTicket.Id, finalTicket.CreatedByUserId, finalTicket.AssignedToUserId, finalTicket.TechnicianId, finalTicket.Status);

        return MapToResponse(finalTicket);
    }

    public async Task<TicketResponse?> UpdateTicketAsync(Guid id, Guid userId, UserRole role, TicketUpdateRequest request)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(id);
        if (ticket == null)
        {
            return null;
        }

        // Validate permission rules
        if (role == UserRole.Client && ticket.CreatedByUserId != userId)
        {
            return null;
        }
        if (role == UserRole.Technician)
        {
            // Check both old single assignment and new multi-technician assignment
            var isAssigned = ticket.TechnicianId == userId || 
                            ticket.AssignedToUserId == userId ||
                            ticket.AssignedTechnicians.Any(tt => tt.TechnicianUserId == userId);
            if (!isAssigned)
            {
                return null;
            }
        }

        if (request.Description != null && role != UserRole.Technician)
        {
            ticket.Description = request.Description;
        }

        if (request.Priority.HasValue && role != UserRole.Technician)
        {
            ticket.Priority = request.Priority.Value;
        }

        TicketStatus? oldStatus = null;
        if (request.Status.HasValue)
        {
            oldStatus = ticket.Status;
            var newStatus = request.Status.Value;

            // Validate status transitions based on role
            if (role == UserRole.Client)
            {
                // Clients can only reopen (set to Open) closed/resolved tickets
                if (newStatus == TicketStatus.Open && 
                    (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed))
                {
                    ticket.Status = newStatus;
                }
                else
                {
                    // Clients cannot change status to other values (ignore silently)
                    _logger.LogInformation("Client attempted invalid status change from {OldStatus} to {NewStatus}. Ignored.", 
                        ticket.Status, newStatus);
                }
            }
            else if (role == UserRole.Technician)
            {
                // Technicians cannot set Closed (only Admin can close)
                if (newStatus == TicketStatus.Closed)
                {
                    throw new StatusChangeForbiddenException("Technicians cannot close tickets. Only Admins can set status to Closed.");
                }
                // Technicians cannot set Submitted (only system/creation can set that)
                if (newStatus == TicketStatus.Submitted)
                {
                    throw new StatusChangeForbiddenException("Technicians cannot set status to Submitted. Only system can set initial status.");
                }
                // Technicians can set: Open, InProgress, Resolved, Viewed
                if (newStatus == TicketStatus.Open || 
                    newStatus == TicketStatus.InProgress || 
                    newStatus == TicketStatus.Resolved ||
                    newStatus == TicketStatus.Viewed)
                {
                    ticket.Status = newStatus;
                }
                else
                {
                    throw new StatusChangeForbiddenException($"Technicians cannot set status to {newStatus}.");
                }
            }
            else if (role == UserRole.Admin)
            {
                // Admin can set any status
                ticket.Status = newStatus;
            }
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
        await _unitOfWork.Tickets.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Create activity if status changed
        if (oldStatus.HasValue && oldStatus.Value != ticket.Status)
        {
            var actor = await _unitOfWork.Users.GetByIdAsync(userId);
            var actorName = actor?.FullName ?? "Unknown";
            await CreateActivityAsync(id, userId, TicketActivityType.StatusChanged,
                $"{actorName} changed status from {oldStatus.Value} to {ticket.Status}");

            // Broadcast collaboration update via SignalR
            try
            {
                var groupName = $"ticket:{id}";
                var collaborationData = await GetCollaborationDataAsync(id, userId, role);
                if (collaborationData != null)
                {
                    await _notificationHub.SendToGroupAsync(groupName, "ticket:collaborationUpdated", collaborationData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast collaboration update via SignalR for TicketId={TicketId}", id);
            }
        }

        return await GetTicketAsync(id, userId, role);
    }

    public async Task<TicketResponse?> AssignTicketAsync(Guid id, Guid technicianId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdAsync(id);
        if (ticket == null)
        {
            return null;
        }

        // Load technician to get UserId (required for AssignedToUserId foreign key)
        var technician = await _unitOfWork.Technicians.GetByIdAsync(technicianId);
        
        if (technician == null || !technician.IsActive)
        {
            return null; // Technician not found or inactive
        }

        // Set both TechnicianId (for display/navigation) and AssignedToUserId (for filtering/queries)
        ticket.TechnicianId = technicianId;
        ticket.AssignedToUserId = technician.UserId; // CRITICAL: Set to Technician.UserId (User.Id), not null
        ticket.Status = TicketStatus.Open; // Changed from InProgress to Open
        ticket.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Tickets.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Notify assigned Technician when ticket is assigned (if setting enabled)
        // Use ticket.AssignedToUserId (already set above) - it's the authoritative field
        if (ticket.AssignedToUserId.HasValue)
        {
            await _notificationService.NotifyTicketAssignedAsync(id, ticket.Title, new List<Guid> { ticket.AssignedToUserId.Value });
        }

        return await GetTicketAsync(id, Guid.Empty, UserRole.Admin);
    }

    public async Task<IEnumerable<TicketMessageDto>> GetMessagesAsync(Guid ticketId, Guid userId, UserRole role)
    {
        var ticket = await GetTicketAsync(ticketId, userId, role);
        if (ticket == null)
        {
            return Enumerable.Empty<TicketMessageDto>();
        }

        var messages = await _unitOfWork.TicketMessages.GetByTicketIdAsync(ticketId);
        var messagesList = messages.ToList();
        
        // Get all user IDs to load user data
        var userIds = messagesList.Select(m => m.AuthorUserId).Distinct().ToList();
        var users = new Dictionary<Guid, User>();
        foreach (var uid in userIds)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(uid);
            if (user != null)
            {
                users[uid] = user;
            }
        }

        return messagesList.OrderBy(m => m.CreatedAt).Select(m => new TicketMessageDto
        {
            Id = m.Id,
            AuthorUserId = m.AuthorUserId,
            AuthorName = users.GetValueOrDefault(m.AuthorUserId)?.FullName ?? "Unknown",
            AuthorEmail = users.GetValueOrDefault(m.AuthorUserId)?.Email ?? "",
            Message = m.Message,
            CreatedAt = m.CreatedAt,
            Status = m.Status
        });
    }

    public async Task<TicketMessageDto?> AddMessageAsync(Guid ticketId, Guid authorId, string message, TicketStatus? status = null)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);
        if (ticket == null)
        {
            return null;
        }

        var author = await _unitOfWork.Users.GetByIdAsync(authorId);
        if (author == null)
        {
            return null;
        }

        // Access control: Client can only access their own tickets
        if (author.Role == UserRole.Client && ticket.CreatedByUserId != authorId)
        {
            return null;
        }

        // Access control: Technician can only access assigned tickets (both old single and new multi-assignment)
        if (author.Role == UserRole.Technician)
        {
            var isAssigned = ticket.TechnicianId == authorId || 
                            ticket.AssignedToUserId == authorId ||
                            ticket.AssignedTechnicians.Any(tt => tt.TechnicianUserId == authorId);
            if (!isAssigned)
            {
                return null;
            }
        }

        // ═══════════════════════════════════════════════════════════════════════════════════
        // STATUS CHANGE PERMISSION RULES (SECURITY-CRITICAL)
        // ═══════════════════════════════════════════════════════════════════════════════════
        // CLOSE (Resolved/Closed): Technician & Admin ONLY - Client FORBIDDEN
        // REOPEN (InProgress from Resolved/Closed): All roles allowed
        // Other status changes: Technician & Admin only
        // ═══════════════════════════════════════════════════════════════════════════════════
        var statusChangedToClosed = false;
        if (status.HasValue)
        {
            var newStatus = status.Value;
            var isClosingStatus = newStatus == TicketStatus.Resolved || newStatus == TicketStatus.Closed;
            var isReopening = newStatus == TicketStatus.Open && 
                              (ticket.Status == TicketStatus.Resolved || ticket.Status == TicketStatus.Closed);

            if (author.Role == UserRole.Client)
            {
                // Client can REOPEN (set Open on resolved/closed ticket)
                // Client CANNOT close tickets (Resolved or Closed)
                if (isClosingStatus)
                {
                    // FORBIDDEN: Client cannot close tickets - throw exception for controller to handle
                    throw new StatusChangeForbiddenException("Clients cannot close tickets. Only Admins can set status to Resolved or Closed.");
                }

                // Client can only set: Open (reopen)
                if (isReopening)
                {
                    ticket.Status = newStatus;
                }
                // Other status changes by Client are silently ignored (no error, just don't apply)
            }
            else if (author.Role == UserRole.Technician)
            {
                // Technicians can set Open, InProgress, Resolved, Viewed
                // They CANNOT set Closed (only Admin can close) or Submitted (only system can set that)
                if (newStatus == TicketStatus.Closed)
                {
                    throw new StatusChangeForbiddenException("Technicians cannot close tickets. Only Admins can set status to Closed.");
                }
                if (newStatus == TicketStatus.Submitted)
                {
                    throw new StatusChangeForbiddenException("Technicians cannot set status to Submitted. Only system can set initial status.");
                }
                if (newStatus == TicketStatus.Open || 
                    newStatus == TicketStatus.InProgress || 
                    newStatus == TicketStatus.Resolved ||
                    newStatus == TicketStatus.Viewed)
                {
                    var previousStatus = ticket.Status;
                    ticket.Status = newStatus;
                    // Track if status changed to resolved (for notification)
                    if (newStatus == TicketStatus.Resolved && previousStatus != newStatus)
                    {
                        statusChangedToClosed = true;
                    }
                }
            }
            else
            {
                // Admin can set any status
                var previousStatus = ticket.Status;
                ticket.Status = newStatus;
                // Track if status changed to closed/resolved (for notification)
                if (isClosingStatus && previousStatus != newStatus)
                {
                    statusChangedToClosed = true;
                }
            }
        }

        ticket.UpdatedAt = DateTime.UtcNow;

        var ticketMessage = new TicketMessage
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            AuthorUserId = authorId,
            Message = message,
            CreatedAt = DateTime.UtcNow,
            Status = status ?? ticket.Status
        };

        await _unitOfWork.TicketMessages.AddAsync(ticketMessage);
        await _unitOfWork.Tickets.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Create activity for comment
        var actorName = author.FullName ?? "Unknown";
        await CreateActivityAsync(ticketId, authorId, TicketActivityType.CommentAdded,
            $"{actorName} added a comment");

        // Notify on new message (Technician and Client, NOT Admin) if setting enabled
        await _notificationService.NotifyTicketMessageAsync(
            ticketId,
            authorId,
            ticket.Title,
            ticket.AssignedToUserId,
            ticket.CreatedByUserId);

        // Notify Client when ticket is closed/resolved (if setting enabled and status changed)
        if (statusChangedToClosed)
        {
            await _notificationService.NotifyTicketClosedAsync(ticketId, ticket.CreatedByUserId, ticket.Title, ticket.Status);
        }

        // Broadcast collaboration update via SignalR
        try
        {
            var groupName = $"ticket:{ticketId}";
            var collaborationData = await GetCollaborationDataAsync(ticketId, authorId, author.Role);
            if (collaborationData != null)
            {
                await _notificationHub.SendToGroupAsync(groupName, "ticket:collaborationUpdated", collaborationData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast collaboration update via SignalR for TicketId={TicketId}", ticketId);
        }

        // Reload message with author info
        var reloadedMessage = await _unitOfWork.TicketMessages.GetByIdAsync(ticketMessage.Id);
        if (reloadedMessage == null)
        {
            return null;
        }

        return new TicketMessageDto
        {
            Id = reloadedMessage.Id,
            AuthorUserId = reloadedMessage.AuthorUserId,
            AuthorName = author?.FullName ?? "Unknown",
            AuthorEmail = author?.Email ?? "",
            Message = reloadedMessage.Message,
            CreatedAt = reloadedMessage.CreatedAt,
            Status = reloadedMessage.Status
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════════
    // MANUAL TEST CHECKLIST (Swagger):
    // 1. POST /api/Tickets → status=New, assignedToUserId=null, assignedToName/email/phone=null
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
            DueDate = ticket.DueDate,
            DynamicFields = (ticket.FieldValues ?? Enumerable.Empty<TicketFieldValue>())
                .Where(fv => fv.FieldDefinition != null)
                .Select(fv => new TicketDynamicFieldResponse
                {
                    FieldDefinitionId = fv.FieldDefinitionId,
                    Key = fv.FieldDefinition!.Key,
                    Label = fv.FieldDefinition.Label,
                    Type = fv.FieldDefinition.Type,
                    Value = fv.Value
                })
                .ToList()
        };
    }

    public async Task<IEnumerable<TicketCalendarResponse>> GetCalendarTicketsAsync(DateTime startDate, DateTime endDate)
    {
        // Get all tickets within the date range (Admin only - no role filtering)
        var tickets = await _unitOfWork.Tickets.GetCalendarTicketsAsync(startDate, endDate);
        var ticketsList = tickets.ToList();

        return ticketsList.Select(t =>
        {
            // Only show technician name when truly assigned (AssignedToUserId != null)
            var assignedTechnicianName = t.AssignedToUserId != null ? (t.Technician?.FullName ?? t.AssignedToUser?.FullName) : null;
            
            return new TicketCalendarResponse
            {
                Id = t.Id,
                TicketNumber = $"T-{t.Id.ToString("N").Substring(0, 8).ToUpper()}",
                Title = t.Title,
                Status = t.Status,
                Priority = t.Priority,
                CategoryName = t.Category?.Name ?? string.Empty,
                AssignedTechnicianName = assignedTechnicianName,
                CreatedAt = t.CreatedAt,
                DueDate = t.DueDate
            };
        });
    }

    /// <summary>
    /// Get tickets/tasks that need assignment (Admin only)
    /// </summary>
    public async Task<AssignmentQueueResponse> GetAssignmentQueueAsync(string? type = null, TicketStatus? status = null, int? page = null, int? pageSize = null)
    {
        // "needs assignment" = AssignedToUserId == null OR TechnicianId == null
        var allTickets = await _unitOfWork.Tickets.GetTicketsAsync(UserRole.Admin, Guid.Empty, status: status);
        var unassignedTickets = allTickets
            .Where(t => t.AssignedToUserId == null || t.TechnicianId == null)
            .OrderByDescending(t => t.CreatedAt);

        // Apply pagination if provided
        IEnumerable<Ticket> ticketsQuery = unassignedTickets;
        if (page.HasValue && pageSize.HasValue)
        {
            ticketsQuery = ticketsQuery
                .Skip((page.Value - 1) * pageSize.Value)
                .Take(pageSize.Value);
        }

        var tickets = ticketsQuery.ToList();

        var ticketsList = tickets.Select(t => new TicketSummaryDto
        {
            Id = t.Id,
            Title = t.Title,
            Priority = t.Priority,
            Status = t.Status,
            CategoryId = t.CategoryId,
            CategoryName = t.Category?.Name ?? string.Empty,
            SubcategoryId = t.SubcategoryId,
            SubcategoryName = t.Subcategory?.Name,
            CreatedAt = t.CreatedAt,
            AssignedToUserId = t.AssignedToUserId,
            AssignedTechnicianName = t.AssignedToUserId != null 
                ? (t.Technician?.FullName ?? t.AssignedToUser?.FullName) 
                : null
        }).ToList();

        // Filter by type if specified
        var filteredTickets = type?.ToLowerInvariant() switch
        {
            "ticket" => ticketsList,
            "task" => new List<TicketSummaryDto>(), // Tasks not implemented yet
            _ => ticketsList // "all" or null
        };

        return new AssignmentQueueResponse
        {
            Tickets = filteredTickets,
            Tasks = new List<TicketSummaryDto>() // Tasks not implemented yet
        };
    }

    // ═══════════════════════════════════════════════════════════════════════════════════
    // MULTI-TECHNICIAN ASSIGNMENT METHODS
    // ═══════════════════════════════════════════════════════════════════════════════════

    /// <summary>
    /// Helper method to create a TicketActivity entry
    /// </summary>
    private async Task CreateActivityAsync(Guid ticketId, Guid actorUserId, TicketActivityType type, string message)
    {
        var activity = new TicketActivity
        {
            Id = Guid.NewGuid(),
            TicketId = ticketId,
            ActorUserId = actorUserId,
            Type = type,
            Message = message,
            CreatedAt = DateTime.UtcNow
        };

        await _unitOfWork.TicketActivities.AddAsync(activity);
        await _unitOfWork.SaveChangesAsync();
    }

    public async Task<List<TicketTechnicianDto>> AssignTechniciansAsync(Guid ticketId, List<Guid> technicianIds, Guid? leadTechnicianId, Guid actorUserId)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);
        
        if (ticket == null)
        {
            throw new ArgumentException($"Ticket {ticketId} not found", nameof(ticketId));
        }

        // Validate technicians exist and get their UserIds
        var allTechnicians = await _unitOfWork.Technicians.GetAllAsync();
        var technicians = allTechnicians
            .Where(t => technicianIds.Contains(t.Id) && t.IsActive && t.UserId != null)
            .ToList();

        if (technicians.Count != technicianIds.Count)
        {
            throw new ArgumentException("One or more technicians not found or inactive", nameof(technicianIds));
        }

        // Remove existing assignments that are not in the new list
        var existingAssignments = ticket.AssignedTechnicians.ToList();
        var toRemove = existingAssignments.Where(ea => !technicianIds.Contains(ea.TechnicianId)).ToList();
        foreach (var assignment in toRemove)
        {
            ticket.AssignedTechnicians.Remove(assignment);
            await _unitOfWork.TicketTechnicians.DeleteByTicketAndTechnicianAsync(ticketId, assignment.TechnicianUserId);
        }

        // Add or update assignments
        var leadSet = false;
        foreach (var technician in technicians)
        {
            var existing = ticket.AssignedTechnicians.FirstOrDefault(tt => tt.TechnicianId == technician.Id);
            var isLead = leadTechnicianId.HasValue && leadTechnicianId.Value == technician.Id;
            
            if (isLead)
            {
                // Remove lead flag from others
                foreach (var tt in ticket.AssignedTechnicians)
                {
                    tt.IsLead = false;
                    await _unitOfWork.TicketTechnicians.UpdateAsync(tt);
                }
                leadSet = true;
            }

            if (existing != null)
            {
                // Update existing
                existing.TechnicianUserId = technician.UserId!.Value;
                existing.IsLead = isLead;
                if (!leadTechnicianId.HasValue && !leadSet)
                {
                    // First technician becomes lead if no lead specified
                    existing.IsLead = true;
                    leadSet = true;
                }
                await _unitOfWork.TicketTechnicians.UpdateAsync(existing);
            }
            else
            {
                // Create new assignment
                var newAssignment = new TicketTechnician
                {
                    TicketId = ticketId,
                    TechnicianId = technician.Id,
                    TechnicianUserId = technician.UserId!.Value,
                    IsLead = isLead || (!leadSet && technician == technicians.First()),
                    State = TicketTechnicianState.Invited,
                    AssignedAt = DateTime.UtcNow
                };
                
                if (newAssignment.IsLead)
                {
                    leadSet = true;
                }

                ticket.AssignedTechnicians.Add(newAssignment);
                await _unitOfWork.TicketTechnicians.AddAsync(newAssignment);
            }
        }

        // If no lead was set but we have assignments, set first one as lead
        if (!leadSet && ticket.AssignedTechnicians.Any())
        {
            var first = ticket.AssignedTechnicians.First();
            first.IsLead = true;
            await _unitOfWork.TicketTechnicians.UpdateAsync(first);
        }

        // Update backwards-compatible fields (lead technician)
        var leadAssignment = ticket.AssignedTechnicians.FirstOrDefault(tt => tt.IsLead);
        if (leadAssignment != null)
        {
            ticket.TechnicianId = leadAssignment.TechnicianId;
            ticket.AssignedToUserId = leadAssignment.TechnicianUserId;
        }
        else if (ticket.AssignedTechnicians.Any())
        {
            // Fallback to first if no lead set
            var first = ticket.AssignedTechnicians.First();
            ticket.TechnicianId = first.TechnicianId;
            ticket.AssignedToUserId = first.TechnicianUserId;
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Tickets.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Create activity
        var techNames = string.Join(", ", technicians.Select(t => t.FullName));
        await CreateActivityAsync(ticketId, actorUserId, TicketActivityType.AssignmentChanged, 
            $"Technicians assigned: {techNames}");

        // Notify assigned technicians
        var userIdsToNotify = technicians
            .Where(t => t.UserId.HasValue && t.UserId.Value != actorUserId)
            .Select(t => t.UserId!.Value)
            .ToList();
        if (userIdsToNotify.Any())
        {
            await _notificationService.NotifyTicketAssignedAsync(ticketId, ticket.Title, userIdsToNotify);
        }

        // Broadcast collaboration update
        try
        {
            var groupName = $"ticket:{ticketId}";
            var collaborationData = await GetCollaborationDataAsync(ticketId, actorUserId, UserRole.Admin);
            if (collaborationData != null)
            {
                await _notificationHub.SendToGroupAsync(groupName, "ticket:collaborationUpdated", collaborationData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast collaboration update for TicketId={TicketId}", ticketId);
        }

        // Return updated list
        return await GetTicketTechniciansAsync(ticketId, actorUserId, UserRole.Admin);
    }

    public async Task<bool> RemoveTechnicianAsync(Guid ticketId, Guid technicianId, Guid actorUserId)
    {
        var assignment = await _unitOfWork.TicketTechnicians.GetByTicketAndTechnicianIdAsync(ticketId, technicianId);
        if (assignment == null)
        {
            return false;
        }

        var technician = assignment.Technician ?? await _unitOfWork.Technicians.GetByIdAsync(technicianId);
        var technicianName = technician?.FullName ?? "Unknown";

        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);
        if (ticket == null)
        {
            return false;
        }

        await _unitOfWork.TicketTechnicians.DeleteByTicketAndTechnicianAsync(ticketId, assignment.TechnicianUserId);
        
        // Reload ticket to refresh AssignedTechnicians collection
        ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);
        if (ticket == null)
        {
            return false;
        }

        // Update backwards-compatible fields if this was the lead
        if (assignment.IsLead)
        {
            var newLead = ticket.AssignedTechnicians.FirstOrDefault();
            if (newLead != null)
            {
                newLead.IsLead = true;
                ticket.TechnicianId = newLead.TechnicianId;
                ticket.AssignedToUserId = newLead.TechnicianUserId;
                await _unitOfWork.TicketTechnicians.UpdateAsync(newLead);
            }
            else
            {
                ticket.TechnicianId = null;
                ticket.AssignedToUserId = null;
            }
        }

        ticket.UpdatedAt = DateTime.UtcNow;
        await _unitOfWork.Tickets.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Create activity
        await CreateActivityAsync(ticketId, actorUserId, TicketActivityType.AssignmentChanged, 
            $"Technician {technicianName} removed from ticket");

        // Broadcast collaboration update
        try
        {
            var groupName = $"ticket:{ticketId}";
            var collaborationData = await GetCollaborationDataAsync(ticketId, actorUserId, UserRole.Admin);
            if (collaborationData != null)
            {
                await _notificationHub.SendToGroupAsync(groupName, "ticket:collaborationUpdated", collaborationData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast collaboration update for TicketId={TicketId}", ticketId);
        }

        return true;
    }

    public async Task<List<TicketTechnicianDto>> GetTicketTechniciansAsync(Guid ticketId, Guid userId, UserRole role)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);

        if (ticket == null)
        {
            return new List<TicketTechnicianDto>();
        }

        // Authorization: Admin can view all, Technician can only view if assigned
        if (role == UserRole.Technician)
        {
            var isAssigned = ticket.AssignedTechnicians.Any(tt => tt.TechnicianUserId == userId) ||
                            ticket.AssignedToUserId == userId;
            if (!isAssigned)
            {
                return new List<TicketTechnicianDto>();
            }
        }

        return ticket.AssignedTechnicians.Select(tt => new TicketTechnicianDto
        {
            TechnicianId = tt.TechnicianId,
            TechnicianUserId = tt.TechnicianUserId,
            TechnicianName = tt.Technician?.FullName ?? "Unknown",
            TechnicianEmail = tt.Technician?.Email ?? "Unknown",
            IsLead = tt.IsLead,
            State = tt.State,
            AssignedAt = tt.AssignedAt
        }).ToList();
    }

    public async Task<TicketTechnicianDto?> UpdateTechnicianStateAsync(Guid ticketId, Guid technicianUserId, TicketTechnicianState newState)
    {
        var assignment = await _unitOfWork.TicketTechnicians.GetByTicketAndTechnicianAsync(ticketId, technicianUserId);

        if (assignment == null)
        {
            return null;
        }

        // Load ticket for updates
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);
        if (ticket == null)
        {
            return null;
        }

        var oldState = assignment.State;
        assignment.State = newState;
        await _unitOfWork.TicketTechnicians.UpdateAsync(assignment);
        await _unitOfWork.SaveChangesAsync();

        // Get actor name for activity
        var actor = await _unitOfWork.Users.GetByIdAsync(technicianUserId);
        var actorName = actor?.FullName ?? "Unknown";

        // Create activity
        await CreateActivityAsync(ticketId, technicianUserId, TicketActivityType.TechnicianStateChanged,
            $"{actorName} changed state from {oldState} to {newState}");

        // Notify other assigned technicians
        await _notificationService.NotifyActivityToAssignedTechniciansAsync(ticketId, technicianUserId,
            $"{actorName} updated their state on ticket '{ticket.Title}' to {newState}");

        // Broadcast collaboration update via SignalR
        try
        {
            var groupName = $"ticket:{ticketId}";
            var collaborationData = await GetCollaborationDataAsync(ticketId, technicianUserId, UserRole.Technician);
            if (collaborationData != null)
            {
                await _notificationHub.SendToGroupAsync(groupName, "ticket:collaborationUpdated", collaborationData);
            }
        }
        catch (Exception ex)
        {
            _logger.LogWarning(ex, "Failed to broadcast collaboration update via SignalR for TicketId={TicketId}", ticketId);
        }

        // Auto-update ticket status based on technician states
        var allAssignments = await _unitOfWork.TicketTechnicians.GetByTicketIdAsync(ticketId);
        var assignmentsList = allAssignments.ToList();

        // If any technician is InProgress, set ticket to InProgress (if not closed/resolved)
        if (newState == TicketTechnicianState.InProgress && 
            ticket.Status != TicketStatus.Closed && 
            ticket.Status != TicketStatus.Resolved)
        {
            ticket.Status = TicketStatus.InProgress;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Tickets.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();
        }

        // If ALL technicians are Done, set ticket to Resolved
        if (assignmentsList.All(tt => tt.State.ToString() == "Done") &&
            assignmentsList.Any())
        {
            ticket.Status = TicketStatus.Resolved;
            ticket.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.Tickets.UpdateAsync(ticket);
            await _unitOfWork.SaveChangesAsync();
            
            await CreateActivityAsync(ticketId, technicianUserId, TicketActivityType.StatusChanged,
                $"Ticket resolved (all technicians completed their work)");

            // Broadcast collaboration update via SignalR
            try
            {
                var groupName = $"ticket:{ticketId}";
                var collaborationData = await GetCollaborationDataAsync(ticketId, technicianUserId, UserRole.Technician);
                if (collaborationData != null)
                {
                    await _notificationHub.SendToGroupAsync(groupName, "ticket:collaborationUpdated", collaborationData);
                }
            }
            catch (Exception ex)
            {
                _logger.LogWarning(ex, "Failed to broadcast collaboration update via SignalR for TicketId={TicketId}", ticketId);
            }
        }

        // Reload assignment to get updated technician info
        var updatedAssignment = await _unitOfWork.TicketTechnicians.GetByTicketAndTechnicianAsync(ticketId, technicianUserId);
        if (updatedAssignment == null)
        {
            return null;
        }

        var technician = await _unitOfWork.Technicians.GetByIdAsync(updatedAssignment.TechnicianId);
        return new TicketTechnicianDto
        {
            TechnicianId = updatedAssignment.TechnicianId,
            TechnicianUserId = updatedAssignment.TechnicianUserId,
            TechnicianName = technician?.FullName ?? "Unknown",
            TechnicianEmail = technician?.Email ?? "Unknown",
            IsLead = updatedAssignment.IsLead,
            State = updatedAssignment.State,
            AssignedAt = updatedAssignment.AssignedAt
        };
    }

    public async Task<List<TicketActivityDto>> GetTicketActivitiesAsync(Guid ticketId, Guid userId, UserRole role)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);

        if (ticket == null)
        {
            return new List<TicketActivityDto>();
        }

        // Authorization: Admin and Client (owner) can view all, Technician can only view if assigned
        if (role == UserRole.Client && ticket.CreatedByUserId != userId)
        {
            return new List<TicketActivityDto>();
        }

        if (role == UserRole.Technician)
        {
            var isAssigned = ticket.AssignedTechnicians.Any(tt => tt.TechnicianUserId == userId) ||
                            ticket.AssignedToUserId == userId;
            if (!isAssigned)
            {
                return new List<TicketActivityDto>();
            }
        }

        var activities = await _unitOfWork.TicketActivities.GetByTicketIdAsync(ticketId);
        var activitiesList = activities.OrderBy(a => a.CreatedAt).ToList();

        var userIds = activitiesList.Select(a => a.ActorUserId).Distinct().ToList();
        var users = new Dictionary<Guid, User>();
        foreach (var uid in userIds)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(uid);
            if (user != null)
            {
                users[uid] = user;
            }
        }

        return activitiesList.Select(a => new TicketActivityDto
        {
            Id = a.Id,
            TicketId = a.TicketId,
            ActorUserId = a.ActorUserId,
            ActorName = users.GetValueOrDefault(a.ActorUserId)?.FullName ?? "Unknown",
            ActorEmail = users.GetValueOrDefault(a.ActorUserId)?.Email ?? "",
            Type = a.Type,
            Message = a.Message,
            CreatedAt = a.CreatedAt
        }).ToList();
    }

    public async Task UpdateWorkSessionAsync(Guid ticketId, Guid technicianUserId, UpdateWorkSessionRequest request)
    {
        // Verify technician is assigned to the ticket
        var assignment = await _unitOfWork.TicketTechnicians.GetByTicketAndTechnicianAsync(ticketId, technicianUserId);

        if (assignment == null)
        {
            throw new UnauthorizedAccessException("Technician is not assigned to this ticket");
        }

        // Get or create work session
        var workSession = await _unitOfWork.TicketWorkSessions.GetByTicketAndTechnicianAsync(ticketId, technicianUserId);

        if (workSession == null)
        {
            workSession = new TicketWorkSession
            {
                TicketId = ticketId,
                TechnicianUserId = technicianUserId,
                TechnicianId = assignment.TechnicianId,
                WorkingOn = request.WorkingOn.Trim(),
                Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
                State = request.State,
                UpdatedAt = DateTime.UtcNow
            };
            await _unitOfWork.TicketWorkSessions.AddOrUpdateAsync(workSession);
        }
        else
        {
            workSession.WorkingOn = request.WorkingOn.Trim();
            workSession.Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim();
            workSession.State = request.State;
            workSession.UpdatedAt = DateTime.UtcNow;
            await _unitOfWork.TicketWorkSessions.AddOrUpdateAsync(workSession);
        }

        await _unitOfWork.SaveChangesAsync();

        // Create activity entry
        var actor = await _unitOfWork.Users.GetByIdAsync(technicianUserId);
        var actorName = actor?.FullName ?? "Unknown";
        var summary = $"در حال بررسی: {request.WorkingOn}";
        if (!string.IsNullOrWhiteSpace(request.Note))
        {
            summary += $" - {request.Note}";
        }
        await CreateActivityAsync(ticketId, technicianUserId, TicketActivityType.WorkNoteAdded,
            $"{actorName} is now {request.State}: {request.WorkingOn}");

        // Notify other assigned technicians (except actor)
        var otherAssignedTechs = await _unitOfWork.TicketTechnicians.GetByTicketIdAsync(ticketId);
        var otherTechUserIds = otherAssignedTechs
            .Where(tt => tt.TechnicianUserId != technicianUserId)
            .Select(tt => tt.TechnicianUserId)
            .Distinct()
            .ToList();

        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);
        foreach (var techUserId in otherTechUserIds)
        {
            await _notificationService.NotifyActivityToAssignedTechniciansAsync(ticketId, technicianUserId,
                $"{actorName} updated their work on ticket '{ticket?.Title ?? "Unknown"}' : {request.WorkingOn}");
        }

        // Broadcast collaboration update via SignalR
        try
        {
            var groupName = $"ticket:{ticketId}";
            var collaborationData = await GetCollaborationDataAsync(ticketId, technicianUserId, UserRole.Technician);
            if (collaborationData != null)
            {
                await _notificationHub.SendToGroupAsync(groupName, "ticket:collaborationUpdated", collaborationData);
            }
        }
        catch (Exception ex)
        {
            // Don't fail if SignalR fails
            _logger.LogWarning(ex, "Failed to broadcast collaboration update via SignalR for TicketId={TicketId}", ticketId);
        }
    }

    public async Task<TicketCollaborationResponse?> GetCollaborationDataAsync(Guid ticketId, Guid userId, UserRole role)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);

        if (ticket == null)
        {
            return null;
        }

        // Authorization: Admin can view, Technician can only view if assigned
        if (role == UserRole.Technician)
        {
            var isAssigned = ticket.AssignedTechnicians.Any(tt => tt.TechnicianUserId == userId) ||
                            ticket.AssignedToUserId == userId ||
                            ticket.TechnicianId == userId;
            if (!isAssigned)
            {
                return null;
            }
        }

        // Get latest activity
        var allActivities = await _unitOfWork.TicketActivities.GetByTicketIdAsync(ticketId);
        var lastActivityEntity = allActivities.OrderByDescending(ta => ta.CreatedAt).FirstOrDefault();

        TicketActivityDto? lastActivity = null;
        if (lastActivityEntity != null)
        {
            var actor = await _unitOfWork.Users.GetByIdAsync(lastActivityEntity.ActorUserId);
            lastActivity = new TicketActivityDto
            {
                Id = lastActivityEntity.Id,
                TicketId = lastActivityEntity.TicketId,
                ActorUserId = lastActivityEntity.ActorUserId,
                ActorName = actor?.FullName ?? "Unknown",
                ActorEmail = actor?.Email ?? "",
                Type = lastActivityEntity.Type,
                Message = lastActivityEntity.Message,
                CreatedAt = lastActivityEntity.CreatedAt
            };
        }

        // Get recent activities (up to 10)
        var recentActivityEntities = await _unitOfWork.TicketActivities.GetRecentByTicketIdAsync(ticketId, 10);
        var recentActivitiesList = recentActivityEntities.ToList();

        var userIds = recentActivitiesList.Select(a => a.ActorUserId).Distinct().ToList();
        var users = new Dictionary<Guid, User>();
        foreach (var uid in userIds)
        {
            var user = await _unitOfWork.Users.GetByIdAsync(uid);
            if (user != null)
            {
                users[uid] = user;
            }
        }

        var recentActivities = recentActivitiesList.Select(a => new TicketActivityDto
        {
            Id = a.Id,
            TicketId = a.TicketId,
            ActorUserId = a.ActorUserId,
            ActorName = users.GetValueOrDefault(a.ActorUserId)?.FullName ?? "Unknown",
            ActorEmail = users.GetValueOrDefault(a.ActorUserId)?.Email ?? "",
            Type = a.Type,
            Message = a.Message,
            CreatedAt = a.CreatedAt
        }).ToList();

        // Get active technicians with work sessions
        var activeTechnicians = new List<ActiveTechnicianDto>();
        var workSessions = await _unitOfWork.TicketWorkSessions.GetByTicketIdAsync(ticketId);
        var workSessionsList = workSessions
            .OrderByDescending(ws => ws.UpdatedAt)
            .ToList();

        foreach (var ws in workSessionsList)
        {
            // Only include if technician is still assigned
            var isStillAssigned = ticket.AssignedTechnicians.Any(tt => tt.TechnicianUserId == ws.TechnicianUserId);
            if (isStillAssigned)
            {
                var technician = await _unitOfWork.Technicians.GetByIdAsync(ws.TechnicianId);
                activeTechnicians.Add(new ActiveTechnicianDto
                {
                    TechnicianId = ws.TechnicianId,
                    TechnicianUserId = ws.TechnicianUserId,
                    Name = technician?.FullName ?? "Unknown",
                    WorkingOn = ws.WorkingOn,
                    Note = ws.Note,
                    State = ws.State,
                    UpdatedAt = ws.UpdatedAt
                });
            }
        }

        return new TicketCollaborationResponse
        {
            TicketId = ticketId,
            Status = ticket.Status,
            LastActivity = lastActivity,
            RecentActivities = recentActivities,
            ActiveTechnicians = activeTechnicians
        };
    }

    public async Task<IEnumerable<TicketResponse>> GetTechnicianTicketsAsync(Guid technicianUserId, string? mode = null)
    {
        // Get technician entity by UserId
        var technician = await _unitOfWork.Technicians.GetByUserIdAsync(technicianUserId);
        if (technician == null)
        {
            _logger.LogWarning("GetTechnicianTicketsAsync: Technician not found for UserId={UserId}", technicianUserId);
            return Enumerable.Empty<TicketResponse>();
        }

        // Get all ticket assignments for this technician
        var assignments = await _unitOfWork.TicketTechnicians.GetByTechnicianUserIdAsync(technicianUserId);
        var ticketIds = assignments.Select(a => a.TicketId).Distinct().ToList();

        if (!ticketIds.Any())
        {
            return Enumerable.Empty<TicketResponse>();
        }

        // Get all tickets for these IDs with includes
        var allTickets = new List<Ticket>();
        foreach (var ticketId in ticketIds)
        {
            var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);
            if (ticket != null)
            {
                allTickets.Add(ticket);
            }
        }

        IEnumerable<Ticket> filteredTickets = allTickets;

        // Filter by mode: "assigned" | "responsible" | null (all)
        if (mode == "assigned")
        {
            // Only tickets where technician is assigned (in AssignedTechnicians)
            filteredTickets = allTickets.Where(t => t.AssignedTechnicians.Any(tt => tt.TechnicianUserId == technicianUserId));
        }
        else if (mode == "responsible")
        {
            // Only tickets where technician is the responsible technician
            filteredTickets = allTickets.Where(t => t.ResponsibleUserId == technicianUserId);
        }
        // else mode == null: return all tickets assigned to technician

        var ticketsList = filteredTickets.ToList();
        _logger.LogDebug("GetTechnicianTicketsAsync: TechnicianUserId={UserId}, Mode={Mode}, Count={Count}",
            technicianUserId, mode ?? "all", ticketsList.Count);

        return ticketsList.Select(MapToResponse);
    }

    public async Task<bool> SetResponsibleTechnicianAsync(Guid ticketId, Guid responsibleTechnicianId, Guid actorUserId, UserRole actorRole)
    {
        var ticket = await _unitOfWork.Tickets.GetByIdWithIncludesAsync(ticketId);
        if (ticket == null)
        {
            _logger.LogWarning("SetResponsibleTechnicianAsync: Ticket not found. TicketId={TicketId}", ticketId);
            return false;
        }

        // Authorization: Only Admin or assigned/lead technician can set responsible
        if (actorRole != UserRole.Admin)
        {
            var isAssignedOrLead = ticket.AssignedTechnicians.Any(tt => 
                tt.TechnicianUserId == actorUserId && tt.IsLead);
            
            if (!isAssignedOrLead)
            {
                _logger.LogWarning("SetResponsibleTechnicianAsync: Unauthorized. ActorUserId={UserId}, TicketId={TicketId}", actorUserId, ticketId);
                return false;
            }
        }

        // Validate that target technician is already assigned to the ticket
        var targetAssignment = ticket.AssignedTechnicians.FirstOrDefault(tt => tt.TechnicianId == responsibleTechnicianId);
        if (targetAssignment == null)
        {
            _logger.LogWarning("SetResponsibleTechnicianAsync: Target technician not assigned. TicketId={TicketId}, TechnicianId={TechId}", 
                ticketId, responsibleTechnicianId);
            return false;
        }

        // Get technician and user entities for logging
        var technician = await _unitOfWork.Technicians.GetByIdAsync(responsibleTechnicianId);
        var actor = await _unitOfWork.Users.GetByIdAsync(actorUserId);
        var actorName = actor?.FullName ?? "Unknown";
        var technicianName = technician?.FullName ?? "Unknown";

        // Store previous responsible for activity log
        var previousResponsibleUserId = ticket.ResponsibleUserId;
        var previousResponsibleName = "None";
        if (previousResponsibleUserId.HasValue)
        {
            var prevTech = await _unitOfWork.Technicians.GetByUserIdAsync(previousResponsibleUserId.Value);
            previousResponsibleName = prevTech?.FullName ?? "Unknown";
        }

        // Update ticket responsible fields
        ticket.ResponsibleTechnicianId = responsibleTechnicianId;
        ticket.ResponsibleUserId = targetAssignment.TechnicianUserId;
        ticket.ResponsibleSetByUserId = actorUserId;
        ticket.ResponsibleSetAt = DateTime.UtcNow;
        ticket.UpdatedAt = DateTime.UtcNow;

        await _unitOfWork.Tickets.UpdateAsync(ticket);
        await _unitOfWork.SaveChangesAsync();

        // Create activity log entry
        var activityMessage = previousResponsibleUserId.HasValue
            ? $"{actorName} changed responsible technician from {previousResponsibleName} to {technicianName}"
            : $"{actorName} set {technicianName} as responsible technician";

        await CreateActivityAsync(ticketId, actorUserId, TicketActivityType.ResponsibleChanged, activityMessage);

        _logger.LogInformation("SetResponsibleTechnicianAsync: Success. TicketId={TicketId}, ResponsibleTechnicianId={TechId}, ActorUserId={ActorId}",
            ticketId, responsibleTechnicianId, actorUserId);

        return true;
    }
}

