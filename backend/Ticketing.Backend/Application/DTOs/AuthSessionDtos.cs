namespace Ticketing.Backend.Application.DTOs;

public class AuthSessionResponse
{
    public bool IsAuthenticated { get; set; }
    public string? UserId { get; set; }
    public string? Email { get; set; }
    public string? DisplayName { get; set; }
    public string[] TikqRoles { get; set; } = Array.Empty<string>();
    public bool IsSupervisor { get; set; }
}
