# Local database

The development database is PostgreSQL.

Default local connection string:

```text
Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres
```

The same value is configured in:

```text
src/Dispatcher.Api/appsettings.Development.json
```

For EF Core design-time commands, the connection string can also be overridden with the `DISPATCHER_DATABASE` environment variable.

## EF Core commands

Restore local tools:

```powershell
dotnet tool restore
```

Create the first migration:

```powershell
dotnet tool run dotnet-ef migrations add InitialCreate --project .\src\Dispatcher.Infrastructure\Dispatcher.Infrastructure.csproj --startup-project .\src\Dispatcher.Api\Dispatcher.Api.csproj --context DispatcherDbContext --output-dir Persistence\Migrations
```

Apply migrations to PostgreSQL:

```powershell
dotnet tool run dotnet-ef database update --project .\src\Dispatcher.Infrastructure\Dispatcher.Infrastructure.csproj --startup-project .\src\Dispatcher.Api\Dispatcher.Api.csproj --context DispatcherDbContext
```
