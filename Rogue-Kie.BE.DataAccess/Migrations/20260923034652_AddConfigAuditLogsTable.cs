using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Rogue_Kie.BE.DataAccess.Migrations
{
    /// <inheritdoc />
    public partial class AddConfigAuditLogsTable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ConfigAuditLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    MaintenanceId = table.Column<int>(type: "integer", nullable: true),
                    TableName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    RecordId = table.Column<int>(type: "integer", nullable: false),
                    Action = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    FieldName = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: true),
                    OldValue = table.Column<string>(type: "text", nullable: true),
                    NewValue = table.Column<string>(type: "text", nullable: true),
                    ChangedBy = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    ChangedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Reason = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfigAuditLogs", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfigAuditLogs_MaintenanceConfigs_MaintenanceId",
                        column: x => x.MaintenanceId,
                        principalTable: "MaintenanceConfigs",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$WuulYjBUgzWIy7AfazIdSuNqXqliv6.TO9z4Ekshz2kJvgVk7B3cu");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$JF9t6c5ykAXMCaLBC1wp4uhIB9M2ZI/R8WSMiFmFEiYTJnoKM4R3W");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigAuditLogs_ChangedAt",
                table: "ConfigAuditLogs",
                column: "ChangedAt");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigAuditLogs_MaintenanceId",
                table: "ConfigAuditLogs",
                column: "MaintenanceId");

            migrationBuilder.CreateIndex(
                name: "IX_ConfigAuditLogs_TableName_RecordId",
                table: "ConfigAuditLogs",
                columns: new[] { "TableName", "RecordId" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ConfigAuditLogs");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 1,
                column: "Password",
                value: "$2a$11$sDFHaPhWwbPHgFhfmq7LpuilKUMhOyXRhbOk4sgn4Z8rWkhcau3A6");

            migrationBuilder.UpdateData(
                table: "Users",
                keyColumn: "Id",
                keyValue: 2,
                column: "Password",
                value: "$2a$11$z40wAIqb744VANiaNUw3W.KgQQVq7uq7/LpgTkeRZuyDwBagyoplW");
        }
    }
}
