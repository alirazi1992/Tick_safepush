using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTechnicianSubcategoryPermissions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TechnicianSubcategoryPermissions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TechnicianId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubcategoryId = table.Column<int>(type: "INTEGER", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TechnicianSubcategoryPermissions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TechnicianSubcategoryPermissions_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TechnicianSubcategoryPermissions_Subcategories_SubcategoryId",
                        column: x => x.SubcategoryId,
                        principalTable: "Subcategories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianSubcategoryPermissions_TechnicianId",
                table: "TechnicianSubcategoryPermissions",
                column: "TechnicianId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianSubcategoryPermissions_SubcategoryId",
                table: "TechnicianSubcategoryPermissions",
                column: "SubcategoryId");

            migrationBuilder.CreateIndex(
                name: "IX_TechnicianSubcategoryPermissions_TechnicianId_SubcategoryId",
                table: "TechnicianSubcategoryPermissions",
                columns: new[] { "TechnicianId", "SubcategoryId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TechnicianSubcategoryPermissions");
        }
    }
}


































