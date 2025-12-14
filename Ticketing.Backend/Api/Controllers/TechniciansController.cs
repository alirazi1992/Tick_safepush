using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Ticketing.Backend.Application.DTOs;
using Ticketing.Backend.Application.Services;
using Ticketing.Backend.Domain.Enums;

namespace Ticketing.Backend.Api.Controllers;

[ApiController]
[Route("api/admin/technicians")]
[Authorize(Roles = nameof(UserRole.Admin))]
public class TechniciansController : ControllerBase
{
    private readonly ITechnicianService _technicianService;
    private readonly ILogger<TechniciansController> _logger;

    public TechniciansController(ITechnicianService technicianService, ILogger<TechniciansController> logger)
    {
        _technicianService = technicianService;
        _logger = logger;
    }

    /// <summary>
    /// Get all technicians
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetAllTechnicians()
    {
        var technicians = await _technicianService.GetAllTechniciansAsync();
        return Ok(technicians);
    }

    /// <summary>
    /// Get technician by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetTechnician(Guid id)
    {
        var technician = await _technicianService.GetTechnicianByIdAsync(id);
        if (technician == null)
        {
            return NotFound();
        }
        return Ok(technician);
    }

    /// <summary>
    /// Create a new technician
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> CreateTechnician([FromBody] TechnicianCreateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            var technician = await _technicianService.CreateTechnicianAsync(request);
            _logger.LogInformation("Technician created: {TechnicianId}", technician.Id);
            return CreatedAtAction(nameof(GetTechnician), new { id = technician.Id }, technician);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create technician");
            return StatusCode(500, "Failed to create technician");
        }
    }

    /// <summary>
    /// Update technician
    /// </summary>
    /// <param name="id">Technician ID</param>
    /// <param name="request">Technician update request including fullName, email, phone, department, and isActive</param>
    /// <returns>Updated technician</returns>
    /// <response code="200">Technician updated successfully</response>
    /// <response code="400">Invalid request data</response>
    /// <response code="404">Technician not found</response>
    /// <response code="401">Unauthorized</response>
    /// <response code="403">Forbidden - Admin role required</response>
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TechnicianResponse), 200)]
    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(401)]
    [ProducesResponseType(403)]
    public async Task<IActionResult> UpdateTechnician(Guid id, [FromBody] TechnicianUpdateRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var technician = await _technicianService.UpdateTechnicianAsync(id, request);
        if (technician == null)
        {
            return NotFound();
        }

        return Ok(technician);
    }

    /// <summary>
    /// Update technician status (active/inactive)
    /// </summary>
    [HttpPatch("{id}/status")]
    public async Task<IActionResult> UpdateTechnicianStatus(Guid id, [FromBody] TechnicianStatusUpdateRequest request)
    {
        var success = await _technicianService.UpdateTechnicianStatusAsync(id, request.IsActive);
        if (!success)
        {
            return NotFound();
        }

        return Ok(new { message = "Technician status updated successfully" });
    }
}

