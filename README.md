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

Созданы базовые проекты, Domain-модель, Application-сервисы, EF Core Infrastructure и первые API endpoints.

## API endpoints

Health:

```http
GET /api/health
```

Devices:

```http
GET /api/devices
GET /api/devices/{id}
POST /api/devices/modbus-tcp
POST /api/devices/snmp
POST /api/devices/{id}/enable
POST /api/devices/{id}/disable
```

До создания базы данных и миграций endpoints устройств могут вернуть ошибку подключения или отсутствия таблиц. Это будет исправлено на следующих шагах.
