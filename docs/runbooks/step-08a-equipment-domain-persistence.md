# Step 8A — Equipment domain and persistence

## Scope

This substep adds the canonical `Equipment` entity and PostgreSQL persistence only.

Included:

- `Dispatcher.Domain.Assets.Equipment`
- EF Core configuration for `assets.equipment`
- migration `20260718003000_AddEquipmentBaseline`
- model snapshot update
- unit tests for domain invariants
- integration model smoke test

Not included:

- Equipment API
- Equipment Web UI
- DataPoint or TelemetrySource
- protocol fields, Modbus addresses, SNMP OIDs or connection settings
- MaintenanceObject

## Invariant

`Equipment` is a product asset model. Protocol-specific fields must remain outside this entity and will be introduced later through `TelemetrySource`, `DataPoint` and protocol mappings.

## Verification

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres;Include Error Detail=false"
dotnet tool run dotnet-ef database update --project .\src\Dispatcher.Infrastructure\Dispatcher.Infrastructure.csproj --startup-project .\src\Dispatcher.Api\Dispatcher.Api.csproj --context DispatcherDbContext
```
