using System;
using Ticketing.Backend.Application.DTOs;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Domain.Entities;

namespace Ticketing.Backend.Application.Services;

public interface ICategoryService
{
    Task<IEnumerable<CategoryResponse>> GetAllAsync();
    Task<CategoryListResponse> GetAdminCategoriesAsync(string? search = null, int page = 1, int pageSize = 50);
    Task<CategoryResponse?> CreateAsync(CategoryRequest request, IEnumerable<SubcategoryRequest>? subcategories = null);
    Task<CategoryResponse?> UpdateAsync(int id, CategoryRequest request);
    Task<bool> DeleteAsync(int id);
    Task<IEnumerable<SubcategoryResponse>> GetSubcategoriesAsync(int categoryId);
    Task<SubcategoryResponse?> CreateSubcategoryAsync(int categoryId, SubcategoryRequest request);
    Task<SubcategoryResponse?> UpdateSubcategoryAsync(int id, SubcategoryRequest request);
    Task<bool> DeleteSubcategoryAsync(int id);
}

public class CategoryService : ICategoryService
{
    private readonly ICategoryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public CategoryService(ICategoryRepository repository, IUnitOfWork unitOfWork)
    {
        _repository = repository;
        _unitOfWork = unitOfWork;
    }

    public async Task<IEnumerable<CategoryResponse>> GetAllAsync()
    {
        // Public endpoint - only return active categories
        var categories = await _repository.GetActiveCategoriesAsync();
        return categories.Select(c => new CategoryResponse
        {
            Id = c.Id,
            Name = c.Name,
            Description = c.Description,
            IsActive = c.IsActive,
            CreatedAt = c.CreatedAt,
            Subcategories = c.Subcategories
                .Where(s => s.IsActive)
                .Select(MapSubcategoryToResponse)
        });
    }

    public async Task<CategoryListResponse> GetAdminCategoriesAsync(string? search = null, int page = 1, int pageSize = 50)
    {
        var skip = (page - 1) * pageSize;
        var totalCount = await _repository.CountAsync(search);
        var items = await _repository.SearchAsync(search, skip, pageSize);

        return new CategoryListResponse
        {
            Items = items.Select(MapToResponse),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        };
    }

    public async Task<CategoryResponse?> CreateAsync(CategoryRequest request, IEnumerable<SubcategoryRequest>? subcategories = null)
    {
        // Check for duplicate name
        var exists = await _repository.ExistsByNameAsync(request.Name);
        if (exists)
        {
            throw new InvalidOperationException($"Category with name '{request.Name}' already exists");
        }

        var category = new Category
        {
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow,
            Subcategories = subcategories?.Select(sc => new Subcategory 
            { 
                Name = sc.Name,
                Description = sc.Description,
                IsActive = sc.IsActive,
                CreatedAt = DateTime.UtcNow
            }).ToList() ?? new List<Subcategory>()
        };

        await _repository.AddAsync(category);
        await _unitOfWork.SaveChangesAsync();

        return MapToResponse(category);
    }

    public async Task<CategoryResponse?> UpdateAsync(int id, CategoryRequest request)
    {
        var category = await _repository.GetByIdWithSubcategoriesAsync(id);
        if (category == null)
        {
            return null;
        }

        // Check for duplicate name (excluding current category)
        var exists = await _repository.ExistsByNameExcludingIdAsync(request.Name, id);
        if (exists)
        {
            throw new InvalidOperationException($"Category with name '{request.Name}' already exists");
        }

        category.Name = request.Name;
        category.Description = request.Description;
        category.IsActive = request.IsActive;
        await _repository.UpdateAsync(category);
        await _unitOfWork.SaveChangesAsync();
        return MapToResponse(category);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await _repository.GetByIdWithTicketsAndSubcategoriesAsync(id);
        if (category == null)
        {
            return false;
        }

        // Check if category is used by tickets
        if (category.Tickets.Any())
        {
            throw new InvalidOperationException("Cannot delete category that is used by tickets. Consider deactivating it instead.");
        }

        // Check if category has subcategories
        if (category.Subcategories.Any())
        {
            throw new InvalidOperationException("Cannot delete category that has subcategories. Please delete subcategories first.");
        }

        var deleted = await _repository.DeleteAsync(id);
        if (deleted)
        {
            await _unitOfWork.SaveChangesAsync();
        }
        return true;
    }

    public async Task<IEnumerable<SubcategoryResponse>> GetSubcategoriesAsync(int categoryId)
    {
        var subcategories = await _repository.GetSubcategoriesByCategoryIdAsync(categoryId);
        return subcategories.Select(MapSubcategoryToResponse);
    }

    public async Task<SubcategoryResponse?> CreateSubcategoryAsync(int categoryId, SubcategoryRequest request)
    {
        var category = await _repository.GetByIdAsync(categoryId);
        if (category == null)
        {
            return null;
        }

        // Check for duplicate name within the category
        var exists = await _repository.SubcategoryExistsByNameAsync(categoryId, request.Name);
        if (exists)
        {
            throw new InvalidOperationException($"Subcategory with name '{request.Name}' already exists in this category");
        }

        var subcategory = new Subcategory
        {
            CategoryId = categoryId,
            Name = request.Name,
            Description = request.Description,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        await _repository.AddSubcategoryAsync(subcategory);
        await _unitOfWork.SaveChangesAsync();

        return MapSubcategoryToResponse(subcategory);
    }

    public async Task<SubcategoryResponse?> UpdateSubcategoryAsync(int id, SubcategoryRequest request)
    {
        var subcategory = await _repository.GetSubcategoryByIdAsync(id);
        if (subcategory == null)
        {
            return null;
        }

        // Check for duplicate name within the same category (excluding current subcategory)
        var exists = await _repository.SubcategoryExistsByNameExcludingIdAsync(subcategory.CategoryId, request.Name, id);
        if (exists)
        {
            throw new InvalidOperationException($"Subcategory with name '{request.Name}' already exists in this category");
        }

        subcategory.Name = request.Name;
        subcategory.Description = request.Description;
        subcategory.IsActive = request.IsActive;
        await _repository.UpdateSubcategoryAsync(subcategory);
        await _unitOfWork.SaveChangesAsync();

        return MapSubcategoryToResponse(subcategory);
    }

    public async Task<bool> DeleteSubcategoryAsync(int id)
    {
        var subcategory = await _repository.GetSubcategoryByIdWithTicketsAsync(id);
        if (subcategory == null)
        {
            return false;
        }

        // Check if subcategory is used by tickets
        if (subcategory.Tickets.Any())
        {
            throw new InvalidOperationException("Cannot delete subcategory that is used by tickets. Consider deactivating it instead.");
        }

        var deleted = await _repository.DeleteSubcategoryAsync(id);
        if (deleted)
        {
            await _unitOfWork.SaveChangesAsync();
        }
        return true;
    }

    private static CategoryResponse MapToResponse(Category category) => new()
    {
        Id = category.Id,
        Name = category.Name,
        Description = category.Description,
        IsActive = category.IsActive,
        CreatedAt = category.CreatedAt,
        Subcategories = category.Subcategories.Select(MapSubcategoryToResponse)
    };

    private static SubcategoryResponse MapSubcategoryToResponse(Subcategory subcategory) => new()
    {
        Id = subcategory.Id,
        Name = subcategory.Name,
        Description = subcategory.Description,
        IsActive = subcategory.IsActive,
        CreatedAt = subcategory.CreatedAt
    };
}
