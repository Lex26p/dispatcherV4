using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dispatcher.Infrastructure.Persistence;

public sealed class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<DispatcherDbContext>
{
    public DispatcherDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DISPATCHER_CONNECTION_STRING")
            ?? "Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Include Error Detail=false";

        var options = new DbContextOptionsBuilder<DispatcherDbContext>()
            .UseNpgsql(connectionString)
            .Options;

        return new DispatcherDbContext(options);
    }
}
