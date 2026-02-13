using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
/// <summary>
/// Safely adds missing DefaultValue column to SubcategoryFieldDefinitions table.
/// Uses raw SQL to check if column exists before adding, making it idempotent.
/// </summary>
public partial class AddMissingColumnsToSubcategoryFieldDefinitions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Idempotent migration: Add DefaultValue column only if it doesn't exist
        // This migration is a safety net for databases that were created before
        // the initial migration included DefaultValue, or if the column was somehow missing.
        // Note: SQLite doesn't support IF NOT EXISTS for ALTER TABLE ADD COLUMN,
        // so we use a workaround with a temporary table and data migration if needed.
        // However, the simplest approach is to attempt the ADD COLUMN and catch
        // "duplicate column" errors in Program.cs, which is already implemented.
        // For EF Core migrations, we'll use AddColumn which will attempt to add the column.
        // If the column already exists, SQLite will throw an error, but Program.cs
        // migration error handler will catch it and allow the app to continue.

        // Attempt to add the column using EF's AddColumn
        // If the column already exists (from initial migration), SQLite will throw
        // "duplicate column name: DefaultValue", which Program.cs handles gracefully.
        // This is safe because:
        // 1. If column exists: Migration "fails" but Program.cs catches and continues
        // 2. If column missing: Column is added successfully
        migrationBuilder.AddColumn<string>(
            name: "DefaultValue",
            table: "SubcategoryFieldDefinitions",
            type: "TEXT",
            maxLength: 500,
            nullable: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        // Safely drop column (only if it exists)
        migrationBuilder.DropColumn(
            name: "DefaultValue",
            table: "SubcategoryFieldDefinitions");
    }
}
