using Microsoft.EntityFrameworkCore;
using Ticketing.Backend.Application.DTOs;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Application.Services;

public interface ITechnicianService
{
    Task<IEnumerable<TechnicianResponse>> GetAllTechniciansAsync();
    Task<TechnicianResponse?> GetTechnicianByIdAsync(Guid id);
    Task<TechnicianResponse> CreateTechnicianAsync(TechnicianCreateRequest request);
    Task<TechnicianResponse?> UpdateTechnicianAsync(Guid id, TechnicianUpdateRequest request);
    Task<bool> UpdateTechnicianStatusAsync(Guid id, bool isActive);
    Task<bool> IsTechnicianActiveAsync(Guid id);
}

public class TechnicianService : ITechnicianService
{
    private readonly AppDbContext _context;

    public TechnicianService(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<TechnicianResponse>> GetAllTechniciansAsync()
    {
        var technicians = await _context.Technicians
            .OrderBy(t => t.FullName)
            .ToListAsync();

        return technicians.Select(MapToResponse);
    }

    public async Task<TechnicianResponse?> GetTechnicianByIdAsync(Guid id)
    {
        var technician = await _context.Technicians
            .FirstOrDefaultAsync(t => t.Id == id);

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

        _context.Technicians.Add(technician);
        await _context.SaveChangesAsync();

        return MapToResponse(technician);
    }

    public async Task<TechnicianResponse?> UpdateTechnicianAsync(Guid id, TechnicianUpdateRequest request)
    {
        var technician = await _context.Technicians
            .FirstOrDefaultAsync(t => t.Id == id);

        if (technician == null)
        {
            return null;
        }

        technician.FullName = request.FullName;
        technician.Email = request.Email;
        technician.Phone = request.Phone;
        technician.Department = request.Department;
        technician.IsActive = request.IsActive; // Update IsActive status

        await _context.SaveChangesAsync();

        return MapToResponse(technician);
    }

    public async Task<bool> UpdateTechnicianStatusAsync(Guid id, bool isActive)
    {
        var technician = await _context.Technicians
            .FirstOrDefaultAsync(t => t.Id == id);

        if (technician == null)
        {
            return false;
        }

        technician.IsActive = isActive;
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<bool> IsTechnicianActiveAsync(Guid id)
    {
        var technician = await _context.Technicians
            .FirstOrDefaultAsync(t => t.Id == id);

        return technician != null && technician.IsActive;
    }

    private static TechnicianResponse MapToResponse(Technician technician) => new()
    {
        Id = technician.Id,
        FullName = technician.FullName,
        Email = technician.Email,
        Phone = technician.Phone,
        Department = technician.Department,
        IsActive = technician.IsActive,
        CreatedAt = technician.CreatedAt
    };
}

