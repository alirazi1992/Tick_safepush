using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Ticketing.Backend.Application.Repositories;
using Ticketing.Backend.Application.Services;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Domain.Enums;
using Ticketing.Backend.Infrastructure.Data;

namespace Ticketing.Backend.Infrastructure.Services;

public sealed class BossDbUserDirectoryService : IUserDirectoryService
{
    private readonly BossDbOptions _bossOptions;
    private readonly RoleMappingOptions _roleMapping;
    private readonly IUserRepository _userRepository;
    private readonly ITechnicianRepository _technicianRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMemoryCache _cache;
    private readonly ILogger<BossDbUserDirectoryService> _logger;

    public BossDbUserDirectoryService(
        IOptions<BossDbOptions> bossOptions,
        IOptions<HybridAuthOptions> authOptions,
        IUserRepository userRepository,
        ITechnicianRepository technicianRepository,
        IUnitOfWork unitOfWork,
        IMemoryCache cache,
        ILogger<BossDbUserDirectoryService> logger)
    {
        _bossOptions = bossOptions.Value;
        _roleMapping = authOptions.Value.RoleMapping;
        _userRepository = userRepository;
        _technicianRepository = technicianRepository;
        _unitOfWork = unitOfWork;
        _cache = cache;
        _logger = logger;
    }

