using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    // Discovery attributes added for SQL Server EF migration discovery.
    /// <inheritdoc />
    public partial class AddIsSupervisorToTechnician : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsSupervisor",
                table: "Technicians",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsSupervisor",
                table: "Technicians");
        }
    }
}

