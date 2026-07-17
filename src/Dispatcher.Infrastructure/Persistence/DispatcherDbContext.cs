using Dispatcher.Application.Abstractions.Persistence;
using Dispatcher.Domain.Devices;
using Dispatcher.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Infrastructure.Persistence;

public sealed class DispatcherDbContext : DbContext, IUnitOfWork
{
    public DispatcherDbContext(DbContextOptions<DispatcherDbContext> options)
        : base(options)
    {
    }

    public DbSet<Device> Devices => Set<Device>();

    public DbSet<Tag> Tags => Set<Tag>();

    public DbSet<TagValue> CurrentTagValues => Set<TagValue>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DispatcherDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
