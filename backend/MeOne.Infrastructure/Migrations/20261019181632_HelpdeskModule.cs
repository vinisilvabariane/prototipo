using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeOne.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class HelpdeskModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "helpdesk_field_templates",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    type = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    label = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    required = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    options = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_helpdesk_field_templates", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "helpdesk_sectors",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    color_hex = table.Column<string>(type: "varchar(7)", maxLength: 7, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    icon = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    badges = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    external_url = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    form_schema = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resolution_schema = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    summary_fields = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    attendant_role_ids = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_helpdesk_sectors", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "helpdesk_tickets",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sector_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    opened_by = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    opened_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    line_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    post_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    post_name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    is_line_stopped = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    stopped_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    defects = table.Column<int>(type: "int", nullable: false),
                    form_snapshot = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    answers = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    assigned_to = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    started_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    resolution_snapshot = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    resolution_answers = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    note = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    finished_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    rejected_by = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    reject_reason = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_helpdesk_tickets", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "code", "description", "global_only" },
                values: new object[] { "helpdesk.manage", "Montar setores, formulários e modelos de campo do help desk", false });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "description", "name" },
                values: new object[] { 13ul, "Gestor do help desk: monta setores, formulários e modelos de campo", "gestor-helpdesk" });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "id", "fk_permission", "fk_role" },
                values: new object[] { 31ul, "helpdesk.manage", 13ul });

            migrationBuilder.CreateIndex(
                name: "IX_helpdesk_field_templates_name",
                table: "helpdesk_field_templates",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_helpdesk_sectors_name",
                table: "helpdesk_sectors",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_helpdesk_tickets_line_id_stopped_at",
                table: "helpdesk_tickets",
                columns: new[] { "line_id", "stopped_at" });

            migrationBuilder.CreateIndex(
                name: "IX_helpdesk_tickets_sector_id_status",
                table: "helpdesk_tickets",
                columns: new[] { "sector_id", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_helpdesk_tickets_status_opened_at",
                table: "helpdesk_tickets",
                columns: new[] { "status", "opened_at" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "helpdesk_field_templates");

            migrationBuilder.DropTable(
                name: "helpdesk_sectors");

            migrationBuilder.DropTable(
                name: "helpdesk_tickets");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumn: "id",
                keyValue: 31ul);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "helpdesk.manage");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 13ul);
        }
    }
}
