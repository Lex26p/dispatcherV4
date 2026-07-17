# Step 1 — solution skeleton

## Purpose

Создан минимальный компилируемый solution для промышленного проекта «Диспетчер».

## Created projects

- `Dispatcher.Api`
- `Dispatcher.Web`
- `Dispatcher.Domain`
- `Dispatcher.Application`
- `Dispatcher.Infrastructure`
- `Dispatcher.Contracts`
- `Dispatcher.Telemetry.Worker`
- `Dispatcher.Workers`
- `Dispatcher.UnitTests`
- `Dispatcher.IntegrationTests`

## Health endpoints

- `GET /api/health/live`
- `GET /api/health/ready`

## Frontend routes

- `/`
- `/home`
- `/not-found`

## Local verification

```powershell
cd C:\Projects\dispatcherV4
```

```powershell
dotnet restore .\Dispatcher.sln
```

```powershell
dotnet build .\Dispatcher.sln --no-restore
```

```powershell
dotnet test .\Dispatcher.sln --no-build
```
