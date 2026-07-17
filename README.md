# Диспетчер

Диспетчер — web-система мониторинга оборудования с аварийной сигнализацией.

## Текущее состояние

В проект добавлены базовые слои Domain, Application, Infrastructure и API.

На Step 11 добавлены endpoints текущих значений тегов:

- `GET /api/tag-values/current`
- `GET /api/tag-values/current/{tagId}`
- `POST /api/tag-values/current`
- `POST /api/tag-values/current/{tagId}`
- `GET /api/tags/{tagId}/current-value`

Эти endpoints нужны для будущего polling worker и SignalR realtime-обновлений.

## Важно

PostgreSQL может быть не установлен локально. В этом случае сборка и `/api/health` должны работать, но endpoints данных будут полноценно работать только после запуска PostgreSQL и применения EF Core миграции.

## Репозиторий

Репозиторий: `dispatcherV4`  
Solution: `Dispatcher.slnx`  
Namespace: `Dispatcher`
