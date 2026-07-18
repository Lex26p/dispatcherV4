# Диспетчер

Промышленная платформа диспетчеризации, эксплуатации, событий, инцидентов и ТОиР.

## Текущий статус

Репозиторий очищен от учебного кода и переведен на промышленный baseline.

Текущий шаг: **Step 6 — Web shell baseline**.

## Источники истины

- `DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md` — master-ТЗ и roadmap.
- `DISPATCHER_AI_IMPLEMENTATION_SPEC.md` — AI-focused execution guide.
- `PROJECT_STATE.md` — текущее состояние репозитория и следующий шаг.

## Текущий состав solution

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

## Step 4 database baseline

- `DispatcherDbContext`
- `SchemaNames`
- design-time DbContext factory
- empty baseline migration `20260718000000_BaselineDatabase`
- PostgreSQL-aware `/api/health/ready`
- optional PostgreSQL integration smoke test

No business tables are created in this step.


## Step 6 Web shell baseline

- единая Blazor shell-структура: `AppShell`, `GlobalHeader`, `NavigationRail`, `ContextDrawerHost`;
- routes `/home`, `/me`, `/settings`, `/settings/profile`, `/admin`, `/admin/users`, `/forbidden`, `/not-found`;
- route catalog для будущей permission-aware navigation;
- dark industrial theme tokens;
- временный контекстный drawer без real telemetry/data quality;
- без dashboard editor, SVG editor, kiosk, realtime и production auth provider.

## Проверка без локальной БД

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

## Проверка с локальной PostgreSQL

```powershell
cd C:\Projects\dispatcherV4
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=YOUR_LOCAL_PASSWORD;Include Error Detail=false"
dotnet tool restore
dotnet tool run dotnet-ef database update --project .\src\Dispatcher.Infrastructure\Dispatcher.Infrastructure.csproj --startup-project .\src\Dispatcher.Api\Dispatcher.Api.csproj --context DispatcherDbContext
```

## API smoke

```powershell
cd C:\Projects\dispatcherV4
dotnet run --project .\src\Dispatcher.Api\Dispatcher.Api.csproj
```

Проверить в другом окне:

```powershell
Invoke-WebRequest http://localhost:5076/api/health/live -UseBasicParsing | Select-Object StatusCode,Headers
try { Invoke-WebRequest http://localhost:5076/api/health/ready -UseBasicParsing | Select-Object StatusCode,Content,Headers } catch { $_.Exception.Response | Select-Object StatusCode,Headers; $reader = New-Object System.IO.StreamReader($_.Exception.Response.GetResponseStream()); $reader.ReadToEnd(); $reader.Close() }
```

## Важные правила

- Не начинать C++ до production/load-test metrics.
- Не создавать все будущие проекты solution заранее.
- Не смешивать EventRecord, AlarmOccurrence и Incident.
- Не читать draft в runtime.
- Не считать hidden UI backend authorization.
- После каждого шага обновлять `PROJECT_STATE.md`.
