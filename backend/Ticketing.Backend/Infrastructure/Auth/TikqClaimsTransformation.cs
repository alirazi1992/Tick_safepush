using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Application.Services;
using Ticketing.Backend.Domain.Enums;

namespace Ticketing.Backend.Infrastructure.Auth;

public sealed class TikqClaimsTransformation : IClaimsTransformation
{
    private readonly IUserDirectoryService _userDirectoryService;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly ILogger<TikqClaimsTransformation> _logger;

    public TikqClaimsTransformation(
        IUserDirectoryService userDirectoryService,
        ITechnicianRepository technicianRepository,
        ILogger<TikqClaimsTransformation> logger)
    {
        _userDirectoryService = userDirectoryService;
        _technicianRepository = technicianRepository;
        _logger = logger;
    }

    public async Task<ClaimsPrincipal> TransformAsync(ClaimsPrincipal principal)
    {
        if (principal.Identity?.IsAuthenticated != true)
        {
            return principal;
        }

        if (principal.HasClaim(c => c.Type == UserIdentityClaims.Resolved))
        {
            return principal;
        }

        var identifier = UserIdentityClaims.ExtractIdentifier(principal);
        if (string.IsNullOrWhiteSpace(identifier))
        {
            return Deny(principal, "No email/UPN claim was provided by the identity provider.");
        }

        BossDirectoryUser? directoryUser;
        try
        {
            directoryUser = await _userDirectoryService.FindByEmailOrUpn(identifier);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Directory lookup failed for {Identifier}.", identifier);
            return Deny(principal, "User directory is temporarily unavailable.");
        }
        if (directoryUser == null)
        {
            _logger.LogWarning("Authenticated principal {Identifier} not found in BossDb. Access denied.", identifier);
            return Deny(principal, "No access to TikQ");
        }

        if (_userDirectoryService.IsDisabled(directoryUser))
        {
            _logger.LogInformation("Authenticated principal {Identifier} is disabled in BossDb.", identifier);
            return Deny(principal, "Your account is disabled.");
        }

        var localUser = await _userDirectoryService.SyncLocalUser(directoryUser);
        var tech = localUser.Role == UserRole.Technician
            ? await _technicianRepository.GetByUserIdAsync(localUser.Id)
            : null;

        var clone = ClonePrincipal(principal);
        var roleValue = localUser.Role.ToString();
        AddOrReplace(clone, ClaimTypes.NameIdentifier, localUser.Id.ToString());
        AddOrReplace(clone, ClaimTypes.Email, localUser.Email);
        AddOrReplace(clone, "email", localUser.Email);
        AddOrReplace(clone, ClaimTypes.Name, localUser.FullName);
        AddOrReplace(clone, "name", localUser.FullName);
        AddOrReplace(clone, ClaimTypes.Role, roleValue);
        AddOrReplace(clone, "isSupervisor", tech?.IsSupervisor == true ? "true" : "false");
        AddOrReplace(clone, UserIdentityClaims.Identifier, identifier);
        AddOrReplace(clone, UserIdentityClaims.Access, "allowed");
        AddOrReplace(clone, UserIdentityClaims.Resolved, "true");

        return clone;
    }

    private static ClaimsPrincipal ClonePrincipal(ClaimsPrincipal principal)
    {
        var identities = principal.Identities.Select(identity =>
        {
            var filteredClaims = identity.Claims
                .Where(c => c.Type != ClaimTypes.NameIdentifier
                            && c.Type != ClaimTypes.Role
                            && c.Type != UserIdentityClaims.Access
                            && c.Type != UserIdentityClaims.DenyReason
                            && c.Type != UserIdentityClaims.Resolved)
                .ToList();

            return new ClaimsIdentity(filteredClaims, identity.AuthenticationType, ClaimTypes.Name, ClaimTypes.Role);
        });

        return new ClaimsPrincipal(identities);
    }

    private static ClaimsPrincipal Deny(ClaimsPrincipal principal, string reason)
    {
        var clone = ClonePrincipal(principal);
        AddOrReplace(clone, UserIdentityClaims.Access, "denied");
        AddOrReplace(clone, UserIdentityClaims.DenyReason, reason);
        AddOrReplace(clone, UserIdentityClaims.Resolved, "true");
        return clone;
    }

    private static void AddOrReplace(ClaimsPrincipal principal, string claimType, string value)
    {
        var identities = principal.Identities.ToList();
        if (identities.Count == 0)
        {
            return;
        }

        foreach (var identity in identities)
        {
            var existing = identity.FindAll(claimType).ToList();
            foreach (var claim in existing)
            {
                identity.RemoveClaim(claim);
            }

            identity.AddClaim(new Claim(claimType, value));
        }
    }
}
