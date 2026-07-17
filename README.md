# Диспетчер

Промышленная платформа диспетчеризации, эксплуатации, событий, инцидентов и ТОиР.

## Текущий статус

Репозиторий очищен от учебного кода и переведен на промышленный baseline.

Текущий шаг: **Step 2 — shared contracts and API correlation**.

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

## Проверка

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

## API smoke

```powershell
cd C:\Projects\dispatcherV4
dotnet run --project .\src\Dispatcher.Api\Dispatcher.Api.csproj
```

Проверить в другом окне:

```powershell
Invoke-WebRequest http://localhost:5076/api/health/live | Select-Object StatusCode,Headers
Invoke-WebRequest -Headers @{ 'X-Correlation-ID' = 'manual-step-02' } http://localhost:5076/api/health/ready | Select-Object StatusCode,Headers
```

## Важные правила

- Не начинать C++ до production/load-test metrics.
- Не создавать все будущие проекты solution заранее.
- Не смешивать EventRecord, AlarmOccurrence и Incident.
- Не читать draft в runtime.
- Не считать hidden UI backend authorization.
- После каждого шага обновлять `PROJECT_STATE.md`.
