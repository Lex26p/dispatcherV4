# Blazor Devices UI

Step 15 adds the first real data page in Blazor WebAssembly.

## Page

```text
/devices
```

## Features

- Loads devices from `GET /api/devices`.
- Creates Modbus TCP devices through `POST /api/devices/modbus-tcp`.
- Creates SNMP devices through `POST /api/devices/snmp`.
- Enables and disables devices through API commands.
- Shows friendly messages when the API or PostgreSQL is not available.

## Local development

Run API:

```powershell
dotnet run --project .\src\Dispatcher.Api\Dispatcher.Api.csproj
```

Run Blazor:

```powershell
dotnet run --project .\src\Dispatcher.Web\Dispatcher.Web.csproj
```

Open:

```text
http://localhost:5048/devices
```

## Database note

The page builds and opens without PostgreSQL, but device list and save operations require PostgreSQL and the EF Core migration to be applied.
