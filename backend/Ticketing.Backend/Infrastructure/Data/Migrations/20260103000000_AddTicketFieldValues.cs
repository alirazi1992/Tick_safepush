using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
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
                    Id = table.Column<string>(type: "TEXT", nullable: false),
                    TicketId = table.Column<string>(type: "TEXT", nullable: false),
                    FieldDefinitionId = table.Column<int>(type: "INTEGER", nullable: false),
                    Value = table.Column<string>(type: "TEXT", maxLength: 2000, nullable: false),
                    CreatedAt = table.Column<string>(type: "TEXT", nullable: false),
                    UpdatedAt = table.Column<string>(type: "TEXT", nullable: true)
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

