using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;

namespace Ticketing.Backend.Infrastructure.Auth;

public static class UserIdentityClaims
{
    public const string Access = "tikq_access";
    public const string DenyReason = "tikq_access_reason";
    public const string Resolved = "tikq_resolved";
    public const string Identifier = "tikq_identifier";

    public static string? ExtractIdentifier(ClaimsPrincipal principal)
    {
        var preferredClaims = new[]
        {
            ClaimTypes.Email,
            "email",
            "upn",
            ClaimTypes.Upn,
            "preferred_username",
            "unique_name"
        };

        foreach (var claimType in preferredClaims)
        {
            var value = principal.FindFirstValue(claimType);
            if (!string.IsNullOrWhiteSpace(value))
            {
                return value.Trim();
            }
        }

        var name = principal.Identity?.Name;
        return string.IsNullOrWhiteSpace(name) ? null : name.Trim();
    }
}
