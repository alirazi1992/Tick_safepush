using Ticketing.Backend.Application.DTOs;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Domain.Enums;

namespace Ticketing.Backend.Application.Services;

public interface IFieldDefinitionService
{
    Task<IEnumerable<FieldDefinitionResponse>> GetFieldDefinitionsAsync(int subcategoryId, bool includeInactive = false);
    Task<FieldDefinitionResponse?> GetFieldDefinitionAsync(int id);
    Task<FieldDefinitionResponse?> CreateFieldDefinitionAsync(int subcategoryId, CreateFieldDefinitionRequest request);
    Task<FieldDefinitionResponse?> UpdateFieldDefinitionAsync(int id, UpdateFieldDefinitionRequest request);
    Task<bool> DeleteFieldDefinitionAsync(int id);
}

public class FieldDefinitionService : IFieldDefinitionService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly ICategoryRepository _categoryRepository;
    private readonly ILogger<FieldDefinitionService> _logger;

    public FieldDefinitionService(
        IUnitOfWork unitOfWork,
        ICategoryRepository categoryRepository,
        ILogger<FieldDefinitionService> logger)
    {
        _unitOfWork = unitOfWork;
        _categoryRepository = categoryRepository;
        _logger = logger;
    }

    public async Task<IEnumerable<FieldDefinitionResponse>> GetFieldDefinitionsAsync(int subcategoryId, bool includeInactive = false)
    {
        var fields = await _unitOfWork.FieldDefinitions.GetBySubcategoryIdAsync(subcategoryId, includeInactive);
        return fields.Select(MapToResponse);
    }

    public async Task<FieldDefinitionResponse?> GetFieldDefinitionAsync(int id)
    {
        var field = await _unitOfWork.FieldDefinitions.GetByIdAsync(id);
        return field == null ? null : MapToResponse(field);
    }

    public async Task<FieldDefinitionResponse?> CreateFieldDefinitionAsync(int subcategoryId, CreateFieldDefinitionRequest request)
    {
        // Verify subcategory exists
        var subcategory = await _categoryRepository.GetSubcategoryByIdAsync(subcategoryId);
        if (subcategory == null)
        {
            _logger.LogWarning("CreateFieldDefinition: Subcategory {SubcategoryId} not found", subcategoryId);
            return null;
        }

        // Check for duplicate key
        var exists = await _unitOfWork.FieldDefinitions.ExistsAsync(subcategoryId, request.Key);
        if (exists)
        {
            throw new InvalidOperationException($"A field with key '{request.Key}' already exists for this subcategory.");
        }

        // Parse Type enum
        if (!Enum.TryParse<FieldType>(request.Type, ignoreCase: true, out var fieldType))
        {
            throw new InvalidOperationException($"Invalid field type: {request.Type}");
        }

        // Validate Select type requires options
        if (fieldType == FieldType.Select && (request.Options == null || !request.Options.Any()))
        {
            throw new InvalidOperationException("Select field type requires at least one option.");
        }

        var field = new SubcategoryFieldDefinition
        {
            SubcategoryId = subcategoryId,
            Name = request.Name,
            Label = request.Label,
            Key = request.Key,
            Type = fieldType,
            IsRequired = request.IsRequired,
            DefaultValue = request.DefaultValue,
            OptionsJson = request.Options != null ? System.Text.Json.JsonSerializer.Serialize(request.Options) : null,
            Min = request.Min,
            Max = request.Max
        };

        await _unitOfWork.FieldDefinitions.AddAsync(field);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Created field definition {FieldId} for subcategory {SubcategoryId}", field.Id, subcategoryId);

        return MapToResponse(field);
    }

    public async Task<FieldDefinitionResponse?> UpdateFieldDefinitionAsync(int id, UpdateFieldDefinitionRequest request)
    {
        var field = await _unitOfWork.FieldDefinitions.GetByIdAsync(id);
        if (field == null)
        {
            return null;
        }

        // Check for duplicate key (if key is being changed)
        if (request.Key != null && request.Key != field.Key)
        {
            var exists = await _unitOfWork.FieldDefinitions.ExistsAsync(field.SubcategoryId, request.Key);
            if (exists)
            {
                throw new InvalidOperationException($"A field with key '{request.Key}' already exists for this subcategory.");
            }
        }

        // Update fields
        if (request.Name != null) field.Name = request.Name;
        if (request.Label != null) field.Label = request.Label;
        if (request.Key != null) field.Key = request.Key;
        if (!string.IsNullOrWhiteSpace(request.Type))
        {
            if (!Enum.TryParse<FieldType>(request.Type, ignoreCase: true, out var fieldType))
            {
                throw new InvalidOperationException($"Invalid field type: {request.Type}");
            }
            field.Type = fieldType;
        }
        if (request.IsRequired.HasValue) field.IsRequired = request.IsRequired.Value;
        if (request.DefaultValue != null) field.DefaultValue = request.DefaultValue;
        if (request.Options != null) field.OptionsJson = System.Text.Json.JsonSerializer.Serialize(request.Options);
        if (request.Min.HasValue) field.Min = request.Min;
        if (request.Max.HasValue) field.Max = request.Max;

        // Validate Select type requires options
        if (field.Type == FieldType.Select && string.IsNullOrWhiteSpace(field.OptionsJson))
        {
            throw new InvalidOperationException("Select field type requires at least one option.");
        }

        await _unitOfWork.FieldDefinitions.UpdateAsync(field);
        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Updated field definition {FieldId}", id);

        return MapToResponse(field);
    }

    public async Task<bool> DeleteFieldDefinitionAsync(int id)
    {
        var deleted = await _unitOfWork.FieldDefinitions.DeleteAsync(id);
        if (!deleted)
        {
            return false;
        }

        await _unitOfWork.SaveChangesAsync();

        _logger.LogInformation("Deleted field definition {FieldId}", id);

        return true;
    }

    private static FieldDefinitionResponse MapToResponse(SubcategoryFieldDefinition field)
    {
        List<FieldOption>? options = null;
        if (!string.IsNullOrWhiteSpace(field.OptionsJson))
        {
            try
            {
                options = System.Text.Json.JsonSerializer.Deserialize<List<FieldOption>>(field.OptionsJson);
            }
            catch
            {
                // If deserialization fails, leave as null
            }
        }

        return new FieldDefinitionResponse
        {
            Id = field.Id,
            SubcategoryId = field.SubcategoryId,
            Name = field.Name,
            Label = field.Label,
            Key = field.Key,
            Type = field.Type.ToString(),
            IsRequired = field.IsRequired,
            DefaultValue = field.DefaultValue,
            Options = options,
            Min = field.Min,
            Max = field.Max
        };
    }
}
