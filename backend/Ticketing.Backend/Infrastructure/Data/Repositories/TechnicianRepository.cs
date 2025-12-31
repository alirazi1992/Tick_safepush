using Microsoft.EntityFrameworkCore;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Infrastructure.Data.Repositories;

public class TechnicianRepository : ITechnicianRepository
{
    private readonly AppDbContext _context;

    public TechnicianRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<Technician>> GetAllAsync()
    {
        return await _context.Technicians
            .OrderBy(t => t.FullName)
            .ToListAsync();
    }

    public async Task<IEnumerable<Technician>> GetActiveWithUserIdAsync()
    {
        return await _context.Technicians
            .Where(t => t.IsActive && t.UserId != null)
            .OrderBy(t => t.FullName)
            .ToListAsync();
    }

    public async Task<Technician?> GetByIdAsync(Guid id)
    {
        return await _context.Technicians
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<Technician> AddAsync(Technician technician)
    {
        await _context.Technicians.AddAsync(technician);
        return technician;
    }

    public Task UpdateAsync(Technician technician)
    {
        _context.Technicians.Update(technician);
        return Task.CompletedTask;
    }
}

