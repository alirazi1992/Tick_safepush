using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Ticketing.Backend.Infrastructure.Data.Migrations
{
    /// <summary>
    /// Renames SubcategoryFieldDefinitions.Key to FieldKey for SQL Server reserved keyword compatibility.
    /// SQL Server: conditional DROP INDEX / RENAME / CREATE INDEX (IF EXISTS). SQLite: add column, copy, drop.
    /// </summary>
    [Migration("20260223100000_RenameSubcategoryFieldDefinitionKeyToFieldKey")]
    public partial class RenameSubcategoryFieldDefinitionKeyToFieldKey : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            var isSqlServer = ActiveProvider != null && ActiveProvider.Contains("SqlServer");
            var isSqlite = ActiveProvider != null && ActiveProvider.Contains("Sqlite");

            if (isSqlServer)
            {
                // SQL Server: use raw SQL with IF EXISTS so migration does not fail if index/column already changed
                migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SubcategoryFieldDefinitions_SubcategoryId_Key' AND object_id = OBJECT_ID(N'dbo.SubcategoryFieldDefinitions'))
    DROP INDEX [IX_SubcategoryFieldDefinitions_SubcategoryId_Key] ON [dbo].[SubcategoryFieldDefinitions];
");

                migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = N'dbo' AND TABLE_NAME = N'SubcategoryFieldDefinitions' AND COLUMN_NAME = N'Key')
BEGIN
    EXEC sp_rename N'dbo.SubcategoryFieldDefinitions.[Key]', N'FieldKey', N'COLUMN';
END
");

                migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SubcategoryFieldDefinitions_SubcategoryId_FieldKey' AND object_id = OBJECT_ID(N'dbo.SubcategoryFieldDefinitions'))
BEGIN
    CREATE UNIQUE INDEX [IX_SubcategoryFieldDefinitions_SubcategoryId_FieldKey] ON [dbo].[SubcategoryFieldDefinitions] ([SubcategoryId], [FieldKey]);
END
");
            }
            else if (isSqlite)
            {
                migrationBuilder.DropIndex(
                    name: "IX_SubcategoryFieldDefinitions_SubcategoryId_Key",
                    table: "SubcategoryFieldDefinitions");

                migrationBuilder.AddColumn<string>(
                    name: "FieldKey",
                    table: "SubcategoryFieldDefinitions",
                    type: "TEXT",
                    maxLength: 100,
                    nullable: false,
                    defaultValue: "");

                migrationBuilder.Sql(
                    "UPDATE SubcategoryFieldDefinitions SET FieldKey = \"Key\" WHERE FieldKey = '' OR FieldKey IS NULL");

                migrationBuilder.DropColumn(
                    name: "Key",
                    table: "SubcategoryFieldDefinitions");

                migrationBuilder.CreateIndex(
                    name: "IX_SubcategoryFieldDefinitions_SubcategoryId_FieldKey",
                    table: "SubcategoryFieldDefinitions",
                    columns: new[] { "SubcategoryId", "FieldKey" },
                    unique: true);
            }
            else
            {
                migrationBuilder.DropIndex(
                    name: "IX_SubcategoryFieldDefinitions_SubcategoryId_Key",
                    table: "SubcategoryFieldDefinitions");
                migrationBuilder.RenameColumn(
                    name: "Key",
                    table: "SubcategoryFieldDefinitions",
                    newName: "FieldKey");
                migrationBuilder.CreateIndex(
                    name: "IX_SubcategoryFieldDefinitions_SubcategoryId_FieldKey",
                    table: "SubcategoryFieldDefinitions",
                    columns: new[] { "SubcategoryId", "FieldKey" },
                    unique: true);
            }
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            var isSqlServer = ActiveProvider != null && ActiveProvider.Contains("SqlServer");
            var isSqlite = ActiveProvider != null && ActiveProvider.Contains("Sqlite");

            if (isSqlServer)
            {
                migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SubcategoryFieldDefinitions_SubcategoryId_FieldKey' AND object_id = OBJECT_ID(N'dbo.SubcategoryFieldDefinitions'))
    DROP INDEX [IX_SubcategoryFieldDefinitions_SubcategoryId_FieldKey] ON [dbo].[SubcategoryFieldDefinitions];
");
                migrationBuilder.Sql(@"
IF EXISTS (SELECT 1 FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_SCHEMA = N'dbo' AND TABLE_NAME = N'SubcategoryFieldDefinitions' AND COLUMN_NAME = N'FieldKey')
BEGIN
    EXEC sp_rename N'dbo.SubcategoryFieldDefinitions.[FieldKey]', N'Key', N'COLUMN';
END
");
                migrationBuilder.Sql(@"
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = N'IX_SubcategoryFieldDefinitions_SubcategoryId_Key' AND object_id = OBJECT_ID(N'dbo.SubcategoryFieldDefinitions'))
BEGIN
    CREATE UNIQUE INDEX [IX_SubcategoryFieldDefinitions_SubcategoryId_Key] ON [dbo].[SubcategoryFieldDefinitions] ([SubcategoryId], [Key]);
END
");
            }
            else if (isSqlite)
            {
                migrationBuilder.DropIndex(
                    name: "IX_SubcategoryFieldDefinitions_SubcategoryId_FieldKey",
                    table: "SubcategoryFieldDefinitions");
                migrationBuilder.AddColumn<string>(
                    name: "Key",
                    table: "SubcategoryFieldDefinitions",
                    type: "TEXT",
                    maxLength: 100,
                    nullable: false,
                    defaultValue: "");
                migrationBuilder.Sql(
                    "UPDATE SubcategoryFieldDefinitions SET \"Key\" = FieldKey WHERE \"Key\" = '' OR \"Key\" IS NULL");
                migrationBuilder.DropColumn(
                    name: "FieldKey",
                    table: "SubcategoryFieldDefinitions");
                migrationBuilder.CreateIndex(
                    name: "IX_SubcategoryFieldDefinitions_SubcategoryId_Key",
                    table: "SubcategoryFieldDefinitions",
                    columns: new[] { "SubcategoryId", "Key" },
                    unique: true);
            }
            else
            {
                migrationBuilder.DropIndex(
                    name: "IX_SubcategoryFieldDefinitions_SubcategoryId_FieldKey",
                    table: "SubcategoryFieldDefinitions");
                migrationBuilder.RenameColumn(
                    name: "FieldKey",
                    table: "SubcategoryFieldDefinitions",
                    newName: "Key");
                migrationBuilder.CreateIndex(
                    name: "IX_SubcategoryFieldDefinitions_SubcategoryId_Key",
                    table: "SubcategoryFieldDefinitions",
                    columns: new[] { "SubcategoryId", "Key" },
                    unique: true);
            }
        }
    }
}
