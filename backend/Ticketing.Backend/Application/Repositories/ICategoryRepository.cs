using Ticketing.Backend.Domain.Entities;

namespace Ticketing.Backend.Application.Repositories;

public interface ICategoryRepository
{
    Task<Category?> GetByIdAsync(int id);
    Task<Subcategory?> GetSubcategoryByIdAsync(int id);
    Task<IEnumerable<Category>> GetAllAsync();
}


