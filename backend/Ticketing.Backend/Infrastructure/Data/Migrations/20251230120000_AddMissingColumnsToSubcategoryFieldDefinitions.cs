using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
/// <summary>
/// Safely adds missing DefaultValue column to SubcategoryFieldDefinitions table.
/// Idempotent: SQL Server uses COL_LENGTH check; SQLite uses try/catch on ADD COLUMN.
/// Smoke test (SQL Server): reset-tikq-db.ps1 -Force, then set Database__Provider=SqlServer,
/// ConnectionStrings__DefaultConnection=Server=.;Database=TikQ;Trusted_Connection=True;TrustServerCertificate=True;, dotnet run.
/// </summary>
[DbContext(typeof(AppDbContext))]
[Migration("20251230120000_AddMissingColumnsToSubcategoryFieldDefinitions")]
public partial class AddMissingColumnsToSubcategoryFieldDefinitions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider?.Contains("SqlServer") == true)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'DefaultValue') IS NULL
BEGIN
    ALTER TABLE [SubcategoryFieldDefinitions] ADD [DefaultValue] nvarchar(500) NULL;
END");
        }
        else if (ActiveProvider?.Contains("Sqlite") == true)
        {
            try
            {
                migrationBuilder.Sql("ALTER TABLE SubcategoryFieldDefinitions ADD COLUMN DefaultValue TEXT NULL;");
            }
            catch
            {
                // Column already exists (e.g. from 20251230000000); ignore.
            }
        }
        else
        {
            try
            {
                migrationBuilder.AddColumn<string>(
                    name: "DefaultValue",
                    table: "SubcategoryFieldDefinitions",
                    maxLength: 500,
                    nullable: true);
            }
            catch
            {
                // Column already exists; avoid crash.
            }
        }
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider?.Contains("SqlServer") == true)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'DefaultValue') IS NOT NULL
BEGIN
    ALTER TABLE [SubcategoryFieldDefinitions] DROP COLUMN [DefaultValue];
END");
        }
        else
        {
            migrationBuilder.DropColumn(
                name: "DefaultValue",
                table: "SubcategoryFieldDefinitions");
        }
    }
}
