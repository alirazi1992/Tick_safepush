using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

/// <summary>
/// Adds SortOrder, IsActive, CreatedAt, UpdatedAt to SubcategoryFieldDefinitions on SQL Server.
/// These columns were never added by prior migrations on SQL Server (only on SQLite via AddCategoryFieldDefinitions).
/// Idempotent: uses COL_LENGTH checks. No data loss; existing rows get defaults.
/// </summary>
[DbContext(typeof(AppDbContext))]
[Migration("20260224100000_AddSubcategoryFieldDefinitionAuditColumns")]
public partial class AddSubcategoryFieldDefinitionAuditColumns : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider?.Contains("SqlServer") != true)
            return;

        // SortOrder int NOT NULL DEFAULT 0
        migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'SortOrder') IS NULL
BEGIN
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] ADD [SortOrder] int NOT NULL CONSTRAINT [DF_SubcategoryFieldDefinitions_SortOrder] DEFAULT 0;
END");

        // IsActive bit NOT NULL DEFAULT 1
        migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'IsActive') IS NULL
BEGIN
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] ADD [IsActive] bit NOT NULL CONSTRAINT [DF_SubcategoryFieldDefinitions_IsActive] DEFAULT 1;
END");

        // CreatedAt datetime2 NOT NULL DEFAULT GETUTCDATE()
        migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'CreatedAt') IS NULL
BEGIN
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] ADD [CreatedAt] datetime2(7) NOT NULL CONSTRAINT [DF_SubcategoryFieldDefinitions_CreatedAt] DEFAULT GETUTCDATE();
END");

        // UpdatedAt datetime2 NULL
        migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'UpdatedAt') IS NULL
BEGIN
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] ADD [UpdatedAt] datetime2(7) NULL;
END
");
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        if (ActiveProvider?.Contains("SqlServer") != true)
            return;

        migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'UpdatedAt') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] DROP CONSTRAINT IF EXISTS [DF_SubcategoryFieldDefinitions_UpdatedAt];
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] DROP COLUMN [UpdatedAt];
END");
        migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'CreatedAt') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] DROP CONSTRAINT IF EXISTS [DF_SubcategoryFieldDefinitions_CreatedAt];
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] DROP COLUMN [CreatedAt];
END");
        migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'IsActive') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] DROP CONSTRAINT IF EXISTS [DF_SubcategoryFieldDefinitions_IsActive];
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] DROP COLUMN [IsActive];
END");
        migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.SubcategoryFieldDefinitions', N'SortOrder') IS NOT NULL
BEGIN
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] DROP CONSTRAINT IF EXISTS [DF_SubcategoryFieldDefinitions_SortOrder];
    ALTER TABLE [dbo].[SubcategoryFieldDefinitions] DROP COLUMN [SortOrder];
END");
    }
}
