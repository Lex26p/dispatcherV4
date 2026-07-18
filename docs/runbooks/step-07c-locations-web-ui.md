# Step 7C — Locations Web UI

## Цель

Добавить первый рабочий Blazor UI поверх Locations API без изменения доменной модели и без новых migrations.

## Состав изменения

- `Dispatcher.Web/Api/DispatcherApiClient.cs` — минимальный typed client для Locations API.
- `Dispatcher.Web/Pages/Locations.razor` — список, создание, обновление и архивирование локаций.
- `RouteCatalog` — route `/locations`.
- `Dispatcher.Api/Program.cs` — development-only CORS для localhost WebAssembly client.
- Web launch settings and development API base URL.

## Проверки

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

Запуск API:

```powershell
cd C:\Projects\dispatcherV4
$env:DISPATCHER_CONNECTION_STRING="Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Password=postgres;Include Error Detail=false"
dotnet run --project .\src\Dispatcher.Api\Dispatcher.Api.csproj
```

Запуск Web:

```powershell
cd C:\Projects\dispatcherV4
dotnet run --project .\src\Dispatcher.Web\Dispatcher.Web.csproj
```

Открыть:

```text
http://localhost:5048/locations
```

## Ожидаемый результат

- `/locations` открывается в shell.
- UI загружает persisted локации из `Dispatcher.Api`.
- Можно создать, переименовать и архивировать локацию.
- При остановленном API UI показывает понятную ошибку.

## Ограничения

- Это не финальный UX для Asset Model.
- Нет drag-and-drop tree, bulk actions, breadcrumbs, scoped permissions UI и audit view.
- Backend authorization остается источником истины; Web UI не считается security boundary.
