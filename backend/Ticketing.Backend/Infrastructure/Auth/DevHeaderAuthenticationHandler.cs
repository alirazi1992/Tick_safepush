using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Ticketing.Backend.Infrastructure.Auth;

public sealed class DevHeaderAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IWebHostEnvironment _environment;
    private readonly IOptions<HybridAuthOptions> _authOptions;

    public DevHeaderAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        IWebHostEnvironment environment,
        IOptions<HybridAuthOptions> authOptions)
        : base(options, logger, encoder)
    {
        _environment = environment;
        _authOptions = authOptions;
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!_environment.IsDevelopment() || !_authOptions.Value.DevHeaderAuth.Enabled)
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        if (!Request.Headers.TryGetValue("X-Dev-User", out var devUserRaw))
        {
            return Task.FromResult(AuthenticateResult.NoResult());
        }

        var identifier = devUserRaw.ToString().Trim();
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return Task.FromResult(AuthenticateResult.Fail("X-Dev-User header is empty."));
        }

        var claims = new List<Claim>
        {
            new(UserIdentityClaims.Identifier, identifier),
            new(ClaimTypes.Email, identifier),
            new("email", identifier),
            new("preferred_username", identifier),
            new("unique_name", identifier)
        };

        if (Request.Headers.TryGetValue("X-Dev-Roles", out var rolesHeader))
        {
            foreach (var role in rolesHeader.ToString().Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries))
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }
        }

        var identity = new ClaimsIdentity(claims, Scheme.Name, ClaimTypes.Name, ClaimTypes.Role);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
