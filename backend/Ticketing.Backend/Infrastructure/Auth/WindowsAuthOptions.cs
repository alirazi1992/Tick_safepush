namespace Ticketing.Backend.Infrastructure.Auth;

/// <summary>
/// Options for Windows Integrated Authentication (Negotiate).
/// When Enabled is false, the Smart scheme uses JWT only; when true, requests without Bearer may use Negotiate.
/// </summary>
public class WindowsAuthOptions
{
    public const string SectionName = "WindowsAuth";

    /// <summary>
    /// When true, allow Negotiate for requests that do not have Authorization: Bearer.
    /// When false (default), only JWT is used; login form is the only way to authenticate.
    /// </summary>
    public bool Enabled { get; set; }
}