    public async Task<BossDirectoryUser?> FindByEmailOrUpn(string identifier, CancellationToken cancellationToken = default)
    {
        var normalized = Normalize(identifier);
        if (string.IsNullOrWhiteSpace(normalized))
        {
            return null;
        }

        var cacheKey = $"boss-user:{normalized}";
        if (_cache.TryGetValue(cacheKey, out BossDirectoryUser? cached))
        {
            return cached;
        }

        if (string.IsNullOrWhiteSpace(_bossOptions.ConnectionString))
        {
            _logger.LogWarning("BossDb connection string is empty. Denying access for {Identifier}.", normalized);
            return null;
        }

        var table = SafeSqlIdentifier(_bossOptions.UserTable);
        var emailColumn = SafeSqlIdentifier(_bossOptions.EmailColumn);
        var upnColumn = SafeSqlIdentifier(_bossOptions.UpnColumn);
        var displayNameColumn = SafeSqlIdentifier(_bossOptions.DisplayNameColumn);
        var disabledColumn = SafeSqlIdentifier(_bossOptions.DisabledColumn);

        var sql = $"""
            SELECT TOP (1)
                [{emailColumn}] AS Email,
                [{upnColumn}] AS Upn,
                [{displayNameColumn}] AS DisplayName,
                [{disabledColumn}] AS DisabledValue
            FROM [{table}]
            WHERE LOWER([{emailColumn}]) = @identifier OR LOWER([{upnColumn}]) = @identifier
            """;

        await using var connection = new SqlConnection(_bossOptions.ConnectionString);
        await using var command = new SqlCommand(sql, connection)
        {
            CommandTimeout = 5
        };
        command.Parameters.AddWithValue("@identifier", normalized);

        try
        {
            await connection.OpenAsync(cancellationToken);
            await using var reader = await command.ExecuteReaderAsync(cancellationToken);
            if (!await reader.ReadAsync(cancellationToken))
            {
                _cache.Set(cacheKey, null, TimeSpan.FromMinutes(5));
                return null;
            }

            var disabledRaw = reader["DisabledValue"]?.ToString();
            var result = new BossDirectoryUser
            {
                Identifier = normalized,
                Email = reader["Email"]?.ToString(),
                Upn = reader["Upn"]?.ToString(),
                DisplayName = reader["DisplayName"]?.ToString(),
                IsDisabled = ParseDisabled(disabledRaw)
            };

            _cache.Set(cacheKey, result, TimeSpan.FromMinutes(5));
            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "BossDb lookup failed for {Identifier}. Failing closed.", normalized);
            throw new InvalidOperationException("User directory is temporarily unavailable.", ex);
        }
    }

    public bool IsDisabled(BossDirectoryUser user) => user.IsDisabled;

    public async Task<User> SyncLocalUser(BossDirectoryUser user, CancellationToken cancellationToken = default)
    {
        var preferredEmail = Normalize(user.Email) ?? Normalize(user.Upn) ?? Normalize(user.Identifier) ?? string.Empty;
        var fullName = string.IsNullOrWhiteSpace(user.DisplayName) ? preferredEmail : user.DisplayName.Trim();

        var localUser = await _userRepository.GetByEmailAsync(preferredEmail);
        var mappedRole = DetermineMappedRole(preferredEmail);
        var configuredDefaultRole = Enum.TryParse<UserRole>(_roleMapping.DefaultRole, ignoreCase: true, out var parsedDefaultRole)
            ? parsedDefaultRole
            : UserRole.Client;
        var targetRole = localUser?.Role ?? mappedRole ?? configuredDefaultRole;
        if (mappedRole.HasValue)
        {
            targetRole = mappedRole.Value;
        }

        if (localUser == null)
        {
            localUser = new User
            {
                Id = Guid.NewGuid(),
                Email = preferredEmail,
                FullName = fullName,
                CreatedAt = DateTime.UtcNow,
                Role = targetRole,
                PasswordHash = string.Empty
            };
            await _userRepository.AddAsync(localUser);
        }
        else
        {
            localUser.FullName = fullName;
            localUser.Email = preferredEmail;
            localUser.Role = targetRole;
            await _userRepository.UpdateAsync(localUser);
        }

        if (targetRole == UserRole.Technician)
        {
            var tech = await _technicianRepository.GetByUserIdAsync(localUser.Id);
            if (tech == null)
            {
                tech = new Technician
                {
                    Id = Guid.NewGuid(),
                    UserId = localUser.Id,
                    Email = localUser.Email,
                    FullName = localUser.FullName,
                    IsActive = true,
                    IsSupervisor = IsEmailInList(preferredEmail, _roleMapping.SupervisorEmails)
                };
                await _technicianRepository.AddAsync(tech);
            }
            else
            {
                tech.Email = localUser.Email;
                tech.FullName = localUser.FullName;
                tech.IsActive = true;
                tech.IsSupervisor = IsEmailInList(preferredEmail, _roleMapping.SupervisorEmails);
                await _technicianRepository.UpdateAsync(tech);
            }
        }

        await _unitOfWork.SaveChangesAsync(cancellationToken);
        return localUser;
    }

    private static bool ParseDisabled(string? rawValue)
    {
        if (string.IsNullOrWhiteSpace(rawValue))
        {
            return false;
        }

        var value = rawValue.Trim();
        return value.Equals("1", StringComparison.OrdinalIgnoreCase)
               || value.Equals("true", StringComparison.OrdinalIgnoreCase)
               || value.Equals("yes", StringComparison.OrdinalIgnoreCase)
               || value.Equals("disabled", StringComparison.OrdinalIgnoreCase);
    }

    private static string SafeSqlIdentifier(string value)
    {
        var trimmed = value.Trim();
        foreach (var c in trimmed)
        {
            if (!(char.IsLetterOrDigit(c) || c == '_'))
            {
                throw new InvalidOperationException($"Unsafe SQL identifier '{value}'.");
            }
        }

        return trimmed;
    }

    private static string? Normalize(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim().ToLowerInvariant();
    }

    private UserRole? DetermineMappedRole(string email)
    {
        if (IsEmailInList(email, _roleMapping.AdminEmails))
        {
            return UserRole.Admin;
        }

        if (IsEmailInList(email, _roleMapping.TechnicianEmails) || IsEmailInList(email, _roleMapping.SupervisorEmails))
        {
            return UserRole.Technician;
        }

        return null;
    }

    private static bool IsEmailInList(string email, IEnumerable<string> values)
    {
        return values.Any(v => string.Equals(v?.Trim(), email, StringComparison.OrdinalIgnoreCase));
    }
}
