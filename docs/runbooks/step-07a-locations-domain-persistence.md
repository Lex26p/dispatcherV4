# Step 7A — Locations domain and persistence

## Goal

Add the first Assets bounded-context entity without adding API endpoints or frontend routes yet.

## Scope

- `Location` domain entity.
- EF Core configuration for `assets.locations`.
- Migration `20260718002000_AddLocationsBaseline`.
- Unit tests for basic domain invariants.
- Integration model smoke test.

## Explicitly out of scope

- REST endpoints for locations.
- Blazor `/locations` UI.
- Permission checks for locations.
- Full hierarchy cycle detection across persisted rows.
- Equipment.

## Verification

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

Apply migration when PostgreSQL is available:

```powershell
cd C:\Projects\dispatcherV4
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres;Include Error Detail=false"
dotnet tool restore
dotnet tool run dotnet-ef database update --project .\src\Dispatcher.Infrastructure\Dispatcher.Infrastructure.csproj --startup-project .\src\Dispatcher.Api\Dispatcher.Api.csproj --context DispatcherDbContext
```
