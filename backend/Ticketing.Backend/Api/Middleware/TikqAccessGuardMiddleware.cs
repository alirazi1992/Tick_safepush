using Microsoft.AspNetCore.Authorization;
using Ticketing.Backend.Infrastructure.Auth;

namespace Ticketing.Backend.Api.Middleware;

public sealed class TikqAccessGuardMiddleware
{
    private readonly RequestDelegate _next;

    public TikqAccessGuardMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task Invoke(HttpContext context)
    {
        var endpoint = context.GetEndpoint();
        var allowAnonymous = endpoint?.Metadata.GetMetadata<IAllowAnonymous>() != null;

        if (!allowAnonymous && context.User.Identity?.IsAuthenticated == true)
        {
            var access = context.User.FindFirst(UserIdentityClaims.Access)?.Value;
            if (string.Equals(access, "denied", StringComparison.OrdinalIgnoreCase))
            {
                var reason = context.User.FindFirst(UserIdentityClaims.DenyReason)?.Value ?? "No access to TikQ";
                context.Response.StatusCode = StatusCodes.Status403Forbidden;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsJsonAsync(new
                {
                    message = reason
                });
                return;
            }
        }

        await _next(context);
    }
}
