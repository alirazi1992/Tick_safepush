using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Ticketing.Backend.Application.DTOs;
using Ticketing.Backend.Application.Services;
using Ticketing.Backend.Domain.Enums;
using Ticketing.Backend.Infrastructure.Auth;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly AppDbContext _context;
    private readonly HybridAuthOptions _authOptions;

    public AuthController(
        IUserService userService,
        AppDbContext context,
        IOptions<HybridAuthOptions> authOptions)
    {
        _userService = userService;
        _context = context;
        _authOptions = authOptions.Value;
    }

    [HttpGet("debug-users")]
    [AllowAnonymous]
    public async Task<IActionResult> GetDebugUsers()
    {
        var users = await _context.Users
            .Select(u => new
            {
                u.Email,
                u.FullName,
                Role = u.Role.ToString(),
                u.Department
            })
            .ToListAsync();

        return Ok(users);
    }

    [HttpPost("register")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (!request.Role.HasValue)
        {
            return BadRequest(new
            {
                message = "Role is required and must be explicitly specified. Valid values: Client (0), Technician (1), Admin (2)",
                error = "ROLE_REQUIRED",
                validRoles = new[] { "Client", "Technician", "Admin" }
            });
        }

        var role = request.Role.Value;
        if (!Enum.IsDefined(typeof(UserRole), role))
        {
            return BadRequest(new
            {
                message = "Invalid role specified. Role must be explicitly set to one of: Client (0), Technician (1), Admin (2)",
                error = "INVALID_ROLE",
                validRoles = new[] { "Client", "Technician", "Admin" },
                receivedRole = role.ToString()
            });
        }

        if (role == UserRole.Admin)
        {
            if (User.Identity?.IsAuthenticated != true)
            {
                return StatusCode(403, new
                {
                    message = "Admin account creation requires authentication. Only authenticated Admin users can create Admin accounts.",
                    error = "ADMIN_REGISTRATION_REQUIRES_AUTH",
                    requestedRole = role.ToString()
                });
            }

            var callerRoleClaim = User.FindFirstValue(ClaimTypes.Role);
            if (string.IsNullOrEmpty(callerRoleClaim) ||
                !Enum.TryParse<UserRole>(callerRoleClaim, out var callerRole) ||
                callerRole != UserRole.Admin)
            {
                return StatusCode(403, new
                {
                    message = "Only Admin users can create Admin accounts. Your role does not have permission to create Admin users.",
                    error = "ADMIN_REGISTRATION_FORBIDDEN",
                    requestedRole = role.ToString(),
                    callerRole = callerRoleClaim ?? "unknown"
                });
            }
        }

        UserRole creatorRole = UserRole.Client;
        if (User.Identity?.IsAuthenticated == true)
        {
            var roleClaim = User.FindFirstValue(ClaimTypes.Role);
            if (!string.IsNullOrWhiteSpace(roleClaim) && Enum.TryParse<UserRole>(roleClaim, out var parsedRole))
            {
                creatorRole = parsedRole;
            }
        }

        var serviceRequest = new RegisterRequest
        {
            FullName = request.FullName,
            Email = request.Email,
            Password = request.Password,
            Role = role,
            PhoneNumber = request.PhoneNumber,
            Department = request.Department
        };

        var response = await _userService.RegisterAsync(serviceRequest, creatorRole);
        if (response == null)
        {
            var emailExists = await _context.Users.AnyAsync(u => u.Email.ToLower() == serviceRequest.Email.ToLower());
            if (emailExists)
            {
                return Conflict(new
                {
                    message = "Email address is already registered.",
                    error = "EMAIL_EXISTS"
                });
            }

            return BadRequest(new
            {
                message = "Unable to register user. Please check your request and try again.",
                error = "REGISTRATION_FAILED"
            });
        }

        if (response.User?.Role != role)
        {
            return StatusCode(500, new
            {
                message = "System error: Role mismatch detected. Contact administrator.",
                error = "ROLE_MISMATCH"
            });
        }

        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var response = await _userService.LoginAsync(request);
        if (response == null)
        {
            return Unauthorized(new
            {
                message = "Invalid email or password.",
                error = "INVALID_CREDENTIALS"
            });
        }

        return Ok(response);
    }

    [HttpGet("me")]
    [Authorize]
    public async Task<ActionResult<AuthSessionResponse>> Me()
    {
        if (User.Identity?.IsAuthenticated != true)
        {
            return Challenge();
        }

        var access = User.FindFirstValue(UserIdentityClaims.Access);
        if (string.Equals(access, "denied", StringComparison.OrdinalIgnoreCase))
        {
            return StatusCode(403, new
            {
                message = User.FindFirstValue(UserIdentityClaims.DenyReason) ?? "No access to TikQ"
            });
        }

        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var userId))
        {
            return StatusCode(403, new { message = "No access to TikQ" });
        }

        var user = await _userService.GetByIdAsync(userId);
        if (user == null)
        {
            return StatusCode(403, new { message = "No access to TikQ" });
        }

        return Ok(new AuthSessionResponse
        {
            IsAuthenticated = true,
            UserId = user.Id.ToString(),
            Email = user.Email,
            DisplayName = user.FullName,
            TikqRoles = new[] { user.Role.ToString() },
            IsSupervisor = user.IsSupervisor
        });
    }

    [HttpGet("whoami")]
    [Authorize]
    public Task<ActionResult<AuthSessionResponse>> WhoAmI()
    {
        return Me();
    }

    [HttpGet("adfs/login")]
    [AllowAnonymous]
    public IActionResult StartAdfsLogin([FromQuery] string? returnUrl = "/")
    {
        if (!_authOptions.Oidc.Enabled)
        {
            return StatusCode(503, new { message = "Company SSO is not configured yet." });
        }

        var safeReturnUrl = string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl)
            ? "/"
            : returnUrl;

        return Challenge(
            new AuthenticationProperties { RedirectUri = safeReturnUrl },
            AuthSchemes.AdfsOidc);
    }

    [HttpPost("adfs/logout")]
    [Authorize]
    public IActionResult AdfsLogout([FromQuery] string? returnUrl = "/")
    {
        var safeReturnUrl = string.IsNullOrWhiteSpace(returnUrl) || !Url.IsLocalUrl(returnUrl)
            ? "/"
            : returnUrl;

        return SignOut(
            new AuthenticationProperties { RedirectUri = safeReturnUrl },
            AuthSchemes.ExternalCookie);
    }

    [HttpPut("me")]
    [Authorize]
    public async Task<ActionResult<UserDto>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var userId))
        {
            return Unauthorized();
        }

        var user = await _userService.UpdateProfileAsync(userId, request);
        if (user == null)
        {
            return Conflict("Unable to update profile with the provided information.");
        }

        return Ok(user);
    }

    [HttpPost("change-password")]
    [Authorize]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var idValue = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (!Guid.TryParse(idValue, out var userId))
        {
            return Unauthorized("کاربر احراز هویت نشده است");
        }

        var (success, errorMessage) = await _userService.ChangePasswordAsync(
            userId,
            request.CurrentPassword,
            request.NewPassword,
            request.ConfirmNewPassword);

        if (!success)
        {
            return BadRequest(new { message = errorMessage ?? "رمز عبور قابل تغییر نیست" });
        }

        return Ok(new { success = true, message = "رمز عبور با موفقیت تغییر کرد" });
    }
}
