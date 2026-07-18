# Step 9A — Telemetry domain and persistence

## Scope

This step adds the protocol-neutral telemetry configuration foundation:

- `TelemetrySource` — technical source of telemetry samples; stores endpoint/configuration and secret reference, never secret plaintext.
- `DataPoint` — product signal/parameter linked to Equipment; remains protocol-neutral.
- `ProtocolMapping` — protocol-specific mapping between `DataPoint` and `TelemetrySource`.
- EF Core mappings and migration `20260718004000_AddTelemetryConfigurationBaseline`.

## Out of scope

- REST API for telemetry configuration.
- Web UI for points/sources.
- Current values and history tables.
- Polling worker/adapters.
- Modbus/SNMP driver execution.
- Secret value storage.

## Verification

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

```powershell
cd C:\Projects\dispatcherV4
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres;Include Error Detail=false"
dotnet tool restore
dotnet tool run dotnet-ef database update --project .\src\Dispatcher.Infrastructure\Dispatcher.Infrastructure.csproj --startup-project .\src\Dispatcher.Api\Dispatcher.Api.csproj --context DispatcherDbContext
```

## Architectural notes

- Equipment remains protocol-neutral.
- DataPoint identity does not depend on Modbus registers, SNMP OIDs or future OPC UA/BACnet node identifiers.
- `ProtocolMapping.MappingJson` is intentionally bounded and schema-versioned.
- `TelemetrySource.SecretReference` is a reference only; plaintext secrets are rejected by domain rules.
