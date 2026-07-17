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
- TimescaleDB в будущем для истории временных рядов

## Репозиторий

Репозиторий: `dispatcherV4`  
Solution: `Dispatcher.sln`  
Namespace: `Dispatcher`
