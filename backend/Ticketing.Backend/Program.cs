using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Ticketing.Backend.Application.Services;
using Ticketing.Backend.Domain.Entities;
using Ticketing.Backend.Infrastructure.Auth;
using Ticketing.Backend.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// =======================
// JWT configuration
// =======================
var jwtSettings = new JwtSettings();
builder.Configuration.GetSection("Jwt").Bind(jwtSettings);

// Fallback secret for local development
if (string.IsNullOrWhiteSpace(jwtSettings.Secret))
{
    jwtSettings.Secret =
        builder.Configuration["JWT_SECRET"] ?? "SuperSecretDevelopmentKey!ChangeMe";
}

builder.Services.AddSingleton(jwtSettings);

// =======================
// DbContext (SQLite) - DETERMINISTIC PATH
// =======================
// Resolve SQLite DB path to an absolute path based on ContentRoot
// This ensures the same DB file is used regardless of working directory
var sqliteDbPath = ResolveSqliteDbPath(builder.Configuration, builder.Environment.ContentRootPath);
var sqliteConnectionString = $"Data Source={sqliteDbPath}";

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConnectionString));

// Log resolved path at startup (safe: no secrets)
Console.WriteLine($"[STARTUP] Resolved SQLite DB Path: {sqliteDbPath}");

// Helper: Resolve SQLite DB path to absolute path under ContentRoot
static string ResolveSqliteDbPath(IConfiguration config, string contentRoot)
{
    var connectionString = config.GetConnectionString("DefaultConnection");
    
    // Default relative path if not configured
    var relativePath = "App_Data/ticketing.db";
    
    if (!string.IsNullOrWhiteSpace(connectionString))
    {
        // Extract file path from "Data Source=<path>" format
        var dataSourcePrefix = "Data Source=";
        if (connectionString.StartsWith(dataSourcePrefix, StringComparison.OrdinalIgnoreCase))
        {
            var extractedPath = connectionString.Substring(dataSourcePrefix.Length).Trim();
            if (!string.IsNullOrWhiteSpace(extractedPath))
            {
                relativePath = extractedPath;
            }
        }
    }
    
    // If already absolute, use as-is
    if (Path.IsPathRooted(relativePath))
    {
        var directory = Path.GetDirectoryName(relativePath);
        if (!string.IsNullOrEmpty(directory) && !Directory.Exists(directory))
        {
            Directory.CreateDirectory(directory);
        }
        return relativePath;
    }
    
    // Convert relative path to absolute under ContentRoot
    var absolutePath = Path.Combine(contentRoot, relativePath);
    var absoluteDirectory = Path.GetDirectoryName(absolutePath);
    
    // Ensure directory exists
    if (!string.IsNullOrEmpty(absoluteDirectory) && !Directory.Exists(absoluteDirectory))
    {
        Directory.CreateDirectory(absoluteDirectory);
    }
    
    return absolutePath;
}

