using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;

namespace Ticketing.Backend.Infrastructure.Auth;

public sealed class DisabledAdfsAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public DisabledAdfsAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        return Task.FromResult(AuthenticateResult.NoResult());
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        Response.StatusCode = StatusCodes.Status503ServiceUnavailable;
        Response.ContentType = "application/json";
        return Response.WriteAsJsonAsync(new
        {
            message = "ADFS OIDC login is not configured."
        });
    }
}
