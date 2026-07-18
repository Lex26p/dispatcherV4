# Database runbook

## Purpose

Step 4 introduces PostgreSQL infrastructure for the industrial Dispatcher baseline.
It intentionally creates only an empty EF Core baseline migration. Business tables start in later vertical slices.

## Connection string

Use user secrets or an environment variable. Do not commit passwords.

Recommended local environment variable name:

```powershell
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=YOUR_LOCAL_PASSWORD;Include Error Detail=false"
```

For test database runs:

```powershell
$env:DISPATCHER_RUN_DB_TESTS="1"
$env:DISPATCHER_TEST_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher_test;Username=postgres;Password=YOUR_LOCAL_PASSWORD;Include Error Detail=false"
```

## Migration commands

```powershell
cd C:\Projects\dispatcherV4
dotnet tool restore
dotnet tool run dotnet-ef database update --project .\src\Dispatcher.Infrastructure\Dispatcher.Infrastructure.csproj --startup-project .\src\Dispatcher.Api\Dispatcher.Api.csproj --context DispatcherDbContext
```

## Health checks

- `/api/health/live` checks only that the API process is running.
- `/api/health/ready` checks PostgreSQL connectivity and returns `503` when unavailable.

`ready` must not expose connection strings, passwords, host secrets or stack traces.
