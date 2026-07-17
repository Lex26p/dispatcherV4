# Диспетчер

SCADA-lite / web-система мониторинга оборудования.

## Текущее состояние

Готовы базовые слои Domain, Application, Infrastructure, API, Worker и Blazor WebAssembly UI.

Step 16 добавляет рабочую Blazor-страницу тегов:

- выбор устройства;
- список тегов выбранного устройства;
- создание Modbus TCP тегов;
- создание SNMP тегов;
- включение и отключение тегов;
- дружелюбное сообщение об ошибке, если PostgreSQL пока не готов.

## Решения

- Product name: Диспетчер
- Solution: Dispatcher.slnx
- Main branch: master
- Target framework: .NET 10
- Backend: ASP.NET Core
- Frontend: Blazor WebAssembly
- ORM: Entity Framework Core
- Database: PostgreSQL, later TimescaleDB
- Realtime: SignalR
- Worker: .NET Worker Service

## Локальная проверка

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.slnx
dotnet build .\Dispatcher.slnx
```

API:

```powershell
cd C:\Projects\dispatcherV4
dotnet run --project .\src\Dispatcher.Api\Dispatcher.Api.csproj
```

Blazor:

```powershell
cd C:\Projects\dispatcherV4
dotnet run --project .\src\Dispatcher.Web\Dispatcher.Web.csproj
```

Открыть:

```text
http://localhost:5048/tags
```

Data-запросы будут полноценно работать после запуска PostgreSQL и применения миграции.
