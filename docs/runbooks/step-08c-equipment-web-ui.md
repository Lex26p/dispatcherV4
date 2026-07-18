# Step 8C — Equipment Web UI

## Scope

This step adds a small Blazor Web UI for the existing Equipment API.

Included:

- `/equipment` route.
- Equipment list with location filter and archive filter.
- Create equipment.
- Edit name/description.
- Move equipment between active locations.
- Archive and restore equipment.
- Navigation catalog entry.
- `DispatcherApiClient` equipment methods.

Not included:

- Telemetry configuration.
- DataPoint mapping.
- Modbus/SNMP fields.
- Dashboard or faceplate runtime.
- New migration.

## Verification

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

Run API:

```powershell
cd C:\Projects\dispatcherV4
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres;Include Error Detail=false"
dotnet run --project .\src\Dispatcher.Api\Dispatcher.Api.csproj
```

Run Web:

```powershell
cd C:\Projects\dispatcherV4
dotnet run --project .\src\Dispatcher.Web\Dispatcher.Web.csproj
```

Browser smoke:

- Open `/equipment`.
- Confirm locations are loaded.
- Create equipment inside an active location.
- Edit equipment.
- Move equipment to another location if at least two active locations exist.
- Archive and restore equipment.
- Stop API and confirm UI shows an error instead of a blank page.

## Design constraints

- Equipment remains protocol-neutral.
- API remains the source of truth for authorization.
- Web UI is intentionally simple and not final visual polish.