// =======================
// Schema Guard: Ensures SubcategoryFieldDefinitions table has all required columns
// =======================
static async Task EnsureSubcategoryFieldDefinitionsSchemaAsync(
    AppDbContext context,
    ILogger logger,
    string dbPath)
{
    try
    {
        logger.LogInformation("[SCHEMA_GUARD] Verifying SubcategoryFieldDefinitions table schema...");
        
        // Use PRAGMA table_info to check existing columns
        var connection = context.Database.GetDbConnection();
        var wasOpen = connection.State == System.Data.ConnectionState.Open;
        
        if (!wasOpen)
        {
            await connection.OpenAsync();
        }
        
        // First, check if table exists
        bool tableExists = false;
        using (var checkTableCommand = connection.CreateCommand())
        {
            checkTableCommand.CommandText = @"
                SELECT name FROM sqlite_master 
                WHERE type='table' AND name='SubcategoryFieldDefinitions';
            ";
            var result = await checkTableCommand.ExecuteScalarAsync();
            tableExists = result != null;
        }
        
        if (!tableExists)
        {
            logger.LogWarning("[SCHEMA_GUARD] Table SubcategoryFieldDefinitions does not exist. Migrations should have created it.");
            if (!wasOpen)
            {
                await connection.CloseAsync();
            }
            return; // Table will be created by migrations, schema guard only fixes existing tables
        }
        
        var columns = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        using (var command = connection.CreateCommand())
        {
            command.CommandText = "PRAGMA table_info(SubcategoryFieldDefinitions)";
            using (var reader = await command.ExecuteReaderAsync())
            {
                while (await reader.ReadAsync())
                {
                    var columnName = reader.GetString(1); // column name is at index 1
                    columns.Add(columnName);
                }
            }
        }

        if (!wasOpen)
        {
            await connection.CloseAsync();
        }

        logger.LogInformation("[SCHEMA_GUARD] Existing columns: {Columns}", string.Join(", ", columns));

        // Required columns with their SQL definitions (only nullable ones can be safely added)
        var requiredColumns = new Dictionary<string, string>
        {
            { "DefaultValue", "TEXT" },
            { "OptionsJson", "TEXT" },
            { "Min", "REAL" },
            { "Max", "REAL" }
        };

        var missingColumns = new List<string>();
        foreach (var required in requiredColumns)
        {
            if (!columns.Contains(required.Key))
            {
                missingColumns.Add(required.Key);
            }
        }

        if (missingColumns.Count == 0)
        {
            logger.LogInformation("[SCHEMA_GUARD] All required columns exist - schema is valid");
            return;
        }

        logger.LogWarning("[SCHEMA_GUARD] Missing columns detected: {MissingColumns}", string.Join(", ", missingColumns));

        // Backup database before making schema changes
        if (File.Exists(dbPath))
        {
            var backupPath = $"{dbPath}.backup.{DateTime.UtcNow:yyyyMMddHHmmss}";
            try
            {
                File.Copy(dbPath, backupPath, overwrite: true);
                logger.LogInformation("[SCHEMA_GUARD] Database backed up to: {BackupPath}", backupPath);
            }
            catch (Exception backupEx)
            {
                logger.LogWarning(backupEx, "[SCHEMA_GUARD] Failed to create backup: {Error}", backupEx.Message);
                // Continue anyway - schema fix is important
            }
        }

        // Add missing columns one by one
        foreach (var missingColumn in missingColumns)
        {
            try
            {
                var columnDef = requiredColumns[missingColumn];
                var addColumnSql = $"ALTER TABLE SubcategoryFieldDefinitions ADD COLUMN {missingColumn} {columnDef};";
                
                logger.LogInformation("[SCHEMA_GUARD] Adding missing column: {Column} with definition: {Definition}", 
                    missingColumn, columnDef);
                
                await context.Database.ExecuteSqlRawAsync(addColumnSql);
                
                logger.LogInformation("[SCHEMA_GUARD] Successfully added column: {Column}", missingColumn);
            }
            catch (Exception addEx)
            {
                // Check if column was added by another process or already exists
                if (addEx.Message.Contains("duplicate column", StringComparison.OrdinalIgnoreCase) ||
                    addEx.Message.Contains("already exists", StringComparison.OrdinalIgnoreCase))
                {
                    logger.LogInformation("[SCHEMA_GUARD] Column {Column} already exists (possibly added concurrently)", missingColumn);
                }
                else
                {
                    logger.LogError(addEx, "[SCHEMA_GUARD] Failed to add column {Column}: {Error}", 
                        missingColumn, addEx.Message);
                    // Continue with other columns
                }
            }
        }

        logger.LogInformation("[SCHEMA_GUARD] Schema guard completed");
    }
    catch (Exception ex)
    {
        // Log but don't fail startup - let runtime handle errors
        logger.LogWarning(ex, "[SCHEMA_GUARD] Schema guard encountered an error: {Error}", ex.Message);
    }
}

// =======================
// Repositories
// =======================
builder.Services.AddScoped<Ticketing.Backend.Application.Repositories.IFieldDefinitionRepository, 
    Ticketing.Backend.Infrastructure.Data.Repositories.FieldDefinitionRepository>();
builder.Services.AddScoped<Ticketing.Backend.Application.Repositories.ICategoryRepository, 
    Ticketing.Backend.Infrastructure.Data.Repositories.CategoryRepository>();
builder.Services.AddScoped<Ticketing.Backend.Application.Repositories.ISystemSettingsRepository, 
    Ticketing.Backend.Infrastructure.Data.Repositories.SystemSettingsRepository>();
builder.Services.AddScoped<Ticketing.Backend.Application.Repositories.IUserPreferencesRepository, 
    Ticketing.Backend.Infrastructure.Data.Repositories.UserPreferencesRepository>();
builder.Services.AddScoped<Ticketing.Backend.Application.Repositories.INotificationRepository, 
    Ticketing.Backend.Infrastructure.Data.Repositories.NotificationRepository>();
builder.Services.AddScoped<Ticketing.Backend.Application.Repositories.ITechnicianRepository, 
    Ticketing.Backend.Infrastructure.Data.Repositories.TechnicianRepository>();
builder.Services.AddScoped<Ticketing.Backend.Application.Repositories.IUserRepository, 
    Ticketing.Backend.Infrastructure.Data.Repositories.UserRepository>();
builder.Services.AddScoped<Ticketing.Backend.Application.Repositories.ITicketRepository, 
    Ticketing.Backend.Infrastructure.Data.Repositories.TicketRepository>();
