using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Ticketing.Backend.Api.Middleware;
using Ticketing.Backend.Infrastructure.Auth;
using Xunit;

namespace Ticketing.Backend.Tests;

public class HybridAuthTests
{
    [Fact]
    public void ExtractIdentifier_PrefersEmailThenUpnOrder()
    {
        var principal = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim("unique_name", "legacy@corp.local"),
            new Claim("upn", "user@corp.local"),
            new Claim(ClaimTypes.Email, "primary@corp.local")
        }, "Test"));

        var identifier = UserIdentityClaims.ExtractIdentifier(principal);

        Assert.Equal("primary@corp.local", identifier);
    }

    [Fact]
    public async Task AccessGuard_Returns403_ForDeniedAuthenticatedPrincipal()
    {
        var middleware = new TikqAccessGuardMiddleware(_ => Task.CompletedTask);
        var context = new DefaultHttpContext();
        context.SetEndpoint(new Endpoint(
            _ => Task.CompletedTask,
            new EndpointMetadataCollection(new AuthorizeAttribute()),
            "auth-endpoint"));

        var identity = new ClaimsIdentity("Test");
        identity.AddClaim(new Claim(UserIdentityClaims.Access, "denied"));
        identity.AddClaim(new Claim(UserIdentityClaims.DenyReason, "No access to TikQ"));
        context.User = new ClaimsPrincipal(identity);

        await middleware.Invoke(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
    }
}
