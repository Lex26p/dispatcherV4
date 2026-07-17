# Диспетчер

Диспетчер — web-система мониторинга оборудования с аварийной сигнализацией.

## Текущий статус

В проекте создана базовая архитектура:

- Domain layer
- Application contracts and services
- Infrastructure persistence on EF Core
- API wiring and device endpoints
- EF Core migration tooling

PostgreSQL пока не готов. Применение миграции к базе можно выполнить позже.

## Step 09

Шаг 09 выравнивает версии EF Core в `Dispatcher.Infrastructure` и `Dispatcher.Worker`, чтобы убрать предупреждение MSB3277 о конфликте `Microsoft.EntityFrameworkCore.Relational`.

## Решение

Solution: `Dispatcher.slnx`  
Main branch: `master`
