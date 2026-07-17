# Диспетчер

Диспетчер — web-система мониторинга оборудования с аварийной сигнализацией.

## MVP

Первая версия системы предназначена для чтения данных с устройств по Modbus TCP и SNMP, отображения текущих значений, хранения истории, регистрации аварий и отправки Email-уведомлений.

## Технологии

- ASP.NET Core / C#
- Blazor WebAssembly
- SignalR
- .NET Worker Service
- PostgreSQL
- Entity Framework Core
- TimescaleDB в будущем для истории временных рядов

## Репозиторий

Репозиторий: `dispatcherV4`  
Solution: `Dispatcher.slnx`  
Namespace: `Dispatcher`

## Текущий статус

Сейчас добавлены:

- Domain base types.
- Device and Tag domain entities.
- Application contracts and services.
- EF Core infrastructure and migration tooling.
- Device API endpoints.
- Tag API endpoints.
- Current tag value API endpoints.
- Mock polling worker.
- SignalR realtime API foundation.
- Blazor WebAssembly UI foundation.

PostgreSQL пока не готов локально, поэтому database update и data endpoints можно проверить позже.
