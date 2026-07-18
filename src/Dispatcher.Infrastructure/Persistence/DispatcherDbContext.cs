using Dispatcher.Domain.Assets;
using Dispatcher.Domain.IdentityAccess;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Infrastructure.Persistence;

public sealed class DispatcherDbContext(DbContextOptions<DispatcherDbContext> options) : DbContext(options)
{
    public DbSet<UserAccount> UserAccounts => Set<UserAccount>();

    public DbSet<Role> Roles => Set<Role>();

    public DbSet<PermissionScope> PermissionScopes => Set<PermissionScope>();

    public DbSet<RoleAssignment> RoleAssignments => Set<RoleAssignment>();

    public DbSet<Location> Locations => Set<Location>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.HasDefaultSchema(SchemaNames.Public);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(DispatcherDbContext).Assembly);
        base.OnModelCreating(modelBuilder);
    }
}
