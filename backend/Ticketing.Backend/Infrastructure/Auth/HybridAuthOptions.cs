namespace Ticketing.Backend.Infrastructure.Auth;

public sealed class HybridAuthOptions
{
    public string Mode { get; set; } = "Hybrid";
    public WindowsAuthOptions Windows { get; set; } = new();
    public HybridJwtOptions Jwt { get; set; } = new();
    public DevHeaderAuthOptions DevHeaderAuth { get; set; } = new();
    public AdfsOidcOptions Oidc { get; set; } = new();
    public RoleMappingOptions RoleMapping { get; set; } = new();
}

public sealed class WindowsAuthOptions
{
    public bool Enabled { get; set; } = true;
}

public sealed class HybridJwtOptions
{
    public bool Enabled { get; set; } = true;
    public string? Authority { get; set; }
    public string? Audience { get; set; }
    public string? MetadataAddress { get; set; }
    public string[] ValidIssuers { get; set; } = Array.Empty<string>();
}

public sealed class DevHeaderAuthOptions
{
    public bool Enabled { get; set; } = false;
}

public sealed class AdfsOidcOptions
{
    public bool Enabled { get; set; } = false;
    public string? Authority { get; set; }
    public string? MetadataAddress { get; set; }
    public string? ClientId { get; set; }
    public string? ClientSecret { get; set; }
    public string? CallbackPath { get; set; }
    public string? SignedOutCallbackPath { get; set; }
    public string? Scope { get; set; } = "openid profile email";
}

public sealed class RoleMappingOptions
{
    public string DefaultRole { get; set; } = "Client";
    public string[] AdminEmails { get; set; } = Array.Empty<string>();
    public string[] SupervisorEmails { get; set; } = Array.Empty<string>();
    public string[] TechnicianEmails { get; set; } = Array.Empty<string>();
}
