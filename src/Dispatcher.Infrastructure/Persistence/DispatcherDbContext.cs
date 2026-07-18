using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Infrastructure.Persistence;

public sealed class DispatcherDbContext(DbContextOptions<DispatcherDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaNames.Public);
        base.OnModelCreating(modelBuilder);
    }
}
