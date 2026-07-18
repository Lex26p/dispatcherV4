using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dispatcher.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DispatcherDbContext))]
[Migration("20260718005000_AddCurrentValuesBaseline")]
public partial class AddCurrentValuesBaseline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.CreateTable(
            name: "current_values",
            schema: SchemaNames.Telemetry,
            columns: table => new
            {
                data_point_id = table.Column<Guid>(type: "uuid", nullable: false),
                telemetry_source_id = table.Column<Guid>(type: "uuid", nullable: true),
                sequence = table.Column<long>(type: "bigint", nullable: false),
                value_kind = table.Column<int>(type: "integer", nullable: false),
                raw_value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                unit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                quality = table.Column<int>(type: "integer", nullable: false),
                source_timestamp_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                received_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_current_values", x => x.data_point_id);
                table.ForeignKey(name: "fk_current_values_data_points_data_point_id", column: x => x.data_point_id, principalSchema: SchemaNames.Telemetry, principalTable: "data_points", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_current_values_telemetry_sources_telemetry_source_id", column: x => x.telemetry_source_id, principalSchema: SchemaNames.Telemetry, principalTable: "telemetry_sources", principalColumn: "id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateTable(
            name: "historical_values",
            schema: SchemaNames.Telemetry,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                data_point_id = table.Column<Guid>(type: "uuid", nullable: false),
                telemetry_source_id = table.Column<Guid>(type: "uuid", nullable: true),
                sequence = table.Column<long>(type: "bigint", nullable: false),
                value_kind = table.Column<int>(type: "integer", nullable: false),
                raw_value = table.Column<string>(type: "character varying(2000)", maxLength: 2000, nullable: false),
                unit = table.Column<string>(type: "character varying(40)", maxLength: 40, nullable: true),
                quality = table.Column<int>(type: "integer", nullable: false),
                source_timestamp_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                received_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                error_message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_historical_values", x => x.id);
                table.ForeignKey(name: "fk_historical_values_data_points_data_point_id", column: x => x.data_point_id, principalSchema: SchemaNames.Telemetry, principalTable: "data_points", principalColumn: "id", onDelete: ReferentialAction.Restrict);
                table.ForeignKey(name: "fk_historical_values_telemetry_sources_telemetry_source_id", column: x => x.telemetry_source_id, principalSchema: SchemaNames.Telemetry, principalTable: "telemetry_sources", principalColumn: "id", onDelete: ReferentialAction.SetNull);
            });

        migrationBuilder.CreateIndex(name: "ix_current_values_quality", schema: SchemaNames.Telemetry, table: "current_values", column: "quality");
        migrationBuilder.CreateIndex(name: "ix_current_values_received_at_utc", schema: SchemaNames.Telemetry, table: "current_values", column: "received_at_utc");
        migrationBuilder.CreateIndex(name: "ix_current_values_telemetry_source_id", schema: SchemaNames.Telemetry, table: "current_values", column: "telemetry_source_id");

        migrationBuilder.CreateIndex(name: "ix_historical_values_data_point_id_sequence", schema: SchemaNames.Telemetry, table: "historical_values", columns: new[] { "data_point_id", "sequence" }, unique: true);
        migrationBuilder.CreateIndex(name: "ix_historical_values_data_point_id_source_timestamp_utc", schema: SchemaNames.Telemetry, table: "historical_values", columns: new[] { "data_point_id", "source_timestamp_utc" });
        migrationBuilder.CreateIndex(name: "ix_historical_values_quality", schema: SchemaNames.Telemetry, table: "historical_values", column: "quality");
        migrationBuilder.CreateIndex(name: "ix_historical_values_received_at_utc", schema: SchemaNames.Telemetry, table: "historical_values", column: "received_at_utc");
        migrationBuilder.CreateIndex(name: "ix_historical_values_telemetry_source_id", schema: SchemaNames.Telemetry, table: "historical_values", column: "telemetry_source_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "historical_values", schema: SchemaNames.Telemetry);
        migrationBuilder.DropTable(name: "current_values", schema: SchemaNames.Telemetry);
    }
}
