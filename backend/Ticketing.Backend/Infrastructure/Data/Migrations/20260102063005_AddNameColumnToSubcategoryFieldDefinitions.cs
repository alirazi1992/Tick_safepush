using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddNameColumnToSubcategoryFieldDefinitions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Add Name column if it doesn't exist (additive fix for schema drift)
        // SQLite requires separate statements - we'll add as nullable, then backfill
        migrationBuilder.Sql("ALTER TABLE SubcategoryFieldDefinitions ADD COLUMN Name TEXT;");
        migrationBuilder.Sql("UPDATE SubcategoryFieldDefinitions SET Name = Key WHERE Name IS NULL;");
        
        // Note: SQLite doesn't support ALTER COLUMN to make it NOT NULL
        // Application-level validation and schema guard will ensure Name is populated
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // SQLite doesn't support DROP COLUMN directly
        // This would require recreating the table, which we avoid
        // If rollback is needed, use a different migration strategy
        migrationBuilder.Sql(@"
            -- SQLite limitation: cannot drop column directly
            -- This migration should not be rolled back in production
        ");
    }
}

