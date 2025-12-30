using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
public partial class AddSubcategoryFieldDefinitions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SubcategoryFieldDefinitions",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                SubcategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Label = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                Key = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                Type = table.Column<string>(type: "TEXT", nullable: false),
                IsRequired = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: false),
                DefaultValue = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true),
                OptionsJson = table.Column<string>(type: "TEXT", nullable: true),
                Min = table.Column<double>(type: "REAL", nullable: true),
                Max = table.Column<double>(type: "REAL", nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SubcategoryFieldDefinitions", x => x.Id);
                table.ForeignKey(
                    name: "FK_SubcategoryFieldDefinitions_Subcategories_SubcategoryId",
                    column: x => x.SubcategoryId,
                    principalTable: "Subcategories",
                    principalColumn: "Id",
                    onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(
            name: "IX_SubcategoryFieldDefinitions_SubcategoryId",
            table: "SubcategoryFieldDefinitions",
            column: "SubcategoryId");

        migrationBuilder.CreateIndex(
            name: "IX_SubcategoryFieldDefinitions_SubcategoryId_Key",
            table: "SubcategoryFieldDefinitions",
            columns: new[] { "SubcategoryId", "Key" },
            unique: true);
    }

    /// <inheritdoc />
    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SubcategoryFieldDefinitions");
    }
}





