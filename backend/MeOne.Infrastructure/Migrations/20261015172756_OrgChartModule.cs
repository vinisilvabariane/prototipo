using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace MeOne.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class OrgChartModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "org_positions",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    title = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fk_department = table.Column<int>(type: "int", nullable: false),
                    fk_parent = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    fk_holder = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_org_positions", x => x.id);
                    table.ForeignKey(
                        name: "org_positions_fk_department",
                        column: x => x.fk_department,
                        principalTable: "departments",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "org_positions_fk_holder",
                        column: x => x.fk_holder,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "org_positions_fk_parent",
                        column: x => x.fk_parent,
                        principalTable: "org_positions",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "code", "description", "global_only" },
                values: new object[] { "orgchart.manage", "Cadastrar e reorganizar os cargos do organograma", true });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "description", "name" },
                values: new object[] { 10ul, "Organograma: mantém a estrutura hierárquica de cargos", "organograma" });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "id", "fk_permission", "fk_role" },
                values: new object[] { 27ul, "orgchart.manage", 10ul });

            migrationBuilder.CreateIndex(
                name: "IX_org_positions_fk_department",
                table: "org_positions",
                column: "fk_department");

            migrationBuilder.CreateIndex(
                name: "IX_org_positions_fk_holder",
                table: "org_positions",
                column: "fk_holder",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_org_positions_fk_parent",
                table: "org_positions",
                column: "fk_parent");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "org_positions");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumn: "id",
                keyValue: 27ul);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "orgchart.manage");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 10ul);
        }
    }
}
