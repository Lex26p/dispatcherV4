# Диспетчер

Промышленная web-платформа диспетчеризации, событий, аварий, инцидентов и эксплуатации распределенных объектов.

## Документы-источники

- `DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md` — master-ТЗ и дорожная карта.
- `DISPATCHER_AI_IMPLEMENTATION_SPEC.md` — execution guide для пошаговой разработки ИИ-агентом.
- `PROJECT_STATE.md` — текущее состояние репозитория.

## Стек первого промышленного релиза

- C# / .NET
- ASP.NET Core
- Blazor
- PostgreSQL
- SignalR
- .NET Worker Services

## Текущее состояние

Step 1 создает минимальный solution skeleton.

## Быстрая проверка

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

## Запуск API

```powershell
dotnet run --project .\src\Dispatcher.Api\Dispatcher.Api.csproj
```

Проверка:

```powershell
Invoke-RestMethod http://localhost:5076/api/health/live
```

## Запуск Web

```powershell
dotnet run --project .\src\Dispatcher.Web\Dispatcher.Web.csproj
```

Открыть `/home` по URL, который покажет `dotnet run`.
