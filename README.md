# Диспетчер

Промышленная платформа диспетчеризации, эксплуатации, событий, инцидентов и ТОиР.

## Текущий статус

Текущий шаг: **Step 7A — Locations domain and persistence**.

Этот шаг намеренно малый: он добавляет только доменную модель `Location`, EF Core mapping, migration `assets.locations` и тесты. API и UI для Locations будут отдельными шагами.

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

## Применение миграции

```powershell
cd C:\Projects\dispatcherV4
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres;Include Error Detail=false"
dotnet tool restore
dotnet tool run dotnet-ef database update --project .\src\Dispatcher.Infrastructure\Dispatcher.Infrastructure.csproj --startup-project .\src\Dispatcher.Api\Dispatcher.Api.csproj --context DispatcherDbContext
```

## Важные правила

- Не начинать C++ до production/load-test metrics.
- Не создавать все будущие проекты solution заранее.
- Не смешивать Location с Equipment или protocol endpoint.
- Не добавлять API/UI в Step 7A.
- После каждого шага обновлять `PROJECT_STATE.md`.

## Current implementation status

- Step 7B: Locations API/application layer added. Web UI for Locations is deferred to Step 7C.
