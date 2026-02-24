using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    // Discovery attributes added for SQL Server EF migration discovery.
    /// <summary>Supervisor–technician link table. TechnicianUserId uses NoAction to avoid SQL Server multiple cascade paths to Users.</summary>
    public partial class AddSupervisorTechnicianLinks : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "SupervisorTechnicianLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    SupervisorUserId = table.Column<Guid>(nullable: false),
                    TechnicianUserId = table.Column<Guid>(nullable: false),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SupervisorTechnicianLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SupervisorTechnicianLinks_Users_SupervisorUserId",
                        column: x => x.SupervisorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_SupervisorTechnicianLinks_Users_TechnicianUserId",
                        column: x => x.TechnicianUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.NoAction);
                });

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorTechnicianLinks_SupervisorUserId_TechnicianUserId",
                table: "SupervisorTechnicianLinks",
                columns: new[] { "SupervisorUserId", "TechnicianUserId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_SupervisorTechnicianLinks_TechnicianUserId",
                table: "SupervisorTechnicianLinks",
                column: "TechnicianUserId");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SupervisorTechnicianLinks");
        }
    }
}

