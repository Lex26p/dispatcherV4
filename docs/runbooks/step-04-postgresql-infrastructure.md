# Step 4 — PostgreSQL infrastructure

## Result

This step adds:

- EF Core infrastructure in `Dispatcher.Infrastructure`.
- `DispatcherDbContext`.
- Schema ownership constants.
- Design-time DbContext factory.
- Empty baseline migration `20260718000000_BaselineDatabase`.
- PostgreSQL-aware `/api/health/ready`.
- Optional PostgreSQL integration smoke test.

## Non-goals

- No business tables.
- No Identity/RBAC entities.
- No Locations/Equipment/Telemetry tables.
- No SQLite or InMemory database replacement for integration tests.
- No committed secrets.

## Verification

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

Optional local database verification:

```powershell
cd C:\Projects\dispatcherV4
dotnet tool restore
dotnet tool run dotnet-ef database update --project .\src\Dispatcher.Infrastructure\Dispatcher.Infrastructure.csproj --startup-project .\src\Dispatcher.Api\Dispatcher.Api.csproj --context DispatcherDbContext
```
