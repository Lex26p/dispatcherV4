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

## Текущее состояние

Реализованы:

- базовая Domain-модель;
- Application contracts;
- Application services;
- EF Core Infrastructure;
- API endpoints для устройств;
- API endpoints для тегов;
- API endpoints для текущих значений тегов;
- EF Core migration setup;
- mock polling worker;
- SignalR hub для realtime-обновлений текущих значений тегов.

PostgreSQL пока может быть не готов локально. В этом случае database update и data endpoints можно проверить позже.
