# Диспетчер

Промышленная платформа диспетчеризации, эксплуатации, событий, инцидентов и ТОиР.

## Текущий статус

Текущий шаг: **Step 8C — Equipment Web UI**.

Этот шаг намеренно малый: он добавляет только Blazor UI для существующего Equipment API. Новых миграций, telemetry-моделей и protocol-полей нет.

## Источники истины

- `DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md` — master-ТЗ и roadmap.
- `DISPATCHER_AI_IMPLEMENTATION_SPEC.md` — AI-focused execution guide.
- `PROJECT_STATE.md` — текущее состояние репозитория и следующий шаг.

## Проверка

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

## Локальный запуск

API:

```powershell
cd C:\Projects\dispatcherV4
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres;Include Error Detail=false"
dotnet run --project .\src\Dispatcher.Api\Dispatcher.Api.csproj
```

Web:

```powershell
cd C:\Projects\dispatcherV4
dotnet run --project .\src\Dispatcher.Web\Dispatcher.Web.csproj
```

Проверить `/equipment`.

## Важные правила

- Не добавлять protocol fields в Equipment.
- Не добавлять telemetry в Step 8C.
- Не считать Web UI финальным дизайном.
- После каждого шага обновлять `PROJECT_STATE.md`.
