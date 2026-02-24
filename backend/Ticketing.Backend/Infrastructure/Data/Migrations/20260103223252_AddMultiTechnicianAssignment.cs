using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    // Discovery attributes added for SQL Server EF migration discovery. (No Designer.cs; attributes here for discovery.)
    /// <inheritdoc />
    [DbContext(typeof(AppDbContext))]
    [Migration("20260103223252_AddMultiTechnicianAssignment")]
    public partial class AddMultiTechnicianAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketTechnicianAssignments",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TicketId = table.Column<Guid>(nullable: false),
                    TechnicianUserId = table.Column<Guid>(nullable: false),
                    TechnicianId = table.Column<Guid>(nullable: true),
                    AssignedAt = table.Column<DateTime>(nullable: false),
                    AssignedByUserId = table.Column<Guid>(nullable: false),
                    IsActive = table.Column<bool>(nullable: false, defaultValue: true),
                    Role = table.Column<string>(maxLength: 50, nullable: true),
                    UpdatedAt = table.Column<DateTime>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketTechnicianAssignments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketTechnicianAssignments_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketTechnicianAssignments_Users_TechnicianUserId",
                        column: x => x.TechnicianUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketTechnicianAssignments_Users_AssignedByUserId",
                        column: x => x.AssignedByUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketTechnicianAssignments_Technicians_TechnicianId",
                        column: x => x.TechnicianId,
                        principalTable: "Technicians",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateTable(
                name: "TicketActivityEvents",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TicketId = table.Column<Guid>(nullable: false),
                    ActorUserId = table.Column<Guid>(nullable: false),
                    ActorRole = table.Column<string>(maxLength: 50, nullable: false),
                    EventType = table.Column<string>(maxLength: 100, nullable: false),
                    OldStatus = table.Column<string>(maxLength: 50, nullable: true),
                    NewStatus = table.Column<string>(maxLength: 50, nullable: true),
                    MetadataJson = table.Column<string>(maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketActivityEvents", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketActivityEvents_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TicketActivityEvents_Users_ActorUserId",
                        column: x => x.ActorUserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketTechnicianAssignments_TicketId",
                table: "TicketTechnicianAssignments",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTechnicianAssignments_TechnicianUserId",
                table: "TicketTechnicianAssignments",
                column: "TechnicianUserId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketTechnicianAssignments_TicketId_TechnicianUserId_IsActive",
                table: "TicketTechnicianAssignments",
                columns: new[] { "TicketId", "TechnicianUserId", "IsActive" });

            migrationBuilder.CreateIndex(
                name: "IX_TicketActivityEvents_TicketId",
                table: "TicketActivityEvents",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketActivityEvents_CreatedAt",
                table: "TicketActivityEvents",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "IX_TicketActivityEvents_TicketId_CreatedAt",
                table: "TicketActivityEvents",
                columns: new[] { "TicketId", "CreatedAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketActivityEvents");

            migrationBuilder.DropTable(
                name: "TicketTechnicianAssignments");
        }
    }
}


































