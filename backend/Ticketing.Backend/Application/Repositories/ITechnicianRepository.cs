using Ticketing.Backend.Domain.Entities;

namespace Ticketing.Backend.Application.Repositories;

public interface ITechnicianRepository
{
    Task<IEnumerable<Technician>> GetAllAsync();
    Task<Technician?> GetByIdAsync(Guid id);
    Task<Technician> AddAsync(Technician technician);
    Task UpdateAsync(Technician technician);
}

