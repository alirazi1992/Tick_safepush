using Microsoft.Extensions.Logging;
using Ticketing.Backend.Application.DTOs;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Domain.Enums;

namespace Ticketing.Backend.Application.Services;

/// <summary>
/// Result of linking a Technician to a User account
/// </summary>
public enum LinkUserResult
{
    Success,
    TechnicianNotFound,
    UserNotFound,
    UserNotTechnicianRole,
    AlreadyLinked
}

public interface ITechnicianService
{
    Task<IEnumerable<TechnicianResponse>> GetAllTechniciansAsync();
    Task<TechnicianResponse?> GetTechnicianByIdAsync(Guid id);
    Task<TechnicianResponse> CreateTechnicianAsync(TechnicianCreateRequest request);
    Task<TechnicianResponse?> UpdateTechnicianAsync(Guid id, TechnicianUpdateRequest request);
    Task<bool> UpdateTechnicianStatusAsync(Guid id, bool isActive);
    Task<bool> IsTechnicianActiveAsync(Guid id);
    Task<(LinkUserResult result, TechnicianResponse? technician)> LinkUserAsync(Guid technicianId, Guid userId);
}

public class TechnicianService : ITechnicianService
{
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUserRepository _userRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly ILogger<TechnicianService> _logger;

    public TechnicianService(
        ITechnicianRepository technicianRepository,
        IUserRepository userRepository,
        IUnitOfWork unitOfWork,
        ILogger<TechnicianService> logger)
    {
        _technicianRepository = technicianRepository;
        _userRepository = userRepository;
        _unitOfWork = unitOfWork;
        _logger = logger;
    }

    public async Task<IEnumerable<TechnicianResponse>> GetAllTechniciansAsync()
    {
        var technicians = await _technicianRepository.GetAllAsync();
        return technicians.Select(MapToResponse);
    }

    public async Task<TechnicianResponse?> GetTechnicianByIdAsync(Guid id)
    {
        var technician = await _technicianRepository.GetByIdAsync(id);
        return technician == null ? null : MapToResponse(technician);
    }

    public async Task<TechnicianResponse> CreateTechnicianAsync(TechnicianCreateRequest request)
    {
        var technician = new Technician
        {
            Id = Guid.NewGuid(),
            FullName = request.FullName,
            Email = request.Email,
            Phone = request.Phone,
            Department = request.Department,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _technicianRepository.AddAsync(technician);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(technician);
    }

    public async Task<TechnicianResponse?> UpdateTechnicianAsync(Guid id, TechnicianUpdateRequest request)
    {
        var technician = await _technicianRepository.GetByIdAsync(id);

        if (technician == null)
        {
            return null;
        }

        technician.FullName = request.FullName;
        technician.Email = request.Email;
        technician.Phone = request.Phone;
        technician.Department = request.Department;
        technician.IsActive = request.IsActive; // Update IsActive status

        await _technicianRepository.UpdateAsync(technician);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(technician);
    }

    public async Task<bool> UpdateTechnicianStatusAsync(Guid id, bool isActive)
    {
        var technician = await _technicianRepository.GetByIdAsync(id);

        if (technician == null)
        {
            return false;
        }

        technician.IsActive = isActive;
        await _technicianRepository.UpdateAsync(technician);
        await _unitOfWork.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsTechnicianActiveAsync(Guid id)
    {
        var technician = await _technicianRepository.GetByIdAsync(id);
        return technician != null && technician.IsActive;
    }

    /// <summary>
    /// Links a Technician record to a User account (Admin-only operation)
    /// </summary>
    public async Task<(LinkUserResult result, TechnicianResponse? technician)> LinkUserAsync(Guid technicianId, Guid userId)
    {
        _logger.LogInformation("LinkUser: Attempting to link Technician {TechnicianId} to User {UserId}", technicianId, userId);

        var technician = await _technicianRepository.GetByIdAsync(technicianId);
        if (technician == null)
        {
            _logger.LogWarning("LinkUser FAILED: Technician {TechnicianId} not found", technicianId);
            return (LinkUserResult.TechnicianNotFound, null);
        }

        // Check if already linked
        if (technician.UserId != null)
        {
            _logger.LogWarning("LinkUser FAILED: Technician {TechnicianId} is already linked to User {ExistingUserId}", technicianId, technician.UserId);
            return (LinkUserResult.AlreadyLinked, null);
        }

        // Verify user exists and has Technician role
        var user = await _userRepository.GetByIdAsync(userId);
        if (user == null)
        {
            // Debug: Log all user IDs to help diagnose
            var allUsers = await _userRepository.GetAllAsync();
            var allUserIds = allUsers.Select(u => new { u.Id, u.Email, u.Role }).ToList();
            _logger.LogWarning("LinkUser FAILED: User {UserId} not found. Total users in DB: {Count}. Users: {@Users}", 
                userId, allUserIds.Count, allUserIds);
            return (LinkUserResult.UserNotFound, null);
        }

        if (user.Role != UserRole.Technician)
        {
            _logger.LogWarning("LinkUser FAILED: User {UserId} has role {Role}, expected Technician", userId, user.Role);
            return (LinkUserResult.UserNotTechnicianRole, null);
        }

        // Link technician to user
        technician.UserId = userId;
        await _technicianRepository.UpdateAsync(technician);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("LinkUser SUCCESS: Technician {TechnicianId} linked to User {UserId} ({UserEmail})", 
            technicianId, userId, user.Email);

        return (LinkUserResult.Success, MapToResponse(technician));
    }

    private static TechnicianResponse MapToResponse(Technician technician) => new()
    {
        Id = technician.Id,
        FullName = technician.FullName,
        Email = technician.Email,
        Phone = technician.Phone,
        Department = technician.Department,
        IsActive = technician.IsActive,
        CreatedAt = technician.CreatedAt,
        UserId = technician.UserId // For debugging: null = cannot be assigned
    };
}

