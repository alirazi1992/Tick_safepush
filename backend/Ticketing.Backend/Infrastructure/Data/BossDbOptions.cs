namespace Ticketing.Backend.Infrastructure.Data;

public sealed class BossDbOptions
{
    public string? ConnectionString { get; set; }
    public string UserTable { get; set; } = "Users";
    public string EmailColumn { get; set; } = "Email";
    public string UpnColumn { get; set; } = "Upn";
    public string DisplayNameColumn { get; set; } = "DisplayName";
    public string DisabledColumn { get; set; } = "IsDisabled";
}
