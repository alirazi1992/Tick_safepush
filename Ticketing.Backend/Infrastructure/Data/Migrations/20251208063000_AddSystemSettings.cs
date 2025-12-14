using System;
using Microsoft.EntityFrameworkCore.Migrations;

namespace Ticketing.Backend.Infrastructure.Data.Migrations;

public partial class AddSystemSettings : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "SystemSettings",
            columns: table => new
            {
                Id = table.Column<int>(type: "INTEGER", nullable: false),
                AppName = table.Column<string>(type: "TEXT", nullable: false),
                SupportEmail = table.Column<string>(type: "TEXT", nullable: false),
                SupportPhone = table.Column<string>(type: "TEXT", nullable: false),
                DefaultLanguage = table.Column<string>(type: "TEXT", nullable: false),
                DefaultTheme = table.Column<string>(type: "TEXT", nullable: false),
                Timezone = table.Column<string>(type: "TEXT", nullable: false),
                DefaultPriority = table.Column<int>(type: "INTEGER", nullable: false),
                DefaultStatus = table.Column<int>(type: "INTEGER", nullable: false),
                ResponseSlaHours = table.Column<int>(type: "INTEGER", nullable: false),
                AutoAssignEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                AllowClientAttachments = table.Column<bool>(type: "INTEGER", nullable: false),
                MaxAttachmentSizeMB = table.Column<int>(type: "INTEGER", nullable: false),
                EmailNotificationsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                SmsNotificationsEnabled = table.Column<bool>(type: "INTEGER", nullable: false),
                NotifyOnTicketCreated = table.Column<bool>(type: "INTEGER", nullable: false),
                NotifyOnTicketAssigned = table.Column<bool>(type: "INTEGER", nullable: false),
                NotifyOnTicketReplied = table.Column<bool>(type: "INTEGER", nullable: false),
                NotifyOnTicketClosed = table.Column<bool>(type: "INTEGER", nullable: false),
                PasswordMinLength = table.Column<int>(type: "INTEGER", nullable: false),
                Require2FA = table.Column<bool>(type: "INTEGER", nullable: false),
                SessionTimeoutMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                AllowedEmailDomains = table.Column<string>(type: "TEXT", nullable: false),
                CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false),
                UpdatedAt = table.Column<DateTime>(type: "TEXT", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("PK_SystemSettings", x => x.Id);
            });
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(
            name: "SystemSettings");
    }
}