builder.Services.AddScoped<Ticketing.Backend.Application.Repositories.ITicketMessageRepository, 
    Ticketing.Backend.Infrastructure.Data.Repositories.TicketMessageRepository>();
builder.Services.AddScoped<Ticketing.Backend.Application.Repositories.IUnitOfWork, 
    Ticketing.Backend.Infrastructure.Data.Repositories.UnitOfWork>();

// =======================
// Application services
// =======================
builder.Services.AddScoped<IJwtTokenGenerator, JwtTokenGenerator>();
builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<IFieldDefinitionService, FieldDefinitionService>();
builder.Services.AddScoped<INotificationService, NotificationService>();
builder.Services.AddScoped<ITicketService, TicketService>();
builder.Services.AddScoped<ISystemSettingsService, SystemSettingsService>();
builder.Services.AddScoped<IUserPreferencesService, UserPreferencesService>();
builder.Services.AddScoped<ISmartAssignmentService, SmartAssignmentService>();
builder.Services.AddScoped<ITechnicianService, TechnicianService>();

// =======================
// Authentication / JWT
// =======================
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings.Issuer,
        ValidAudience = jwtSettings.Audience,
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSettings.Secret))
    };
});

builder.Services.AddAuthorization();

// =======================
// CORS
// =======================
var allowedCorsOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>() ??
    new[]
    {
        "http://localhost:3000",
        "https://localhost:3000",
        "http://localhost:3001",
        "https://localhost:3001"
    };

builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy
            .WithOrigins(allowedCorsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials());
});

// =======================
// MVC / JSON
// =======================
builder.Services.AddControllers().AddJsonOptions(options =>
{
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.DictionaryKeyPolicy = JsonNamingPolicy.CamelCase;
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddEndpointsApiExplorer();

// =======================
// Swagger + JWT Configuration (SECURITY-CRITICAL)
// =======================
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Ticketing.Backend",
        Version = "v1"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        Description = "Enter ONLY the JWT token. Swagger will add 'Bearer ' automatically."
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// =======================
// Apply migrations & seed
// =======================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    var context = services.GetRequiredService<AppDbContext>();
    var passwordHasher = services.GetRequiredService<IPasswordHasher<User>>();
    var logger = services.GetRequiredService<ILogger<Program>>();

    try
    {
        logger.LogInformation("[MIGRATION] Starting database migration...");
        logger.LogInformation("[MIGRATION] Database path: {DbPath}", sqliteDbPath);
        logger.LogInformation("[MIGRATION] Database file exists: {Exists}", File.Exists(sqliteDbPath));
        
        var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
        var appliedMigrations = await context.Database.GetAppliedMigrationsAsync();
        
        logger.LogInformation("[MIGRATION] Applied migrations: {Applied}", string.Join(", ", appliedMigrations));
        logger.LogInformation("[MIGRATION] Pending migrations: {Pending}", string.Join(", ", pendingMigrations));
        
        await context.Database.MigrateAsync();
        
        var appliedAfter = await context.Database.GetAppliedMigrationsAsync();
        logger.LogInformation("[MIGRATION] Migrations after apply: {Applied}", string.Join(", ", appliedAfter));
        logger.LogInformation("[MIGRATION] Database migration completed successfully");
        
        // Post-migration schema guard: Verify SubcategoryFieldDefinitions table schema
        // This handles cases where migrations didn't apply correctly or schema drift occurred
        await EnsureSubcategoryFieldDefinitionsSchemaAsync(context, logger, sqliteDbPath);
    }
    catch (Exception ex)
    {
        logger.LogError(ex, "[MIGRATION] Error applying migrations: {Error}", ex.Message);
        
        // If migration fails due to column already existing, that's okay
        // Check if it's a SQLite error about column already existing
        if (ex.Message.Contains("duplicate column") || ex.Message.Contains("already exists"))
        {
            logger.LogWarning("[MIGRATION] Column may already exist - this is acceptable. Continuing...");
        }
        else if (ex.Message.Contains("no such column"))
        {
            // Schema drift detected - schema guard will handle it
            logger.LogWarning("[MIGRATION] Schema drift detected - schema guard will attempt to fix on next startup");
        }
        else
        {
            // Re-throw if it's a different error
            throw;
        }
    }
    
    await SeedData.InitializeAsync(context, passwordHasher);
}

// =======================
// Middleware pipeline
// =======================
app.UseCors("Frontend");

// Always enable Swagger
app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Ticketing.Backend v1");
    c.RoutePrefix = "swagger";
});

app.UseAuthentication();
app.UseAuthorization();

app.MapGet("/api/ping", () => Results.Ok(new { message = "pong" }));

app.MapControllers();

app.Run();
