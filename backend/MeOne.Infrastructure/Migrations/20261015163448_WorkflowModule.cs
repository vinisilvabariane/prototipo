using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeOne.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WorkflowModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "workflows",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    priority = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    category = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    status = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fk_creator = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    completed_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    due_date = table.Column<DateOnly>(type: "date", nullable: true),
                    version = table.Column<uint>(type: "int unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflows", x => x.id);
                    table.ForeignKey(
                        name: "workflows_fk_creator",
                        column: x => x.fk_creator,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workflow_assignees",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_workflow = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_user = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    assigned_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_assignees", x => x.id);
                    table.ForeignKey(
                        name: "workflow_assignees_fk_user",
                        column: x => x.fk_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "workflow_assignees_fk_workflow",
                        column: x => x.fk_workflow,
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workflow_attachments",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_workflow = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    original_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    stored_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_type = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    uploaded_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_attachments", x => x.id);
                    table.ForeignKey(
                        name: "workflow_attachments_fk_workflow",
                        column: x => x.fk_workflow,
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workflow_steps",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_workflow = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_user = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    action = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(600)", maxLength: 600, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    justification = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    review = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_steps", x => x.id);
                    table.ForeignKey(
                        name: "workflow_steps_fk_user",
                        column: x => x.fk_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "workflow_steps_fk_workflow",
                        column: x => x.fk_workflow,
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "workflow_messages",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_workflow = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_step = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_user = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    text = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_workflow_messages", x => x.id);
                    table.ForeignKey(
                        name: "workflow_messages_fk_step",
                        column: x => x.fk_step,
                        principalTable: "workflow_steps",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "workflow_messages_fk_user",
                        column: x => x.fk_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "workflow_messages_fk_workflow",
                        column: x => x.fk_workflow,
                        principalTable: "workflows",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "code", "description", "global_only" },
                values: new object[] { "workflow.use", "Criar, acompanhar e decidir workflows de aprovação", false });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "description", "name" },
                values: new object[] { 9ul, "Workflow: cria, acompanha e decide fluxos de aprovação", "workflow" });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "id", "fk_permission", "fk_role" },
                values: new object[] { 26ul, "workflow.use", 9ul });

            migrationBuilder.CreateIndex(
                name: "IX_workflow_assignees_fk_user",
                table: "workflow_assignees",
                column: "fk_user");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_assignees_fk_workflow_fk_user",
                table: "workflow_assignees",
                columns: new[] { "fk_workflow", "fk_user" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_attachments_fk_workflow",
                table: "workflow_attachments",
                column: "fk_workflow");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_attachments_stored_name",
                table: "workflow_attachments",
                column: "stored_name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_workflow_messages_fk_step",
                table: "workflow_messages",
                column: "fk_step");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_messages_fk_user",
                table: "workflow_messages",
                column: "fk_user");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_messages_fk_workflow",
                table: "workflow_messages",
                column: "fk_workflow");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_fk_user",
                table: "workflow_steps",
                column: "fk_user");

            migrationBuilder.CreateIndex(
                name: "IX_workflow_steps_fk_workflow_fk_user",
                table: "workflow_steps",
                columns: new[] { "fk_workflow", "fk_user" });

            migrationBuilder.CreateIndex(
                name: "IX_workflows_created_at",
                table: "workflows",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_fk_creator",
                table: "workflows",
                column: "fk_creator");

            migrationBuilder.CreateIndex(
                name: "IX_workflows_status",
                table: "workflows",
                column: "status");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "workflow_assignees");

            migrationBuilder.DropTable(
                name: "workflow_attachments");

            migrationBuilder.DropTable(
                name: "workflow_messages");

            migrationBuilder.DropTable(
                name: "workflow_steps");

            migrationBuilder.DropTable(
                name: "workflows");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumn: "id",
                keyValue: 26ul);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "workflow.use");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 9ul);
        }
    }
}
