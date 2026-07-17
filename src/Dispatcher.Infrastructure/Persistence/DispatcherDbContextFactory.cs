using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Dispatcher.Infrastructure.Persistence;

public sealed class DispatcherDbContextFactory : IDesignTimeDbContextFactory<DispatcherDbContext>
{
    private const string DefaultConnectionString = "Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres";

    public DispatcherDbContext CreateDbContext(string[] args)
    {
        var connectionString = Environment.GetEnvironmentVariable("DISPATCHER_DATABASE");

        if (string.IsNullOrWhiteSpace(connectionString))
        {
            connectionString = DefaultConnectionString;
        }

        var optionsBuilder = new DbContextOptionsBuilder<DispatcherDbContext>();
        optionsBuilder.UseNpgsql(connectionString);

        return new DispatcherDbContext(optionsBuilder.Options);
    }
}
