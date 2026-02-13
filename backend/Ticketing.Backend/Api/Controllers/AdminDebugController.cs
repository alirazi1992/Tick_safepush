// ⚠️ TEMPORARY DEBUG CONTROLLER - REMOVE BEFORE PRODUCTION ⚠️
// This controller exposes internal data for debugging purposes only.

using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Ticketing.Backend.Domain.Enums;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Api.Controllers;

[ApiController]
[Route("api/debug")]
public class AdminDebugController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly ILogger<AdminDebugController> _logger;
    private readonly IWebHostEnvironment _environment;

    public AdminDebugController(AppDbContext context, ILogger<AdminDebugController> logger, IWebHostEnvironment environment)
    {
        _context = context;
        _logger = logger;
        _environment = environment;
    }

    /// <summary>
    /// ⚠️ DEBUG ONLY: List all users in the database
    /// Returns { id, email, role } for each user to verify DB consistency.
    /// REMOVE THIS ENDPOINT BEFORE PRODUCTION.
    /// </summary>
    [HttpGet("users")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetAllUsers()
    {
        _logger.LogWarning("DEBUG ENDPOINT CALLED: GET /api/debug/users - This should be removed before production");

        var users = await _context.Users
            .AsNoTracking()
            .OrderBy(u => u.Email)
            .Select(u => new
            {
                u.Id,
                u.Email,
                u.FullName,
                Role = u.Role.ToString(),
                u.CreatedAt
            })
            .ToListAsync();

        return Ok(new
        {
            totalCount = users.Count,
            databasePath = _context.Database.GetDbConnection().ConnectionString,
            users
        });
    }

    /// <summary>
    /// ⚠️ DEBUG ONLY: List all technicians with their linked User IDs
    /// REMOVE THIS ENDPOINT BEFORE PRODUCTION.
    /// </summary>
    [HttpGet("technicians")]
    [Authorize(Roles = nameof(UserRole.Admin))]
    public async Task<IActionResult> GetAllTechniciansDebug()
    {
        _logger.LogWarning("DEBUG ENDPOINT CALLED: GET /api/debug/technicians - This should be removed before production");

        var technicians = await _context.Technicians
            .AsNoTracking()
            .OrderBy(t => t.Email)
            .Select(t => new
            {
                TechnicianId = t.Id,
                t.Email,
                t.FullName,
                LinkedUserId = t.UserId,
                t.IsActive
            })
            .ToListAsync();

        return Ok(new
        {
            totalCount = technicians.Count,
            linkedCount = technicians.Count(t => t.LinkedUserId != null),
            unlinkedCount = technicians.Count(t => t.LinkedUserId == null),
            technicians
        });
    }

    /// <summary>
    /// ⚠️ DEBUG ONLY: Test ticket query with different include levels
    /// REMOVE THIS ENDPOINT BEFORE PRODUCTION.
    /// </summary>
    [HttpGet("tickets/test-query")]
    [AllowAnonymous]
    public async Task<IActionResult> TestTicketQuery()
    {
        // Allow in both Development and Production for debugging (temporary)
        // TODO: Remove this endpoint before production
        try
        {
            // Test 1: Simple count
            var simpleCount = await _context.Tickets.CountAsync();
            
            // Test 2: Query with basic includes
            var withBasicIncludes = await _context.Tickets
                .Include(t => t.Category)
                .Include(t => t.CreatedByUser)
                .Take(1)
                .ToListAsync();
            
            // Test 3: Query with AssignedTechnicians (without AssignedByUser)
            var withAssignments1 = await _context.Tickets
                .Include(t => t.AssignedTechnicians)
                    .ThenInclude(ta => ta.TechnicianUser)
                .Take(1)
                .ToListAsync();
            
            // Test 4: Query with AssignedTechnicians (with AssignedByUser) - THIS IS THE PROBLEMATIC ONE
            var withAssignments2 = await _context.Tickets
                .Include(t => t.AssignedTechnicians)
                    .ThenInclude(ta => ta.TechnicianUser)
                .Include(t => t.AssignedTechnicians)
                    .ThenInclude(ta => ta.AssignedByUser)
                .Take(1)
                .ToListAsync();
            
            return Ok(new
            {
                test1_simpleCount = simpleCount,
                test2_basicIncludes = withBasicIncludes.Count,
                test3_assignmentsWithoutAssignedBy = withAssignments1.Count,
                test4_assignmentsWithAssignedBy = withAssignments2.Count,
                success = true
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Test query failed: {ExceptionType}, {Message}", ex.GetType().Name, ex.Message);
            return StatusCode(500, new
            {
                error = ex.GetType().Name,
                message = ex.Message,
                innerException = ex.InnerException?.Message,
                stackTrace = ex.StackTrace?.Substring(0, Math.Min(500, ex.StackTrace.Length))
            });
        }
    }

    /// <summary>
    /// ⚠️ DEBUG ONLY: Get ticket count in database
    /// Returns total count of tickets to confirm persistence.
    /// Accessible without auth in Development mode only.
    /// REMOVE THIS ENDPOINT BEFORE PRODUCTION.
    /// </summary>
    [HttpGet("tickets/count")]
    [AllowAnonymous]
    public async Task<IActionResult> GetTicketCount()
    {
        try
        {
            // Only allow in Development
            if (!_environment.IsDevelopment())
            {
                return NotFound(new { message = "Endpoint not available in production" });
            }

            var count = await _context.Tickets.CountAsync();
            return Ok(new { count });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "GetTicketCount: Error counting tickets. Exception: {ExceptionType}, Message: {Message}",
                ex.GetType().Name, ex.Message);
            return StatusCode(500, new { message = "An error occurred while counting tickets", error = ex.Message });
        }
    }
}

