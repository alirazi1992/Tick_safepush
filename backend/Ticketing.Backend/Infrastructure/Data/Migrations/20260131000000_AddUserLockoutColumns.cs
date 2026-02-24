using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    /// <summary>
    /// Adds LockoutEnabled, LockoutEnd, SecurityStamp to Users for seed/lockout support.
    /// Smoke test: reset-tikq-db.ps1 -Force; set Database__Provider=SqlServer and ConnectionStrings__DefaultConnection; dotnet run.
    /// </summary>
    [DbContext(typeof(AppDbContext))]
    [Migration("20260131000000_AddUserLockoutColumns")]
    public partial class AddUserLockoutColumns : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Add LockoutEnabled column with default value false
            migrationBuilder.AddColumn<bool>(
                name: "LockoutEnabled",
                table: "Users",
                nullable: false,
                defaultValue: false);

            // Add LockoutEnd column (nullable)
            migrationBuilder.AddColumn<DateTimeOffset>(
                name: "LockoutEnd",
                table: "Users",
                nullable: true);

            // Add SecurityStamp column (nullable, max 256 to match entity)
            migrationBuilder.AddColumn<string>(
                name: "SecurityStamp",
                table: "Users",
                maxLength: 256,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
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
