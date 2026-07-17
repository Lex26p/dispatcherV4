# Диспетчер

Диспетчер — web-система мониторинга оборудования с аварийной сигнализацией.

## Текущий статус

В проекте уже есть:

- ASP.NET Core API
- Blazor WebAssembly UI
- Application services
- EF Core infrastructure
- EF Core migration setup
- Device API
- Tag API
- Current tag value API
- SignalR realtime foundation
- Mock polling worker
- Blazor device management page

## Локальный запуск API

```powershell
dotnet run --project .\src\Dispatcher.Api\Dispatcher.Api.csproj
```

API по умолчанию:

```text
http://localhost:5076
```

## Локальный запуск Blazor

```powershell
dotnet run --project .\src\Dispatcher.Web\Dispatcher.Web.csproj
```

Blazor по умолчанию:

```text
http://localhost:5048
```

## PostgreSQL

PostgreSQL пока может быть не готов локально. Сборка проекта и запуск Blazor UI должны проходить без PostgreSQL, но операции чтения/создания устройств требуют работающую базу и примененную миграцию.
