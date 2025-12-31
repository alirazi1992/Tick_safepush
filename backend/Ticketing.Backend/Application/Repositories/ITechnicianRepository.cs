using Ticketing.Backend.Domain.Entities;

namespace Ticketing.Backend.Application.Repositories;

public interface ITechnicianRepository
{
    Task<IEnumerable<Technician>> GetAllAsync();
    Task<IEnumerable<Technician>> GetActiveWithUserIdAsync();
    Task<Technician?> GetByIdAsync(Guid id);
    Task<Technician> AddAsync(Technician technician);
    Task UpdateAsync(Technician technician);
}

