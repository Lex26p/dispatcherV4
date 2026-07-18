using Dispatcher.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

#nullable disable

namespace Dispatcher.Infrastructure.Persistence.Migrations;

[DbContext(typeof(DispatcherDbContext))]
public partial class DispatcherDbContextModelSnapshot : ModelSnapshot
{
    protected override void BuildModel(ModelBuilder modelBuilder)
    {
#pragma warning disable 612, 618
        modelBuilder
            .HasDefaultSchema("public")
            .HasAnnotation("ProductVersion", "10.0.0");
#pragma warning restore 612, 618
    }
}
