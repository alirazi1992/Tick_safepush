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
    [Migration("20260210000000_AddAcceptedAtToTicketTechnicianAssignment")]
    public partial class AddAcceptedAtToTicketTechnicianAssignment : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "AcceptedAt",
                table: "TicketTechnicianAssignments",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AcceptedAt",
                table: "TicketTechnicianAssignments");
        }
    }
}
