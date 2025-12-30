using Microsoft.EntityFrameworkCore;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Infrastructure.Data.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _context;

    public CategoryRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<Category?> GetByIdAsync(int id)
    {
        return await _context.Categories.FindAsync(id);
    }

    public async Task<Subcategory?> GetSubcategoryByIdAsync(int id)
    {
        return await _context.Subcategories.FindAsync(id);
    }

    public async Task<IEnumerable<Category>> GetAllAsync()
    {
        return await _context.Categories
            .Include(c => c.Subcategories)
            .ToListAsync();
    }
}

