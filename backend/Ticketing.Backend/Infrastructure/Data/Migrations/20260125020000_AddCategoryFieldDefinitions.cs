using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

public partial class AddCategoryFieldDefinitions : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.AddColumn<int>(
            name: "SortOrder",
            table: "SubcategoryFieldDefinitions",
            type: "INTEGER",
            nullable: false,
            defaultValue: 0);

        migrationBuilder.AddColumn<bool>(
            name: "IsActive",
            table: "SubcategoryFieldDefinitions",
            type: "INTEGER",
            nullable: false,
            defaultValue: true);

        migrationBuilder.AddColumn<DateTime>(
            name: "CreatedAt",
            table: "SubcategoryFieldDefinitions",
            type: "TEXT",
            nullable: false,
            defaultValueSql: "CURRENT_TIMESTAMP");

        migrationBuilder.AddColumn<DateTime>(
            name: "UpdatedAt",
            table: "SubcategoryFieldDefinitions",
            type: "TEXT",
            nullable: true);

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















