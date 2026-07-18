using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Dispatcher.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DispatcherDbContext))]
[Migration("20260718000000_BaselineDatabase")]
public partial class BaselineDatabase : Migration
{
    protected override void Up(MigrationBuilder migrationBuilder)
    {
        // Empty baseline migration.
        // Step 4 establishes EF Core/PostgreSQL infrastructure only.
        // Business schemas and tables are introduced by later vertical slices.
    }

    protected override void Down(MigrationBuilder migrationBuilder)
    {
    }
}

