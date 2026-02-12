using Ticketing.Backend.Domain.Entities;

namespace Ticketing.Backend.Application.Services;

public interface IUserDirectoryService
{
    Task<BossDirectoryUser?> FindByEmailOrUpn(string identifier, CancellationToken cancellationToken = default);
    bool IsDisabled(BossDirectoryUser user);
    Task<User> SyncLocalUser(BossDirectoryUser user, CancellationToken cancellationToken = default);
}

public sealed class BossDirectoryUser
{
    public string Identifier { get; init; } = string.Empty;
    public string? Email { get; init; }
    public string? Upn { get; init; }
    public string? DisplayName { get; init; }
    public bool IsDisabled { get; init; }
}
