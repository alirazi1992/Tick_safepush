using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateTicketStatusEnum : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Map old enum values to new enum values
            // Old: New=0 -> New: Submitted=0 (no change)
            // Old: InProgress=1 -> New: Viewed=1 (map InProgress to Viewed, then we'll handle separately)
            // Old: WaitingForClient=2 -> New: Open=2 (map WaitingForClient to Open)
            // Old: Resolved=3 -> New: InProgress=3 (map Resolved to InProgress temporarily, then fix)
            // Old: Closed=4 -> New: Resolved=4 (map Closed to Resolved, then we'll set Closed correctly)
            // New: Closed=5 (new value)

            // Map old enum values to new enum values:
            // Old: New=0 -> New: Submitted=0 (no change)
            // Old: InProgress=1 -> New: Viewed=1 (maps to same int, but semantically different - we'll change these to Open)
            // Old: WaitingForClient=2 -> New: Open=2 (maps to same int - perfect match)
            // Old: Resolved=3 -> New: InProgress=3 (maps to same int, but semantically different)
            // Old: Closed=4 -> New: Resolved=4 (map to Resolved)
            // New: Closed=5 (new value)
            
            // Step 1: Map old Closed (4) to new Closed (5) - must do first before we change other values
            migrationBuilder.Sql("UPDATE Tickets SET Status = 5 WHERE Status = 4;");
            
            // Step 2: Map old Resolved (3) to new Resolved (4)
            migrationBuilder.Sql("UPDATE Tickets SET Status = 4 WHERE Status = 3;");
            
            // Step 3: Map old InProgress (1) to new Open (2) - tickets that were being worked on become Open
            migrationBuilder.Sql("UPDATE Tickets SET Status = 2 WHERE Status = 1;");
            
            // Note: Old New=0 maps to new Submitted=0 (no change)
            // Note: Old WaitingForClient=2 maps to new Open=2 (no change)
            
            // Also update TicketMessages table
            migrationBuilder.Sql("UPDATE TicketMessages SET Status = 5 WHERE Status = 4;");
            migrationBuilder.Sql("UPDATE TicketMessages SET Status = 4 WHERE Status = 3;");
            migrationBuilder.Sql("UPDATE TicketMessages SET Status = 2 WHERE Status = 1;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // Reverse the mapping
            // Step 1: New Open (2) -> Old InProgress (1) - reverse the change from Up()
            migrationBuilder.Sql("UPDATE Tickets SET Status = 1 WHERE Status = 2;");
            
            // Step 2: New Resolved (4) -> Old Resolved (3)
            migrationBuilder.Sql("UPDATE Tickets SET Status = 3 WHERE Status = 4;");
            
            // Step 3: New Closed (5) -> Old Closed (4)
            migrationBuilder.Sql("UPDATE Tickets SET Status = 4 WHERE Status = 5;");
            
            // Note: New Submitted (0) -> Old New (0) - no change needed
            // Note: New Viewed (1) would map to Old InProgress (1), but we changed Open(2) to InProgress(1) above
            
            // Also update TicketMessages
            migrationBuilder.Sql("UPDATE TicketMessages SET Status = 1 WHERE Status = 2;");
            migrationBuilder.Sql("UPDATE TicketMessages SET Status = 3 WHERE Status = 4;");
            migrationBuilder.Sql("UPDATE TicketMessages SET Status = 4 WHERE Status = 5;");
        }
    }
}

