# Step 6 — Web shell baseline

## Цель

Привести стартовый Blazor UI к промышленной shell-структуре без копирования монолитного prototype `app.js`.

## Что добавлено

- `AppShell`
- `GlobalHeader`
- `NavigationRail`
- `ContextDrawerHost`
- `RouteCatalog`
- `EmptyState`
- routes `/settings`, `/settings/profile`, `/admin`
- dark theme tokens: `tokens.css`
- shell layout: `shell.css`

## Что не входит в шаг

- Dashboard Editor
- SVG Editor
- kiosk/terminal runtime
- realtime values
- production authentication provider
- настоящая permission-aware filtering из `/api/me`
- загрузка admin/users таблицы из API

## Smoke

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

```powershell
cd C:\Projects\dispatcherV4
dotnet run --project .\src\Dispatcher.Web\Dispatcher.Web.csproj
```

Проверить routes:

- `/home`
- `/me`
- `/settings`
- `/settings/profile`
- `/admin`
- `/admin/users`
- `/forbidden`
- `/not-found`

## Ограничения

Shell выглядит лучше стартового skeleton, но это всё ещё baseline. Реальные данные, permission-aware меню, контекст оборудования, realtime state и admin tables появятся отдельными vertical slices.
