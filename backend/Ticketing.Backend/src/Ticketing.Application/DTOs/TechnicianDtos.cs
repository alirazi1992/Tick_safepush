using System.ComponentModel.DataAnnotations;

namespace Ticketing.Application.DTOs;

public class TechnicianResponse
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    /// <summary>
    /// Linked User.Id (for JWT sub / assignment). Null = technician cannot be assigned to tickets.
    /// </summary>
    public Guid? UserId { get; set; }
    /// <summary>
    /// Email of the linked User account (if UserId is not null)
    /// </summary>
    public string? UserEmail { get; set; }
    /// <summary>
    /// List of Subcategory IDs that this technician has permission to work on
    /// </summary>
    public List<int> AllowedSubcategoryIds { get; set; } = new List<int>();
}

public class TechnicianCreateRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; } = string.Empty;

    [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string? Phone { get; set; }

    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string? Department { get; set; }

    public bool IsActive { get; set; } = true;

    /// <summary>
    /// List of Subcategory IDs that this technician has permission to work on
    /// </summary>
    public List<int>? AllowedSubcategoryIds { get; set; }
}

public class TechnicianUpdateRequest
{
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Department { get; set; }
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// List of Subcategory IDs that this technician has permission to work on
    /// </summary>
    public List<int>? AllowedSubcategoryIds { get; set; }
}

public class TechnicianStatusUpdateRequest
{
    public bool IsActive { get; set; }
}

/// <summary>
/// Request to link a Technician record to a User account (Admin-only)
/// </summary>
public class TechnicianLinkUserRequest
{
    /// <summary>
    /// The User.Id (JWT sub) of a Technician-role user to link
    /// </summary>
    public Guid UserId { get; set; }
}

/// <summary>
/// Request to create a Technician with optional User account creation
/// </summary>
public class TechnicianCreateWithUserRequest
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(200, ErrorMessage = "Full name cannot exceed 200 characters")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Invalid email format")]
    [StringLength(256, ErrorMessage = "Email cannot exceed 256 characters")]
    public string Email { get; set; } = string.Empty; // Technician contact email

    [StringLength(50, ErrorMessage = "Phone cannot exceed 50 characters")]
    public string? Phone { get; set; }

    [StringLength(100, ErrorMessage = "Department cannot exceed 100 characters")]
    public string? Department { get; set; }

    public bool CreateUser { get; set; } = false;

    [EmailAddress(ErrorMessage = "Invalid user email format")]
    [StringLength(256, ErrorMessage = "User email cannot exceed 256 characters")]
    public string? UserEmail { get; set; } // Login email for the user account

    [StringLength(128, ErrorMessage = "Password cannot exceed 128 characters")]
    public string? TempPassword { get; set; } // If null, backend will generate

    /// <summary>
    /// List of Subcategory IDs that this technician has permission to work on
    /// </summary>
    public List<int>? AllowedSubcategoryIds { get; set; }
}

/// <summary>
/// Response from creating a Technician with optional User account
/// </summary>
public class TechnicianCreateWithUserResponse
{
    public TechnicianResponse Technician { get; set; } = null!;
    public CreatedUserInfo? CreatedUser { get; set; }
}

/// <summary>
/// Information about a newly created user account
/// </summary>
public class CreatedUserInfo
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string TempPassword { get; set; } = string.Empty; // Returned once after creation
}

/// <summary>
/// Result of linking a Technician to a User account
/// </summary>
public enum LinkUserResult
{
    Success,
    TechnicianNotFound,
    UserNotFound,
    UserNotTechnicianRole,
    AlreadyLinked,
}

/// <summary>
/// Result of deleting a technician
/// </summary>
public enum DeleteTechnicianResult
{
    Success,
    TechnicianNotFound,
    HasAssignedTickets,
    UserDeletionBlocked_UserNotFound,
    UserDeletionBlocked_RoleInvalid,
    UserDeletionBlocked_HasCreatedTickets,
    UserDeletionBlocked_HasAssignedTickets,
}
