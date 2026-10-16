using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814

namespace MeOne.Infrastructure.Migrations
{
    public partial class PermissionsAndScopes : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropIndex(
                name: "IX_user_roles_fk_user_fk_role",
                table: "user_roles");

            migrationBuilder.AddColumn<int>(
                name: "failed_login_count",
                table: "users",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<DateTime>(
                name: "locked_until",
                table: "users",
                type: "datetime(6)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "security_stamp",
                table: "users",
                type: "varchar(64)",
                maxLength: 64,
                nullable: false,
                defaultValue: "")
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.Sql("UPDATE `users` SET `security_stamp` = REPLACE(UUID(), '-', '') WHERE `security_stamp` = '';");

            migrationBuilder.AddColumn<ulong>(
                name: "fk_scope",
                table: "user_roles",
                type: "bigint unsigned",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "password_reset_tokens",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_user = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    code_hash = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    expires = table.Column<DateTime>(type: "datetime(6)", nullable: false),
                    used_at = table.Column<DateTime>(type: "datetime(6)", nullable: true),
                    failed_attempts = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_password_reset_tokens", x => x.id);
                    table.ForeignKey(
                        name: "password_reset_tokens_fk_user",
                        column: x => x.fk_user,
                        principalTable: "users",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "permissions",
                columns: table => new
                {
                    code = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    description = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    global_only = table.Column<bool>(type: "tinyint(1)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_permissions", x => x.code);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "scopes",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    type = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    code = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    name = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4"),
                    fk_parent = table.Column<ulong>(type: "bigint unsigned", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_scopes", x => x.id);
                    table.ForeignKey(
                        name: "scopes_fk_parent",
                        column: x => x.fk_parent,
                        principalTable: "scopes",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Restrict);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.CreateTable(
                name: "role_permissions",
                columns: table => new
                {
                    id = table.Column<ulong>(type: "bigint unsigned", nullable: false)
                        .Annotation("MySql:ValueGenerationStrategy", MySqlValueGenerationStrategy.IdentityColumn),
                    fk_role = table.Column<ulong>(type: "bigint unsigned", nullable: false),
                    fk_permission = table.Column<string>(type: "varchar(50)", maxLength: 50, nullable: false)
                        .Annotation("MySql:CharSet", "utf8mb4")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_role_permissions", x => x.id);
                    table.ForeignKey(
                        name: "role_permissions_fk_permission",
                        column: x => x.fk_permission,
                        principalTable: "permissions",
                        principalColumn: "code",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "role_permissions_fk_role",
                        column: x => x.fk_role,
                        principalTable: "roles",
                        principalColumn: "id",
                        onDelete: ReferentialAction.Cascade);
                })
                .Annotation("MySql:CharSet", "utf8mb4");

            migrationBuilder.InsertData(
                table: "permissions",
                columns: new[] { "code", "description", "global_only" },
                values: new object[,]
                {
                    { "departments.manage", "Cadastrar e editar departamentos", true },
                    { "engineering.manage", "Manter roteiros, setups e parâmetros de processo", false },
                    { "maintenance.execute", "Atender e encerrar chamados de manutenção", false },
                    { "maintenance.request", "Abrir chamados de manutenção", false },
                    { "planning.manage", "Planejar e sequenciar ordens de produção", false },
                    { "production.execute", "Apontar produção", false },
                    { "production.read", "Consultar produção", false },
                    { "production.supervise", "Supervisionar produção (liberar, corrigir e encerrar apontamentos)", false },
                    { "quality.inspect", "Registrar inspeções de qualidade", false },
                    { "quality.release", "Liberar e bloquear material por qualidade", false },
                    { "roles.assign", "Atribuir e revogar papéis de usuários", true },
                    { "roles.manage", "Criar e editar papéis e suas permissões", true },
                    { "roles.read", "Consultar papéis e permissões", true },
                    { "scopes.manage", "Cadastrar e editar plantas, áreas, linhas e postos", true },
                    { "system.health", "Consultar a saúde detalhada do sistema", true },
                    { "users.manage", "Cadastrar, editar e desativar usuários", true },
                    { "users.read", "Consultar usuários", true },
                    { "users.reset-password", "Redefinir a senha de usuários", true }
                });

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 1ul,
                column: "description",
                value: "Administrador: acesso total ao sistema");

            migrationBuilder.InsertData(
                table: "roles",
                columns: new[] { "id", "description", "name" },
                values: new object[,]
                {
                    { 2ul, "Gestor de acessos: cadastra usuários e atribui papéis", "gestor-acessos" },
                    { 3ul, "Supervisor de produção", "supervisor-producao" },
                    { 4ul, "Operador de produção", "operador" },
                    { 5ul, "Qualidade", "qualidade" },
                    { 6ul, "Manutenção", "manutencao" },
                    { 7ul, "Engenharia de processos", "engenharia-processos" },
                    { 8ul, "Planejamento e controle da produção", "pcp" }
                });

            migrationBuilder.InsertData(
                table: "role_permissions",
                columns: new[] { "id", "fk_permission", "fk_role" },
                values: new object[,]
                {
                    { 1ul, "users.read", 2ul },
                    { 2ul, "users.manage", 2ul },
                    { 3ul, "users.reset-password", 2ul },
                    { 4ul, "roles.read", 2ul },
                    { 5ul, "roles.assign", 2ul },
                    { 6ul, "departments.manage", 2ul },
                    { 7ul, "production.read", 3ul },
                    { 8ul, "production.execute", 3ul },
                    { 9ul, "production.supervise", 3ul },
                    { 10ul, "quality.inspect", 3ul },
                    { 11ul, "maintenance.request", 3ul },
                    { 12ul, "production.read", 4ul },
                    { 13ul, "production.execute", 4ul },
                    { 14ul, "maintenance.request", 4ul },
                    { 15ul, "production.read", 5ul },
                    { 16ul, "quality.inspect", 5ul },
                    { 17ul, "quality.release", 5ul },
                    { 18ul, "production.read", 6ul },
                    { 19ul, "maintenance.request", 6ul },
                    { 20ul, "maintenance.execute", 6ul },
                    { 21ul, "production.read", 7ul },
                    { 22ul, "engineering.manage", 7ul },
                    { 23ul, "quality.inspect", 7ul },
                    { 24ul, "production.read", 8ul },
                    { 25ul, "planning.manage", 8ul }
                });

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_fk_scope",
                table: "user_roles",
                column: "fk_scope");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_fk_user_fk_role_fk_scope",
                table: "user_roles",
                columns: new[] { "fk_user", "fk_role", "fk_scope" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_roles_name",
                table: "roles",
                column: "name",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_password_reset_tokens_fk_user",
                table: "password_reset_tokens",
                column: "fk_user");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_fk_permission",
                table: "role_permissions",
                column: "fk_permission");

            migrationBuilder.CreateIndex(
                name: "IX_role_permissions_fk_role_fk_permission",
                table: "role_permissions",
                columns: new[] { "fk_role", "fk_permission" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scopes_code",
                table: "scopes",
                column: "code",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_scopes_fk_parent",
                table: "scopes",
                column: "fk_parent");

            migrationBuilder.AddForeignKey(
                name: "user_roles_fk_scope",
                table: "user_roles",
                column: "fk_scope",
                principalTable: "scopes",
                principalColumn: "id",
                onDelete: ReferentialAction.Restrict);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "user_roles_fk_scope",
                table: "user_roles");

            migrationBuilder.DropTable(
                name: "password_reset_tokens");

            migrationBuilder.DropTable(
                name: "role_permissions");

            migrationBuilder.DropTable(
                name: "scopes");

            migrationBuilder.DropTable(
                name: "permissions");

            migrationBuilder.DropIndex(
                name: "IX_user_roles_fk_scope",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "IX_user_roles_fk_user_fk_role_fk_scope",
                table: "user_roles");

            migrationBuilder.DropIndex(
                name: "IX_roles_name",
                table: "roles");

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 2ul);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 3ul);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 4ul);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 5ul);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 6ul);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 7ul);

            migrationBuilder.DeleteData(
                table: "roles",
                keyColumn: "id",
                keyValue: 8ul);

            migrationBuilder.DropColumn(
                name: "failed_login_count",
                table: "users");

            migrationBuilder.DropColumn(
                name: "locked_until",
                table: "users");

            migrationBuilder.DropColumn(
                name: "security_stamp",
                table: "users");

            migrationBuilder.DropColumn(
                name: "fk_scope",
                table: "user_roles");

            migrationBuilder.UpdateData(
                table: "roles",
                keyColumn: "id",
                keyValue: 1ul,
                column: "description",
                value: "Administração");

            migrationBuilder.CreateIndex(
                name: "IX_user_roles_fk_user_fk_role",
                table: "user_roles",
                columns: new[] { "fk_user", "fk_role" },
                unique: true);
        }
    }
}
