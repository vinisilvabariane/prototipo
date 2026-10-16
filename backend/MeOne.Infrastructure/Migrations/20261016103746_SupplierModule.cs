using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace MeOne.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SupplierModule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "suppliers",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    sap_code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    branch = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    multi_branch = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    origin = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    tax_id = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    corporate_name = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    criticality = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    zip_code = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    district = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    street = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    number = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    contact_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    phone = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    email = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    created_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fk_created_by = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    updated_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fk_updated_by = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_suppliers", x => x.id);
                    table.ForeignKey(
                        name: "suppliers_fk_created_by",
                        column: x => x.fk_created_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "suppliers_fk_updated_by",
                        column: x => x.fk_updated_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "supplier_audits",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_supplier = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_responsible = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    audited_on = table.Column<DateOnly>(type: "date", nullable: false),
                    validity_months = table.Column<int>(type: "int", nullable: false),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: false),
                    score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    checklist_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    checklist_revision = table.Column<int>(type: "int", nullable: true),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_audits", x => x.id);
                    table.ForeignKey(
                        name: "supplier_audits_fk_responsible",
                        column: x => x.fk_responsible,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "supplier_audits_fk_supplier",
                        column: x => x.fk_supplier,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "supplier_documents",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_supplier = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    kind = table.Column<string>(type: "varchar(40)", maxLength: 40, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    owner_id = table.Column<ulong>(type: "bigint unsigned", nullable: true),
                    original_name = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    stored_name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    content_type = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    size = table.Column<long>(type: "bigint", nullable: false),
                    version = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: true),
                    uploaded_at = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    fk_uploaded_by = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_documents", x => x.id);
                    table.ForeignKey(
                        name: "supplier_documents_fk_supplier",
                        column: x => x.fk_supplier,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "supplier_documents_fk_uploaded_by",
                        column: x => x.fk_uploaded_by,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "supplier_materials",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_supplier = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    sku = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    business_unit = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    material_type = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    criticality = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    has_sap_specification = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_materials", x => x.id);
                    table.ForeignKey(
                        name: "supplier_materials_fk_supplier",
                        column: x => x.fk_supplier,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "supplier_self_assessments",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_supplier = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_responsible = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    assessed_on = table.Column<DateOnly>(type: "date", nullable: false),
                    expires_on = table.Column<DateOnly>(type: "date", nullable: false),
                    score = table.Column<decimal>(type: "decimal(5,2)", precision: 5, scale: 2, nullable: true),
                    status = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    editable_link = table.Column<string>(type: "varchar(500)", maxLength: 500, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    document_code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    revision = table.Column<int>(type: "int", nullable: true),
                    needs_on_site_audit = table.Column<bool>(type: "tinyint(1)", nullable: true),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_self_assessments", x => x.id);
                    table.ForeignKey(
                        name: "supplier_self_assessments_fk_responsible",
                        column: x => x.fk_responsible,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "supplier_self_assessments_fk_supplier",
                        column: x => x.fk_supplier,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "supplier_tools",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_supplier = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    asset_tag = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: true)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    active = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_supplier_tools", x => x.id);
                    table.ForeignKey(
                        name: "supplier_tools_fk_supplier",
                        column: x => x.fk_supplier,
                        principalTable: "suppliers",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "code", "description", "global_only" },
                values: new object[,]
                {
                    { "suppliers.manage", "Alterar qualquer fornecedor", false },
                    { "suppliers.register", "Cadastrar fornecedores e manter os que cadastrou", false }
                });

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "description", "name" },
                values: new object[,]
                {
                    { 11ul, "Fornecedores: cadastra e mantém os fornecedores que registrou", "fornecedores" },
                    { 12ul, "Gestor de fornecedores: altera qualquer fornecedor", "gestor-fornecedores" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "id", "fk_permission", "fk_role" },
                values: new object[,]
                {
                    { 28ul, "suppliers.register", 11ul },
                    { 29ul, "suppliers.register", 12ul },
                    { 30ul, "suppliers.manage", 12ul }
                });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_audits_fk_responsible",
                table: "supplier_audits",
                column: "fk_responsible");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_audits_fk_supplier",
                table: "supplier_audits",
                column: "fk_supplier");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_documents_fk_supplier",
                table: "supplier_documents",
                column: "fk_supplier");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_documents_fk_supplier_kind_owner_id",
                table: "supplier_documents",
                columns: new[] { "fk_supplier", "kind", "owner_id" });

            migrationBuilder.CreateIndex(
                name: "IX_supplier_documents_fk_uploaded_by",
                table: "supplier_documents",
                column: "fk_uploaded_by");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_materials_fk_supplier",
                table: "supplier_materials",
                column: "fk_supplier");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_self_assessments_fk_responsible",
                table: "supplier_self_assessments",
                column: "fk_responsible");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_self_assessments_fk_supplier",
                table: "supplier_self_assessments",
                column: "fk_supplier");

            migrationBuilder.CreateIndex(
                name: "IX_supplier_tools_fk_supplier",
                table: "supplier_tools",
                column: "fk_supplier");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_fk_created_by",
                table: "suppliers",
                column: "fk_created_by");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_fk_updated_by",
                table: "suppliers",
                column: "fk_updated_by");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_name",
                table: "suppliers",
                column: "name");

            migrationBuilder.CreateIndex(
                name: "IX_suppliers_sap_code_multi_branch",
                table: "suppliers",
                columns: new[] { "sap_code", "multi_branch" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "supplier_audits");

            migrationBuilder.DropTable(
                name: "supplier_documents");

            migrationBuilder.DropTable(
                name: "supplier_materials");

            migrationBuilder.DropTable(
                name: "supplier_self_assessments");

            migrationBuilder.DropTable(
                name: "supplier_tools");

            migrationBuilder.DropTable(
                name: "suppliers");

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumn: "id",
                keyValue: 28ul);

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumn: "id",
                keyValue: 29ul);

            migrationBuilder.DeleteData(
                table: "role_permissions",
                keyColumn: "id",
                keyValue: 30ul);

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "suppliers.manage");

            migrationBuilder.DeleteData(
                table: "permissions",
                keyColumn: "code",
                keyValue: "suppliers.register");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 11ul);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 12ul);
        }
    }
}
