# Диспетчер

Диспетчер — web-система мониторинга оборудования с аварийной сигнализацией.

## MVP

Первая версия системы предназначена для чтения данных с устройств по Modbus TCP и SNMP, отображения текущих значений, хранения истории, регистрации аварий и отправки Email-уведомлений.

## Технологии

- ASP.NET Core / C#
- Blazor WebAssembly
- SignalR
- .NET Worker Service
- Entity Framework Core
- PostgreSQL
- TimescaleDB в будущем для истории временных рядов

## Репозиторий

Репозиторий: `dispatcherV4`  
Solution: `Dispatcher.slnx`  
Namespace: `Dispatcher`

## Текущий API

```text
GET  /api/health
GET  /api/devices
GET  /api/devices/{id}
POST /api/devices/modbus-tcp
POST /api/devices/snmp
POST /api/devices/{id}/enable
POST /api/devices/{id}/disable

GET  /api/tags/{id}
GET  /api/devices/{deviceId}/tags
POST /api/tags/modbus
POST /api/tags/snmp
POST /api/tags/{id}/enable
POST /api/tags/{id}/disable
```

## Development docs

```text
docs/development/database.md
docs/development/api-tags.md
```
