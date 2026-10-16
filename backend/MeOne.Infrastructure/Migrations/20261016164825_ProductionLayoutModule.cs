using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeOne.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class ProductionLayoutModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "production_lines",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_lines", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "production_products",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(120)", maxLength: 120, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_products", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "production_shifts",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    name = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    start_time = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    end_time = table.Column<TimeOnly>(type: "time(6)", nullable: false),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_shifts", x => x.id);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "line_layouts",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_line = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_product = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_shift = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fk_updated_by = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_line_layouts", x => x.id);
                    table.ForeignKey(
                        name: "line_layouts_fk_line",
                        column: x => x.fk_line,
                        principalTable: "production_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "line_layouts_fk_product",
                        column: x => x.fk_product,
                        principalTable: "production_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "line_layouts_fk_shift",
                        column: x => x.fk_shift,
                        principalTable: "production_shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "line_layouts_fk_updated_by",
                        column: x => x.fk_updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "production_runs",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    layout_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_line = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_product = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_shift = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    layout_snapshot = table.Column<string>(type: "longtext", nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fk_created_by = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    started_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    fk_started_by = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    ended_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    fk_ended_by = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_production_runs", x => x.id);
                    table.ForeignKey(
                        name: "production_runs_fk_created_by",
                        column: x => x.fk_created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "production_runs_fk_ended_by",
                        column: x => x.fk_ended_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "production_runs_fk_line",
                        column: x => x.fk_line,
                        principalTable: "production_lines",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "production_runs_fk_product",
                        column: x => x.fk_product,
                        principalTable: "production_products",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "production_runs_fk_shift",
                        column: x => x.fk_shift,
                        principalTable: "production_shifts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "production_runs_fk_started_by",
                        column: x => x.fk_started_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "layout_decorations",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_layout = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    kind = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    x = table.Column<int>(type: "int", nullable: false),
                    y = table.Column<int>(type: "int", nullable: false),
                    width = table.Column<int>(type: "int", nullable: false),
                    height = table.Column<int>(type: "int", nullable: false),
                    rotation = table.Column<int>(type: "int", nullable: false),
                    label = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    locked = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_layout_decorations", x => x.id);
                    table.ForeignKey(
                        name: "layout_decorations_fk_layout",
                        column: x => x.fk_layout,
                        principalTable: "line_layouts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "layout_posts",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_layout = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    name = table.Column<string>(type: "varchar(60)", maxLength: 60, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    kind = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    x = table.Column<int>(type: "int", nullable: false),
                    y = table.Column<int>(type: "int", nullable: false),
                    width = table.Column<int>(type: "int", nullable: false),
                    height = table.Column<int>(type: "int", nullable: false),
                    rotation = table.Column<int>(type: "int", nullable: false),
                    is_critical = table.Column<bool>(type: "tinyint(1)", nullable: false),
                    target_per_hour = table.Column<decimal>(type: "decimal(10,2)", precision: 10, scale: 2, nullable: true),
                    operation = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_layout_posts", x => x.id);
                    table.ForeignKey(
                        name: "layout_posts_fk_layout",
                        column: x => x.fk_layout,
                        principalTable: "line_layouts",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "run_allocations",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_run = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    post_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_user = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    allocated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    released_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    fk_allocated_by = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_run_allocations", x => x.id);
                    table.ForeignKey(
                        name: "run_allocations_fk_allocated_by",
                        column: x => x.fk_allocated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "run_allocations_fk_run",
                        column: x => x.fk_run,
                        principalTable: "production_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "run_allocations_fk_user",
                        column: x => x.fk_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "run_outputs",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_run = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    post_id = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    hour_start = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    quantity = table.Column<int>(type: "int", nullable: false),
                    fk_user = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    fk_recorded_by = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    recorded_at = table.Column<DateTime>(type: "datetime(6)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_run_outputs", x => x.id);
                    table.ForeignKey(
                        name: "run_outputs_fk_recorded_by",
                        column: x => x.fk_recorded_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "run_outputs_fk_run",
                        column: x => x.fk_run,
                        principalTable: "production_runs",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "run_outputs_fk_user",
                        column: x => x.fk_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "engineering.manage",
                column: "description",
                value: "Manter roteiros, setups, parâmetros de processo, instruções de trabalho, layouts de linha e cadastros de produção");

            migrationBuilder.CreateIndex(
                name: "IX_layout_decorations_fk_layout",
                table: "layout_decorations",
                column: "fk_layout");

            migrationBuilder.CreateIndex(
                name: "IX_layout_posts_fk_layout",
                table: "layout_posts",
                column: "fk_layout");

            migrationBuilder.CreateIndex(
                name: "IX_line_layouts_fk_line_fk_product_fk_shift",
                table: "line_layouts",
                columns: new[] { "fk_line", "fk_product", "fk_shift" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_line_layouts_fk_product",
                table: "line_layouts",
                column: "fk_product");

            migrationBuilder.CreateIndex(
                name: "IX_line_layouts_fk_shift",
                table: "line_layouts",
                column: "fk_shift");

            migrationBuilder.CreateIndex(
                name: "IX_line_layouts_fk_updated_by",
                table: "line_layouts",
                column: "fk_updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_production_lines_name",
                table: "production_lines",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_products_code",
                table: "production_products",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_production_runs_created_at",
                table: "production_runs",
                column: "created_at");

            migrationBuilder.CreateIndex(
                name: "IX_production_runs_fk_created_by",
                table: "production_runs",
                column: "fk_created_by");

            migrationBuilder.CreateIndex(
                name: "IX_production_runs_fk_ended_by",
                table: "production_runs",
                column: "fk_ended_by");

            migrationBuilder.CreateIndex(
                name: "IX_production_runs_fk_line_status",
                table: "production_runs",
                columns: new[] { "fk_line", "status" });

            migrationBuilder.CreateIndex(
                name: "IX_production_runs_fk_product",
                table: "production_runs",
                column: "fk_product");

            migrationBuilder.CreateIndex(
                name: "IX_production_runs_fk_shift",
                table: "production_runs",
                column: "fk_shift");

            migrationBuilder.CreateIndex(
                name: "IX_production_runs_fk_started_by",
                table: "production_runs",
                column: "fk_started_by");

            migrationBuilder.CreateIndex(
                name: "IX_production_runs_layout_id",
                table: "production_runs",
                column: "layout_id");

            migrationBuilder.CreateIndex(
                name: "IX_production_shifts_name",
                table: "production_shifts",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_run_allocations_fk_allocated_by",
                table: "run_allocations",
                column: "fk_allocated_by");

            migrationBuilder.CreateIndex(
                name: "IX_run_allocations_fk_run_post_id",
                table: "run_allocations",
                columns: new[] { "fk_run", "post_id" });

            migrationBuilder.CreateIndex(
                name: "IX_run_allocations_fk_user_released_at",
                table: "run_allocations",
                columns: new[] { "fk_user", "released_at" });

            migrationBuilder.CreateIndex(
                name: "IX_run_outputs_fk_recorded_by",
                table: "run_outputs",
                column: "fk_recorded_by");

            migrationBuilder.CreateIndex(
                name: "IX_run_outputs_fk_run_post_id_hour_start",
                table: "run_outputs",
                columns: new[] { "fk_run", "post_id", "hour_start" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_run_outputs_fk_user",
                table: "run_outputs",
                column: "fk_user");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "layout_decorations");

            migrationBuilder.DropTable(
                name: "layout_posts");

            migrationBuilder.DropTable(
                name: "run_allocations");

            migrationBuilder.DropTable(
                name: "run_outputs");

            migrationBuilder.DropTable(
                name: "line_layouts");

            migrationBuilder.DropTable(
                name: "production_runs");

            migrationBuilder.DropTable(
                name: "production_lines");

            migrationBuilder.DropTable(
                name: "production_products");

            migrationBuilder.DropTable(
                name: "production_shifts");

            migrationBuilder.UpdateData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "engineering.manage",
                column: "description",
                value: "Manter roteiros, setups, parâmetros de processo e instruções de trabalho");
        }
    }
}
