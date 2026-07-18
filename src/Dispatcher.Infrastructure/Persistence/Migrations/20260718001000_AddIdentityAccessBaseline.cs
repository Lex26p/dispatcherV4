using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dispatcher.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DispatcherDbContext))]
[Migration("20260718001000_AddIdentityAccessBaseline")]
public partial class AddIdentityAccessBaseline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: SchemaNames.Identity);

        migrationBuilder.CreateTable(
            name: "permission_scopes",
            schema: SchemaNames.Identity,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                is_archived = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_permission_scopes", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "roles",
            schema: SchemaNames.Identity,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                permissions = table.Column<string>(type: "text", nullable: false),
                is_system = table.Column<bool>(type: "boolean", nullable: false),
                is_archived = table.Column<bool>(type: "boolean", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_roles", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "user_accounts",
            schema: SchemaNames.Identity,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                external_id = table.Column<string>(type: "character varying(160)", maxLength: 160, nullable: false),
                display_name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                email = table.Column<string>(type: "character varying(320)", maxLength: 320, nullable: false),
                is_active = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_user_accounts", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "role_assignments",
            schema: SchemaNames.Identity,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                user_id = table.Column<Guid>(type: "uuid", nullable: false),
                role_id = table.Column<Guid>(type: "uuid", nullable: false),
                scope_id = table.Column<Guid>(type: "uuid", nullable: true),
                source = table.Column<string>(type: "character varying(120)", maxLength: 120, nullable: false),
                reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                revoked_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: true),
                revoked_reason = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_role_assignments", x => x.id);
                table.ForeignKey(name: "fk_role_assignments_permission_scopes_scope_id", column: x => x.scope_id, principalSchema: SchemaNames.Identity, principalTable: "permission_scopes", principalColumn: "id");
                table.ForeignKey(name: "fk_role_assignments_roles_role_id", column: x => x.role_id, principalSchema: SchemaNames.Identity, principalTable: "roles", principalColumn: "id", onDelete: ReferentialAction.Cascade);
                table.ForeignKey(name: "fk_role_assignments_user_accounts_user_id", column: x => x.user_id, principalSchema: SchemaNames.Identity, principalTable: "user_accounts", principalColumn: "id", onDelete: ReferentialAction.Cascade);
            });

        migrationBuilder.CreateIndex(name: "ix_permission_scopes_code", schema: SchemaNames.Identity, table: "permission_scopes", column: "code", unique: true);
        migrationBuilder.CreateIndex(name: "ix_roles_code", schema: SchemaNames.Identity, table: "roles", column: "code", unique: true);
        migrationBuilder.CreateIndex(name: "ix_user_accounts_email", schema: SchemaNames.Identity, table: "user_accounts", column: "email", unique: true);
        migrationBuilder.CreateIndex(name: "ix_user_accounts_external_id", schema: SchemaNames.Identity, table: "user_accounts", column: "external_id", unique: true);
        migrationBuilder.CreateIndex(name: "ix_role_assignments_role_id", schema: SchemaNames.Identity, table: "role_assignments", column: "role_id");
        migrationBuilder.CreateIndex(name: "ix_role_assignments_scope_id", schema: SchemaNames.Identity, table: "role_assignments", column: "scope_id");
        migrationBuilder.CreateIndex(name: "ix_role_assignments_user_id_role_id_scope_id", schema: SchemaNames.Identity, table: "role_assignments", columns: new[] { "user_id", "role_id", "scope_id" });

        migrationBuilder.Sql("""
            INSERT INTO identity_access.permission_scopes (id, code, name, description, is_archived, created_at_utc)
            VALUES ('30000000-0000-0000-0000-000000000001', 'global', 'Global', 'Development global scope for early RBAC baseline.', false, '2026-07-18T00:00:00+00:00');

            INSERT INTO identity_access.roles (id, code, name, description, permissions, is_system, is_archived)
            VALUES
                ('20000000-0000-0000-0000-000000000001', 'administrator', 'Administrator', 'Development administrator with all baseline permissions.', '*', true, false),
                ('20000000-0000-0000-0000-000000000002', 'engineer', 'Engineer', 'Baseline engineer role for read-oriented setup tasks.', 'identity.me.view;identity.users.view;identity.roles.view;identity.scopes.view;identity.assignments.view', true, false),
                ('20000000-0000-0000-0000-000000000003', 'operator', 'Operator', 'Baseline operator role with self profile access only.', 'identity.me.view', true, false);

            INSERT INTO identity_access.user_accounts (id, external_id, display_name, email, is_active, created_at_utc)
            VALUES ('10000000-0000-0000-0000-000000000001', 'dev-admin', 'Development Administrator', 'dev-admin@dispatcher.local', true, '2026-07-18T00:00:00+00:00');

            INSERT INTO identity_access.role_assignments (id, user_id, role_id, scope_id, source, reason, created_at_utc, revoked_at_utc, revoked_reason)
            VALUES ('40000000-0000-0000-0000-000000000001', '10000000-0000-0000-0000-000000000001', '20000000-0000-0000-0000-000000000001', '30000000-0000-0000-0000-000000000001', 'seed', 'Initial development administrator assignment.', '2026-07-18T00:00:00+00:00', null, null);
            """);
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "role_assignments", schema: SchemaNames.Identity);
        migrationBuilder.DropTable(name: "permission_scopes", schema: SchemaNames.Identity);
        migrationBuilder.DropTable(name: "roles", schema: SchemaNames.Identity);
        migrationBuilder.DropTable(name: "user_accounts", schema: SchemaNames.Identity);
    }
}
