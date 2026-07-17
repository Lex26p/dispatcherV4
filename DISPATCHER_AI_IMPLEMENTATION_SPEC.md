# «Диспетчер»: AI-focused инструкция по пошаговой реализации

**Имя файла:** `DISPATCHER_AI_IMPLEMENTATION_SPEC.md`  
**Статус:** обязательный execution guide для ИИ-разработчика  
**Master-ТЗ:** `DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md`  
**Язык реализации и документации:** русский для проектной документации; английский для идентификаторов кода и публичных технических контрактов  
**Стек первого промышленного релиза:** C#/.NET, ASP.NET Core, Blazor, PostgreSQL, SignalR, .NET Worker Services

> Этот документ не заменяет master-ТЗ и не изменяет архитектуру продукта. Он преобразует master-ТЗ в жесткую последовательность действий для ИИ-агента. При расхождении формулировок master-ТЗ имеет приоритет, кроме случаев, когда настоящий документ сознательно сужает объем конкретного раннего AI-step, не меняя целевую архитектуру.

## Содержание

1. [Назначение документа](#1-назначение-документа)
2. [Главные правила для ИИ-разработчика](#2-главные-правила-для-ии-разработчика)
3. [Термины и инварианты](#3-термины-и-инварианты)
4. [Практическая стартовая структура solution](#4-практическая-стартовая-структура-solution)
5. [Repository state file](#5-repository-state-file)
6. [Порядок реализации для ИИ](#6-порядок-реализации-для-ии)
7. [Sprint-to-step mapping](#7-sprint-to-step-mapping)
8. [MVP scope для ИИ](#8-mvp-scope-для-ии)
9. [C++ future extraction guide](#9-c-future-extraction-guide)
10. [Архитектурные запреты](#10-архитектурные-запреты)
11. [Проверки после каждого шага](#11-проверки-после-каждого-шага)
12. [Формат выдачи каждого шага ИИ](#12-формат-выдачи-каждого-шага-ии)
13. [Файловая и naming convention](#13-файловая-и-naming-convention)
14. [API and route implementation order](#14-api-and-route-implementation-order)
15. [Testing plan для ИИ](#15-testing-plan-для-ии)
16. [Acceptance gates](#16-acceptance-gates)
17. [Как использовать исходный Core UI prototype](#17-как-использовать-исходный-core-ui-prototype)
18. [Финальная инструкция](#18-финальная-инструкция)

---

## 1. Назначение документа

Этот документ предназначен **не для менеджера, заказчика или конечного пользователя**, а для ИИ-агента, который создает репозиторий, генерирует код, выполняет миграции, запускает проверки, обновляет документацию и фиксирует текущее состояние проекта.

ИИ должен использовать документ как execution protocol:

1. выполнять только один ограниченный step или небольшой vertical slice за итерацию;
2. сначала прочитать master-ТЗ, этот guide и текущее `PROJECT_STATE.md`;
3. не считать ранее запланированное уже реализованным;
4. проверять существующий код перед созданием новых проектов, сущностей и endpoint;
5. завершать каждый шаг собираемым, проверяемым и документированным состоянием репозитория;
6. не переходить к следующему шагу, пока Definition of Done текущего шага не закрыт либо явно не записано проверяемое ограничение;
7. сохранять архитектурные инварианты даже тогда, когда более быстрый код кажется удобнее.

`DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md` остается **master-ТЗ**: источником продуктового смысла, bounded contexts, целевой архитектуры, MVP, roadmap, спринтов, модели данных, API, маршрутов и критериев приемки.

`DISPATCHER_AI_IMPLEMENTATION_SPEC.md` является **AI-focused execution guide**: он определяет порядок практической реализации, минимальный состав каждого шага, обязательные проверки, запрещенные сокращения и формат передачи результата пользователю.

ИИ не должен трактовать этот файл как разрешение перепроектировать продукт. Его задача — реализовывать master-ТЗ небольшими устойчивыми vertical slices.

---

## 2. Главные правила для ИИ-разработчика

Все правила ниже обязательны. Отклонение разрешается только после явного архитектурного решения, записанного в ADR и `PROJECT_STATE.md`, с объяснением проблемы, рассмотренных альтернатив, последствий и плана отката.

1. **Не менять стек без доказанной причины.** Первый релиз строится на C#/.NET, ASP.NET Core, Blazor, PostgreSQL, SignalR и .NET Worker Services.
2. **Не начинать с C++.** Все C++-ready runtime сначала реализуются на .NET и измеряются.
3. **Не предлагать Kubernetes, Kafka, event sourcing, full microservices, CQRS или сложный workflow engine по умолчанию.** Они отсутствуют в baseline.
4. **Не начинать visual Dashboard Editor и SVG Editor до работающего runtime.** Сначала чтение опубликованной конфигурации, затем editor-ready backend, затем визуальные редакторы.
5. **Не включать реальные физические команды до command safety gate.** В MVP допустимы только control session, backend preflight и симуляция исполнения.
6. **Не смешивать `EventRecord`, `AlarmOccurrence` и `Incident`.** Это разные сущности, таблицы, API и жизненные циклы.
7. **Не смешивать `Equipment` и `MaintenanceObject`.** Объект ТОиР может существовать без подключения к диспетчеризации.
8. **Не хранить secrets в открытом виде.** В БД и конфигурации хранится secret reference; значение не возвращается в API, audit, logs или DOM.
9. **Не считать hidden UI авторизацией.** Любой endpoint, hub subscription и прямой route повторно проверяют право и scope на backend.
10. **Не читать draft в runtime.** Runtime использует только целую опубликованную revision или заранее подготовленный immutable seed package.
11. **Не создавать все будущие проекты solution сразу.** Новый проект появляется только при наличии реальной ответственности и минимум одного работающего use case.
12. **Не делать архитектуру ради архитектуры.** Не добавлять generic repository, universal service layer, mediator abstraction, message broker или отдельный deployment unit без конкретной потребности.
13. **Каждый шаг должен собираться.** Нельзя оставлять репозиторий в состоянии с некомпилируемым кодом, незапускаемой миграцией или сломанным маршрутом.
14. **После каждого шага обновлять `PROJECT_STATE.md`.** Состояние репозитория является частью результата, а не вспомогательной заметкой.
15. **После изменения поведения обновлять документацию и тесты.** Код без отражения в state/README/API notes не считается завершенным.
16. **Сначала читать существующий код.** Не создавать дублирующие сущности, endpoint, DTO, миграции или helpers.
17. **Работать vertical slice.** Предпочитать одну завершенную функцию от БД до UI и теста вместо нескольких незавершенных слоев.
18. **Не выполнять protocol call внутри business transaction.** Polling и command transport живут за отдельной runtime boundary.
19. **SignalR не является source of truth.** Snapshot читается из committed state; realtime доставляет изменения и сигнализирует gap.
20. **Всегда сохранять `value + unit + quality + timestamp + freshness + source`.** Значение без этих атрибутов не считается достоверным operator data.
21. **Последнее известное значение не должно выглядеть актуальным после потери связи.** UI и API обязаны различать stale/offline.
22. **Использовать UTC для хранения времени.** Локализация выполняется на границе представления.
23. **Mutating operations должны быть auditable.** Для значимых действий фиксируются actor, time, target, action, before/after, reason и correlation ID.
24. **Не переносить EF entities в публичные contracts.** DTO и persistence model разделены.
25. **Не закрывать ошибку заглушкой, которая выглядит как готовая функция.** Mock/simulator допустим только при явной маркировке и тестируемом contract.
26. **Не переходить к следующему step при красной сборке.** Сначала восстановить green state либо откатить незавершенное изменение.
27. **Не добавлять future scope в текущий step.** Любое улучшение вне шага записывается в `next steps`, а не реализуется скрытно.
28. **Не удалять данные неявно.** CSV import не удаляет оборудование; archive используется вместо физического удаления бизнес-сущностей, если master-ТЗ не требует иного.
29. **Не ослаблять safety ради demo.** History, gap, stale, offline, lack of permission и expired control session блокируют command preview/execution.
30. **Фиксировать неизвестное.** Если количественное требование не определено, использовать конфигурируемое безопасное значение и отметить его в `known limitations`, не выдавая предположение за решение продукта.

---

## 3. Термины и инварианты

| Термин | Короткое значение | Нельзя смешивать с | Где живет | Обязательный инвариант |
|---|---|---|---|---|
| `Location` | Узел физической/организационной иерархии объекта | `Equipment`, protocol endpoint, permission scope как самостоятельная сущность | Asset Model; `assets` schema | Иерархия не содержит циклов; доступ фильтруется по scope |
| `Equipment` | Канонический пользовательский объект диспетчеризации | `TelemetrySource`, Modbus/SNMP address, `MaintenanceObject` | Asset Model; `assets` schema | Протокольные поля не входят в core Equipment model |
| `MaintenanceObject` | Объект технического обслуживания | `Equipment` и `DataPoint` | Maintenance; `maintenance` schema | Может существовать без telemetry и иметь optional link к Equipment |
| `DataPoint` | Описанный параметр/сигнал оборудования | `CurrentValue`, protocol register/OID, UI widget | Telemetry configuration; `telemetry` schema | Имеет тип, unit, mapping и freshness policy; идентичность не зависит от протокола |
| `TelemetrySource` | Настроенный технический источник опроса | `Equipment`, `DataPoint`, secret plaintext | Telemetry configuration; `telemetry` schema | Endpoint/config маскируются; secrets доступны только по reference |
| `CurrentValue` | Последнее принятое состояние DataPoint | `HistoricalValue`, SignalR message, raw protocol payload | Telemetry ingestion/read model | Одна актуальная запись на DataPoint с sequence, quality и timestamps |
| `HistoricalValue` | Append-only исторический sample | `CurrentValue`, audit event | Telemetry history; partitioned `telemetry` tables | Не обновляется как business record; запрос ограничен range/resolution |
| `DataQuality` | Доверие к данным и состоянию источника | Alarm severity, building event, platform health | Telemetry/Admin read models | `Good`, `Uncertain`, `Bad`, `Stale`, `Offline`, `Initializing` независимы от alarm lifecycle |
| `EventRecord` | Неизменяемая запись о факте/изменении | `AlarmOccurrence`, `Incident`, notification | Events & Alarms; `events` schema | Append-only; чтение события не выполняет acknowledgement |
| `AlarmRule` | Версионируемое правило определения alarm condition | `AlarmOccurrence`, command interlock, UI filter | Events & Alarms | Evaluation использует конкретную активную revision |
| `AlarmOccurrence` | Экземпляр состояния alarm condition во времени | `EventRecord`, `Incident`, единый `AlarmStatus` | Events & Alarms | Condition, acknowledgement, assignment, shelving и suppression меняются независимо |
| `Incident` | Координационный процесс длительной ситуации | `AlarmOccurrence`, work order, event | Incidents | Создание/закрытие Incident не изменяет condition или acknowledgement автоматически |
| `Dashboard` | Версионируемая операторская композиция | `/home`, employee page, visual editor draft | Dashboards | Runtime читает только published revision |
| `DashboardWindow` | Каноническое адресуемое окно Dashboard | UI-only string `screen`, Widget, MimicDiagram | Dashboards; runtime API | Имеет устойчивый ID/code и восстанавливается из URL |
| `MimicDiagram` | Версионируемая SVG-мнемосхема с bindings | DashboardWindow, произвольный HTML/SVG script | Dashboards & Mimics | SVG sanitization обязательна; selection не выполняет команду |
| `ControlSession` | Временный backend-authorized режим управления | Пользовательская сессия, command execution | Commanding | Имеет scope, expiry и немедленное завершение; не заменяет preflight |
| `CommandExecution` | Отслеживаемая попытка выполнить конкретную команду | ControlSession, UI click, alarm acknowledgement | Commanding | Имеет idempotency key и состояния, включая `Unknown`; успех не предполагается |
| `AuditEntry` | Неизменяемая запись значимого действия | `EventRecord`, application log | Audit | Append-only; нет update/delete API; secrets вырезаются |
| `Terminal` | Уникальная device identity панели/kiosk | User account, `KioskProfile`, dashboard assignment | Terminals | Блокировка одной панели не блокирует другие с тем же профилем |
| `KioskProfile` | Общая политика и отображение группы терминалов | Terminal identity, startup dashboard assignment | Terminals | Профиль не является credential и назначение dashboard хранится отдельно |

### 3.1. Сквозные инварианты

- `EventRecord`, `AlarmOccurrence`, `Incident`, `NotificationDelivery` и `AuditEntry` не являются разными представлениями одной таблицы.
- `EquipmentId` не обязан существовать у `MaintenanceObject`; `MaintenanceObjectId` не обязан существовать у `Equipment`.
- `DataPoint` является продуктовой сущностью, а register/OID/node/property — protocol mapping.
- Realtime update применяется только если sequence новее текущего; gap требует resnapshot.
- History не допускает acknowledgement, assignment, shelving, bulk actions, incident/request creation и commands.
- Dashboard access не автоматически разрешает все связанные DataPoints; backend фильтрует каждую композицию.
- Draft/validation/publication — разные состояния; validation относится к точной сохраненной revision.
- Command preflight выполняется на backend непосредственно перед симуляцией/исполнением и не кешируется как вечное разрешение.
- Ошибки platform health и data quality не превращаются автоматически в технологические alarms объекта.

---

## 4. Практическая стартовая структура solution

Целевая структура master-ТЗ шире стартовой. ИИ не должен создавать десятки пустых проектов. На раннем этапе bounded contexts разделяются namespace, folders, schema ownership и dependency rules внутри небольшого числа проектов. Выделение в отдельный project выполняется при появлении реального кода, самостоятельных контрактов, отдельного deployment lifecycle или измеримой причины.

### 4.1. Создать сразу

| Проект | Зачем нужен | Когда создавать | Что запрещено размещать |
|---|---|---|---|
| `Dispatcher.Api` | ASP.NET Core composition root, REST endpoints, auth middleware, SignalR registration, health | Step 1 | Domain rules в controllers, SQL, protocol clients, secret values |
| `Dispatcher.Web` | Blazor shell, routes, UI state, REST/SignalR clients | Step 1 | EF entities, direct DB, backend authorization как источник истины, protocol logic |
| `Dispatcher.Domain` | Минимальные primitives, value objects и инварианты контекстов | Step 1; наполнять с Step 3 | EF Core, HTTP, SignalR, UI labels, protocol-specific address fields |
| `Dispatcher.Application` | Use cases, interfaces, authorization context, transaction boundaries | Step 1 | Blazor components, adapter transport, direct environment access |
| `Dispatcher.Infrastructure` | EF Core/PostgreSQL, migrations, clocks, secret references, persistence adapters | Step 1 | Product decisions, UI DTO, protocol polling schedule logic |
| `Dispatcher.Contracts` | Versioned REST/SignalR DTO, error model, public enums | Step 1–2 | EF entities, internal aggregates, database-specific types |
| `Dispatcher.Telemetry.Worker` | Simulator-first polling runtime, orchestrator и ранние adapters как изолированные folders | Step 1, активная реализация с Step 11 | RBAC decisions, incidents, dashboards, maintenance logic |
| `Dispatcher.Workers` | Freshness, rule evaluation, outbox и служебные background jobs | Step 1, jobs добавлять по шагам | HTTP controllers, UI, protocol session implementation |
| `Dispatcher.UnitTests` | Unit tests для primitives и domain transitions | Step 1 | Tests, требующие реальный PostgreSQL/браузер |
| `Dispatcher.IntegrationTests` | PostgreSQL/API/SignalR integration tests | Step 1 | Долгие browser/load suites как default test run |

**Стартовая структура каталогов:**

```text
Dispatcher.sln
  src/
    Dispatcher.Api/
    Dispatcher.Web/
    Dispatcher.Domain/
      Common/
      IdentityAccess/
      Assets/
      Telemetry/
      EventsAlarms/
      Dashboards/
      Maintenance/
      Commanding/
    Dispatcher.Application/
      Abstractions/
      IdentityAccess/
      Assets/
      Telemetry/
      EventsAlarms/
      Dashboards/
      Maintenance/
      Commanding/
    Dispatcher.Infrastructure/
      Persistence/
      Identity/
      Secrets/
      Time/
      Outbox/
    Dispatcher.Contracts/
      Common/
      Identity/
      Assets/
      Telemetry/
      Events/
      Dashboards/
      Maintenance/
      Commanding/
      Realtime/
    Dispatcher.Telemetry.Worker/
      Orchestration/
      Simulation/
      Adapters/Modbus/
      Adapters/Snmp/
    Dispatcher.Workers/
      Freshness/
      Rules/
      Outbox/
  tests/
    Dispatcher.UnitTests/
    Dispatcher.IntegrationTests/
  docs/
    adr/
    runbooks/
```

### 4.2. Создавать позже, когда появится реальная необходимость

| Проект/сервис | Создавать когда | Зачем выделять | Что запрещено размещать |
|---|---|---|---|
| `Dispatcher.EventsAlarms` | Внутренний модуль стал крупным, имеет собственные migrations/tests и мешает независимой работе | Явная code ownership boundary | Incident close, notification delivery, protocol parsing |
| `Dispatcher.Dashboards` | Появились runtime revisions, composition и editor-ready backend | Версионируемый runtime context | Telemetry polling и Blazor editor monolith |
| `Dispatcher.Maintenance` | Реализуется Step 22 и объем превышает удобный folder-module | Независимый CMMS bounded context | Protocol configuration и обязательный FK только на Equipment |
| `Dispatcher.Commanding` | После Step 23, перед реальным safety-reviewed execution | Отдельная security/state-machine boundary | Решение protocol transport внутри API controller |
| `Dispatcher.Terminals` | Начат post-MVP terminal/kiosk epic | Device identity и lifecycle | Обычные user accounts вместо terminal identity |
| `Dispatcher.Telemetry.Contracts` | Modbus/SNMP runtime требуется запускать отдельным процессом или стабилизировать gRPC | C++-ready process contract | EF entities и UI DTO |
| `Dispatcher.Telemetry.Orchestrator` | Poll scheduling и lease lifecycle требуют независимого scaling/deployment | Изоляция scheduler load | Protocol payload parsing и alarm business logic |
| `Dispatcher.Telemetry.Adapters.Modbus` | Adapter запускается отдельно либо имеет самостоятельный release lifecycle | Изоляция Modbus sessions | Equipment UI/RBAC/Incident logic |
| `Dispatcher.Telemetry.Adapters.Snmp` | Adapter запускается отдельно либо имеет самостоятельный release lifecycle | Изоляция SNMP security/session model | Notification/work-order logic |
| `Dispatcher.Telemetry.Ingestion` | History/current ingestion становится самостоятельным high-throughput path | Backpressure и batch scaling | Dashboard rendering и rule authoring |
| `Dispatcher.Commanding.Gateway` | Разрешается pilot real command execution после safety gate | Техническое исполнение и idempotency | Право, second-user, business approval |
| OPC UA/BACnet adapters | После MVP и наличия реальных устройств/профилей | Новые protocol boundaries | Создание «универсального protocol model» в Equipment |
| C++ native services | Только после production/load metrics и ADR | Замена конкретного measured bottleneck | UI, API, RBAC, incidents, maintenance, dashboards, audit, publication |

### 4.3. Правило выделения нового проекта

Новый project создается только если одновременно выполнены минимум два условия:

- есть самостоятельная ответственность и понятный owner;
- есть минимум один работающий use case;
- требуется отдельный process/deployment lifecycle;
- нужен стабильный external contract;
- профиль нагрузки или security boundary существенно отличается;
- dependency graph становится проще, а не только длиннее.

Пустой project «на будущее» запрещен.

---

## 5. Repository state file

В корне репозитория должен существовать обязательный файл `PROJECT_STATE.md`. Он создается в Step 0 и обновляется после **каждого** AI-step, включая исправления, откаты и hardening.

### 5.1. Назначение

`PROJECT_STATE.md` — машиночитаемая человеком точка восстановления контекста. Следующий ИИ-агент должен по этому файлу понять, что реально существует в repository, какие миграции применены, какие endpoint/routes работают, что ограничено и какой step разрешено делать дальше.

Файл не заменяет Git history, ADR, README или issue tracker. Он фиксирует краткое текущее состояние.

### 5.2. Обязательная структура

```markdown
# PROJECT_STATE

## Product
- Name: Диспетчер
- Master specification: DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md
- AI guide: DISPATCHER_AI_IMPLEMENTATION_SPEC.md
- Stack: .NET / ASP.NET Core / Blazor / PostgreSQL / SignalR / Worker Services

## Current phase
- Phase: Foundation
- Status: In progress

## Current sprint
- Sprint: 1
- Goal: Архитектурный baseline и рабочий репозиторий

## Current step
- Step: 1
- Name: solution skeleton
- Status: Completed | In progress | Blocked
- Started: 2026-07-18T00:00:00Z
- Completed: null

## Completed steps
| Step | Date UTC | Commit | Result | Notes |
|---|---|---|---|---|

## Architecture decisions
| ADR | Decision | Status | Consequences |
|---|---|---|---|

## Created projects
| Project | Purpose | Created in step | Build status |
|---|---|---|---|

## Database status
- Provider: PostgreSQL
- Connection method: local development secret / environment variable
- Schemas created:
- Current migration:
- Clean install verified: No
- Upgrade verified: No

## Migrations
| Migration | Step | Applied locally | Roll-forward tested | Notes |
|---|---|---|---|---|

## API endpoints
| Method | Route | Authorization | Implemented in step | Tests |
|---|---|---|---|---|

## Frontend routes
| Route | Status | Authorization UX | Smoke test |
|---|---|---|---|

## Workers
| Worker/job | Host | Schedule/trigger | Health/metrics | Status |
|---|---|---|---|---|

## Known limitations
- None recorded.

## Next steps
1. Step N — ...

## Commit hash history
| Date UTC | Step | Commit hash | Message |
|---|---|---|---|
```

### 5.3. Правила обновления

- Не записывать endpoint как implemented, пока он не отвечает и не имеет хотя бы smoke/integration проверки.
- Не записывать migration как applied, пока она не применена к локальному PostgreSQL.
- Не отмечать step completed при красной сборке.
- Любой временный simulator/mock перечислять в `Known limitations`.
- Архитектурное отклонение обязательно связывать с ADR.
- Commit hash добавляется после того, как пользователь выполнил commit и прислал hash; до этого использовать `pending`.
- Следующий step должен быть конкретным и соответствовать разделу 6.

---

## 6. Порядок реализации для ИИ

### 6.1. Общий протокол выполнения step

Перед каждым step ИИ обязан:

1. прочитать master-ТЗ, этот guide, `PROJECT_STATE.md`, последние ADR и relevant code;
2. проверить текущую ветку и незакоммиченные изменения;
3. сформулировать один ограниченный результат;
4. не менять публичные contracts вне scope шага;
5. реализовать code + tests + docs;
6. выполнить checklist раздела 11;
7. обновить `PROJECT_STATE.md`;
8. выдать пользователю результат в формате раздела 12.
### Step 0 — подготовка репозитория
**Цель.** Создать воспроизводимую основу repository и зафиксировать источники истины до генерации production-кода.
**Затрагиваемые проекты.** `repository root`.
**Примерные файлы и каталоги.**
- `README.md`
- `PROJECT_STATE.md`
- `DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md`
- `DISPATCHER_AI_IMPLEMENTATION_SPEC.md`
- `.gitignore`
- `.editorconfig`
- `Directory.Build.props`
- `docs/adr/ADR-0001-architecture-baseline.md`
- `docs/runbooks/local-development.md`

**API и frontend routes.**
- API и UI routes не создаются; фиксируется будущий `/api/health/live` как первый machine endpoint.

**Проверки и тесты.**
- Проверить, что master-ТЗ и AI guide находятся в repo и читаются.
- Проверить `.gitignore` для `bin/`, `obj/`, `.vs/`, user secrets, local DB volumes и generated artifacts.
- Проверить отсутствие secret values через поиск типичных ключей `password`, `community`, `secret`, `token`.
- Проверить `git status` и сохранить исходный baseline.

**Ограничения шага.**
- Не создавать Docker/Kubernetes/messaging infrastructure, если ее нет в текущем local run requirement.
- Не переписывать master-ТЗ.

**Что записать в `PROJECT_STATE.md`.**
- Product, current phase `Preparation`, sprint 1, step 0.
- Путь к master-ТЗ и AI guide.
- ADR-0001 и принятый baseline stack.
- Known limitation: solution еще не создан.
- Next step: Step 1.

**Definition of Done.**
- Repository имеет понятный root и базовую документацию.
- `PROJECT_STATE.md` соответствует шаблону раздела 5.
- Ни один secret не закоммичен.
- Git diff содержит только ожидаемые документы и настройки.

---
### Step 1 — solution skeleton
**Цель.** Создать минимальный компилируемый .NET solution, API, Blazor Web, Worker hosts и test projects.
**Затрагиваемые проекты.** `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Telemetry.Worker`, `Dispatcher.Workers`, `Dispatcher.UnitTests`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `Dispatcher.sln`
- `src/Dispatcher.Api/Program.cs`
- `src/Dispatcher.Web/Program.cs`
- `src/Dispatcher.Telemetry.Worker/Program.cs`
- `src/Dispatcher.Workers/Program.cs`
- `tests/Dispatcher.UnitTests/SmokeTests.cs`
- `tests/Dispatcher.IntegrationTests/AssemblySmokeTests.cs`
- `Directory.Packages.props`

**API и frontend routes.**
- Добавить `GET /api/health/live` и `GET /api/health/ready`.
- Добавить frontend routes `/`, `/home`, `/not-found` как skeleton; `/` перенаправляет на `/home` после app startup.

**Проверки и тесты.**
- `dotnet restore`.
- `dotnet build --no-restore`.
- `dotnet test --no-build`.
- Запустить API и проверить `/api/health/live`.
- Запустить Web и проверить `/home` browser smoke.

**Ограничения шага.**
- Не добавлять EF Core domain entities, auth provider или protocol libraries в этом шаге.
- Не создавать отдельные микросервисы/deployment manifests.

**Что записать в `PROJECT_STATE.md`.**
- Список созданных projects и dependency directions.
- Health endpoints и route skeleton.
- Build/test result.
- Next step: Step 2.

**Definition of Done.**
- Clean checkout восстанавливается и собирается.
- API, Web и два Worker hosts запускаются.
- Tests projects подключены к solution.
- Нет пустых future context projects.

---
### Step 2 — shared contracts and project state
**Цель.** Зафиксировать общие public contracts, error model, correlation и правила обновления состояния проекта.
**Затрагиваемые проекты.** `Dispatcher.Contracts`, `Dispatcher.Application`, `Dispatcher.Api`, `Dispatcher.Web`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Contracts/Common/ApiProblemDetails.cs`
- `src/Dispatcher.Contracts/Common/PagedResponse.cs`
- `src/Dispatcher.Contracts/Common/CorrelationId.cs`
- `src/Dispatcher.Application/Abstractions/ICurrentUser.cs`
- `src/Dispatcher.Application/Abstractions/IClock.cs`
- `src/Dispatcher.Api/Middleware/CorrelationMiddleware.cs`
- `src/Dispatcher.Api/Middleware/ExceptionHandlingMiddleware.cs`
- `docs/api/error-model.md`
- `PROJECT_STATE.md`

**API и frontend routes.**
- Health endpoints начинают возвращать correlation ID header.
- Frontend получает global loading/error boundary и отображает safe error code/correlation ID.

**Проверки и тесты.**
- Unit test machine error code mapping.
- API integration test: unhandled exception превращается в Problem Details без stack trace.
- Проверить propagation входного/сгенерированного correlation ID.
- Проверить JSON contracts на отсутствие persistence types.

**Ограничения шага.**
- Не создавать универсальный `BaseEntity`, generic repository или `ServiceResult<T>` с неограниченной ответственностью.

**Что записать в `PROJECT_STATE.md`.**
- Public error model и correlation policy.
- Список созданных common contracts.
- Решение о versioning `/api` baseline.
- Next step: Step 3.

**Definition of Done.**
- Все API errors имеют единый contract.
- Correlation ID присутствует в response и logs scope.
- Web не зависит от Infrastructure/EF.
- `PROJECT_STATE.md` обновлен и build green.

---
### Step 3 — domain primitives
**Цель.** Создать минимальные типы идентичности, времени, качества и domain results, необходимые первым vertical slices.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Contracts`, `Dispatcher.UnitTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Common/EntityId.cs`
- `src/Dispatcher.Domain/Common/DomainError.cs`
- `src/Dispatcher.Domain/Common/UtcTimestamp.cs`
- `src/Dispatcher.Domain/Telemetry/DataQuality.cs`
- `src/Dispatcher.Domain/Telemetry/TypedValue.cs`
- `src/Dispatcher.Domain/Telemetry/FreshnessState.cs`
- `src/Dispatcher.Domain/Common/ConcurrencyToken.cs`

**API и frontend routes.**
- Новых бизнес-endpoints нет; contracts получают стабильные строковые/Guid IDs и ISO-8601 UTC timestamps.

**Проверки и тесты.**
- Unit tests для ID validation/equality.
- Unit tests для allowed `DataQuality` values.
- Serialization tests для UTC timestamp и typed value.
- Dependency test: Domain не ссылается на EF/ASP.NET/SignalR.

**Ограничения шага.**
- Не создавать все product entities заранее; добавлять сущности в соответствующем step.

**Что записать в `PROJECT_STATE.md`.**
- Добавленные primitives и их namespace.
- Принятое правило ID/timestamps/nullability.
- Next step: Step 4.

**Definition of Done.**
- Primitives не содержат infrastructure/UI concerns.
- Quality и freshness — разные понятия.
- Nullable warnings не подавлены глобально.
- Unit tests green.

---
### Step 4 — PostgreSQL infrastructure
**Цель.** Подключить PostgreSQL, EF Core migrations, schema ownership и проверяемый clean install.
**Затрагиваемые проекты.** `Dispatcher.Infrastructure`, `Dispatcher.Api`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Infrastructure/Persistence/DispatcherDbContext.cs`
- `src/Dispatcher.Infrastructure/Persistence/DesignTimeDbContextFactory.cs`
- `src/Dispatcher.Infrastructure/Persistence/SchemaNames.cs`
- `src/Dispatcher.Infrastructure/Persistence/Migrations/`
- `src/Dispatcher.Api/Configuration/DatabaseOptions.cs`
- `tests/Dispatcher.IntegrationTests/Database/DatabaseFixture.cs`
- `docs/runbooks/database.md`

**API и frontend routes.**
- `/api/health/ready` проверяет доступность PostgreSQL без раскрытия connection string.
- Бизнес API еще не добавляется.

**Проверки и тесты.**
- Создать пустую baseline migration.
- Применить migration к чистой БД.
- Integration test открывает transaction и выполняет простой query.
- Проверить failure mode при недоступной БД: live остается live, ready становится unhealthy.
- Проверить connection string только через environment/user secrets.

**Ограничения шага.**
- Не использовать SQLite/InMemory как замену PostgreSQL integration tests.
- Не создавать все будущие таблицы одной migration.

**Что записать в `PROJECT_STATE.md`.**
- Provider/version, schema list baseline, имя migration.
- Clean install status.
- Локальная команда применения migration.
- Next step: Step 5.

**Definition of Done.**
- Migration применима к пустой PostgreSQL.
- API startup валидирует database options.
- Secrets отсутствуют в repository.
- Ready health отражает состояние БД.

---
### Step 5 — Identity/RBAC baseline
**Цель.** Реализовать authentication baseline, users/roles/scopes, backend authorization и last-admin guard.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/IdentityAccess/UserAccount.cs`
- `src/Dispatcher.Domain/IdentityAccess/Role.cs`
- `src/Dispatcher.Domain/IdentityAccess/PermissionScope.cs`
- `src/Dispatcher.Domain/IdentityAccess/RoleAssignment.cs`
- `src/Dispatcher.Application/IdentityAccess/`
- `src/Dispatcher.Infrastructure/Persistence/Configurations/Identity/`
- `src/Dispatcher.Api/Endpoints/Identity/`
- `src/Dispatcher.Web/Authentication/`
- `docs/security/rbac.md`

**API и frontend routes.**
- `GET /api/me`.
- Baseline `/api/users`, `/api/roles`, `/api/permission-scopes`, `/api/role-assignments` для list/get/grant/revoke.
- Frontend `/home`, `/me`, `/admin/users`, `/forbidden`; navigation формируется по effective permissions.

**Проверки и тесты.**
- Positive/negative policy tests.
- Прямой API/URL для non-admin возвращает 403, даже если nav скрыт.
- Last scope admin removal блокируется.
- Session revoke/effective permission refresh baseline.
- Role assignment создает audit hook placeholder без потери transaction.

**Ограничения шага.**
- Должность/профессиональная роль не дает admin/command rights автоматически.
- Не строить плоскую checkbox-матрицу без scope/source.

**Что записать в `PROJECT_STATE.md`.**
- Seed users/roles и их назначение только для development.
- Endpoint matrix с authorization.
- Migration name и applied status.
- Known limitation выбранного auth provider.
- Next step: Step 6.

**Definition of Done.**
- Backend policy присутствует на каждом protected endpoint.
- UI-only guards не считаются security.
- Role/scope source можно объяснить через `/api/me`.
- Tests и migration green.

---
### Step 6 — Web shell baseline
**Цель.** Перенести ключевую shell-модель prototype в компонентную Blazor-архитектуру без копирования монолитного app.js.
**Затрагиваемые проекты.** `Dispatcher.Web`, `Dispatcher.Contracts`, `Dispatcher.Api`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Web/Layout/AppShell.razor`
- `src/Dispatcher.Web/Layout/GlobalHeader.razor`
- `src/Dispatcher.Web/Layout/NavigationRail.razor`
- `src/Dispatcher.Web/Layout/ContextDrawerHost.razor`
- `src/Dispatcher.Web/Components/Feedback/`
- `src/Dispatcher.Web/wwwroot/css/tokens.css`
- `src/Dispatcher.Web/wwwroot/css/shell.css`
- `src/Dispatcher.Web/Navigation/RouteCatalog.cs`

**API и frontend routes.**
- Routes `/home`, `/me`, `/settings`, `/settings/profile`, `/admin`, `/forbidden`, `/not-found`.
- API остается `/api/me` и health; shell строится на effective permissions.

**Проверки и тесты.**
- Browser smoke каждого route.
- Visual snapshot 1920×1080: один header 48px, rail 56px, одна правая overlay panel.
- Keyboard focus/skip link.
- Back/Forward между routes.
- Non-admin не видит admin nav, но backend test остается обязательным.

**Ограничения шага.**
- Не реализовывать dashboard editor, notifications, kiosk или full profile settings в этом шаге.

**Что записать в `PROJECT_STATE.md`.**
- Созданные layout components и theme tokens.
- Frontend routes/status.
- Reference prototype files, использованные как UX source.
- Next step: Step 7.

**Definition of Done.**
- Shell не расходует высоту вторым постоянным header.
- Компоненты разделены; нет одного гигантского `App.razor`.
- Loading/error/empty baseline присутствует.
- Route smoke green.

---
### Step 7 — Locations
**Цель.** Реализовать scoped hierarchy локаций как первый полноценный vertical slice от БД до UI.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Assets/Location.cs`
- `src/Dispatcher.Application/Assets/Locations/`
- `src/Dispatcher.Infrastructure/Persistence/Configurations/Assets/LocationConfiguration.cs`
- `src/Dispatcher.Api/Endpoints/Locations/`
- `src/Dispatcher.Web/Pages/Locations/`
- `tests/Dispatcher.IntegrationTests/Locations/`

**API и frontend routes.**
- `GET/POST /api/locations`, `GET/PUT /api/locations/{id}`, `POST /api/locations/{id}/move`, `POST /api/locations/{id}/archive`.
- Frontend `/locations` и `/locations/{id}`.

**Проверки и тесты.**
- Hierarchy cycle prevention.
- Scope-filtered list/tree.
- Archive не удаляет children неявно.
- ETag/concurrency conflict для update/move.
- Browser flow location list → detail → Back.

**Ограничения шага.**
- Не добавлять protocol/source fields в Location.

**Что записать в `PROJECT_STATE.md`.**
- `assets` schema tables/migration.
- Location endpoints/routes.
- Permission actions `locations.view/manage`.
- Next step: Step 8.

**Definition of Done.**
- Локации работают с PostgreSQL, API и UI.
- Пользователь вне scope не может прочитать ID прямым запросом.
- Hierarchy constraints протестированы.
- Migration и docs обновлены.

---
### Step 8 — Equipment
**Цель.** Реализовать канонический Equipment registry/card без протокольного центра и без обязательной зависимости от ТОиР.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Assets/Equipment.cs`
- `src/Dispatcher.Domain/Assets/EquipmentType.cs`
- `src/Dispatcher.Domain/Assets/AssetRelation.cs`
- `src/Dispatcher.Application/Assets/Equipment/`
- `src/Dispatcher.Api/Endpoints/Equipment/`
- `src/Dispatcher.Web/Pages/Equipment/`
- `src/Dispatcher.Web/Components/Equipment/EquipmentDrawer.razor`

**API и frontend routes.**
- `/api/equipment`, `/api/equipment/{id}`, `/api/equipment-types`, `/api/asset-relations` baseline.
- Frontend `/equipment`, `/equipment/{id}`, optional drawer deep-link.

**Проверки и тесты.**
- Location scope filtering.
- Archive behavior and stable paging/filter/sort.
- Equipment IDOR negative test.
- Protocol fields absent from core DTO/entity.
- Route/back-forward preserves selected item/filter.

**Ограничения шага.**
- Не создавать MaintenanceObject как подкласс Equipment.
- Не добавлять delete endpoint для normal use.

**Что записать в `PROJECT_STATE.md`.**
- Equipment tables/migration, API/routes, permission actions.
- Known limitation: telemetry not connected yet.
- Next step: Step 9.

**Definition of Done.**
- Equipment registry/card displays real persisted data.
- No Modbus/SNMP address in Equipment model.
- Access scope applied to all queries.
- Smoke/e2e flow green.

---
### Step 9 — DataPoints and TelemetrySource
**Цель.** Добавить protocol-neutral DataPoint и masked TelemetrySource configuration, включая safe equipment staging baseline.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Telemetry/DataPoint.cs`
- `src/Dispatcher.Domain/Telemetry/TelemetrySource.cs`
- `src/Dispatcher.Domain/Telemetry/ProtocolMapping.cs`
- `src/Dispatcher.Application/Telemetry/Configuration/`
- `src/Dispatcher.Infrastructure/Secrets/SecretReference.cs`
- `src/Dispatcher.Api/Endpoints/TelemetrySources/`
- `src/Dispatcher.Api/Endpoints/DataPoints/`
- `src/Dispatcher.Web/Pages/Equipment/EquipmentAdd.razor`
- `src/Dispatcher.Web/Pages/Equipment/EquipmentPoints.razor`

**API и frontend routes.**
- `/api/telemetry-sources`, `/api/data-points`.
- Baseline `/api/equipment-imports` staging: create session, rows, validate, apply summary; CSV/manual share one table.
- Frontend `/equipment/add`, `/equipment/{id}/points`, `/equipment/{id}/diagnostics` placeholder.

**Проверки и тесты.**
- Masked source DTO never returns secrets.
- CSV comma/semicolon and no-delete tests.
- Existing ID defaults `skip/review`; structural validation is Apply gate.
- Template/copy excludes identity/address/secrets.
- DataPoint mapping type/unit/freshness validation.

**Ограничения шага.**
- Connection/poll test does not block structurally valid Apply.
- Protocol config JSONB must have schema version and typed validation.

**Что записать в `PROJECT_STATE.md`.**
- Telemetry configuration tables and migration.
- Secret provider strategy/reference format.
- Import endpoints/routes and limitations.
- Next step: Step 10.

**Definition of Done.**
- DataPoint and TelemetrySource are distinct persisted entities.
- Import cannot delete or silently activate equipment.
- Secrets absent in API/logs/audit/DOM.
- Structural validation tests green.

---
### Step 10 — current values
**Цель.** Создать committed current-value read model и bounded history foundation до подключения реальных adapters.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Telemetry/CurrentValue.cs`
- `src/Dispatcher.Domain/Telemetry/HistoricalValue.cs`
- `src/Dispatcher.Application/Telemetry/Values/`
- `src/Dispatcher.Infrastructure/Persistence/Configurations/Telemetry/CurrentValueConfiguration.cs`
- `src/Dispatcher.Infrastructure/Persistence/Configurations/Telemetry/HistoricalValueConfiguration.cs`
- `src/Dispatcher.Api/Endpoints/Values/`
- `src/Dispatcher.Web/Components/Values/ValueDisplay.razor`

**API и frontend routes.**
- `GET /api/values/current` batch by point/equipment/location.
- `GET /api/values/history` baseline range query with limits/resolution placeholder.
- Equipment detail показывает value, unit, quality, source timestamp, receive timestamp и freshness.

**Проверки и тесты.**
- One current row per DataPoint.
- Sequence guard prevents older sample rollback.
- History append/dedup baseline.
- Range/query limit validation.
- UI snapshot для Good/Stale/Offline without simulator.

**Ограничения шага.**
- Не использовать SignalR вместо таблицы CurrentValue.
- Не считать отсутствие нового sample автоматически alarm occurrence.

**Что записать в `PROJECT_STATE.md`.**
- Current/history tables, indexes, migration.
- Value DTO contract и query limits.
- Known limitation: values seeded/test-only until Step 11.
- Next step: Step 11.

**Definition of Done.**
- Committed current snapshot читается через REST.
- Каждое значение содержит unit/quality/timestamps/freshness/source.
- Out-of-order test green.
- History query не делает unbounded full scan.

---
### Step 11 — telemetry worker simulator
**Цель.** Реализовать simulator-first polling vertical slice с нормализованными samples, source state, current upsert и history batch.
**Затрагиваемые проекты.** `Dispatcher.Telemetry.Worker`, `Dispatcher.Workers`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Telemetry.Worker/Simulation/TelemetrySimulator.cs`
- `src/Dispatcher.Telemetry.Worker/Orchestration/PollingAssignment.cs`
- `src/Dispatcher.Telemetry.Worker/Orchestration/PollScheduler.cs`
- `src/Dispatcher.Application/Telemetry/Ingestion/TelemetrySample.cs`
- `src/Dispatcher.Infrastructure/Telemetry/CurrentValueWriter.cs`
- `src/Dispatcher.Infrastructure/Telemetry/HistoryBatchWriter.cs`
- `src/Dispatcher.Api/Endpoints/TelemetryDiagnostics/`

**API и frontend routes.**
- `GET /api/telemetry/diagnostics/sources/{id}`.
- `POST /api/telemetry/diagnostics/sources/{id}/sample-poll` development/admin only.
- Existing values routes reflect simulator updates.

**Проверки и тесты.**
- Long-running simulator smoke.
- Restart does not duplicate/roll back sequence.
- Bounded queue/backpressure behavior.
- Source heartbeat and state transitions.
- History batch dedup/integration test.

**Ограничения шага.**
- Не скрывать simulator под названием production adapter.
- Не выполнять UI/business code в worker.

**Что записать в `PROJECT_STATE.md`.**
- Worker names, schedules/triggers, health and metrics.
- Simulator marker and development-only restriction.
- Samples/sec, queue depth and last success metrics names.
- Next step: Step 12.

**Definition of Done.**
- Simulator generates normalized protocol-neutral samples.
- Worker can restart safely.
- API process does not perform polling.
- Current/history values update and tests green.

---
### Step 12 — Modbus adapter baseline
**Цель.** Подключить первый реальный protocol adapter за protocol-neutral boundary, сохранив simulator для deterministic tests.
**Затрагиваемые проекты.** `Dispatcher.Telemetry.Worker`, `Dispatcher.Contracts`, `Dispatcher.Infrastructure`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Telemetry.Worker/Adapters/Modbus/ModbusAdapter.cs`
- `src/Dispatcher.Telemetry.Worker/Adapters/Modbus/ModbusMapping.cs`
- `src/Dispatcher.Telemetry.Worker/Adapters/Modbus/ModbusValueDecoder.cs`
- `src/Dispatcher.Telemetry.Worker/Adapters/Modbus/ModbusConnectionPool.cs`
- `tests/Dispatcher.UnitTests/Telemetry/ModbusValueDecoderTests.cs`
- `docs/runbooks/modbus.md`

**API и frontend routes.**
- Telemetry source config поддерживает Modbus TCP mapping в masked API.
- Diagnostics: connection test/sample poll; это informational result, не Apply gate.
- Equipment points UI показывает source diagnostics.

**Проверки и тесты.**
- Datatype/endian/scaling decoder unit tests.
- Grouped read and address range tests.
- Timeout/backoff/reconnect integration with simulator server.
- No secret or endpoint leakage beyond authorized masked fields.
- Adapter project/folder dependency test: no Incidents/Maintenance/UI references.

**Ограничения шага.**
- Не добавлять Modbus classes в Domain/Equipment DTO.
- Не реализовывать command writes в MVP adapter step.

**Что записать в `PROJECT_STATE.md`.**
- Modbus library/version, config schema version, diagnostics endpoints.
- Metrics: registers/sec, request latency, retries, active connections.
- Next step: Step 13.

**Definition of Done.**
- Modbus adapter produces the same normalized `TelemetrySample` contract as simulator.
- Timeout/reconnect does not crash worker.
- Protocol address remains outside Equipment.
- Tests and runbook green.

---
### Step 13 — SNMP adapter baseline
**Цель.** Добавить второй protocol adapter и завершить history partition/batching baseline.
**Затрагиваемые проекты.** `Dispatcher.Telemetry.Worker`, `Dispatcher.Workers`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Telemetry.Worker/Adapters/Snmp/SnmpAdapter.cs`
- `src/Dispatcher.Telemetry.Worker/Adapters/Snmp/SnmpMapping.cs`
- `src/Dispatcher.Telemetry.Worker/Adapters/Snmp/SnmpValueConverter.cs`
- `src/Dispatcher.Workers/History/HistoryPartitionJob.cs`
- `src/Dispatcher.Web/Pages/Trends/TagTrend.razor`
- `docs/runbooks/snmp.md`

**API и frontend routes.**
- SNMP v2c/v3 source configuration через `/api/telemetry-sources` с secret references.
- `GET /api/values/history` получает production-like partitioned data.
- Frontend `/trends/tag/{dataPointId}` с period/quality markers.

**Проверки и тесты.**
- SNMP typed varbind conversion.
- Auth/timeout error normalization.
- Community/auth/privacy values отсутствуют в DTO/log/audit.
- History partition creation/rollover.
- Batch ingestion/backpressure metrics.

**Ограничения шага.**
- SNMP traps, OPC UA и BACnet не входят в шаг.

**Что записать в `PROJECT_STATE.md`.**
- SNMP config schema and secret references.
- History partitions/index/retention baseline.
- Trend route/status.
- Next step: Step 14.

**Definition of Done.**
- Modbus и SNMP одновременно обновляют current/history.
- Secrets masked end-to-end.
- Partition maintenance проверяется integration test.
- Trend query bounded and route smoke green.

---
### Step 14 — SignalR snapshot/update/gap
**Цель.** Реализовать authorized realtime protocol с snapshot, monotonic sequence, gap detection и resynchronization.
**Затрагиваемые проекты.** `Dispatcher.Api`, `Dispatcher.Contracts`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Web`, `Dispatcher.Workers`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Contracts/Realtime/TelemetryMessages.cs`
- `src/Dispatcher.Api/Hubs/TelemetryHub.cs`
- `src/Dispatcher.Application/Telemetry/Realtime/AuthorizedSnapshotService.cs`
- `src/Dispatcher.Web/Realtime/TelemetryConnection.cs`
- `src/Dispatcher.Web/Realtime/RealtimeStateStore.cs`
- `src/Dispatcher.Workers/Freshness/FreshnessWorker.cs`

**API и frontend routes.**
- `/hubs/telemetry` subscribe/unsubscribe и messages `CurrentValueChanged`, `QualityChanged`, `SourceStateChanged`, `Gap`, `ResyncRequired`.
- REST snapshot остается `/api/values/current` с cursor/sequence.
- UI отображает `Live`, `Reconnecting`, `Gap`, `Resynchronizing`, `Offline`.

**Проверки и тесты.**
- Initial snapshot then updates monotonic.
- Dropped message creates visible gap.
- Reconnect performs REST resnapshot before Live.
- Duplicate/out-of-order update idempotent.
- Permission change/session revoke removes subscription.
- Commands placeholder disabled in gap/offline/history.

**Ограничения шага.**
- Не добавлять message broker без measured need.

**Что записать в `PROJECT_STATE.md`.**
- Hub route/messages, snapshot cursor contract.
- Client realtime states.
- Freshness worker and metrics.
- Next step: Step 15.

**Definition of Done.**
- SignalR не хранит истину и не заменяет snapshot.
- Last known value visibly stale/offline.
- Каждая subscription backend-authorized.
- Realtime integration tests green.

---
### Step 15 — events and alarm rules
**Цель.** Создать append-only EventRecord, versioned AlarmRule и детерминированную rule evaluation baseline.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Workers`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/EventsAlarms/EventRecord.cs`
- `src/Dispatcher.Domain/EventsAlarms/AlarmRule.cs`
- `src/Dispatcher.Domain/EventsAlarms/AlarmRuleRevision.cs`
- `src/Dispatcher.Application/EventsAlarms/Rules/`
- `src/Dispatcher.Workers/Rules/RuleEvaluationWorker.cs`
- `src/Dispatcher.Api/Endpoints/AlarmRules/`
- `src/Dispatcher.Api/Endpoints/Events/`

**API и frontend routes.**
- `GET /api/events`, `GET /api/events/{id}`.
- `/api/alarm-rules` list/get/create revision/validate/enable/disable; publication может быть restricted seed/admin baseline.
- UI пока допускает простой read-only events list skeleton.

**Проверки и тесты.**
- Threshold/hysteresis/delay unit tests.
- Bad quality policy tests.
- Exact rule revision recorded.
- Duplicate sample/restart idempotency.
- EventRecord append-only persistence.

**Ограничения шага.**
- Не использовать arbitrary scripting/workflow engine.
- Не создавать один универсальный `Alarm` aggregate.

**Что записать в `PROJECT_STATE.md`.**
- Rule/event tables and migration.
- Worker trigger and evaluation metrics.
- Known limitation authoring/publication UI absent.
- Next step: Step 16.

**Definition of Done.**
- Rule evaluator создает immutable EventRecord and deterministic decision result.
- Rules versioned; no mutable active formula without revision.
- No Incident/ack state in EventRecord.
- Unit/integration tests green.

---
### Step 16 — AlarmOccurrence lifecycle
**Цель.** Реализовать отдельную state machine alarm condition и независимые acknowledgement/assignment/shelving fields.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Workers`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/EventsAlarms/AlarmOccurrence.cs`
- `src/Dispatcher.Domain/EventsAlarms/Acknowledgement.cs`
- `src/Dispatcher.Domain/EventsAlarms/AlarmAssignment.cs`
- `src/Dispatcher.Domain/EventsAlarms/Shelving.cs`
- `src/Dispatcher.Application/EventsAlarms/Occurrences/`
- `src/Dispatcher.Api/Endpoints/AlarmOccurrences/`
- `src/Dispatcher.Workers/Rules/ShelvingExpiryWorker.cs`

**API и frontend routes.**
- `/api/alarm-occurrences` list/get/timeline.
- Nested acknowledgement, assignment, shelving endpoints и `/bulk-preflight`.
- Realtime `/hubs/events` baseline messages for occurrence changes.

**Проверки и тесты.**
- Open/update/clear idempotency.
- Clearing не acknowledges и не closes incident.
- Acknowledgement не assigns/clears.
- Shelving requires reason/until and expires.
- History mode/action authorization negatives.
- Bulk preflight returns eligible/ineligible.

**Ограничения шага.**
- Не кодировать lifecycle одним enum.
- Не удалять occurrence при clear.

**Что записать в `PROJECT_STATE.md`.**
- Occurrence/action tables/migration.
- Distinct permission actions and audit events.
- Hub messages/status.
- Next step: Step 17.

**Definition of Done.**
- EventRecord, AlarmRule и AlarmOccurrence имеют отдельные models/tables/contracts.
- Independent lifecycle tests green.
- All actions audited.
- No hidden side effects.

---
### Step 17 — `/events` UI
**Цель.** Довести Диспетчер событий до рабочего operator flow с Realtime/History, фильтрами и безопасными actions.
**Затрагиваемые проекты.** `Dispatcher.Web`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Web/Pages/Events/EventDispatcher.razor`
- `src/Dispatcher.Web/Pages/Events/EventDrawer.razor`
- `src/Dispatcher.Web/Pages/Events/EventFilters.cs`
- `src/Dispatcher.Web/Pages/Events/OccurrenceActions.razor`
- `src/Dispatcher.Web/Realtime/EventConnection.cs`
- `tests/Dispatcher.EndToEndTests/Events/ (создать при первом Playwright flow)`

**API и frontend routes.**
- Frontend `/events`, `/events/{id}`, URL query filters/view/mode.
- Использовать `/api/events`, `/api/alarm-occurrences` и `/hubs/events`.
- History route state read-only.

**Проверки и тесты.**
- Stable list: новые строки через `Новых: N`, без jump.
- Filters state/location/assignee/search restored by URL.
- History disables ack/assign/shelve/bulk/create downstream actions.
- Action dialogs require reason/comment as contract.
- Playwright `E2E-EVENT-SEM-001` и `E2E-EVENT-HISTORY-001`.

**Ограничения шага.**
- Не выполнять acknowledgement при чтении или открытии drawer.

**Что записать в `PROJECT_STATE.md`.**
- Events frontend routes/components.
- Первый Playwright project/suite при необходимости.
- Known limitations: Incident UI еще отсутствует.
- Next step: Step 18.

**Definition of Done.**
- Realtime и History визуально различимы.
- Condition/ack/assignment columns independent.
- Direct event route restores shell+drawer.
- Critical Playwright flows green.

---
### Step 18 — dashboard catalog/runtime
**Цель.** Реализовать catalog и addressable DashboardWindow runtime на immutable published seed/revision data.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Dashboards/Dashboard.cs`
- `src/Dispatcher.Domain/Dashboards/DashboardWindow.cs`
- `src/Dispatcher.Domain/Dashboards/DashboardRevision.cs`
- `src/Dispatcher.Application/Dashboards/Runtime/`
- `src/Dispatcher.Api/Endpoints/Dashboards/`
- `src/Dispatcher.Web/Pages/Dashboards/DashboardCatalog.razor`
- `src/Dispatcher.Web/Pages/Dashboards/DashboardRuntime.razor`
- `src/Dispatcher.Web/Components/Dashboards/WindowSelector.razor`

**API и frontend routes.**
- `/api/dashboards`, `/api/dashboards/{id}/windows`, `/api/dashboard-context` baseline.
- Frontend `/dashboards`, `/d/{dashboard}/{screen}`.
- Catalog first; last available dashboard behavior и explicit `Все дашборды`.

**Проверки и тесты.**
- Runtime reads only published pointer/immutable seed.
- Back/Forward restores exact window.
- Inaccessible dashboard/window fallback.
- Live/History and period always visible.
- Playwright `E2E-NAV-001`, `E2E-DASH-URL-001`, `E2E-DASH-TIME-001`.

**Ограничения шага.**
- Не создавать Dashboard Editor.
- Не использовать arbitrary widget-to-widget hidden chains.

**Что записать в `PROJECT_STATE.md`.**
- Dashboard/window/revision tables and seed publication.
- Routes, published pointer rule, favorites/last route baseline.
- Known limitation: no visual editor/publication UI.
- Next step: Step 19.

**Definition of Done.**
- `DashboardWindow` — persisted entity, not UI string.
- Runtime cannot access draft rows.
- Canonical URL restores exact context.
- Catalog/runtime tests green.

---
### Step 19 — static mimic runtime
**Цель.** Добавить безопасную static SVG mnemonic, bindings и единый selected-object context.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Dashboards/MimicDiagram.cs`
- `src/Dispatcher.Domain/Dashboards/MimicBinding.cs`
- `src/Dispatcher.Application/Dashboards/Mimics/`
- `src/Dispatcher.Infrastructure/Dashboards/SvgSanitizer.cs`
- `src/Dispatcher.Api/Endpoints/Mimics/`
- `src/Dispatcher.Web/Components/Mimics/MimicRuntime.razor`
- `src/Dispatcher.Web/State/SelectedContextStore.cs`
- `src/Dispatcher.Web/Components/Context/EquipmentFaceplate.razor`

**API и frontend routes.**
- `GET /api/mimics/{id}` published SVG/bindings.
- `GET /api/dashboard-context` returns selected equipment, values, trend and linked events.
- Runtime remains `/d/{dashboard}/{screen}` with mimic/combined window.

**Проверки и тесты.**
- SVG script/external reference sanitization.
- Selecting symbol synchronizes highlight, faceplate, trend and events.
- First click only selects, never commands.
- History blocks command preview.
- Fullscreen/zoom and route restore.
- Playwright `E2E-DASH-CONTEXT-001`, `E2E-CMD-SELECT-001`.

**Ограничения шага.**
- Не вставлять raw SVG with script via `MarkupString` без sanitizer.
- Не выполнять command on symbol click.

**Что записать в `PROJECT_STATE.md`.**
- Mimic revision/binding tables, sanitizer policy.
- Selected context state rules.
- Known limitation: no SVG editor/wheel-pan advanced runtime if deferred.
- Next step: Step 20.

**Definition of Done.**
- Unsafe SVG content rejected/sanitized.
- Selected context is explicit and stable.
- Runtime reads published mimic only.
- Critical UX tests green.

---
### Step 20 — audit and admin health
**Цель.** Сделать значимые действия доказуемыми и дать admin отдельную картину состояния компонентов.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.Workers`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Audit/AuditEntry.cs`
- `src/Dispatcher.Application/Audit/IAuditWriter.cs`
- `src/Dispatcher.Infrastructure/Audit/AuditWriter.cs`
- `src/Dispatcher.Application/Admin/Health/`
- `src/Dispatcher.Workers/Admin/ComponentHealthCollector.cs`
- `src/Dispatcher.Api/Endpoints/Audit/`
- `src/Dispatcher.Api/Endpoints/AdminHealth/`
- `src/Dispatcher.Web/Pages/Admin/Audit.razor`
- `src/Dispatcher.Web/Pages/Admin/Health.razor`

**API и frontend routes.**
- Read-only `/api/audit`.
- `/api/health/components` и machine `/api/health/live|ready`.
- Frontend `/admin/audit`, `/admin/health`, `/admin/settings` baseline.

**Проверки и тесты.**
- Audit append-only; update/delete endpoint absent/method not allowed.
- Secret redaction in before/after.
- Health source/worker failures do not create building alarms.
- Admin direct URL/API authorization.
- Correlation links action → audit.

**Ограничения шага.**
- Не использовать application logs как audit substitute.

**Что записать в `PROJECT_STATE.md`.**
- Audit/health tables/migrations and retention/index baseline.
- Covered action list and gaps.
- Health metrics/runbook links.
- Next step: Step 21.

**Definition of Done.**
- Critical mutating use cases produce AuditEntry in same committed workflow/outbox strategy.
- Audit query scoped/read-only.
- Platform health separate from `/events`.
- Tests green.

---
### Step 21 — data quality
**Цель.** Материализовать отдельный data quality/admin workflow и централизовать freshness/quality semantics.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.Workers`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Telemetry/DataQualityIssue.cs`
- `src/Dispatcher.Application/Telemetry/DataQuality/`
- `src/Dispatcher.Workers/DataQuality/DataQualityScanner.cs`
- `src/Dispatcher.Api/Endpoints/DataQuality/`
- `src/Dispatcher.Web/Pages/Admin/DataQuality.razor`
- `src/Dispatcher.Web/Components/Values/QualityBadge.razor`

**API и frontend routes.**
- `/api/data-quality/issues` list/get/assign/resolve/ignore with reason.
- Frontend `/admin/data-quality` и links к source/equipment/DataPoint.
- Existing value/event/dashboard screens use unified quality rendering.

**Проверки и тесты.**
- Good/Uncertain/Bad mapping per adapter outcome.
- Initializing, Stale, Offline thresholds with fake clock.
- Future/old timestamps, wrong datatype/unit/mapping.
- Issue assignment does not acknowledge alarm.
- Playwright `E2E-QUALITY-001` and admin health separation.

**Ограничения шага.**
- Не создавать platform errors как alarms автоматически.

**Что записать в `PROJECT_STATE.md`.**
- Quality issue table/migration, scanner schedule and metrics.
- Configured thresholds marked as provisional if product values unknown.
- Next step: Step 22.

**Definition of Done.**
- Quality states consistent across API/UI.
- Last-known values never look fresh after threshold.
- Data quality issue lifecycle separate from alarm lifecycle.
- Tests green.

---
### Step 22 — maintenance baseline
**Цель.** Реализовать минимальный независимый MaintenanceObject/WorkOrder vertical slice и связи с Equipment/Event без side effects.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.Workers`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Maintenance/MaintenanceObject.cs`
- `src/Dispatcher.Domain/Maintenance/WorkRequest.cs`
- `src/Dispatcher.Domain/Maintenance/WorkOrder.cs`
- `src/Dispatcher.Domain/Maintenance/ChecklistSnapshot.cs`
- `src/Dispatcher.Application/Maintenance/`
- `src/Dispatcher.Api/Endpoints/Maintenance/`
- `src/Dispatcher.Web/Pages/Maintenance/`
- `src/Dispatcher.Workers/Maintenance/OverdueWorker.cs`

**API и frontend routes.**
- `/api/maintenance/objects`, `/api/maintenance/requests`, `/api/maintenance/work-orders` baseline и checklist action.
- Frontend `/maintenance`, `/maintenance/assets`, `/maintenance/requests`, `/maintenance/work-orders`, `/maintenance/work-orders/{id}`.
- Optional link from event/equipment; no automatic acknowledgement.

**Проверки и тесты.**
- Standalone MaintenanceObject without Equipment.
- WorkRequest from event preserves occurrence condition/ack.
- Required checklist blocks submit/complete transition.
- Forecast/order/personal task distinct IDs if forecast baseline added.
- Playwright `E2E-MAINT-ASSET-001`, `E2E-MAINT-CHECK-001`, `E2E-MAINT-EVENT-001`.

**Ограничения шага.**
- Не привязывать WorkOrder только к Equipment.
- Не строить generic workflow engine.

**Что записать в `PROJECT_STATE.md`.**
- Maintenance tables/migration, routes and transitions.
- Optional equipment/event links.
- Known limitations: no full CMMS/resources/procedure designer.
- Next step: Step 23.

**Definition of Done.**
- Maintenance module works without TelemetrySource.
- State transitions explicit/audited.
- No event acknowledgement side effect.
- Critical tests green.

---
### Step 23 — command preflight simulation
**Цель.** Подготовить безопасную command architecture, не изменяя физическое оборудование.
**Затрагиваемые проекты.** `Dispatcher.Domain`, `Dispatcher.Application`, `Dispatcher.Infrastructure`, `Dispatcher.Contracts`, `Dispatcher.Api`, `Dispatcher.Web`, `Dispatcher.Workers`, `Dispatcher.IntegrationTests`.
**Примерные файлы и каталоги.**
- `src/Dispatcher.Domain/Commanding/ControlSession.cs`
- `src/Dispatcher.Domain/Commanding/CommandDefinition.cs`
- `src/Dispatcher.Domain/Commanding/CommandExecution.cs`
- `src/Dispatcher.Application/Commanding/Preflight/`
- `src/Dispatcher.Application/Commanding/Simulation/`
- `src/Dispatcher.Api/Endpoints/ControlSessions/`
- `src/Dispatcher.Api/Endpoints/Commands/`
- `src/Dispatcher.Web/Components/Commanding/ControlModeIndicator.razor`
- `src/Dispatcher.Web/Components/Commanding/CommandPreview.razor`

**API и frontend routes.**
- `/api/control-sessions` create/get/terminate.
- `/api/commands/definitions`, `/api/commands/preflight`, `/api/commands/executions` simulation only.
- Optional `/hubs/commands` for status.
- Command UI only in Live trusted context and active control session.

**Проверки и тесты.**
- Control off/on/expired and dashboard scope.
- Unauthorized, stale/offline/gap/history/interlock/expected-version blocks.
- Reason/confirmation/re-auth/second-user policy states.
- Duplicate idempotency key.
- Simulated completed/error/unknown; unknown never shown as success.
- Logout/revoke/gap terminates session; no retry after reconnect.

**Ограничения шага.**
- Не создавать C++ DLL/PInvoke.
- Не подключать adapter write methods к UI.
- Не сообщать success до подтвержденного result state.

**Что записать в `PROJECT_STATE.md`.**
- Command/control tables/migration, endpoint matrix and audit phases.
- Feature flag/policy `PhysicalExecutionEnabled=false`.
- Safety findings/known limitations.
- Next step: Step 24.

**Definition of Done.**
- No physical transport call exists in production path.
- Every preview comes from backend preflight.
- Control session is temporary and scoped.
- Command-off tests and audit green.

---
### Step 24 — MVP hardening
**Цель.** Закрыть MVP gates, отказоустойчивость, безопасность, производительность baseline и pilot runbooks.
**Затрагиваемые проекты.** `all existing projects`, `Dispatcher.EndToEndTests`, `optional Dispatcher.LoadTests when first load profile exists`.
**Примерные файлы и каталоги.**
- `tests/Dispatcher.EndToEndTests/`
- `tests/Dispatcher.LoadTests/`
- `docs/runbooks/backup-restore.md`
- `docs/runbooks/telemetry-recovery.md`
- `docs/runbooks/operator-support.md`
- `docs/security/threat-review.md`
- `RELEASE_NOTES.md`
- `PROJECT_STATE.md`

**API и frontend routes.**
- Новых feature API/routes без gate necessity не добавлять.
- Все MVP routes проходят smoke; health/metrics endpoints готовы для operation.

**Проверки и тесты.**
- Clean install and upgrade migration.
- Backup/restore rehearsal.
- Full unit/integration/API/SignalR/Playwright critical suite.
- IDOR/secret/session revoke/security matrix.
- Soak/fault: adapter down, DB restart, reconnect storm, backlog recovery.
- Performance baseline with documented pilot profile.
- 1920×1080 UX review, keyboard/accessibility.

**Ограничения шага.**
- Не начинать post-MVP editors/notifications/kiosk до formal MVP acceptance.

**Что записать в `PROJECT_STATE.md`.**
- Все completed steps с commit hashes.
- Final endpoint/route/worker/migration inventory.
- MVP known limitations and explicit non-MVP list.
- Acceptance gates status with evidence.
- Next step: Sprint 17 post-MVP only after approval.

**Definition of Done.**
- All MVP-critical gates in section 16 passed.
- Critical/high defects closed or explicitly accepted by owner.
- Release package reproducible.
- Runbooks, migrations, backup/restore and monitoring verified.
- Physical command execution remains disabled.

---
## 7. Sprint-to-step mapping

Один двухнедельный sprint состоит из нескольких маленьких AI-steps. AI-step не обязан занимать весь sprint и не должен искусственно растягиваться. Спринт считается закрытым только по DoD master-ТЗ; завершение одного AI-step не означает автоматическое завершение sprint.

| Sprint | Цель | AI steps | MVP-critical |
|---|---|---|---|
| 1 | Архитектурный baseline и рабочий репозиторий | Steps 0–4 | Да |
| 2 | Identity, roles, scopes и shell | Steps 5–6 | Да |
| 3 | Locations и базовая модель оборудования | Steps 7–8 | Да |
| 4 | Equipment staging и DataPoint configuration | Step 9, часть Step 10 | Да |
| 5 | Modbus TCP polling vertical slice | Steps 10–12 | Да |
| 6 | SNMP polling и history ingestion | Step 13 | Да |
| 7 | Quality, freshness и realtime resynchronization | Step 14 и foundation Step 21 | Да |
| 8 | Alarm rules, EventRecord и AlarmOccurrence | Steps 15–16 | Да |
| 9 | Диспетчер событий и операторские действия | Step 17 | Да |
| 10 | Incidents baseline и «Моя работа» | Отдельный follow-up vertical slice после Step 17; допускается внутри Sprint 10 до Dashboard runtime, если master backlog требует | Да по master-ТЗ; в Steps 0–24 минимальный фокус — alarm/event core, поэтому Incident/My Work необходимо закрыть как подшаг 17A перед окончательным MVP gate |
| 11 | Dashboard catalog и runtime windows | Step 18 | Да |
| 12 | Static mimic, selected context и trends | Step 19 | Да |
| 13 | Audit, platform health и data quality | Steps 20–21 | Да |
| 14 | Maintenance baseline | Step 22 | Да |
| 15 | Control mode и command preflight preview | Step 23 | Да |
| 16 | MVP hardening и пилотная приемка | Step 24 | Да |
| 17 | In-app notifications и персональные настройки | Post-MVP Steps 25+; создавать после MVP acceptance | Нет |
| 18 | Publication workflow и editor-ready backend | Post-MVP Steps 25+; backend revisions before visual editors | Нет |

### 7.1. Обязательный подшаг 17A — Incidents и My Work

Поскольку список Steps 0–24 во втором задании не выделяет отдельный номер для master Sprint 10, ИИ обязан выполнить **Step 17A** до окончательного закрытия MVP:

- `Incident` создается из Event/Occurrence без изменения condition/acknowledgement;
- `/api/incidents`, `/api/incidents/{id}/links`, `/api/my-work`, `/api/my-work/{id}/actions` baseline;
- frontend `/incidents/{id}` и `/my-work`;
- My Work является projection, а не владельцем source state;
- tests: incident creation preserves occurrence; transfer/return recount; inaccessible source hidden;
- `PROJECT_STATE.md` фиксирует Step 17A как отдельную completed запись.

Это уточнение не меняет архитектуру и необходимо для соответствия master-ТЗ и Sprint 10.

---

## 8. MVP scope для ИИ

### 8.1. В MVP обязательно

ИИ должен считать MVP только сквозной промышленный baseline, в котором реальные данные проходят от source/simulator до operator UI, alarms и diagnostics:

- единая shell: header, rail, context drawer, loading/error/forbidden states;
- authentication и RBAC baseline с role + scope + backend authorization;
- `Location` hierarchy;
- `Equipment` registry/card;
- safe equipment staging/import без delete;
- `DataPoint`;
- `TelemetrySource` с masked config и secret references;
- current values и ограниченная history;
- value/unit/quality/timestamps/freshness/source;
- simulator-first polling;
- Modbus TCP baseline;
- SNMP baseline;
- SignalR snapshot/update/gap/resync;
- `EventRecord`;
- versioned `AlarmRule`;
- `AlarmOccurrence` lifecycle;
- acknowledgement, assignment, shelving baseline;
- `/events` Realtime/History UI;
- minimal `Incident` и `/my-work` согласно Step 17A;
- dashboard catalog и published runtime;
- persisted `DashboardWindow` и canonical URL;
- static sanitized mimic runtime;
- selected object → faceplate/trend/events;
- append-only audit;
- admin health;
- data quality issues;
- minimal independent `MaintenanceObject`, request и `WorkOrder`;
- temporary `ControlSession`;
- backend command preflight;
- **command execution simulation only**;
- PostgreSQL migrations, backup/restore baseline, health, metrics, runbooks и automated critical tests.

### 8.2. Не делать в MVP

- full visual Dashboard Editor;
- SVG Editor;
- реальное физическое command execution;
- C++ runtime или native DLL;
- OPC UA production adapter;
- BACnet production adapter;
- production edge gateway;
- full CMMS: ресурсы, материалы, сложные процедуры, полноценная оптимизация планирования;
- full notification escalation и внешние каналы;
- production kiosk security/enrollment;
- mobile offline application;
- report designer;
- arbitrary scripting/rule DSL;
- complex workflow engine;
- full microservices decomposition;
- Kafka/Kubernetes/event sourcing/CQRS infrastructure;
- автоматическое удаление оборудования при CSV import;
- visual publication workflows до editor-ready backend Sprint 18.

### 8.3. Правило simulator-first

MVP не должен ждать доступности реального оборудования. Каждый protocol vertical slice сначала подтверждается deterministic simulator. После этого подключается реальный adapter, а simulator остается в automated tests. Simulator не считается заменой production adapter и всегда помечается как development/test component.

---

## 9. C++ future extraction guide

### 9.1. Как проектировать C#-сервисы для возможной замены

1. Первая реализация всегда C#/.NET Worker Service.
2. Потенциально заменяемый runtime изолируется процессом или ясным application interface, но не требует отдельного repository/deployment на первом шаге.
3. Внешние contracts должны быть protocol-neutral, versioned и не содержать EF entities, UI labels, RBAC, incidents или work-order states.
4. Business decisions остаются в ASP.NET/Application layer. Native runtime получает минимальное assignment и возвращает технический результат.
5. Для process extraction предпочтительны versioned protobuf/gRPC control/streaming contracts после появления отдельного процесса. P/Invoke/C++ DLL внутри ASP.NET API запрещены.
6. До предложения C++ измеряются .NET implementation, PostgreSQL, serialization, batching, allocations, GC, network и backpressure.
7. Любая extraction требует ADR с current metrics, target SLO, profile evidence, alternatives, migration/rollback plan и contract compatibility tests.

### 9.2. Стабилизируемые контракты

- `PollingAssignment`: source ID, adapter type/version, endpoint secret reference, point mappings, interval, timeout, lease, config revision.
- `TelemetrySample`: DataPoint ID, typed value, unit, quality, source/receive timestamps, source ID, sequence, diagnostics.
- `SourceState`: source ID, connection state, last success/error, error class, latency, config revision.
- `TelemetryBatch` / `AckCursor`: ordered batches, dedup cursor, rejection reasons and backpressure.
- `CommandRequest`: execution ID, idempotency key, target mapping, payload, deadline, expected state/version, short-lived policy token.
- `CommandResult`: execution ID, phase, protocol evidence, timestamps, verification and error class.
- `RuleEvaluationInput/Result`: point/value/quality/timestamps, exact rule revision, previous state and deterministic transition result.

### 9.3. Future extraction candidates

| Candidate | Первая реализация | Стабильная граница | Метрики для решения | C++ может обсуждаться только если |
|---|---|---|---|---|
| Telemetry Polling Runtime | .NET Worker, async I/O, bounded channels | PollingAssignment, lease/heartbeat, PollResult, SourceState | p99 poll drift, samples/sec/core, CPU, allocations, GC pause, backlog | Оптимизированный .NET runtime не выдерживает согласованный load/SLO |
| Modbus Adapter | .NET Worker/library | protocol-neutral PollBatch/Sample and CommandTransport result | registers/sec, p99 latency, connections, CPU per 1000 registers, retry rate | Есть большой measured connection/register load или edge constraint |
| SNMP Adapter | .NET Worker | PollBatch/Sample/SourceState; trap envelope later | OIDs/sec, packets/sec, timeout, memory/session, p99 cycle | High-frequency polling/trap storm измеримо превышает baseline |
| OPC UA Adapter | Post-MVP .NET Worker | SubscriptionAssignment/Sample/SourceState | monitored items, notifications/sec, publish lag, reconnect, memory | Реальный OPC UA profile доказал bottleneck/edge need |
| BACnet Adapter | Post-MVP .NET Worker | Discovery/Poll/Sample/CommandTransport | devices/objects, discovery duration, packets/sec, p99 response | Реальная сеть и discovery нагрузка измерены |
| Edge Gateway | .NET Worker bundle with encrypted store | Registration, ConfigSnapshot, TelemetryBatch, AckCursor | RAM/disk, startup, buffered samples, recovery time, CPU | Target hardware/footprint не выдерживает .NET после optimization |
| Command Execution Gateway | .NET Worker, physical execution disabled until gate | CommandRequest/CommandResult | p99 dispatch, concurrency, timeout/unknown, idempotency conflicts | Very-low latency/high concurrency реально необходимы; не ради security |
| History Ingestion | .NET Worker + Npgsql batching | TelemetryBatch/AckCursor | samples/sec, ingest p99, backlog age, allocation, DB saturation | Schema/partition/index/batch tuning выполнены, bottleneck остается в CPU/copy |
| Rule Evaluation Engine | .NET Worker compiled plans | RuleEvaluationInput/Result | evaluations/sec, p99 lag, CPU/allocations, partition skew | Миллионы eval/sec или unacceptable lag подтверждены профилем |

### 9.4. Когда запрещено предлагать C++

C++ запрещено предлагать:

- до завершения .NET baseline и load profile;
- для UI, REST API, RBAC, users, incidents, maintenance, dashboards, audit, configuration publishing и user workflows;
- чтобы «повысить безопасность» command path без measured runtime need;
- из-за предположения, что native всегда быстрее;
- до анализа PostgreSQL, network, batching, allocation и serialization bottleneck;
- если replacement потребует совместного memory lifecycle с ASP.NET через DLL/PInvoke;
- если нет contract tests и rollback plan;
- если текущая проблема — неправильная архитектура, unbounded queue, N+1 queries, плохие indexes или excessive JSON.

### 9.5. Признаки потенциальной необходимости C++

- sustained CPU выше согласованного порога при целевой нагрузке после profiling/optimization;
- p99 poll/evaluation/dispatch drift превышает SLO;
- allocation/GC pauses остаются dominant bottleneck;
- target edge hardware не удовлетворяет footprint/startup/power constraints;
- serialization/copy dominates history ingestion после DB tuning;
- throughput per core экономически неприемлем при подтвержденном large-scale deployment.

Числовые пороги устанавливаются pilot load profile. До этого они являются hypotheses, а не архитектурным решением.

---

## 10. Архитектурные запреты

ИИ должен воспринимать этот раздел как список **НЕ ДЕЛАТЬ**.

1. Не делать один `Alarm` enum/entity для EventRecord, condition, acknowledgement, assignment, incident и close.
2. Не хранить Modbus register, SNMP OID, host, Unit ID или credentials в `Equipment`.
3. Не использовать SignalR как source of truth.
4. Не разрешать UI выполнять command без backend preflight.
5. Не считать disabled/hidden button авторизацией.
6. Не делать DTO равными EF entities.
7. Не возвращать EF navigation graph из API.
8. Не хранить secrets в JSON/JSONB, appsettings, audit, logs или browser storage.
9. Не удалять оборудование при CSV import.
10. Не активировать existing imported equipment без явного privileged update.
11. Не делать `DashboardWindow` просто строкой в UI.
12. Не читать dashboard/mimic draft в operator runtime.
13. Не публиковать несохраненный или измененный после validation draft.
14. Не обновлять runtime частями одной revision.
15. Не привязывать `WorkOrder` только к `Equipment`.
16. Не делать `MaintenanceObject` подклассом `Equipment`.
17. Не делать Terminal обычным user account.
18. Не использовать shared `KioskProfile` как device identity.
19. Не создавать C++ DLL внутри ASP.NET API.
20. Не вызывать protocol libraries из controller/Blazor component.
21. Не удерживать DB transaction во время protocol/network call.
22. Не выполнять polling в API process request lifecycle.
23. Не создавать distributed transaction между context/runtime.
24. Не вводить event sourcing ради audit/history.
25. Не вводить Kafka/message broker до measured need; transactional outbox не равен event sourcing.
26. Не создавать отдельную БД на каждый context до MVP.
27. Не создавать generic repository поверх EF Core без конкретной проблемы.
28. Не создавать `Dispatcher.Shared`/Common как свалку всех models/helpers.
29. Не использовать arbitrary JSONB для status, permissions, core relations и lifecycle.
30. Не хранить local time без timezone как canonical timestamp.
31. Не принимать unbounded history/list query.
32. Не применять out-of-order realtime update поверх более нового state.
33. Не переходить в `Live` после reconnect без snapshot/resync.
34. Не показывать last known value как fresh.
35. Не считать repeated constant value автоматически Good.
36. Не смешивать data quality issues с building alarms.
37. Не подтверждать alarm при чтении notification/event.
38. Не изменять acknowledgement при создании Incident/WorkRequest.
39. Не выполнять actions в History.
40. Не отправлять command первым кликом по mimic symbol.
41. Не повторять command автоматически после reconnect.
42. Не считать transmitted command успешным при потерянном result; использовать `Unknown`.
43. Не создавать visual editors до runtime/publication contracts.
44. Не копировать весь prototype `app.js` в один Blazor component/service.
45. Не создавать все целевые solution projects заранее.
46. Не маскировать незавершенный backend fake data без явного development marker.
47. Не подавлять nullable warnings глобально.
48. Не ловить и игнорировать `CancellationToken`/shutdown.
49. Не коммитить код с красным build/test.
50. Не отмечать step completed без обновления `PROJECT_STATE.md`.

---

## 11. Проверки после каждого шага

Каждый AI-step заканчивается одинаковым обязательным checklist. Неприменимый пункт отмечается `N/A` с причиной; он не удаляется молча.

### 11.1. Build и tests

```powershell
dotnet restore
```

```powershell
dotnet build --no-restore
```

```powershell
dotnet test --no-build
```

Дополнительно:

- выполнить targeted unit/integration tests текущего slice;
- при изменении PostgreSQL создать migration с осмысленным именем;
- применить migration к локальной БД;
- при изменении существующей migration history проверить upgrade from previous state;
- не редактировать уже примененную shared migration без явной причины.

### 11.2. Runtime smoke

- `/api/health/live` отвечает success;
- `/api/health/ready` отражает реальные зависимости;
- новый endpoint отвечает happy path и минимум один negative path;
- новый frontend route открывается напрямую и через navigation;
- Back/Forward не повторяет mutating action;
- Worker запускается, корректно останавливается и отражается в health/metrics;
- SignalR change проверяется snapshot → update → reconnect/gap, если применимо.

### 11.3. Security и data safety

- backend authorization присутствует;
- IDOR negative test добавлен для entity endpoint;
- secret values отсутствуют в source, config, API response, logs, audit и DOM;
- input validation и size/range limits присутствуют;
- mutating action имеет audit/correlation requirements;
- History/gap/offline/stale restrictions не ослаблены.

### 11.4. Documentation/state

- `PROJECT_STATE.md` обновлен;
- README/local runbook обновлены, если команды запуска изменились;
- API/route/worker/migration inventory обновлен;
- known limitations записаны явно;
- broken file/project references отсутствуют;
- suggested commit message подготовлен;
- после commit пользователь должен прислать commit hash для добавления в state history.

### 11.5. Минимальная команда проверки ссылок и secrets

Конкретные utilities могут отличаться, но проверка должна быть однострочной. Пример для PowerShell без внешних зависимостей:

```powershell
Get-ChildItem -Recurse -File | Select-String -Pattern 'password\s*[:=]|community\s*[:=]|secret\s*[:=]|token\s*[:=]' -CaseSensitive:$false
```

Результаты должны быть просмотрены вручную: совпадение в документации или типе `SecretReference` не равно утечке.

---

## 12. Формат выдачи каждого шага ИИ

После выполнения каждого step ИИ отвечает пользователю кратко и одинаково. Полный code dump в чат не нужен, если создан patch/archive/files.

### 12.1. Обязательные блоки ответа

1. **Что реализовано** — 3–8 предложений, без маркетинга.
2. **Измененные/созданные файлы** — сгруппированный список.
3. **Архив или patch** — ссылка на созданный artifact либо точный patch.
4. **PowerShell команды для Windows** — только однострочные команды.
5. **Команды проверки** — restore/build/test/migration/API/UI smoke.
6. **Git команды** — status/add/commit; push только если пользователь просил.
7. **Просьба прислать commit hash** — для фиксации в `PROJECT_STATE.md` следующей итерацией.
8. **Known limitations** — только фактические ограничения текущего результата.

### 12.2. Шаблон ответа

````markdown
## Step N завершен: <название>

Реализовано: ...

### Файлы
- `path/file.cs` — ...

### Артефакт
- `<link to patch/archive>`

### PowerShell
```powershell
dotnet restore
```
```powershell
dotnet build --no-restore
```
```powershell
dotnet test --no-build
```

### Git
```powershell
git status
```
```powershell
git add .
```
```powershell
git commit -m "feat(step-N): <imperative summary>"
```

После commit пришлите hash, чтобы добавить его в `PROJECT_STATE.md`.

### Known limitations
- ...
````

### 12.3. Правила PowerShell

- Одна команда — одна строка.
- Не использовать сложные многострочные pipelines.
- Не использовать Unix-only syntax (`&&`, `export`, `rm -rf`) в блоке Windows commands.
- Пути с пробелами брать в двойные кавычки.
- Не включать реальные credentials в команды.
- Migration command должен указывать startup/project явно, если это требуется solution layout.
- Команды должны быть копируемыми из repository root.

---

## 13. Файловая и naming convention

### 13.1. Namespace и project boundaries

- Root namespace: `Dispatcher`.
- Namespace повторяет project и feature: `Dispatcher.Application.Telemetry.Values`.
- Не использовать папки `Helpers`, `Utils`, `Managers`, `Misc` без узкой ответственности.
- Feature folders предпочтительнее технических свалок: `Equipment/GetEquipment`, а не `Services/EquipmentService`.
- Domain не ссылается на Application/Infrastructure/API/Web.
- Contracts не ссылаются на Infrastructure или EF.
- Web использует Contracts/client abstractions.

### 13.2. Файлы и классы

- Один primary public type на файл.
- Имя файла совпадает с primary type.
- Use case naming: `CreateEquipmentUseCase`, `CreateEquipmentHandler`, `GetEquipmentUseCase`; не вводить CQRS/Mediator framework только ради организации файлов.
- Validator: `CreateEquipmentRequestValidator`.
- EF configuration: `EquipmentConfiguration`.
- Endpoint module: `EquipmentEndpoints` или feature-specific static mapping class.
- Worker: `FreshnessWorker`, job: `HistoryPartitionJob`.

### 13.3. DTO naming

- Input: `CreateEquipmentRequest`, `UpdateEquipmentRequest`, `AcknowledgeOccurrenceRequest`.
- Output: `EquipmentResponse`, `EquipmentSummaryResponse`, `CurrentValueResponse`.
- Realtime: `CurrentValueChangedMessage`, `GapMessage`.
- Не использовать `Dto` как единственное различие между разными shapes; имя должно отражать purpose.
- Не повторять entity graph в response.

### 13.4. Endpoint naming

- Collection nouns: `/api/equipment`, `/api/data-points`, `/api/alarm-occurrences`.
- Lifecycle actions моделировать subresources, когда это отдельная сущность: `/acknowledgements`, `/assignments`, `/shelvings`.
- Command-like endpoint допустим для transition/preflight: `/move`, `/archive`, `/bulk-preflight`, `/close`.
- Не использовать verbs вроде `/getEquipment`.
- Breaking public contract получает versioning, а не скрытое изменение.

### 13.5. Frontend route naming

- Lowercase kebab-case segments.
- Entity ID является route parameter: `/equipment/{id}`.
- Устойчивый view/filter/time state помещается в URL/query.
- Drawer entity может иметь direct route, но восстанавливает общую shell.
- Hash routing остается только свойством reference prototype; production routes server-supported.

### 13.6. Migration naming

Формат: `<Verb><Feature><Purpose>`, например:

- `CreateIdentityBaseline`;
- `AddLocationHierarchy`;
- `CreateTelemetryCurrentValues`;
- `PartitionHistoricalValues`;
- `AddAlarmOccurrenceActions`.

Не использовать `Migration1`, `UpdateDb`, `Fix` без смысла. Applied migration не переписывается ради косметики.

### 13.7. Enum/status naming

- Enum type — singular PascalCase: `DataQuality`, `OccurrenceConditionState`.
- Values — PascalCase: `Good`, `Stale`, `Cleared`.
- Разные жизненные циклы имеют разные enums: `OccurrenceConditionState`, `AcknowledgementState`, `IncidentStatus`, `WorkOrderStatus`.
- Не использовать общий `Status` без context.
- Публичные string values фиксируются contract tests.

### 13.8. IDs

- Каждый bounded context владеет ID своего aggregate.
- Внутри .NET предпочтительны strongly typed IDs/value types, если они не создают excess boilerplate; public JSON может использовать UUID/string.
- Не использовать protocol address как entity ID.
- Не переиспользовать `EquipmentId` как `MaintenanceObjectId`.
- Correlation ID и Idempotency Key — разные значения.

### 13.9. Timestamps

- Хранение: UTC (`DateTimeOffset`/PostgreSQL `timestamptz`).
- Имя содержит смысл: `SourceTimestampUtc`, `ReceivedAtUtc`, `AcknowledgedAtUtc`, `ExpiresAtUtc`.
- Не использовать неуточненное `DateTime.Now`.
- Business logic получает `IClock`, чтобы freshness/expiry тестировались fake clock.

### 13.10. Cancellation tokens

- Любой async I/O public method принимает `CancellationToken` последним параметром.
- Endpoint передает `HttpContext.RequestAborted`.
- Worker использует `stoppingToken`.
- Не заменять cancellation общим timeout без propagation.
- Не глотать `OperationCanceledException` как error при normal shutdown.

### 13.11. Nullable

- `<Nullable>enable</Nullable>` для всех projects.
- Не использовать null для обязательного lifecycle state.
- Optional relation явно моделируется nullable ID/option и валидируется use case.
- `default!` допустим только в controlled persistence/serialization boundary с объяснением.

### 13.12. Error model

Public error — RFC Problem Details compatible:

- `type`/machine code;
- `title`;
- HTTP status;
- safe detail;
- `correlationId`;
- field errors;
- optional retryability/precondition metadata.

Не возвращать stack trace, SQL, endpoint credential, protocol secret или internal exception message.

### 13.13. Commit naming

Рекомендуемый формат:

- `chore(step-0): initialize repository baseline`;
- `feat(step-7): add scoped location hierarchy`;
- `fix(step-14): resnapshot after realtime gap`;
- `test(step-16): cover independent alarm lifecycle`;
- `docs(step-24): add pilot recovery runbook`.

---

## 14. API and route implementation order

Порядок ниже обязателен. Полный target API master-ТЗ не означает, что все endpoint создаются сразу.

| Priority | AI step | Backend API/hub | Frontend routes | Причина порядка |
|---|---:|---|---|---|
| 1 | 1–4 | `/api/health/live`, `/api/health/ready`, global errors | `/home`, error/forbidden skeleton | Проверяемый runtime и PostgreSQL foundation |
| 2 | 5–6 | `/api/me`, users/roles/scopes/assignments baseline | `/home`, `/me`, `/admin/users`, `/settings/profile` baseline | Access до данных |
| 3 | 7 | `/api/locations` | `/locations`, `/locations/{id}` | Первый scoped domain slice |
| 4 | 8 | `/api/equipment`, types, relations | `/equipment`, `/equipment/{id}` | Канонический asset model |
| 5 | 9 | `/api/telemetry-sources`, `/api/data-points`, `/api/equipment-imports` | `/equipment/add`, equipment points/diagnostics | Конфигурация до polling |
| 6 | 10–13 | `/api/values/current`, `/api/values/history`, telemetry diagnostics | equipment values, `/trends/tag/{id}` | Committed data path до realtime |
| 7 | 14 | `/hubs/telemetry`, snapshot cursor | realtime states in existing routes | Gap-safe delivery после source of truth |
| 8 | 15–16 | `/api/events`, `/api/alarm-rules`, `/api/alarm-occurrences` and actions, `/hubs/events` | read-only events skeleton | Alarm semantics до complex UI |
| 9 | 17 | Existing event APIs, bulk preflight | `/events`, `/events/{id}` | Рабочий operator flow |
| 10 | 17A | `/api/incidents`, `/api/my-work` | `/incidents/{id}`, `/my-work` | Separate coordination after alarm semantics |
| 11 | 18 | `/api/dashboards`, windows, dashboard context | `/dashboards`, `/d/{dashboard}/{screen}` | Runtime после trusted data/events |
| 12 | 19 | `/api/mimics`, context trend/events | same dashboard runtime | Static visual context after dashboard URL model |
| 13 | 20–21 | `/api/audit`, `/api/health/components`, `/api/data-quality/issues` | `/admin/audit`, `/admin/health`, `/admin/data-quality`, `/admin/settings` | Operational diagnostics before pilot |
| 14 | 22 | maintenance objects/requests/work-orders | `/maintenance/*` MVP routes | Independent CMMS baseline after asset/event links |
| 15 | 23 | control sessions, definitions, preflight, simulated executions, `/hubs/commands` optional | command preview in dashboard/mimic | Safety comes after reliable observation |
| 16 | 24 | No scope expansion; harden all | Smoke all MVP routes | Pilot gate |
| 17 | post-MVP | notifications/personal settings | `/notifications`, `/settings/notifications/*` | Sprint 17 |
| 18 | post-MVP | revisions/validation/impact/publications | admin revision screens, later editors | Sprint 18; runtime remains first |

### 14.1. Endpoint completion rule

Endpoint считается implemented только если:

- contract documented;
- backend authorization present;
- validation and Problem Details implemented;
- PostgreSQL behavior tested where applicable;
- audit requirement decided;
- positive and negative API test exists;
- `PROJECT_STATE.md` inventory updated.

### 14.2. Route completion rule

Route считается implemented только если:

- direct navigation works;
- loading/empty/error/forbidden states exist;
- backend data is real or simulator explicitly marked;
- Back/Forward behavior tested;
- scope loss/inaccessible entity handled safely;
- route is in state inventory and smoke test.

---

## 15. Testing plan для ИИ

### 15.1. Практический порядок

1. **Build/smoke first.** На каждом step restore/build/start/health/route smoke.
2. **Domain unit tests.** Инварианты, state transitions, quality/freshness, value decoding, preflight matrix.
3. **PostgreSQL integration tests.** Migrations, constraints, scoped queries, transactions, partitions, outbox/idempotency.
4. **API tests.** Auth, validation, Problem Details, IDOR, concurrency, audit.
5. **SignalR tests.** Snapshot/update/sequence/gap/resync/permission revoke.
6. **Playwright critical flows.** Shell, access, import, events, dashboard URL/context, maintenance, command-off.
7. **Load/fault tests.** Только после существования production-like flow и пилотного load profile.

Нельзя начинать load test пустого API ради красивого числа RPS. Тест должен моделировать source count, DataPoints, poll interval, history retention, connected users и subscriptions.

### 15.2. Первые 10 тестов

| № | Тест | Уровень | Реализовать в | Почему первый |
|---:|---|---|---|---|
| 1 | Solution clean restore/build/test | Smoke | Step 1 | Предотвращает накопление broken state |
| 2 | API live/ready health with PostgreSQL available/unavailable | Integration | Step 4 | Отделяет process health от dependency readiness |
| 3 | Non-admin direct admin API/route returns forbidden | API/Playwright | Step 5–6 | Hidden UI не является authorization |
| 4 | Last scope admin cannot be removed | Domain/Integration | Step 5 | Критический RBAC invariant |
| 5 | Location hierarchy cycle rejected and scope filtered | Domain/API | Step 7 | Первый scoped asset invariant |
| 6 | Equipment DTO/entity has no protocol address and IDOR is blocked | Architecture/API | Step 8 | Защищает product model и access |
| 7 | CurrentValue ignores duplicate/older sequence and preserves timestamps/quality | Domain/PostgreSQL | Step 10 | Data trust foundation |
| 8 | Secret values absent in TelemetrySource API/log/audit | API/Security | Step 9–13 | Критический security invariant |
| 9 | SignalR gap triggers resnapshot before Live | Integration/Playwright | Step 14 | Realtime не становится source of truth |
| 10 | Alarm clear, acknowledgement, assignment and Incident creation are independent | Domain/API | Steps 16–17A | Главная семантика продукта |

### 15.3. Следующая волна critical tests

- CSV import does not delete; existing defaults skip/review.
- Stale/offline value visually non-current.
- Event History disables actions.
- Dashboard Back/Forward restores exact window.
- Runtime cannot read draft.
- SVG sanitizer blocks script/external references.
- MaintenanceObject exists without Equipment.
- Required checklist blocks WorkOrder transition.
- Audit has no mutating endpoint.
- Command preflight blocks history/gap/offline/expired control; physical execution disabled.

### 15.4. Test data

- Test fixtures не должны скрывать authorization: создавать минимум admin/operator/engineer и две location scopes.
- Использовать deterministic fake clock для freshness/expiry.
- Использовать simulator для Modbus/SNMP failure cases.
- PostgreSQL tests должны изолироваться transaction/schema/database strategy.
- Secret fixtures используют фиктивные значения и проверяют redaction.
- E2E data package versioned и воспроизводим.

### 15.5. Load/fault baseline

После Step 24 измерять минимум:

- DataPoints and sources;
- poll intervals;
- samples/sec;
- p50/p95/p99 poll drift;
- current/history write latency;
- SignalR connected clients/subscriptions/fan-out;
- worker backlog age/queue depth;
- alarm evaluation lag;
- CPU, memory, allocations, GC pause;
- DB locks/index/partition behavior;
- recovery after adapter/API/DB restart.

Результаты записываются в `docs/performance/<date>-baseline.md` и `PROJECT_STATE.md`.

---

## 16. Acceptance gates

| Gate | Что доказывает | Закрывающие steps | Обязательное evidence |
|---|---|---|---|
| Data trust gate | Значения имеют unit/quality/timestamps/freshness/source и stale не выглядит fresh | 3, 9–14, 21, 24 | Unit/integration/realtime/UI quality tests |
| Alarm semantics gate | EventRecord, AlarmOccurrence, Incident и actions независимы | 15–17, 17A, 24 | State-machine tests and Playwright semantics |
| Access gate | Role+scope enforced backend; hidden UI not security | 5–8, 14, 17–24 | Policy/IDOR/direct route/API matrix |
| Audit gate | Significant actions append-only, scoped, redacted | 5, 9, 16, 20–23, 24 | Audit integration tests and read-only API |
| Publication gate | Runtime reads only complete published revision; draft isolated | 18–19; full lifecycle post-MVP Sprint 18 | Runtime pointer/draft isolation tests |
| Telemetry gate | Simulator/Modbus/SNMP normalize samples with backpressure/metrics | 10–14, 24 | Adapter contracts, soak and restart tests |
| Recovery gate | Restart/gap/backup/restore do not silently corrupt state | 11, 14–16, 20–21, 24 | Resnapshot, idempotency, backup/restore, fault tests |
| Command-off gate | Physical execution disabled; preflight blocks unsafe context | 14, 19, 23, 24 | Feature flag, negative matrix, no adapter write path |
| QA gate | Critical user flows automated and release build reproducible | 6–24 | Unit/integration/API/SignalR/Playwright suite green |
| Operational gate | Health, metrics, logs, runbooks, migrations and recovery ready | 1, 4, 11–14, 20–21, 24 | Health/metrics inventory, runbooks, pilot rehearsal |

### 16.1. Gate status в `PROJECT_STATE.md`

Добавить раздел:

```markdown
## Acceptance gates
| Gate | Status | Evidence | Blocking issue |
|---|---|---|---|
| Data trust | In progress | ... | ... |
```

Статусы: `Not started`, `In progress`, `Passed`, `Blocked`. `Passed` без ссылки на test/runbook/commit запрещен.

---

## 17. Как использовать исходный Core UI prototype

Файлы `index.html`, `styles.css`, `app.js`, `README.md`, `DECISIONS.md`, `REVIEW.md` являются **reference prototype**, а не production codebase.

### 17.1. Что брать из prototype

- layout: один compact global header, navigation rail, рабочая область и одна правая context panel;
- UX behavior: catalog/last dashboard, drawer, selected object, Live/History, stable event list, control mode indication;
- terminology: «Диспетчер событий», «Моя работа», «Моя страница», «Настройки», DashboardWindow semantics;
- route expectations и Back/Forward behavior;
- dark theme tokens, spacing, header/rail dimensions и operator density;
- loading/empty/error/disabled explanation patterns;
- `DECISIONS.md` как источник зафиксированных решений;
- `REVIEW.md` как traceability checklist для Playwright/manual QA;
- demo scenarios для seed/test data.

### 17.2. Что не копировать механически

- один большой `app.js` как один Blazor component/service/state object;
- localStorage как production persistence/source of truth;
- hash routing в production;
- fixture data как business backend;
- simulated commands как реальное execution;
- client-side authorization;
- hard-coded users/roles/secrets;
- inline HTML strings и unsafe SVG rendering;
- глобальный mutable state без feature ownership;
- prototype limitations, которые master-ТЗ относит к будущей industrial implementation.

### 17.3. Рекомендуемое разбиение Blazor

- `Layout`: header, rail, drawer host;
- `Pages/<Feature>`: route components;
- `Components/<Feature>`: reusable view components;
- `Clients`: typed REST clients;
- `Realtime`: hub connections and sequence/resync state;
- `State`: small feature-scoped state stores;
- `wwwroot/css/tokens.css`: theme variables;
- `wwwroot/css/components/*.css`: component styles;
- Domain/business rules остаются на backend.

### 17.4. REVIEW traceability

Каждый relevant пункт `REVIEW.md` получает:

- test ID, если автоматизируем;
- manual UX checklist item, если зависит от восприятия;
- target sprint/step;
- current status/evidence.

Не следует пытаться автоматизировать субъективную понятность интерфейса pixel assertions. Но размеры shell, наличие/отсутствие controls, route behavior и state transitions должны быть automated.

---

## 18. Финальная инструкция

ИИ-разработчик всегда действует по следующему правилу:

> **Если возникает противоречие между быстрым кодом и архитектурным инвариантом — выбрать инвариант. Если возникает желание добавить технологию — сначала доказать необходимость. Если задача слишком большая — разбить на меньший vertical slice.**

Перед началом каждого шага:

1. прочитать master-ТЗ, AI guide и `PROJECT_STATE.md`;
2. проверить существующее состояние repository;
3. назвать текущий step и его DoD;
4. реализовать только необходимый slice;
5. выполнить проверки;
6. обновить state/docs;
7. передать patch/artifact, commands, limitations и запросить commit hash.

Нельзя компенсировать отсутствие понимания дополнительной технологией. Нельзя ускорять разработку смешиванием сущностей. Нельзя считать код готовым только потому, что он скомпилировался: готовность включает semantics, authorization, persistence, tests, audit, health и документацию.

---

## Приложение A. Краткая стартовая команда для нового ИИ-агента

```text
Прочитай DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md, DISPATCHER_AI_IMPLEMENTATION_SPEC.md и PROJECT_STATE.md. Определи текущий незавершенный step. Не меняй архитектуру и стек. Выполни только этот step как минимальный vertical slice, обнови tests/docs/PROJECT_STATE.md, проверь restore/build/test/health/routes и выдай patch, однострочные PowerShell-команды, known limitations и запрос commit hash.
```

## Приложение B. Критерий остановки

ИИ должен остановить расширение scope текущего шага, когда:

- DoD шага закрыт;
- build/tests green;
- state/docs updated;
- оставшиеся идеи относятся к следующему step;
- следующая функция потребует нового bounded context, technology или migration вне текущей цели.

Остановиться после завершенного slice предпочтительнее, чем оставить несколько наполовину реализованных функций.
