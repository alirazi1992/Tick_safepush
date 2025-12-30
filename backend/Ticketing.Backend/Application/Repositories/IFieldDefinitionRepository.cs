using Ticketing.Backend.Domain.Entities;

namespace Ticketing.Backend.Application.Repositories;

public interface IFieldDefinitionRepository
{
    Task<SubcategoryFieldDefinition?> GetByIdAsync(int id);
    Task<IEnumerable<SubcategoryFieldDefinition>> GetBySubcategoryIdAsync(int subcategoryId, bool includeInactive = true);
    Task<SubcategoryFieldDefinition> AddAsync(SubcategoryFieldDefinition fieldDefinition);
    Task<SubcategoryFieldDefinition> UpdateAsync(SubcategoryFieldDefinition fieldDefinition);
    Task<bool> DeleteAsync(int id);
    Task<bool> ExistsAsync(int subcategoryId, string key);
}

