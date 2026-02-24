using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

/// <inheritdoc />
[DbContext(typeof(AppDbContext))]
[Migration("20251230000000_AddSubcategoryFieldDefinitions")]
public partial class AddSubcategoryFieldDefinitions : Migration
{
    /// <inheritdoc />
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SubcategoryFieldDefinitions",
            columns: table => new
            {
                Id = table.Column<int>(nullable: false)
                    .Annotation("Sqlite:Autoincrement", true),
                SubcategoryId = table.Column<int>(nullable: false),
                Name = table.Column<string>(maxLength: 200, nullable: false),
                Label = table.Column<string>(maxLength: 200, nullable: false),
                Key = table.Column<string>(maxLength: 100, nullable: false),
                Type = table.Column<string>(nullable: false),
                IsRequired = table.Column<bool>(nullable: false, defaultValue: false),
                DefaultValue = table.Column<string>(maxLength: 500, nullable: true),
                OptionsJson = table.Column<string>(nullable: true),
                Min = table.Column<double>(nullable: true),
                Max = table.Column<double>(nullable: true)
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










