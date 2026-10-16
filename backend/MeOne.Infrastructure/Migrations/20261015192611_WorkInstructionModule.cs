using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeOne.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class WorkInstructionModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "work_instructions",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    title = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    document_number = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    product = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    prepared_by = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    approved_by = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    required_ppe = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    process_specifications = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    notes = table.Column<string>(type: "varchar(1000)", maxLength: 1000, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pdf_stored_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    pdf_size = table.Column<long>(type: "bigint", nullable: false),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fk_created_by = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fk_updated_by = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_instructions", x => x.id);
                    table.ForeignKey(
                        name: "work_instructions_fk_created_by",
                        column: x => x.fk_created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "work_instructions_fk_updated_by",
                        column: x => x.fk_updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "work_instruction_steps",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_instruction = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    position = table.Column<int>(type: "int", nullable: false),
                    text = table.Column<string>(type: "varchar(2000)", maxLength: 2000, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_stored_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    image_content_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_work_instruction_steps", x => x.id);
                    table.ForeignKey(
                        name: "work_instruction_steps_fk_instruction",
                        column: x => x.fk_instruction,
                        principalTable: "work_instructions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.UpdateData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "engineering.manage",
                column: "description",
                value: "Manter roteiros, setups, parâmetros de processo e instruções de trabalho");

            migrationBuilder.CreateIndex(
                name: "IX_work_instruction_steps_fk_instruction_position",
                table: "work_instruction_steps",
                columns: new[] { "fk_instruction", "position" });

            migrationBuilder.CreateIndex(
                name: "IX_work_instructions_code",
                table: "work_instructions",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_work_instructions_fk_created_by",
                table: "work_instructions",
                column: "fk_created_by");

            migrationBuilder.CreateIndex(
                name: "IX_work_instructions_fk_updated_by",
                table: "work_instructions",
                column: "fk_updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_work_instructions_title",
                table: "work_instructions",
                column: "title");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "work_instruction_steps");

            migrationBuilder.DropTable(
                name: "work_instructions");

            migrationBuilder.UpdateData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "engineering.manage",
                column: "description",
                value: "Manter roteiros, setups e parâmetros de processo");
        }
    }
}
