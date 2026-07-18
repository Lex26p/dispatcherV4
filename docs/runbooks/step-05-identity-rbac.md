# Step 5 — Identity/RBAC baseline runbook

## Build

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

## Apply migration

```powershell
cd C:\Projects\dispatcherV4
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres;Include Error Detail=false"
dotnet tool restore
dotnet tool run dotnet-ef database update --project .\src\Dispatcher.Infrastructure\Dispatcher.Infrastructure.csproj --startup-project .\src\Dispatcher.Api\Dispatcher.Api.csproj --context DispatcherDbContext
```

## Smoke checks

```powershell
Invoke-WebRequest http://localhost:5076/api/me -UseBasicParsing | Select-Object StatusCode,Content,Headers
Invoke-WebRequest http://localhost:5076/api/users -UseBasicParsing | Select-Object StatusCode,Content,Headers
try { Invoke-WebRequest -Headers @{ 'X-Dispatcher-Permissions' = 'identity.me.view' } http://localhost:5076/api/users -UseBasicParsing | Select-Object StatusCode,Content,Headers } catch { $_.Exception.Response | Select-Object StatusCode,Headers; $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream()); $reader.ReadToEnd(); $reader.Close() }
```

Expected result: `/api/users` returns 200 for the default dev admin and 403 for a request that only has `identity.me.view`.
