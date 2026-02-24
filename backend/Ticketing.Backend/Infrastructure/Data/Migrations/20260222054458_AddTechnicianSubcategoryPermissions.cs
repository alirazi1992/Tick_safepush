using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    // Discovery attributes added for SQL Server EF migration discovery.
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
                    Id = table.Column<Guid>(nullable: false),
                    TechnicianId = table.Column<Guid>(nullable: false),
                    SubcategoryId = table.Column<int>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
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
