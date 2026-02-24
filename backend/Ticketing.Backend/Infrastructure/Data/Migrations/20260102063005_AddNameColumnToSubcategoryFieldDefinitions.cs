using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20260102063005_AddNameColumnToSubcategoryFieldDefinitions")]
public partial class AddNameColumnToSubcategoryFieldDefinitions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider?.Contains("SqlServer") == true)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'Name') IS NULL
BEGIN
    ALTER TABLE [SubcategoryFieldDefinitions] ADD [Name] nvarchar(200) NULL;
END");
        }
        else if (ActiveProvider?.Contains("Sqlite") == true)
        {
            try
            {
                migrationBuilder.Sql("ALTER TABLE SubcategoryFieldDefinitions ADD COLUMN Name TEXT NULL;");
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
                    name: "Name",
                    table: "SubcategoryFieldDefinitions",
                    maxLength: 200,
                    nullable: true);
            }
            catch
            {
                // Column already exists; avoid crash.
            }
        }

        migrationBuilder.Sql("UPDATE SubcategoryFieldDefinitions SET Name = [Key] WHERE Name IS NULL;");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider?.Contains("SqlServer") == true)
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'Name') IS NOT NULL
BEGIN
    ALTER TABLE [SubcategoryFieldDefinitions] DROP COLUMN [Name];
END");
        }
        else
        {
            // SQLite doesn't support DROP COLUMN directly; no-op for rollback.
            migrationBuilder.Sql(@"
            -- SQLite limitation: cannot drop column directly
            -- This migration should not be rolled back in production
            ");
        }
    }
}

