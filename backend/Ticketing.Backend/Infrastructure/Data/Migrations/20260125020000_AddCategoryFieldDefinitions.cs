using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

public partial class AddCategoryFieldDefinitions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Ensure SubcategoryFieldDefinitions exists and has SortOrder, IsActive, CreatedAt, UpdatedAt.
        // Handles: (1) table missing (e.g. 20251230000000 not applied); (2) table exists with old schema.
        // SQLite has no IF EXISTS for ALTER TABLE ADD COLUMN, so we use create-if-not-exists + table-copy.
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

        migrationBuilder.CreateTable(
            name: "CategoryFieldDefinitions",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                CategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Label = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Type = table.Column<string>(type: "TEXT", nullable: false),
                IsRequired = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                DefaultValue = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                OptionsJson = table.Column<string>(type: "TEXT", nullable: true),
                Min = table.Column<double>(type: "REAL", nullable: true),
                Max = table.Column<double>(type: "REAL", nullable: true),
                SortOrder = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 0),
                IsActive = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
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















