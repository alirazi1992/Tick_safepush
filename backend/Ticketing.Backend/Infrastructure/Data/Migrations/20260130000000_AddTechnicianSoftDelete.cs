using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Adds soft delete fields to Technician entity:
    /// - IsDeleted: marks if technician is soft-deleted
    /// - DeletedAt: timestamp of deletion
    /// - DeletedByUserId: admin who performed the deletion
    /// 
    /// Also adds lockout fields to User entity:
    /// - LockoutEnabled: whether account can be locked
    /// - LockoutEnd: when lockout ends
    /// - SecurityStamp: for invalidating sessions
    /// </summary>
    public partial class AddTechnicianSoftDelete : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Technician soft delete fields
            migrationBuilder.AddColumn<bool>(
                name: "IsDeleted",
                table: "Technicians",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTime>(
                name: "DeletedAt",
                table: "Technicians",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "DeletedByUserId",
                table: "Technicians",
                type: "TEXT",
                nullable: true);

            // User lockout fields
            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "Users",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "Users",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Users",
                type: "TEXT",
                maxLength: 256,
                nullable: true);

            // Indexes
            migrationBuilder.CreateIndex(
                name: "IX_Technicians_DeletedByUserId",
                table: "Technicians",
                column: "DeletedByUserId");

            migrationBuilder.CreateIndex(
                name: "IX_Technicians_IsDeleted",
                table: "Technicians",
                column: "IsDeleted");

            // Foreign key
            migrationBuilder.AddForeignKey(
                name: "FK_Technicians_Users_DeletedByUserId",
                table: "Technicians",
                column: "DeletedByUserId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
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

            migrationBuilder.DropColumn(
                name: "LockoutEnabled",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LockoutEnd",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "SecurityStamp",
                table: "Users");
        }
    }
}
