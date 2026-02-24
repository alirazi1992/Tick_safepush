using System;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Ticketing.Backend.Infrastructure.Data;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    // Discovery attributes added for SQL Server EF migration discovery.
    /// <inheritdoc />
    public partial class AddTicketFieldValues : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TicketFieldValues",
                columns: table => new
                {
                    Id = table.Column<Guid>(nullable: false),
                    TicketId = table.Column<Guid>(nullable: false),
                    FieldDefinitionId = table.Column<int>(nullable: false),
                    Value = table.Column<string>(maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<string>(nullable: false),
                    UpdatedAt = table.Column<string>(nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TicketFieldValues", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TicketFieldValues_SubcategoryFieldDefinitions_FieldDefinitionId",
                        column: x => x.FieldDefinitionId,
                        principalTable: "SubcategoryFieldDefinitions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_TicketFieldValues_Tickets_TicketId",
                        column: x => x.TicketId,
                        principalTable: "Tickets",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TicketFieldValues_TicketId",
                table: "TicketFieldValues",
                column: "TicketId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketFieldValues_FieldDefinitionId",
                table: "TicketFieldValues",
                column: "FieldDefinitionId");

            migrationBuilder.CreateIndex(
                name: "IX_TicketFieldValues_TicketId_FieldDefinitionId",
                table: "TicketFieldValues",
                columns: new[] { "TicketId", "FieldDefinitionId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TicketFieldValues");
        }
    }
}

