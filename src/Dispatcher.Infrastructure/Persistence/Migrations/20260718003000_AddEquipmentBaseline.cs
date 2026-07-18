using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dispatcher.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DispatcherDbContext))]
[Migration("20260718003000_AddEquipmentBaseline")]
public partial class AddEquipmentBaseline : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.EnsureSchema(name: SchemaNames.Assets);

        migrationBuilder.CreateTable(
            name: "equipment",
            schema: SchemaNames.Assets,
            columns: table => new
            {
                id = table.Column<Guid>(type: "uuid", nullable: false),
                location_id = table.Column<Guid>(type: "uuid", nullable: false),
                code = table.Column<string>(type: "character varying(80)", maxLength: 80, nullable: false),
                name = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                description = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: true),
                is_archived = table.Column<bool>(type: "boolean", nullable: false),
                created_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                updated_at_utc = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
            },
            constraints: table =>
            {
                table.PrimaryKey("pk_equipment", x => x.id);
                table.ForeignKey(
                    name: "fk_equipment_locations_location_id",
                    column: x => x.location_id,
                    principalSchema: SchemaNames.Assets,
                    principalTable: "locations",
                    principalColumn: "id",
                    onDelete: ReferentialAction.Restrict);
            });

        migrationBuilder.CreateIndex(
            name: "ix_equipment_code",
            schema: SchemaNames.Assets,
            table: "equipment",
            column: "code",
            unique: true);

        migrationBuilder.CreateIndex(
            name: "ix_equipment_location_id",
            schema: SchemaNames.Assets,
            table: "equipment",
            column: "location_id");
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
        migrationBuilder.DropTable(name: "equipment", schema: SchemaNames.Assets);
    }
}
