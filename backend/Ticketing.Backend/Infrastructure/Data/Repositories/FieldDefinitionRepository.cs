using Microsoft.EntityFrameworkCore;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Infrastructure.Data.Repositories;

public class FieldDefinitionRepository : IFieldDefinitionRepository
{
    private readonly AppDbContext _context;

    public FieldDefinitionRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SubcategoryFieldDefinition?> GetByIdAsync(int id)
    {
        return await _context.SubcategoryFieldDefinitions.FindAsync(id);
    }

    public async Task<IEnumerable<SubcategoryFieldDefinition>> GetBySubcategoryIdAsync(int subcategoryId, bool includeInactive = true)
    {
        var query = _context.SubcategoryFieldDefinitions
            .Where(f => f.SubcategoryId == subcategoryId);

        // For now, we don't have an IsActive field, so return all
        // This can be added later if needed

        return await query.OrderBy(f => f.Id).ToListAsync();
    }

    public async Task<SubcategoryFieldDefinition> AddAsync(SubcategoryFieldDefinition fieldDefinition)
    {
        await _context.SubcategoryFieldDefinitions.AddAsync(fieldDefinition);
        return fieldDefinition;
    }

    public Task<SubcategoryFieldDefinition> UpdateAsync(SubcategoryFieldDefinition fieldDefinition)
    {
        _context.SubcategoryFieldDefinitions.Update(fieldDefinition);
        return Task.FromResult(fieldDefinition);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var field = await _context.SubcategoryFieldDefinitions.FindAsync(id);
        if (field == null)
        {
            return false;
        }

        _context.SubcategoryFieldDefinitions.Remove(field);
        return true;
    }

    public async Task<bool> ExistsAsync(int subcategoryId, string key)
    {
        return await _context.SubcategoryFieldDefinitions
            .AnyAsync(f => f.SubcategoryId == subcategoryId && f.Key == key);
    }
}

