using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Backend.Application.DTOs;
using Ticketing.Backend.Application.Services;
using Ticketing.Backend.Domain.Enums;

namespace Ticketing.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CategoriesController : ControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(ICategoryService categoryService, ILogger<CategoriesController> logger)
    {
        _categoryService = categoryService;
        _logger = logger;
    }

    [HttpGet]
    [AllowAnonymous]
    public async Task<IActionResult> GetAll()
    {
        try
        {
            var categories = await _categoryService.GetAllAsync();
            return Ok(categories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving categories", error = ex.Message });
        }
    }

    [HttpGet("admin")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetAdminCategories([FromQuery] string? search, [FromQuery] int page = 1, [FromQuery] int pageSize = 50)
    {
        var result = await _categoryService.GetAdminCategoriesAsync(search, page, pageSize);
        return Ok(result);
    }

    [HttpPost]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Create([FromBody] CategoryRequest request)
    {
        _logger.LogInformation("CreateCategory: Received request - Name={Name}, Description={Description}, IsActive={IsActive}",
            request?.Name, request?.Description, request?.IsActive);

        if (request is null)
        {
            return BadRequest(new { message = "Request body is required" });
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("CreateCategory: ModelState invalid - Errors={Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _categoryService.CreateAsync(request);
            _logger.LogInformation("CreateCategory: SUCCESS - Created category Id={Id}, Name={Name}", result?.Id, result?.Name);
            return CreatedAtAction(nameof(GetAll), new { id = result!.Id }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("CreateCategory: InvalidOperation - Message={Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateCategory: FAILED - Exception={ExceptionType}, Message={Message}", ex.GetType().Name, ex.Message);
            return StatusCode(500, new { message = "Failed to create category", error = ex.Message });
        }
    }

    [HttpPut("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Update(int id, [FromBody] CategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _categoryService.UpdateAsync(id, request);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            var deleted = await _categoryService.DeleteAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{categoryId}/subcategories")]
    [Authorize] // Allow all authenticated roles to read subcategories
    public async Task<IActionResult> GetSubcategories(int categoryId)
    {
        try
        {
            var subcategories = await _categoryService.GetSubcategoriesAsync(categoryId);
            return Ok(subcategories);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new { message = "An error occurred while retrieving subcategories", error = ex.Message });
        }
    }

    [HttpPost("{categoryId}/subcategories")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> CreateSubcategory(int categoryId, [FromBody] SubcategoryRequest request)
    {
        _logger.LogInformation("CreateSubcategory: Received request - CategoryId={CategoryId}, Name={Name}, Description={Description}, IsActive={IsActive}",
            categoryId, request?.Name, request?.Description, request?.IsActive);

        if (request is null)
        {
            return BadRequest(new { message = "Request body is required" });
        }

        if (!ModelState.IsValid)
        {
            _logger.LogWarning("CreateSubcategory: ModelState invalid - Errors={Errors}",
                string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _categoryService.CreateSubcategoryAsync(categoryId, request);
            if (result == null)
            {
                _logger.LogWarning("CreateSubcategory: Category not found - CategoryId={CategoryId}", categoryId);
                return NotFound(new { message = "Category not found" });
            }
            _logger.LogInformation("CreateSubcategory: SUCCESS - Created subcategory Id={Id}, Name={Name}, CategoryId={CategoryId}",
                result.Id, result.Name, result.CategoryId);
            return CreatedAtAction(nameof(GetSubcategories), new { categoryId }, result);
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogWarning("CreateSubcategory: InvalidOperation - Message={Message}", ex.Message);
            return BadRequest(new { message = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "CreateSubcategory: FAILED - Exception={ExceptionType}, Message={Message}", ex.GetType().Name, ex.Message);
            return StatusCode(500, new { message = "Failed to create subcategory", error = ex.Message });
        }
    }

    [HttpPut("subcategories/{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> UpdateSubcategory(int id, [FromBody] SubcategoryRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var result = await _categoryService.UpdateSubcategoryAsync(id, request);
            if (result == null)
            {
                return NotFound();
            }
            return Ok(result);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("subcategories/{id}")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> DeleteSubcategory(int id)
    {
        try
        {
            var deleted = await _categoryService.DeleteSubcategoryAsync(id);
            if (!deleted)
            {
                return NotFound();
            }
            return NoContent();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}
