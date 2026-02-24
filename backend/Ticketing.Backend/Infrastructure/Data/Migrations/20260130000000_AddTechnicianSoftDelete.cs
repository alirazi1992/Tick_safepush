using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    // Discovery attributes added for SQL Server EF migration discovery. (No Designer.cs; attributes here for discovery.)
    /// <summary>
    /// Adds soft delete fields to Technician entity. FK DeletedByUserId uses NoAction to avoid SQL Server multiple cascade paths to Users. Indexes added so Down() can drop them.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260130000000_AddTechnicianSoftDelete")]
    public partial class AddTechnicianSoftDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Technician soft delete fields
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Technicians",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Technicians",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Technicians",
                nullable: true);

            // Indexes for FK and query filter (required so Down() can drop them).
            migrationBuilder.CreateIndex(
                name: "IX_Technicians_DeletedByUserId",
                table: "Technicians",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_IsDeleted",
                table: "Technicians",
                column: "IsDeleted");

            // Foreign key (NoAction for SQL Server: avoids multiple cascade paths to Users)
            migrationBuilder.AddForeignKey(
                name: "FK_Technicians_Users_DeletedByUserId",
                table: "Technicians",
                column: "DeletedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.NoAction);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Technicians_Users_DeletedByUserId",
                table: "Technicians");

            migrationBuilder.DropIndex(
                name: "IX_Technicians_DeletedByUserId",
                table: "Technicians");

            migrationBuilder.DropIndex(
                name: "IX_Technicians_IsDeleted",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "IsDeleted",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "DeletedAt",
                table: "Technicians");

            migrationBuilder.DropColumn(
                name: "DeletedByUserId",
                table: "Technicians");
        }
    }
}
