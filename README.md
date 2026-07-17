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

## Текущий статус

- Создана структура solution.
- Добавлены базовые проекты.
- Добавлена основа Domain-модели.
- Добавлены сущности устройств, тегов и текущих значений.
- Добавлены контракты Application layer для устройств, тегов и текущих значений.
- Добавлен Infrastructure persistence layer на EF Core с PostgreSQL provider.
