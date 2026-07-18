# Диспетчер

Industrial Dispatcher baseline repository.

## Current development state

- Current small step: Step 8B — Equipment API.
- Last completed step: Step 8A — Equipment domain and persistence.
- Last completed commit: `3bc727ce2325cecaefd7d582f2b8d956a06204b8`.

## Local checks

```powershell
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

## Database

PostgreSQL connection for local development:

```powershell
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres;Include Error Detail=false"
```

## Step 8B scope

Step 8B adds REST API for canonical Equipment. Equipment remains an asset registry entity only: no protocol addresses, telemetry points, Modbus, SNMP, commands, maintenance, current values or history are introduced in this step.
