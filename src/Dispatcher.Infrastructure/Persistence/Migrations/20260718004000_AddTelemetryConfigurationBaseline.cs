using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dispatcher.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DispatcherDbContext))]
[Migration("20260718004000_AddTelemetryConfigurationBaseline")]
public partial class AddTelemetryConfigurationBaseline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: SchemaNames.Telemetry);

        migrationBuilder.CreateTable(
            name: "telemetry_sources",
            schema: SchemaNames.Telemetry,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                protocol = table.Column<int>(type: "integer", nullable: false),
                endpoint = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: false),
                configuration_schema_version = table.Column<int>(type: "integer", nullable: false),
                configuration_json = table.Column<string>(type: "jsonb", nullable: false),
                secret_reference = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                is_enabled = table.Column<bool>(type: "boolean", nullable: false),
                is_archived = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_telemetry_sources", x => x.id);
            });

        migrationBuilder.CreateTable(
            name: "data_points",
            schema: SchemaNames.Telemetry,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                equipment_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                value_kind = table.Column<int>(type: "integer", nullable: false),
                unit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                freshness_timeout_seconds = table.Column<int>(type: "integer", nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                is_archived = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_data_points", x => x.id);
                table.ForeignKey(name: "fk_data_points_equipment_equipment_id", column: x => x.equipment_id, principalSchema: SchemaNames.Assets, principalTable: "equipment", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateTable(
            name: "protocol_mappings",
            schema: SchemaNames.Telemetry,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                data_point_id = table.Column<Guid>(type: "uuid", nullable: false),
                telemetry_source_id = table.Column<Guid>(type: "uuid", nullable: false),
                protocol = table.Column<int>(type: "integer", nullable: false),
                mapping_schema_version = table.Column<int>(type: "integer", nullable: false),
                mapping_json = table.Column<string>(type: "jsonb", nullable: false),
                is_archived = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_protocol_mappings", x => x.id);
                table.ForeignKey(name: "fk_protocol_mappings_data_points_data_point_id", column: x => x.data_point_id, principalSchema: SchemaNames.Telemetry, principalTable: "data_points", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_protocol_mappings_telemetry_sources_telemetry_source_id", column: x => x.telemetry_source_id, principalSchema: SchemaNames.Telemetry, principalTable: "telemetry_sources", principalColumn: "id", onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(name: "ix_telemetry_sources_code", schema: SchemaNames.Telemetry, table: "telemetry_sources", column: "code", unique: true);
        migrationBuilder.CreateIndex(name: "ix_telemetry_sources_protocol", schema: SchemaNames.Telemetry, table: "telemetry_sources", column: "protocol");
        migrationBuilder.CreateIndex(name: "ix_data_points_code", schema: SchemaNames.Telemetry, table: "data_points", column: "code", unique: true);
        migrationBuilder.CreateIndex(name: "ix_data_points_equipment_id", schema: SchemaNames.Telemetry, table: "data_points", column: "equipment_id");
        migrationBuilder.CreateIndex(name: "ix_protocol_mappings_data_point_id", schema: SchemaNames.Telemetry, table: "protocol_mappings", column: "data_point_id", unique: true);
        migrationBuilder.CreateIndex(name: "ix_protocol_mappings_telemetry_source_id", schema: SchemaNames.Telemetry, table: "protocol_mappings", column: "telemetry_source_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "protocol_mappings", schema: SchemaNames.Telemetry);
        migrationBuilder.DropTable(name: "data_points", schema: SchemaNames.Telemetry);
        migrationBuilder.DropTable(name: "telemetry_sources", schema: SchemaNames.Telemetry);
    }
}
