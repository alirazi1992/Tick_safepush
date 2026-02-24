using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

// Discovery attributes added for SQL Server EF migration discovery.
/// <summary>
/// Adds CategoryFieldDefinitions table. SQLite-only raw SQL adds SubcategoryFieldDefinitions columns; SQL Server uses prior migrations. Down() drops SubcategoryFieldDefinitions columns only on Sqlite.
/// </summary>
public partial class AddCategoryFieldDefinitions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // SQLite-only: ensure SubcategoryFieldDefinitions exists and has SortOrder, IsActive, CreatedAt, UpdatedAt.
        // On SQL Server, SubcategoryFieldDefinitions was created by AddSubcategoryFieldDefinitions with correct schema; skip raw SQL.
        if ((ActiveProvider ?? "").Contains("Sqlite"))
        {
            try
            {
                migrationBuilder.Sql(@"
            CREATE TABLE IF NOT EXISTS SubcategoryFieldDefinitions (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SubcategoryId INTEGER NOT NULL,
                Name TEXT NOT NULL,
                Label TEXT NOT NULL,
                Key TEXT NOT NULL,
                Type TEXT NOT NULL,
                IsRequired INTEGER NOT NULL DEFAULT 0,
                DefaultValue TEXT,
                OptionsJson TEXT,
                Min REAL,
                Max REAL,
                SortOrder INTEGER NOT NULL DEFAULT 0,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT,
                FOREIGN KEY (SubcategoryId) REFERENCES Subcategories(Id) ON DELETE CASCADE
            );
        ");

                migrationBuilder.Sql(@"
            PRAGMA foreign_keys = OFF;
            CREATE TABLE SubcategoryFieldDefinitions_new (
                Id INTEGER PRIMARY KEY AUTOINCREMENT,
                SubcategoryId INTEGER NOT NULL,
                Name TEXT NOT NULL,
                Label TEXT NOT NULL,
                Key TEXT NOT NULL,
                Type TEXT NOT NULL,
                IsRequired INTEGER NOT NULL DEFAULT 0,
                DefaultValue TEXT,
                OptionsJson TEXT,
                Min REAL,
                Max REAL,
                SortOrder INTEGER NOT NULL DEFAULT 0,
                IsActive INTEGER NOT NULL DEFAULT 1,
                CreatedAt TEXT NOT NULL DEFAULT CURRENT_TIMESTAMP,
                UpdatedAt TEXT,
                FOREIGN KEY (SubcategoryId) REFERENCES Subcategories(Id) ON DELETE CASCADE
            );
            INSERT INTO SubcategoryFieldDefinitions_new (Id, SubcategoryId, Name, Label, Key, Type, IsRequired, DefaultValue, OptionsJson, Min, Max, SortOrder, IsActive, CreatedAt, UpdatedAt)
            SELECT Id, SubcategoryId, COALESCE(Name, Key), Label, Key, Type, IsRequired, DefaultValue, OptionsJson, Min, Max, 0, 1, datetime('now'), NULL FROM SubcategoryFieldDefinitions;
            DROP TABLE SubcategoryFieldDefinitions;
            ALTER TABLE SubcategoryFieldDefinitions_new RENAME TO SubcategoryFieldDefinitions;
            PRAGMA foreign_keys = ON;
        ");

                migrationBuilder.Sql(@"
            CREATE INDEX IF NOT EXISTS IX_SubcategoryFieldDefinitions_SubcategoryId ON SubcategoryFieldDefinitions(SubcategoryId);
            CREATE UNIQUE INDEX IF NOT EXISTS IX_SubcategoryFieldDefinitions_SubcategoryId_Key ON SubcategoryFieldDefinitions(SubcategoryId, Key);
        ");
            }
            catch
            {
                // Already applied or table already has schema; ignore.
            }
        }

        // SQL Server: add Categories.NormalizedName for seed (SQLite gets it from schema guard after migrations). Idempotent: COL_LENGTH and index-existence checks.
        // Split into separate batches so SQL Server does not compile UPDATE in same batch as ADD (avoids "Invalid column name" at parse time).
        if ((ActiveProvider ?? "").Contains("SqlServer"))
        {
            migrationBuilder.Sql(@"
IF COL_LENGTH(N'dbo.Categories', N'NormalizedName') IS NULL
BEGIN
    ALTER TABLE [Categories] ADD [NormalizedName] nvarchar(200) NULL;
END");
            migrationBuilder.Sql(@"
UPDATE [Categories] SET [NormalizedName] = LOWER([Name]) WHERE [NormalizedName] IS NULL");
            migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_Categories_NormalizedName' AND object_id = OBJECT_ID(N'dbo.Categories'))
BEGIN
    CREATE UNIQUE INDEX [IX_Categories_NormalizedName] ON [Categories]([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;
END");
        }

        migrationBuilder.CreateTable(
            name: "CategoryFieldDefinitions",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CategoryId = table.Column<int>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Label = table.Column<string>(maxLength: 200, nullable: false),
                Key = table.Column<string>(maxLength: 100, nullable: false),
                Type = table.Column<string>(nullable: false),
                IsRequired = table.Column<bool>(nullable: false, defaultValue: false),
                DefaultValue = table.Column<string>(maxLength: 500, nullable: true),
                OptionsJson = table.Column<string>(nullable: true),
                Min = table.Column<double>(nullable: true),
                Max = table.Column<double>(nullable: true),
                SortOrder = table.Column<int>(nullable: false, defaultValue: 0),
                IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(nullable: false),
                UpdatedAt = table.Column<DateTime>(nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_CategoryFieldDefinitions", x => x.Id);
                table.ForeignKey(
                    name: "FK_CategoryFieldDefinitions_Categories_CategoryId",
                    column: x => x.CategoryId,
                    principalTable: "Categories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_CategoryFieldDefinitions_CategoryId",
            table: "CategoryFieldDefinitions",
            column: "CategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_CategoryFieldDefinitions_CategoryId_Key",
            table: "CategoryFieldDefinitions",
            columns: new[] { "CategoryId", "Key" },
            unique: true);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "CategoryFieldDefinitions");

        // SubcategoryFieldDefinitions columns were added only by SQLite raw SQL in Up(); on SQL Server skip dropping them.
        if ((ActiveProvider ?? "").Contains("Sqlite"))
        {
            migrationBuilder.DropColumn(
                name: "SortOrder",
                table: "SubcategoryFieldDefinitions");

            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "SubcategoryFieldDefinitions");

            migrationBuilder.DropColumn(
                name: "CreatedAt",
                table: "SubcategoryFieldDefinitions");

            migrationBuilder.DropColumn(
                name: "UpdatedAt",
                table: "SubcategoryFieldDefinitions");
        }
    }
}















