# «Диспетчер»: техническое задание и дорожная карта промышленной разработки

**Статус:** рабочая архитектурно-техническая спецификация  
**Назначение:** основание для планирования, декомпозиции backlog, проектирования solution и запуска промышленной разработки  
**Язык:** русский  
**Целевой стек первого промышленного релиза:** C#/.NET, ASP.NET Core, Blazor, PostgreSQL, SignalR, .NET Worker Services  
**Исходные материалы:** продуктовая концепция, WEB_INTERFACE_SPECIFICATION, Core UI prototype (`index.html`, `styles.css`, `app.js`), `README.md`, `DECISIONS.md`, `REVIEW.md`

> Документ не переопределяет продукт. Он переводит зафиксированную продуктовую и UX/UI-модель в архитектуру, состав модулей, MVP, roadmap, sprint plan, модель данных, API, маршруты и критерии приемки.

## Содержание

1. [Executive summary](#1-executive-summary)
2. [Целевой продуктовый контур](#2-целевой-продуктовый-контур)
3. [Bounded contexts](#3-bounded-contexts)
4. [Архитектура solution](#4-архитектура-solution)
5. [C++-ready architecture](#5-c-ready-architecture)
6. [Первый промышленный MVP](#6-первый-промышленный-mvp)
7. [Roadmap](#7-roadmap)
8. [Sprint plan](#8-sprint-plan)
9. [Модель данных](#9-модель-данных)
10. [API map](#10-api-map)
11. [Frontend routes](#11-frontend-routes)
12. [Testing and acceptance](#12-testing-and-acceptance)
13. [Риски и спорные места](#13-риски-и-спорные-места)
14. [Итоговые рекомендации](#14-итоговые-рекомендации)

---

## 1. Executive summary

«Диспетчер» — промышленная web-платформа верхнего уровня для наблюдения, разрешенного управления, обработки событий и аварий, координации инцидентов и технической эксплуатации распределенных объектов. Пользователь работает не с протоколами и контроллерами, а с локациями, оборудованием, точками данных, ситуациями, работами и доступным контекстом.

Проект нельзя проектировать как простую SCADA по следующим причинам:

- SCADA-представление телеметрии является только одним контуром продукта; рядом существуют независимые контуры инцидентов, ТОиР, персональной работы, уведомлений, публикации конфигураций, терминалов и администрирования;
- событие, `AlarmOccurrence` и `Incident` имеют разные жизненные циклы и не могут храниться как одна «авария со статусом»;
- наличие права просмотра не дает права управления; каждая команда проходит самостоятельный preflight и исполняется только в ограниченном по времени control mode;
- текущая величина без `unit`, `quality`, `timestamp`, `freshness` и источника недостоверна для оператора;
- операторский runtime использует только целую опубликованную revision, а не текущий черновик редактора;
- объект ТОиР может существовать без телеметрии, поэтому ТОиР не является вкладкой карточки устройства;
- kiosk/terminal имеет device identity и профиль панели, а не обычную пользовательскую учетную запись;
- platform health и data quality описывают состояние самой платформы и потока данных, но не должны создавать ложные технологические аварии здания.

Проект также нельзя строить как обычную CRUD-систему. Основная сложность находится не в формах справочников, а в непрерывной обработке данных, семантике качества, realtime-доставке, безопасных переходах состояния, правах с областью действия, атомарной публикации и доказуемом аудите.

### 1.1. Рекомендуемый архитектурный старт

Первый промышленный релиз следует строить как **модульный монолит бизнес-функций** с несколькими отдельными runtime-процессами:

1. **ASP.NET Core API** — авторизация, бизнес-use cases, REST API, SignalR, контроль транзакций и доступ к бизнес-модулям.
2. **Blazor Web client** — единая shell и пользовательские рабочие области, без бизнес-авторизации на стороне браузера. Рекомендуемый hosting profile — Blazor Web App с интерактивным WebAssembly-клиентом, использующим отдельные REST/SignalR contracts; долгоживущий server-side circuit не должен быть единственным способом работы операторского runtime.
3. **Telemetry runtime и protocol adapters** — отдельные .NET Worker Services с заранее стабильными внешними контрактами.
4. **Background workers** — alarm/rule evaluation, история, outbox, планировщики и служебные задачи.
5. **PostgreSQL** — единый кластер на MVP, разделенный схемами и правами по bounded contexts; нативное партиционирование для истории.

Проекты solution и deployment units не должны автоматически становиться микросервисами. Выделяются только те процессы, у которых действительно различаются жизненный цикл, профиль нагрузки, требования к изоляции или будущая возможность замены реализации.

### 1.2. Главная последовательность реализации

Порядок разработки:

1. зафиксировать термины, контракты и архитектурные границы;
2. реализовать identity, RBAC и shell;
3. реализовать локации, оборудование, точки данных и источники;
4. получить нормализованную телеметрию Modbus TCP и SNMP;
5. реализовать current values, quality/freshness, history и SignalR;
6. добавить alarm rules, events и alarm occurrences;
7. построить Диспетчер событий и минимальные incidents;
8. подключить dashboard/mimic runtime только к опубликованным revision;
9. добавить базовый ТОиР, аудит, platform health и data quality;
10. только после стабилизации наблюдения включать реальное управление оборудованием;
11. редакторы, расширенный CMMS, production kiosk и сложные уведомления развивать после MVP;
12. вопрос C++ решать по метрикам промышленной эксплуатации, а не заранее.

### 1.3. Критические инварианты

Следующие правила должны проверяться архитектурой, API и тестами:

- protocol-specific поля не проникают в пользовательскую доменную модель;
- последнее значение при потере связи не выглядит актуальным;
- чтение персонального уведомления не подтверждает alarm occurrence;
- acknowledgement не назначает ответственного и не закрывает incident;
- создание incident или work request из события не меняет acknowledgement;
- History является read-only для технологических действий;
- первый клик по объекту выбирает контекст, но не отправляет команду;
- скрытый UI-элемент никогда не заменяет backend authorization;
- черновик, сохранение, validation и publication являются разными действиями;
- publication применяется атомарно и только к проверенной сохраненной revision;
- импорт оборудования не удаляет существующие объекты и не активирует их неявно;
- terminal profile, terminal assignment и device identity являются независимыми сущностями.

---

## 2. Целевой продуктовый контур

### 2.1. Диспетчеризация

Назначение — дать оператору достоверную картину объекта от уровня организации и локации до оборудования и точки данных.

Состав:

- физическая и функциональная структура локаций;
- реестр оборудования и технические связи;
- точки данных и источники телеметрии;
- current values и history;
- качество, свежесть, связь и агрегированное состояние;
- dashboard runtime, widgets, trends и mimic runtime;
- realtime updates с явным отображением разрыва связи.

Ключевой результат: оператор понимает **что происходит, где, насколько данным можно доверять и когда они обновились**.

### 2.2. Управление ситуациями

Назначение — разделить регистрацию факта, аварийное состояние, реакцию и координацию длительной ситуации.

Состав:

- `EventRecord` как зафиксированный факт;
- `AlarmRule` и `AlarmOccurrence` как состояние контролируемого условия;
- acknowledgement, assignment, clearing, shelving и suppression;
- `Incident` как отдельный процесс координации;
- связь с локацией, оборудованием, параметрами, инструкциями и работами;
- «Моя работа» как персональная проекция назначенных действий.

Ключевой результат: система не скрывает различия между условием, реакцией оператора и завершением организационного процесса.

### 2.3. Эксплуатация / ТОиР

Назначение — вести объекты обслуживания, планы, прогнозы, заявки, дефекты, наряды и контроль выполнения.

Состав:

- независимый `MaintenanceObject`;
- maintenance plans и календарный прогноз;
- work requests / defects;
- work orders и переходы состояния;
- процедуры, чек-листы и измерения;
- связь с оборудованием и событиями без обязательной зависимости от телеметрии;
- персональные задания исполнителей.

Ключевой результат: ТОиР остается отдельным bounded context и может обслуживать физические объекты без устройства диспетчеризации.

### 2.4. Дашборды и мнемосхемы

Назначение — представлять рабочий контекст в виде адресуемых и версионируемых операторских экранов.

Состав:

- `Dashboard`;
- `DashboardWindow` — каноническая сущность окна/экрана;
- `Widget`;
- `MimicDiagram`;
- режимы `Live` и `History`;
- выбранный объект, time range и связанные компоненты;
- runtime, отделенный от Dashboard Editor и SVG Editor;
- draft, validation, impact preview, publication, versions и rollback.

Ключевой результат: оператор всегда видит опубликованную целую revision, а редактор не меняет runtime до публикации.

### 2.5. Администрирование

Назначение — управлять учетными записями, назначениями ролей, областями доступа, интеграциями, терминалами, системными политиками и состоянием платформы.

Состав:

- accounts, roles, permission scopes и effective permissions;
- admin-only `/admin/*`;
- system settings и policy overrides;
- connections/integrations;
- terminal registry;
- platform health;
- data quality;
- read-only technical audit.

Ключевой результат: административная роль отделена от должности сотрудника и не предоставляет технологическое управление автоматически.

### 2.6. Персональный контур пользователя

Назначение — дать сотруднику рабочую идентичность, персональную очередь и настройки без смешивания с системным администрированием.

Состав:

- `/home` — оперативный Рабочий стол;
- `/me` и `/users/{userId}` — страницы сотрудников;
- `/my-work` — единая персональная очередь;
- `/notifications` — inbox доставленных сообщений;
- `/settings/*` — профиль, интерфейс, безопасность, избранное и настройки уведомлений;
- avatar menu как компактный вход в личные маршруты.

Ключевой результат: чтение, персонализация и доступный контекст не изменяют права и состояния исходных бизнес-сущностей.

### 2.7. Kiosk / wallboard / terminals

Назначение — обеспечить фиксированные панели и терминалы с отдельной идентичностью устройства и управляемым профилем безопасности.

Состав:

- `Terminal` и device identity;
- `KioskProfile`;
- назначенный dashboard/window;
- режим только просмотра или control policy;
- enrollment, block, revoke и rebind;
- kiosk runtime без обычной shell;
- wallboard playlists как последующее расширение.

Ключевой результат: терминал не маскируется под пользователя, а действия при необходимости атрибутируются одновременно устройству и сотруднику.

### 2.8. Интеграции и источники данных

Назначение — подключать технические источники, нормализовать данные и изолировать протокольную специфику.

Состав:

- `TelemetrySource`;
- polling schedules и connection diagnostics;
- Modbus TCP и SNMP в MVP;
- OPC UA и BACnet позже;
- edge gateway для удаленных площадок;
- command execution gateway;
- history ingestion и rule evaluation;
- интеграции уведомлений, отчетов и внешних систем в последующих релизах.

Ключевой результат: protocol adapters являются заменяемыми техническими компонентами, а не ядром пользовательской модели.

---

## 3. Bounded contexts

### 3.1. Сводная карта

| Context | Назначение | MVP | Основной риск |
|---|---|---:|---|
| Identity & Access | Аутентификация, роли, область и effective permissions | Да | Скрытые или противоречивые права |
| Locations & Asset Model | Локации, оборудование, связи и классификация | Да | Смешивание физической и функциональной иерархий |
| Telemetry | Источники, polling, values, quality, freshness, history | Да | Нагрузка и недостоверные stale values |
| Commanding | Control mode, preflight, выполнение и результат команды | Частично | Опасное или неоднозначное управление |
| Events & Alarms | EventRecord, rules, occurrences и операторские действия | Да | Смешивание разных жизненных циклов |
| Incidents | Координация длительной ситуации | Базово | Преждевременный workflow engine |
| Dashboards & Mimics | Runtime, окна, виджеты, мнемосхемы и revisions | Runtime | Слишком ранний visual editor |
| Maintenance / CMMS | Объекты, планы, заявки и наряды | Базово | Превращение в чрезмерно широкий CMMS |
| Notifications | Policies, deliveries, inbox, schedules и coverage | In-app базово | Сложность эскалаций и каналов |
| Reports & Documents | Отчеты, документы, инструкции, shift log | Позже | Неопределенные форматы и объем хранения |
| Admin & Platform Health | Настройки, connections, health и data quality | Да | Смешивание технических проблем с alarms |
| Audit & Configuration Publishing | Неизменяемый аудит и атомарная публикация revisions | Да, базово | Неполный след изменений |
| Kiosk / Terminals | Device identity, profile, enrollment и kiosk runtime | Модель/позже | Компрометация общей панели |

### 3.2. Identity & Access

**Назначение.** Управлять учетными записями, аутентификацией, ролями, областями и вычислением effective permissions для каждого запроса.

**Основные сущности.** `User`, `Role`, `Permission`, `PermissionScope`, `RoleAssignment`, `Session`, `EmployeeProfile`.

**Основные use cases.**

- вход и завершение сеанса;
- получение текущего пользователя и его доступного контекста;
- назначение роли на организацию, локацию, дисциплину или набор объектов;
- impact preview перед изменением назначения;
- защита последнего администратора области;
- повторная авторизация для чувствительного действия;
- проверка доступа к прямому URL и каждой API-операции.

**API.** `/api/auth`, `/api/me`, `/api/users`, `/api/roles`, `/api/permission-scopes`, `/api/access/effective`, `/api/sessions`.

**Background workers.** Очистка истекших сессий; синхронизация с внешним identity provider при его подключении; пересчет materialized effective permissions только как оптимизация, а не источник истины.

**Realtime events.** `AccessChanged`, `SessionRevoked`, `CurrentUserProfileChanged`.

**UI routes.** `/me`, `/users/{userId}`, `/settings/profile`, `/settings/security`, `/admin/users`, `/admin/roles`.

**MVP.** Простые роли, назначения на локацию/дисциплину, серверная policy-проверка и аудит изменений.

**Риски сложности.** Комбинаторный рост правил; расхождение UI и backend; автоматическое предоставление прав из должности; невозможность объяснить источник effective permission.

### 3.3. Locations & Asset Model

**Назначение.** Представлять физический и функциональный контекст, оборудование и именованные связи, не привязывая домен к протоколам.

**Основные сущности.** `Location`, `Equipment`, `EquipmentType`, `AssetRelation`, `DataPoint`, `MaintenanceObjectLink`.

**Основные use cases.**

- ведение дерева локаций;
- ведение функциональных групп и дисциплин;
- создание, обновление, архивирование оборудования;
- просмотр карточки и агрегированного состояния;
- staging ручного добавления и CSV;
- шаблоны без identity/address/secrets;
- создание связей «расположено в», «питает», «обслуживает», «связано с».

**API.** `/api/locations`, `/api/equipment`, `/api/equipment-types`, `/api/asset-relations`, `/api/equipment-imports`.

**Background workers.** Пересчет агрегированного состояния локаций; проверка целостности и циклов связей; применение валидных staging rows.

**Realtime events.** `EquipmentStateChanged`, `LocationAggregateChanged`, `AssetModelPublished`.

**UI routes.** `/locations`, `/equipment`, `/equipment/{id}`, `/equipment/add`.

**MVP.** Да: локации, equipment registry/card, связи первого уровня и универсальный staging для Modbus TCP/SNMP.

**Риски сложности.** Попытка построить единственное дерево для всех связей; протокольные поля в `Equipment`; неявные удаления при импорте.

### 3.4. Telemetry

**Назначение.** Планировать опрос, получать и нормализовать значения, определять качество/свежесть, хранить current/history и раздавать realtime.

**Основные сущности.** `TelemetrySource`, `DataPoint`, `PollingAssignment`, `CurrentValue`, `HistoricalValue`, `DataQuality`, `SourceConnectionState`.

**Основные use cases.**

- конфигурация источника и точки;
- connection test и sample poll как диагностика;
- периодический Modbus TCP/SNMP polling;
- нормализация типа, unit, timestamp, source timestamp и quality;
- вычисление freshness/stale/offline;
- запись current value с optimistic version;
- пакетная запись истории;
- snapshot после reconnect;
- запрос текущих и исторических данных.

**API.** `/api/telemetry-sources`, `/api/data-points`, `/api/values/current`, `/api/values/history`, `/api/telemetry/diagnostics`.

**Background workers.** Poll scheduler; adapter runners; current value ingestion; history batch writer; freshness evaluator; retention/partition maintenance.

**Realtime events.** `CurrentValueChanged`, `DataPointQualityChanged`, `SourceConnectionChanged`, `RealtimeGapDetected`, `RealtimeResynchronized`.

**UI routes.** `/equipment/{id}/points`, `/trends`, `/trends/tag/{tag}`, dashboard runtime, `/admin/data-quality`.

**MVP.** Да: Modbus TCP, SNMP, current/history, quality/freshness, SignalR.

**Риски сложности.** Poll drift, storm после reconnect, запись каждой величины отдельной транзакцией, неверная семантика source timestamp, неограниченная история.

### 3.5. Commanding

**Назначение.** Безопасно подготовить, авторизовать, передать и отследить технологическую команду, не смешивая это с визуальным элементом UI.

**Основные сущности.** `ControlSession`, `CommandDefinition`, `CommandRequest`, `CommandPreflight`, `CommandExecution`, `CommandResult`, `CommandInterlock`.

**Основные use cases.**

- включить control mode на ограниченную область и время;
- выполнить preflight по правам, связи, quality, freshness, блокировкам и политике;
- запросить reason, confirmation, re-auth или second user;
- передать команду gateway;
- получить состояния `accepted`, `transmitted`, `confirmed`, `completed`, `error`, `unknown`;
- немедленно отключить управление при timeout, logout, scope change, realtime gap или revoke;
- аудит каждого шага.

**API.** `/api/control-sessions`, `/api/commands/definitions`, `/api/commands/preflight`, `/api/commands/executions`.

**Background workers.** Expiration worker для control sessions; command timeout/reconciliation; gateway connector; result verifier.

**Realtime events.** `ControlSessionChanged`, `CommandExecutionChanged`, `CommandRejected`, `CommandResultUnknown`.

**UI routes.** Dashboard runtime, equipment card, kiosk runtime.

**MVP.** Архитектура, preview/simulation и аудит; реальное управление допускается только отдельным pilot gate после Phase 7.

**Риски сложности.** Повторная отправка, command replay, неверная область, потеря подтверждения, ложное отображение успеха.

### 3.6. Events & Alarms

**Назначение.** Регистрировать факты и управлять жизненным циклом контролируемых аварийных условий.

**Основные сущности.** `EventRecord`, `AlarmRule`, `AlarmOccurrence`, `AlarmAcknowledgement`, `AlarmAssignment`, `AlarmShelving`, `AlarmSuppression`.

**Основные use cases.**

- создать event из телеметрии, платформы или пользователя;
- оценить alarm rule;
- открыть/обновить/clear occurrence;
- acknowledge с optional/required comment;
- назначить ответственного;
- shelve с обязательными reason/until;
- suppress по политике/зависимости;
- выполнить bulk action с предварительным расчетом допустимых строк;
- просмотреть read-only history.

**API.** `/api/events`, `/api/alarm-rules`, `/api/alarm-occurrences`, `/api/alarm-occurrences/{id}/acknowledgements`, `/assignments`, `/shelvings`, `/suppressions`.

**Background workers.** Rule evaluator; lifecycle reconciler; shelving expiry; suppression dependency recalculation; event retention.

**Realtime events.** `EventCreated`, `AlarmOccurrenceOpened`, `AlarmOccurrenceUpdated`, `AlarmAcknowledged`, `AlarmAssigned`, `AlarmCleared`, `AlarmShelved`, `AlarmUnshelved`.

**UI routes.** `/events`, event drawer, dashboard linked-events widgets.

**MVP.** Да: базовые threshold/state rules, occurrences, acknowledgement, assignment, shelving и Realtime/History.

**Риски сложности.** Одна таблица «alarms» для всех понятий; изменение condition при acknowledgement; отсутствие идемпотентности rule evaluation.

### 3.7. Incidents

**Назначение.** Координировать длительную ситуацию, объединяющую события, участников, задачи и контекст.

**Основные сущности.** `Incident`, `IncidentLink`, `IncidentParticipant`, `IncidentTimelineEntry`, `IncidentTask`.

**Основные use cases.**

- создать incident из выбранного event/occurrence без изменения его состояния;
- назначить координатора;
- привязать события, оборудование, документы, work requests и задачи;
- менять priority/status;
- закрыть incident с результатом и причиной;
- открыть исходные контексты.

**API.** `/api/incidents`, `/api/incidents/{id}/links`, `/participants`, `/timeline`, `/close`.

**Background workers.** SLA/timer worker; проекция incident assignments в «Мою работу»; контроль незакрытых обязательных действий.

**Realtime events.** `IncidentCreated`, `IncidentUpdated`, `IncidentAssignmentChanged`, `IncidentClosed`.

**UI routes.** `/incidents/{id}`, `/events`, `/my-work`.

**MVP.** Минимальная сущность, создание из события, координатор, status и links. Полный workflow позже.

**Риски сложности.** Попытка реализовать универсальный BPM/workflow engine; автоматическое закрытие incident при clearing alarms.

### 3.8. Dashboards & Mimics

**Назначение.** Предоставить адресуемый runtime дашбордов, окон, widgets и мнемосхем с версионируемой конфигурацией.

**Основные сущности.** `Dashboard`, `DashboardWindow`, `Widget`, `MimicDiagram`, `MimicObjectBinding`, `ConfigurationRevision`, `Publication`.

**Основные use cases.**

- каталог, избранное и последний доступный dashboard;
- открыть `/d/{dashboard}/{screen}`;
- переключить `Live/History`;
- выбрать объект и синхронизировать mimic, faceplate, trend и events;
- fullscreen/canvas-only runtime;
- получить published revision;
- позже: draft/save/validate/impact/publish/rollback.

**API.** `/api/dashboards`, `/api/dashboards/{id}/windows`, `/api/widgets`, `/api/mimics`, `/api/configuration-revisions`, `/api/publications`.

**Background workers.** Validation; publication build/materialization; cache invalidation; impact calculation; snapshot generation.

**Realtime events.** `DashboardPublicationChanged`, `MimicPublicationChanged`, `DashboardContextChanged` только клиентский, `WidgetDataInvalidated`.

**UI routes.** `/dashboards`, `/d/{dashboard}/{screen}`, `/builder/dashboards/{id}`, `/builder/mimics/{id}`.

**MVP.** Runtime, static/config-driven widgets, static SVG mimic, published revision contract. Visual editors позже.

**Риски сложности.** Произвольные widget-to-widget зависимости; частичная публикация; runtime, читающий draft.

### 3.9. Maintenance / CMMS

**Назначение.** Управлять объектами обслуживания и жизненным циклом плановых/внеплановых работ.

**Основные сущности.** `MaintenanceObject`, `MaintenancePlan`, `MaintenanceForecast`, `MaintenanceRequest`, `Defect`, `WorkOrder`, `Procedure`, `ChecklistItem`.

**Основные use cases.**

- вести maintenance asset независимо от equipment;
- связать его с equipment при наличии;
- планировать ППР;
- создавать прогноз и work order как разные сущности;
- создавать request из event без acknowledgement;
- назначать, принимать, выполнять и передавать work order на приемку;
- вести обязательный checklist.

**API.** `/api/maintenance/objects`, `/plans`, `/forecast`, `/requests`, `/defects`, `/work-orders`, `/procedures`.

**Background workers.** Forecast generator; overdue/SLA evaluator; recurring plan scheduler; projection into My Work.

**Realtime events.** `WorkOrderCreated`, `WorkOrderAssigned`, `WorkOrderStatusChanged`, `MaintenanceForecastGenerated`, `MaintenanceRequestCreated`.

**UI routes.** `/maintenance`, `/maintenance/assets`, `/maintenance/plans`, `/maintenance/calendar`, `/maintenance/requests`, `/maintenance/work-orders`, `/maintenance/work-orders/{id}`.

**MVP.** MaintenanceObject, simple plan, request и work order с минимальным checklist/status flow.

**Риски сложности.** Смешивание maintenance object и equipment; попытка охватить запасы, закупки и сложные ресурсы в MVP.

### 3.10. Notifications

**Назначение.** Доставлять обязательные и дополнительные сообщения, сохраняя различие между source state и delivery/read state.

**Основные сущности.** `NotificationPolicy`, `NotificationSubscription`, `NotificationDelivery`, `Notification`, `DeliveryChannel`, `UserSchedule`, `CoverageAssignment`.

**Основные use cases.**

- сформировать in-app notification из доступного source;
- read/unread без изменения source;
- настроить дополнительные subscriptions;
- применить mandatory rules, quiet hours и schedule;
- включить vacation только после принятого coverage;
- тестировать канал;
- позже: escalation и production SMS/push.

**API.** `/api/notifications`, `/api/notification-settings`, `/api/notification-subscriptions`, `/api/notification-policies`, `/api/delivery-channels`.

**Background workers.** Delivery dispatcher; digest builder; retry/dead-letter; quiet-hours scheduler; escalation/coverage worker.

**Realtime events.** `NotificationDelivered`, `NotificationRead`, `NotificationBadgeChanged`, `CoverageAccepted`.

**UI routes.** `/notifications`, `/settings/notifications`, header inbox, `/admin/settings/notifications` позже.

**MVP.** In-app notification и read/unread; сложные каналы и escalation later.

**Риски сложности.** Смешивание inbox с Events; пользовательское отключение обязательной доставки; утечка недоступного source через notification.

### 3.11. Reports & Documents

**Назначение.** Формировать отчеты, хранить инструкции/вложения и вести shift log.

**Основные сущности.** `ReportDefinition`, `ReportRun`, `Document`, `DocumentLink`, `ShiftLogEntry`.

**Основные use cases.**

- запуск отчета с параметрами;
- асинхронное формирование и скачивание результата;
- прикрепление документа к equipment/incident/work order;
- просмотр актуальной инструкции;
- ведение журнала смены.

**API.** `/api/reports`, `/api/report-runs`, `/api/documents`, `/api/shift-log`.

**Background workers.** Report renderer; cleanup/retention; document virus-scan integration при production storage.

**Realtime events.** `ReportRunCompleted`, `ReportRunFailed`, `ShiftLogEntryCreated`.

**UI routes.** `/reports`, `/shift-log`, контекстные links из equipment/incidents/maintenance.

**MVP.** Необязателен, кроме контрактов links и простого shift-log при наличии ресурса.

**Риски сложности.** Произвольный report designer; хранение больших файлов в PostgreSQL; отсутствие политики retention.

### 3.12. Admin & Platform Health

**Назначение.** Показывать техническое состояние приложения и качество потока данных отдельно от технологических events.

**Основные сущности.** `SystemSetting`, `IntegrationConnection`, `PlatformComponent`, `HealthObservation`, `DataQualityIssue`, `PolicyOverride`.

**Основные use cases.**

- просмотреть/изменить системную политику с impact preview;
- диагностировать connection без раскрытия secret;
- просмотреть health компонентов;
- фильтровать/назначать data quality issues;
- различать inherited и overridden settings.

**API.** `/api/admin/settings`, `/api/admin/integrations`, `/api/health/components`, `/api/data-quality/issues`.

**Background workers.** Health collector; data quality analyzer; stale-source scanner; settings consistency check.

**Realtime events.** `PlatformHealthChanged`, `DataQualityIssueOpened`, `DataQualityIssueResolved`, `SystemPolicyChanged`.

**UI routes.** `/admin/settings`, `/admin/settings/connections`, `/admin/health`, `/admin/data-quality`.

**MVP.** Да: read model health, data quality, connection diagnostics и ограниченные system settings.

**Риски сложности.** Генерация alarm occurrence из каждой внутренней ошибки; отображение endpoint/secrets; отсутствие области доступа для admin.

### 3.13. Audit & Configuration Publishing

**Назначение.** Обеспечить неизменяемый след значимых действий и единый управляемый lifecycle конфигурации.

**Основные сущности.** `AuditEntry`, `ConfigurationDocument`, `ConfigurationRevision`, `ValidationResult`, `ImpactPreview`, `Approval`, `Publication`, `RollbackRequest`.

**Основные use cases.**

- записать actor, device, action, scope, before/after, reason и correlation;
- сохранить draft;
- validate конкретную сохраненную revision;
- вычислить impact;
- опубликовать атомарно;
- откатить старую revision как новую publication;
- просмотреть read-only audit.

**API.** `/api/audit`, `/api/configuration-documents`, `/api/configuration-revisions`, `/validate`, `/impact`, `/publish`, `/rollback`.

**Background workers.** Outbox dispatcher; validation runner; publication materializer; audit integrity checker.

**Realtime events.** `ConfigurationValidated`, `PublicationSucceeded`, `PublicationFailed`, `AuditEntryCreated` для admin stream.

**UI routes.** `/admin/audit`, builder routes позже, version/impact dialogs.

**MVP.** Audit обязателен. Publication baseline нужен для dashboard/mimic runtime даже без visual editor.

**Риски сложности.** Изменяемый audit; validation несохраненного состояния; partial apply; rollback, перезаписывающий историю.

### 3.14. Kiosk / Terminals

**Назначение.** Управлять физическими панелями с собственной device identity и общими профилями.

**Основные сущности.** `Terminal`, `TerminalDeviceIdentity`, `KioskProfile`, `TerminalAssignment`, `EnrollmentToken`.

**Основные use cases.**

- зарегистрировать терминал одноразовым кодом;
- назначить профиль и стартовый dashboard независимо;
- block/unblock/revoke identity;
- rebind без изменения identity;
- открыть kiosk runtime;
- при разрешении — включить control mode по terminal policy и атрибутировать сотрудника.

**API.** `/api/terminals`, `/api/kiosk-profiles`, `/api/terminal-enrollments`, `/api/terminals/{id}/assignment`, `/state`.

**Background workers.** Enrollment expiry; heartbeat/offline detector; configuration synchronization; certificate rotation later.

**Realtime events.** `TerminalStateChanged`, `TerminalAssignmentChanged`, `TerminalConfigurationChanged`, `TerminalControlRevoked`.

**UI routes.** `/admin/terminals`, `/terminal/enroll`, kiosk runtime route/profile.

**MVP.** Сущности и admin read model допустимы; production enrollment, certificates и wallboard playlists позже.

**Риски сложности.** Общий PIN, неотзываемая identity, очередь команд offline, повтор команды после reconnect.

---

## 4. Архитектура solution

### 4.1. Архитектурный стиль

Рекомендуется **модульный монолит для бизнес-контуров** и **отдельные process boundaries для telemetry/protocol/command runtime**.

Причины:

- бизнес-use cases требуют согласованных транзакций и общего темпа изменения;
- команда не должна обслуживать распределенную транзакционность раньше появления реальной необходимости;
- protocol adapters имеют другой профиль нагрузки и должны быть заменяемыми;
- telemetry ingestion, polling и rule evaluation нуждаются в независимом масштабировании;
- C++-ready границы проще обеспечить на уровне gRPC/process contract, чем внутри ASP.NET процесса.

### 4.2. Deployment units первого релиза

| Unit | Состав | Масштабирование |
|---|---|---|
| `dispatcher-web-api` | ASP.NET Core API, business modules, auth, SignalR; публикация Blazor assets | По числу web/API соединений |
| `dispatcher-telemetry-orchestrator` | Poll scheduling, assignment, source state, normalization coordination | По числу источников/точек |
| `dispatcher-adapter-modbus` | Modbus TCP sessions, reads/writes, protocol errors | По соединениям и poll load |
| `dispatcher-adapter-snmp` | SNMP polling/traps later, protocol errors | По устройствам и poll load |
| `dispatcher-worker` | Rules, history batching, freshness, outbox, maintenance timers, notifications | По backlog каждого worker |
| `postgresql` | Context schemas, current/history, configuration, audit | Вертикально; history partitioning |

В development profile adapters могут запускаться вместе, но production contract и конфигурация должны позволять отдельный процесс без изменения бизнес-кода.

### 4.3. Структура solution

```text
Dispatcher.sln
  src/
    Dispatcher.Api
    Dispatcher.Web
    Dispatcher.Domain
    Dispatcher.Application
    Dispatcher.Infrastructure
    Dispatcher.Contracts
    Dispatcher.Shared
    Dispatcher.Realtime
    Dispatcher.IdentityAccess
    Dispatcher.AssetModel
    Dispatcher.EventsAlarms
    Dispatcher.Incidents
    Dispatcher.Dashboards
    Dispatcher.Maintenance
    Dispatcher.Notifications
    Dispatcher.Admin
    Dispatcher.AuditPublishing
    Dispatcher.Terminals
    Dispatcher.Telemetry.Contracts
    Dispatcher.Telemetry.Orchestrator
    Dispatcher.Telemetry.Ingestion
    Dispatcher.Telemetry.Adapters.Modbus
    Dispatcher.Telemetry.Adapters.Snmp
    Dispatcher.Telemetry.Adapters.OpcUa
    Dispatcher.Telemetry.Adapters.Bacnet
    Dispatcher.Commanding.Contracts
    Dispatcher.Commanding.Gateway
    Dispatcher.Workers
  tests/
    Dispatcher.UnitTests
    Dispatcher.IntegrationTests
    Dispatcher.ContractTests
    Dispatcher.EndToEndTests
    Dispatcher.LoadTests
```

### 4.4. Ответственность проектов

| Проект | Ответственность | Нельзя размещать | Допустимые зависимости | Почему нужен |
|---|---|---|---|---|
| `Dispatcher.Api` | HTTP endpoints, auth middleware, request validation, composition root | Domain rules, protocol clients, SQL в controllers | Application, Contracts, Realtime, Infrastructure composition | Единая серверная граница для web/API |
| `Dispatcher.Web` | Blazor UI, route composition, client state, API/SignalR clients | Авторизацию как источник истины, direct DB, protocol logic | Contracts/client SDK only | Отделяет браузер от backend и сохраняет route model |
| `Dispatcher.Domain` | Общие доменные типы и инварианты, не принадлежащие одному модулю | EF Core, HTTP, SignalR, protocol fields | Shared only | Минимальное ядро; не должен стать «свалкой» |
| `Dispatcher.Application` | Общие application abstractions, transactions, authorization context | UI rendering, adapter implementation | Domain, Shared, Contracts | Единые правила выполнения use cases |
| `Dispatcher.Infrastructure` | PostgreSQL, migrations, secret references, clocks, file storage adapters | Бизнес-решения и route-specific DTO | Application abstractions, context modules | Изолирует технические реализации |
| `Dispatcher.Contracts` | Versioned REST/SignalR DTO и public error model | EF entities, внутренние aggregates | Shared | Стабильные web contracts |
| `Dispatcher.Shared` | IDs, value objects, result/error, correlation, time abstractions | Доменные сущности контекстов | Нет или BCL | Минимальный общий слой без бизнес-зависимостей |
| `Dispatcher.Realtime` | SignalR hubs, subscriptions, authorization, snapshot/resync protocol | Хранение истины и rule evaluation | Contracts, Application read services | Общая realtime-модель и gap handling |
| `Dispatcher.IdentityAccess` | Users, roles, scopes, effective policy, sessions | Equipment/telemetry business logic | Domain/Application abstractions | Отдельная модель прав и областей |
| `Dispatcher.AssetModel` | Locations, equipment, relations, staging/import | Polling implementation, CMMS lifecycle | Domain/Application | Каноническая пользовательская модель объектов |
| `Dispatcher.EventsAlarms` | Events, rules, occurrences, ack/assign/shelve/suppress | Incident close logic, notification delivery | Domain/Application, Telemetry contracts | Сохраняет отдельный alarm lifecycle |
| `Dispatcher.Incidents` | Incident coordination, links, participants, timeline | Alarm condition calculation | Domain/Application | Не позволяет incident стать полем alarm |
| `Dispatcher.Dashboards` | Runtime definitions, windows, widgets, mimic bindings | Telemetry polling, visual editor UI code | Domain/Application, AuditPublishing abstractions | Отдельные versioned runtime artifacts |
| `Dispatcher.Maintenance` | Maintenance objects, plans, requests, work orders | Equipment protocol configuration | Domain/Application, Asset references | Отдельный CMMS bounded context |
| `Dispatcher.Notifications` | Policies, subscriptions, deliveries, inbox | Изменение source state | Domain/Application | Read state и delivery не смешиваются с alarms |
| `Dispatcher.Admin` | System settings, integrations, health/data-quality read models | Технологические alarms, secret plaintext | Application, Infrastructure abstractions | Admin-only системный контур |
| `Dispatcher.AuditPublishing` | Audit append, revisions, validation, publication, rollback | Редактирование произвольных business records в обход modules | Domain/Application | Общий управляемый lifecycle изменений |
| `Dispatcher.Terminals` | Terminal identity, profile, enrollment, assignment | Обычные user sessions как identity панели | Identity abstractions, Dashboards refs | Отдельная security-модель kiosk |
| `Dispatcher.Telemetry.Contracts` | Protobuf/contract messages для polling, samples и states | EF models, UI DTO | Shared only | Стабильная C++-ready граница |
| `Dispatcher.Telemetry.Orchestrator` | Poll plans, leases, scheduling, adapter routing | Parsing protocol payloads, business alarm logic | Telemetry.Contracts, Infrastructure | Независимое управление опросом |
| `Dispatcher.Telemetry.Ingestion` | Normalize, deduplicate, current-value update, history batches | UI rendering, protocol sessions | Telemetry.Contracts, Infrastructure | Единая точка семантики value/quality/freshness |
| `Dispatcher.Telemetry.Adapters.Modbus` | Modbus TCP transport, address/read/write conversion | Equipment UI, RBAC, alarm rules | Telemetry.Contracts | Заменяемый низкоуровневый adapter |
| `Dispatcher.Telemetry.Adapters.Snmp` | SNMP sessions, OID reads, v2c/v3 handling | User subscriptions, work orders | Telemetry.Contracts | Заменяемый низкоуровневый adapter |
| `Dispatcher.Telemetry.Adapters.OpcUa` | Future OPC UA adapter | Business context logic | Telemetry.Contracts | Future, не MVP |
| `Dispatcher.Telemetry.Adapters.Bacnet` | Future BACnet adapter | Business context logic | Telemetry.Contracts | Future, не MVP |
| `Dispatcher.Commanding.Contracts` | Command/preflight/execution contracts | UI or adapter implementations | Shared | Стабильная service boundary |
| `Dispatcher.Commanding.Gateway` | Protocol execution routing, idempotency, result tracking | Решение о правах и business approval | Commanding.Contracts, Telemetry adapters | Изолирует низкоуровневое исполнение |
| `Dispatcher.Workers` | Hosts для rules, history, freshness, outbox, schedules | Controllers и Blazor components | Context application services | Управляет background lifecycle |

### 4.5. Правила зависимостей

1. `Dispatcher.Web` не ссылается на EF entities или infrastructure.
2. `Dispatcher.Api` не знает реализацию протоколов.
3. Protocol adapters не знают `User`, `Incident`, `WorkOrder`, `Dashboard`.
4. Business contexts обмениваются через application contracts и persisted IDs, а не через общий mutable entity graph.
5. `Dispatcher.Shared` остается минимальным и не содержит «универсальных» repository/service abstractions.
6. Realtime сообщения строятся из committed state; SignalR не является source of truth.
7. Audit вызывается через обязательный application pipeline и дополняется database controls для критических таблиц.
8. Public contracts versioned; internal class libraries могут изменяться совместно.

### 4.6. PostgreSQL и транзакции

Для MVP используется один PostgreSQL cluster со схемами:

- `iam`;
- `assets`;
- `telemetry`;
- `events`;
- `incidents`;
- `dashboards`;
- `maintenance`;
- `notifications`;
- `admin`;
- `audit`;
- `terminals`.

Правила:

- business transaction не должна охватывать protocol call;
- command execution использует state machine и idempotency key;
- cross-context notification/realtime запускается через transactional outbox после commit;
- это не event sourcing: текущие сущности хранятся обычным способом, outbox нужен только для надежной доставки изменений;
- `CurrentValue` хранит одну актуальную запись на `DataPoint`;
- `HistoricalValue` append-only и партиционируется по времени;
- JSONB допустим для layout/widget/protocol-specific snapshots, но не заменяет ключевые связи и статусы;
- secrets хранятся через secret reference/provider, не в audit и не в открытом JSONB.

### 4.7. Realtime protocol

SignalR должен поддерживать:

- initial authorized snapshot;
- подписку по location/dashboard/equipment/data-point scope;
- sequence/cursor на канал;
- heartbeat и server timestamp;
- обнаружение gap;
- явный client state `Live`, `Reconnecting`, `Gap`, `Resynchronizing`, `Offline`;
- повторный snapshot после reconnect;
- невозможность выполнения command при недоверенном realtime state.

Клиент не должен просто «продолжать» поток после reconnect, если нельзя доказать отсутствие пропущенных изменений.

---

## 5. C++-ready architecture

### 5.1. Общие правила

1. Первый вариант всех перечисленных сервисов реализуется на C#/.NET.
2. C++ не используется для UI, API, RBAC, incidents, maintenance, dashboards, audit, configuration publishing или пользовательских workflow.
3. Замена реализации возможна только за внешней process boundary.
4. Основной путь — versioned protobuf/gRPC для control/configuration и streaming; для надежных асинхронных результатов применяется outbox/inbox или выбранный после нагрузочных тестов message broker.
5. P/Invoke или C++ DLL внутри ASP.NET API не являются основным вариантом: они связывают lifecycle, memory safety и deployment web-процесса с низкоуровневым runtime.
6. Переписывание обосновано только production/load-test метриками при корректно оптимизированной .NET реализации.
7. Business decisions остаются в C#; C++ получает минимальные нормализованные assignments и возвращает технический результат.

### 5.2. Стабильные контракты

Минимальные сообщения:

- **PollingAssignment:** source ID, protocol adapter type/version, endpoint reference, data-point mappings, interval, timeout, lease, config revision.
- **TelemetrySample:** data-point ID, typed value, unit, quality, source timestamp, receive timestamp, source ID, sequence, diagnostic flags.
- **SourceState:** source ID, connection state, last success/error, error class, latency, config revision.
- **CommandRequest:** execution ID, idempotency key, target mapping, command payload, deadline, expected state/version, policy token.
- **CommandResult:** execution ID, phase, protocol response, timestamps, verification result, error class.
- **RuleEvaluationInput:** point/value/quality/timestamps, active rule revision, previous evaluation state.

Контракты не содержат UI labels, role logic, incident fields, work-order state или database-specific types.

### 5.3. Telemetry Polling Runtime

| Аспект | Решение |
|---|---|
| Зачем нужен | Распределяет polling assignments, выдерживает интервалы, leases, backoff и fairness между источниками |
| Первый вариант | .NET Worker Service с bounded channels, async I/O и monotonic scheduler |
| Когда C++ имеет смысл | После профилирования, если scheduler/GC/CPU не выдерживают целевое число точек при оптимальной .NET реализации |
| Стабильный контракт | PollingAssignment, PollResult, SourceState, lease/heartbeat API |
| Связь | gRPC control stream к orchestrator; gRPC streaming samples к ingestion |
| Принимает | Assignments, source configuration reference, schedule updates, cancellation |
| Возвращает | Normalized raw samples, source state, diagnostics, metrics |
| Метрики-триггеры | p99 poll start drift; samples/sec/core; CPU; allocation rate; GC pause; timeout/backlog; active connections |
| Почему отдельно | Scheduling и I/O не должны находиться в API или business transaction |

Предварительный критерий оценки C++: sustained CPU выше 70% при целевой нагрузке, p99 drift выше согласованного SLA или неприемлемая стоимость памяти после устранения очевидных bottlenecks. Числа должны быть уточнены нагрузочным профилем проекта.

### 5.4. Modbus Adapter

| Аспект | Решение |
|---|---|
| Зачем нужен | Modbus TCP session management, grouped reads, register decoding и command writes |
| Первый вариант | Отдельный .NET Worker на проверенной .NET library, с собственным connection pool и batching |
| Когда C++ имеет смысл | Очень большое число соединений/регистров, жесткие latency/jitter требования или ограниченный edge hardware |
| Стабильный контракт | Protocol-neutral PollBatch/TelemetrySample и CommandTransportRequest/Result |
| Связь | gRPC с polling runtime и command gateway |
| Принимает | Endpoint, unit ID, function code, address, datatype, endian/scaling, timeout |
| Возвращает | Typed value/quality, protocol diagnostic, timing; для command — transport/result state |
| Метрики-триггеры | requests/sec; registers/sec; connection churn; p99 latency; CPU per 1000 registers; error/retry rate |
| Почему отдельно | Modbus address semantics не должны загрязнять Equipment/DataPoint API |

### 5.5. SNMP Adapter

| Аспект | Решение |
|---|---|
| Зачем нужен | SNMP v2c/v3 polling, OID typing, session/auth handling; traps позже |
| Первый вариант | .NET Worker; credentials доступны только по secret reference |
| Когда C++ имеет смысл | Массовый high-frequency polling, trap storm или edge footprint, которые .NET не выдерживает по метрикам |
| Стабильный контракт | PollBatch/TelemetrySample/SourceState; trap envelope later |
| Связь | gRPC streams к orchestrator/ingestion |
| Принимает | Endpoint, version, OID list, auth/privacy references, timeout/retry |
| Возвращает | Typed varbind values, quality, protocol errors, latency |
| Метрики-триггеры | OIDs/sec; packets/sec; timeout rate; memory per session; CPU; p99 cycle duration |
| Почему отдельно | SNMP security и OID details являются infrastructure concern |

### 5.6. OPC UA Adapter

| Аспект | Решение |
|---|---|
| Зачем нужен | Подписки/reads OPC UA, namespace mapping, certificate/session handling |
| Первый вариант | .NET Worker после MVP, на официально поддерживаемом stack |
| Когда C++ имеет смысл | Экстремальное число monitored items, hard edge constraints или доказанная проблема native stack integration |
| Стабильный контракт | SubscriptionAssignment, TelemetrySample, SourceState, CommandTransportRequest |
| Связь | gRPC; сертификаты через secret/certificate service |
| Принимает | Endpoint, security policy, certificate ref, node mappings, sampling/publishing interval |
| Возвращает | Values, quality, source/server timestamps, subscription diagnostics |
| Метрики-триггеры | monitored items; notifications/sec; publish lag; reconnect time; CPU/memory |
| Почему отдельно | OPC UA session/certificate lifecycle не относится к business API |

### 5.7. BACnet Adapter

| Аспект | Решение |
|---|---|
| Зачем нужен | BACnet/IP discovery/read/write, object/property mapping |
| Первый вариант | Отдельный .NET Worker после уточнения реальных устройств и profiles |
| Когда C++ имеет смысл | Большая сеть BACnet, сложная discovery нагрузка или требования к edge footprint |
| Стабильный контракт | DiscoveryResult, PollAssignment, TelemetrySample, CommandTransportResult |
| Связь | gRPC с orchestrator/gateway |
| Принимает | Network/interface scope, device/object/property mapping, intervals |
| Возвращает | Discovered technical identities, values, quality, network diagnostics |
| Метрики-триггеры | devices/objects; discovery duration; packets/sec; CPU; p99 response |
| Почему отдельно | BACnet network semantics не должны становиться product entity model |

### 5.8. Edge Gateway

| Аспект | Решение |
|---|---|
| Зачем нужен | Сбор на удаленной площадке, локальный buffer, store-and-forward и защищенный uplink |
| Первый вариант | .NET Worker bundle с adapters, encrypted local store и outbound mTLS connection |
| Когда C++ имеет смысл | Малоресурсный hardware, большой локальный throughput, строгие startup/footprint требования |
| Стабильный контракт | GatewayRegistration, ConfigSnapshot, TelemetryBatch, AckCursor, SourceState |
| Связь | Outbound gRPC/mTLS; no inbound public port по умолчанию |
| Принимает | Signed configuration revision, assignments, retention limits |
| Возвращает | Ordered batches, health, backlog, config acknowledgement |
| Метрики-триггеры | RAM/disk footprint; startup; buffered samples; uplink recovery time; CPU; power constraints |
| Почему отдельно | Offline buffer и field deployment не должны усложнять central API |

Edge не должен повторно исполнять команды после reconnect. Для command path требуется online session и новый preflight.

### 5.9. Command Execution Gateway

| Аспект | Решение |
|---|---|
| Зачем нужен | Технически исполнить уже авторизованную команду, обеспечить idempotency, deadline и result correlation |
| Первый вариант | .NET Worker; routing к protocol adapter; durable execution state в PostgreSQL |
| Когда C++ имеет смысл | Только при доказанной необходимости very-low latency/high concurrency или edge hardware; не ради security |
| Стабильный контракт | CommandRequest/CommandResult state machine |
| Связь | gRPC/mTLS от business Commanding service; adapter RPC downstream |
| Принимает | Signed short-lived policy token, target mapping, payload, expected state, deadline, idempotency key |
| Возвращает | Accepted/transmitted/confirmed/completed/error/unknown и protocol evidence |
| Метрики-триггеры | p99 dispatch latency; concurrent executions; timeout/unknown rate; CPU; retry/idempotency conflicts |
| Почему отдельно | Gateway не принимает решение о правах, second user или reason; он выполняет техническое действие |

### 5.10. History Ingestion

| Аспект | Решение |
|---|---|
| Зачем нужен | Принимать большие telemetry batches, дедуплицировать и пакетно писать partitioned history |
| Первый вариант | .NET Worker + Npgsql binary/batch write, bounded buffers и backpressure |
| Когда C++ имеет смысл | Если serialization/copy/CPU ingestion становится bottleneck после batch/DB tuning |
| Стабильный контракт | TelemetryBatch, AckCursor, rejected sample reasons |
| Связь | gRPC streaming от adapters/edge; PostgreSQL downstream |
| Принимает | Ordered sample batches с source/sequence/config revision |
| Возвращает | Ack cursor, rejected items, lag/backpressure state |
| Метрики-триггеры | samples/sec; ingest p99; backlog age; CPU/allocations; DB write saturation; rejected/duplicate rate |
| Почему отдельно | High-throughput path не должен блокировать current-value API и UI |

До C++ следует проверить schema, partitioning, indexes, batch size и retention: bottleneck часто находится в БД, а не в языке.

### 5.11. Alarm/Rule Evaluation Engine

| Аспект | Решение |
|---|---|
| Зачем нужен | Детерминированно оценивать rule revisions и выдавать state transitions |
| Первый вариант | .NET Worker с compiled rule plans, partitioning по DataPoint и идемпотентными transitions |
| Когда C++ имеет смысл | Миллионы evaluations/sec или p99 evaluation lag, не устранимые partitioning/algorithm optimization |
| Стабильный контракт | RuleRevision + RuleEvaluationInput → RuleTransitionCandidate |
| Связь | gRPC stream или локальный ingestion feed; результаты в Events & Alarms application service |
| Принимает | Normalized value/quality/time, previous evaluator state, exact published rule revision |
| Возвращает | Candidate open/update/clear/no-change с evidence; не создает Incident |
| Метрики-триггеры | evaluations/sec; p99 lag; active rules; state size; CPU; allocations; transition duplication |
| Почему отдельно | Engine вычисляет условие, но acknowledgement, assignment, shelving и incident остаются business logic |

### 5.12. Решение о переписывании

Перед C++ extraction обязателен отчет:

1. воспроизводимый load profile;
2. baseline и optimized .NET metrics;
3. точный bottleneck по profiler/trace;
4. проверка, что bottleneck не находится в protocol device, network или PostgreSQL;
5. ожидаемый выигрыш и стоимость поддержки двух toolchains;
6. contract tests, которые новая реализация обязана пройти без изменения бизнес-системы;
7. rollback plan на .NET implementation.


---

## 6. Первый промышленный MVP

### 6.1. Цель MVP

MVP должен доказать не набор экранов, а сквозной промышленный контур:

> источник данных → нормализованное значение → quality/freshness → current/history → rule evaluation → event/alarm occurrence → realtime UI → операторское действие → audit.

Параллельно должны быть заложены минимальные контуры dashboard runtime, incidents, maintenance, admin health и data quality. MVP ориентирован на одну организацию, несколько локаций и ограниченный пилотный набор оборудования.

### 6.2. Функциональный состав MVP

| Область | Входит в MVP | Уточнение |
|---|---:|---|
| Единая shell | Да | Global header, rail, workspace, одна правая context panel, права на routes |
| Identity & Access | Да | Local/OIDC-ready auth, простые роли, scopes, backend authorization |
| Локации | Да | Дерево, карточка, агрегированное состояние |
| Оборудование | Да | Registry, card, staging manual/CSV, create/update/skip/draft |
| Точки данных | Да | Тип, unit, mapping, interval, thresholds, enabled state |
| Current values | Да | Value, unit, quality, source/receive timestamp, freshness |
| History | Да | Ограниченный retention, partitioned storage, trend query |
| Modbus TCP | Да | Polling и диагностический sample read; command path initially disabled/simulated |
| SNMP | Да | v2c и базовый v3 при наличии test devices; секреты по reference |
| SignalR | Да | Snapshot, updates, sequence, gap/reconnect/resync |
| Events & Alarms | Да | Basic rules, EventRecord, AlarmOccurrence, ack, assignment, shelving |
| Incidents | Базово | Create/link/coordinator/status; без сложного workflow |
| Dashboard runtime | Да | Catalog, windows, Live/History, selected context, static/configured widgets |
| Mimic runtime | Да | Published static SVG, bindings, selection, faceplate, no visual editor |
| Trends | Базово | Current context и time range, quality marks |
| Audit | Да | Append-only significant actions, before/after/reason/correlation |
| Platform health | Да | API/worker/adapter/source/DB read model |
| Data quality | Да | stale/offline/bad/missing mapping issues и links к source |
| ТОиР | Базово | MaintenanceObject, simple plan/request/work order/checklist |
| Notifications | Базово | In-app delivery/read state; source state неизменен |
| Seed/demo data | Да | Повторяемый pilot dataset и test users |
| Smoke tests | Да | API, DB migrations, polling simulator, SignalR, primary UI paths |
| Commanding | Preview | Control mode/preflight/state contracts; физическая отправка только после gate |
| Configuration publication | Базово | Published revision для dashboard/mimic/rules, без visual editor |

### 6.3. MVP use cases

#### Оператор

1. Войти и увидеть доступный `/home`.
2. Выбрать локацию и открыть dashboard.
3. Увидеть актуальные значения с quality/freshness.
4. Понять, что источник offline/stale и что последнее значение не актуально.
5. Получить realtime alarm occurrence.
6. Открыть событие, acknowledge с требуемым comment, назначить себя или shelve с reason/until.
7. Создать простой incident или maintenance request без изменения acknowledgement.
8. Перейти к equipment, mimic и trend по связанному контексту.
9. Увидеть назначенную задачу в `/my-work`.

#### Инженер-конфигуратор

1. Добавить equipment вручную или CSV в единый staging.
2. Проверить structural validation, connection и sample poll независимо.
3. Применить только валидные create/update rows; existing остается skip до явного разрешения.
4. Настроить DataPoints и базовые AlarmRules.
5. Опубликовать подготовленную server-side configuration revision.
6. Проверить data quality и source diagnostics.

#### Администратор

1. Создать/активировать user account и назначить role/scope.
2. Увидеть effective permissions и источник назначения.
3. Просмотреть platform health, data quality и audit.
4. Изменить разрешенную system policy после impact preview.
5. Убедиться, что прямой `/admin/*` URL запрещен не-администратору.

#### Планировщик/исполнитель ТОиР

1. Создать MaintenanceObject без telemetry binding.
2. Создать простой MaintenancePlan или request.
3. Получить WorkOrder и выполнить обязательный checklist/status transition.
4. Перейти из work order к связанному equipment/event при наличии права.

### 6.4. Что не входит в MVP

- полноценный visual Dashboard Editor;
- полноценный SVG Editor;
- произвольный widget scripting;
- mobile offline technician app;
- сложный CMMS: запасы, закупки, ресурсы, бюджетирование, подрядные документы;
- full workflow/BPM engine;
- полноценный notification escalation engine;
- production SMS, push и messenger integration;
- high-load C++ runtime;
- OPC UA и BACnet production adapters;
- multi-tenant UX и переключение организаций;
- общий adaptive/touch UI как отдельный продуктовый профиль;
- wallboard playlists;
- production kiosk certificate enrollment;
- полноценный report designer;
- массовая миграция исторических данных;
- автоматическое технологическое управление без pilot approval;
- автоматическое удаление equipment из CSV;
- использование черновиков в runtime.

### 6.5. MVP non-functional baseline

Точные числа требуют отдельного sizing, но MVP обязан иметь измеримые показатели:

- все API requests имеют correlation ID и structured logs;
- все фоновые процессы публикуют heartbeat, backlog и last-success metrics;
- migrations повторяемы на пустой и обновляемой БД;
- никакие secrets не возвращаются UI и не попадают в audit/log;
- authorization проверяется сервером для каждой команды и прямого URL;
- telemetry pipeline имеет bounded buffers и backpressure;
- current value update не зависит от успешной записи каждого history row;
- history retention и partitions управляются автоматически;
- realtime reconnect имеет snapshot/resync, а не silent continuation;
- audit entry создается в той же business transaction или через гарантированный outbox;
- базовые workflows идемпотентны по request/idempotency key;
- приложение имеет health endpoints для liveness/readiness и детальный admin health read model;
- smoke/e2e suite запускается в CI на PostgreSQL и protocol simulators.

### 6.6. MVP release gates

MVP не готов к пилоту, пока не выполнены все gates:

1. **Data trust gate:** stale/offline/bad отображаются корректно, realtime gap тестируется.
2. **Alarm semantics gate:** event, occurrence и incident разделены в БД, API и UI.
3. **Access gate:** negative authorization tests проходят для API и routes.
4. **Audit gate:** значимые действия имеют actor, scope, before/after и correlation.
5. **Publication gate:** runtime никогда не читает draft.
6. **Telemetry gate:** Modbus/SNMP simulators работают длительно без неограниченного backlog.
7. **Recovery gate:** restart worker/API не приводит к повторному alarm transition или потере current state.
8. **Command gate:** production command execution выключен feature/policy gate до отдельной приемки.
9. **QA gate:** обязательные Playwright и integration tests стабильны.
10. **Operational gate:** backup/restore, log rotation, retention и runbook проверены.

---

## 7. Roadmap

### 7.1. Сводный план

| Phase | Название | Ориентир | Результат |
|---|---|---:|---|
| 0 | Подготовка архитектуры и документации | 2–4 недели | Зафиксированы границы, термины, ADR, backlog и test strategy |
| 1 | Foundation | 4 недели | Solution, CI, auth/RBAC, shell, PostgreSQL baseline |
| 2 | Asset/Telemetry MVP | 8 недель | Локации, equipment, DataPoints, Modbus/SNMP, current/history |
| 3 | Events/Alarms/Realtime | 6 недель | SignalR, rules, occurrences, Диспетчер событий |
| 4 | Dashboard runtime | 4 недели | Catalog, windows, Live/History, static mimic/widgets |
| 5 | Maintenance baseline | 4 недели | MaintenanceObject, plans, requests, work orders |
| 6 | Admin/Health/Data quality | 4 недели | Admin settings, health, quality, audit views |
| 7 | Command safety | 4–6 недель | Control mode, preflight, gateway, pilot gate |
| 8 | Extended product | Последующие релизы | Editors, notifications, reports, kiosk, richer incidents/CMMS |
| 9 | C++ evaluation | После production metrics | Решение по extraction на основе профилирования |

Сроки являются плановым порядком и уточняются после sizing пилотного объема. Фазы могут частично пересекаться только при сохранении зависимостей и release gates.

### 7.2. Phase 0 — подготовка архитектуры и документации

**Цель.** Превратить продуктовые материалы в однозначные контракты и управляемый backlog.

**Результат.** Утвержденный glossary, context map, solution skeleton, ADR и pilot sizing assumptions.

**Основные задачи.**

- утвердить канонические сущности и независимые lifecycle;
- согласовать пилотные локации, equipment и protocol devices/simulators;
- описать permission actions/scopes;
- определить telemetry sample envelope и quality mapping;
- определить alarm rule MVP types;
- определить publication revision contract;
- утвердить API error model, correlation и audit fields;
- подготовить threat model для auth/control/kiosk;
- подготовить CI quality gates и test pyramid;
- составить data retention и backup assumptions;
- создать ADR по Blazor hosting model, PostgreSQL schemas и process boundaries.

**Зависимости.** Доступность product owner, инженерных экспертов, security и эксплуатационной команды.

**Acceptance criteria.**

- нет конфликтующих терминов Event/AlarmOccurrence/Incident;
- каждый MVP use case привязан к context, API и route;
- C++ candidates имеют contract boundary, но не включены в разработку;
- открытые вопросы имеют owner и срок решения;
- backlog оценим на уровне epics/features.

**Не делать.** Не начинать visual editor, не выбирать C++ library, не создавать Kubernetes/microservice platform без требования.

### 7.3. Phase 1 — foundation

**Цель.** Создать production-grade основу приложения.

**Результат.** Запускаемый solution, миграции, authentication/RBAC, shell и observability baseline.

**Основные задачи.**

- repositories, build, CI, environments и configuration validation;
- ASP.NET Core API и Blazor Web client;
- PostgreSQL schemas/migrations;
- user/role/scope/effective policy;
- `/home`, shell, rail, header, context drawer framework;
- standard list/filter/pagination/error/loading components;
- audit pipeline и outbox baseline;
- structured logs, metrics, liveness/readiness;
- seed users/roles.

**Зависимости.** Phase 0 ADR.

**Acceptance criteria.**

- user видит только разрешенные routes;
- прямой forbidden API/URL возвращает корректный 403;
- shell соответствует one-header/one-rail/one-drawer principle;
- migrations и seed выполняются автоматически в test environment;
- audit записывает role assignment change;
- CI запускает unit/integration/smoke tests.

**Не делать.** Не строить generalized workflow engine, не добавлять protocol logic в API.

### 7.4. Phase 2 — asset/telemetry MVP

**Цель.** Получить достоверные данные от реальных или симулированных устройств.

**Результат.** Location/Equipment/DataPoint model, Modbus/SNMP polling, current/history и diagnostics.

**Основные задачи.**

- locations, equipment types, equipment registry/card;
- universal staging manual/CSV;
- TelemetrySource/DataPoint configuration;
- protocol-neutral contracts;
- Modbus adapter и simulator tests;
- SNMP adapter и simulator tests;
- polling orchestrator, leases, backoff;
- ingestion, current value, history batching;
- quality/freshness evaluator;
- trends API baseline;
- data point/source diagnostics.

**Зависимости.** Foundation, source test environment, mapping examples.

**Acceptance criteria.**

- values содержат обязательные metadata;
- source offline переводит values в stale/offline по policy;
- connection/poll diagnostics не являются Apply gate;
- CSV не удаляет existing equipment;
- history query возвращает quality и timestamps;
- worker restart не создает duplicate current/history beyond declared dedup policy.

**Не делать.** Не реализовывать OPC UA/BACnet без отдельного решения; не оптимизировать на C++.

### 7.5. Phase 3 — events/alarms/realtime

**Цель.** Сформировать доверенный операторский поток событий и alarm occurrences.

**Результат.** Rule evaluation, lifecycle alarms, SignalR resync и полноценный базовый `/events`.

**Основные задачи.**

- SignalR subscriptions/snapshot/sequence/gap;
- threshold/state/freshness rules;
- immutable EventRecord;
- AlarmOccurrence open/update/clear;
- acknowledgement, assignment, shelving;
- Realtime/History modes;
- stable-list behavior с `Новых: N`;
- bulk preflight и partial result;
- links к equipment/dashboard/trend;
- minimal incident creation;
- in-app notification projection.

**Зависимости.** Telemetry quality/current values и RBAC.

**Acceptance criteria.**

- ack не clear и не assign;
- History запрещает actions;
- reconnect выполняет resnapshot и показывает gap;
- duplicate sample/rule evaluation не создает duplicate occurrence;
- alarm timeline и audit согласованы;
- UI фильтры и URL восстанавливаются.

**Не делать.** Не реализовывать full incident workflow или сложную escalation policy.

### 7.6. Phase 4 — dashboard runtime

**Цель.** Перенести ключевой операторский UX прототипа на реальные данные.

**Результат.** Dashboard catalog/runtime, windows, linked context, static mimic и widgets.

**Основные задачи.**

- Dashboard/DashboardWindow/Widget/MimicDiagram model;
- published revision read model;
- catalog, favorites, last dashboard;
- URL `/d/{dashboard}/{screen}`;
- Live/History state и visible period;
- selected object context;
- synchronized mimic/faceplate/trend/events;
- fullscreen/canvas-only runtime;
- static SVG bindings;
- unavailable/forbidden reference handling.

**Зависимости.** Telemetry, Events, publication baseline.

**Acceptance criteria.**

- Back/Forward восстанавливает exact window;
- first click selects object only;
- History blocks commands;
- runtime читает одну published revision;
- drawer close не сбрасывает selected object;
- unavailable source не раскрывается через hidden links.

**Не делать.** Не строить visual editors и arbitrary scripting.

### 7.7. Phase 5 — maintenance baseline

**Цель.** Доказать независимость ТОиР и обеспечить минимальный рабочий lifecycle.

**Результат.** Maintenance objects, plans, forecast, requests и work orders.

**Основные задачи.**

- independent MaintenanceObject registry;
- optional link to Equipment;
- basic MaintenancePlan;
- forecast/calendar generation;
- request from event;
- WorkOrder status transitions;
- checklist and one-next-action UI;
- My Work projection;
- audit and permissions.

**Зависимости.** Identity, Asset links, events.

**Acceptance criteria.**

- maintenance object существует без equipment;
- forecast, work order и personal task различаются;
- request from event не меняет ack;
- work order transition проверяется backend;
- counters фильтруют одну общую очередь.

**Не делать.** Не добавлять inventory/procurement/mobile offline.

### 7.8. Phase 6 — admin/health/data quality

**Цель.** Сделать платформу управляемой и диагностируемой.

**Результат.** Admin workspaces, health, data quality, integrations diagnostics и audit reader.

**Основные задачи.**

- accounts/roles/effective permissions UI;
- system settings inheritance/override;
- impact preview;
- integrations list и masked diagnostics;
- platform component health;
- source/data quality issues;
- assignment/resolution of quality issues;
- read-only audit filters/details;
- operational dashboards and runbooks.

**Зависимости.** Все MVP components публикуют health/metrics.

**Acceptance criteria.**

- health и data quality не появляются как building alarms;
- secrets/endpoints маскируются;
- last scope admin protection работает;
- audit read-only и содержит before/after;
- policy change имеет impact preview.

**Не делать.** Не превращать admin UI в произвольный DB editor.

### 7.9. Phase 7 — command safety

**Цель.** Реализовать безопасный command path и пройти отдельную пилотную приемку.

**Результат.** Control sessions, preflight, gateway, result lifecycle и kill switches.

**Основные задачи.**

- command definitions and mappings;
- control session scope/time/idle expiration;
- preflight authorization/data trust/interlocks;
- reason/confirmation/re-auth/second-user policies;
- idempotent CommandExecution state machine;
- gateway and protocol write path;
- result verification/unknown handling;
- realtime UI and audit;
- emergency disable policy;
- fault-injection and safety review.

**Зависимости.** Stable telemetry, RBAC, audit, realtime, source diagnostics.

**Acceptance criteria.**

- default state is view-only;
- control ends on timeout/logout/scope change/gap;
- duplicate confirm does not duplicate device write;
- unknown result never displays as completed;
- every phase is audited;
- pilot equipment and commands are explicitly allowlisted;
- independent security/safety sign-off completed.

**Не делать.** Не включать commands globally; не использовать UI state как authorization token.

### 7.10. Phase 8 — extended product

**Цель.** Развить MVP до целевого продуктового контура.

**Результат.** Функции выбираются отдельными release trains по ценности и готовности.

**Кандидаты:**

- full Incident workspace;
- Dashboard Editor;
- SVG Editor;
- validation/impact/approval UI;
- advanced notifications, schedules, coverage и escalation;
- reports/documents/shift log;
- richer CMMS и mobile technician profile;
- kiosk enrollment/certificates и wallboard playlists;
- OPC UA/BACnet;
- edge gateway;
- advanced trends and analytics;
- touch/adaptive profiles.

**Зависимости.** Stable MVP, usage feedback, prioritized product backlog.

**Acceptance criteria.** Для каждого release — отдельный scope, migration plan, acceptance tests и backward-compatible published revision.

**Не делать.** Не запускать все расширения одной программой одновременно.

### 7.11. Phase 9 — C++ evaluation and extraction candidates

**Цель.** Решить, нужна ли замена отдельных runtime-компонентов.

**Результат.** Для каждого кандидата — `keep .NET`, `optimize .NET`, `scale out` или `rewrite behind contract`.

**Основные задачи.**

- собрать production workload profiles;
- выполнить load/fault tests;
- профилировать CPU, allocations, I/O, DB и network;
- оптимизировать .NET baseline;
- проверить horizontal partitioning;
- построить isolated C++ proof of concept только для bottleneck;
- прогнать contract/behavior tests;
- оценить total cost of ownership.

**Зависимости.** Достаточный объем production telemetry и agreed SLO.

**Acceptance criteria.** Решение имеет численное обоснование и не требует изменения business API/domain semantics.

**Не делать.** Не переписывать сервис «на всякий случай» и не переносить business logic в C++.

---

## 8. Sprint plan

### 8.1. Планирование

Предполагаются двухнедельные спринты. Спринты 1–16 формируют MVP и pilot gate. Спринты 17–18 — первый post-MVP extension. Команда должна дробить stories до задач не крупнее нескольких дней и не переносить незавершенный vertical slice как «готовый backend» без UI/tests.

### Sprint 1 — Архитектурный baseline и рабочий репозиторий

**Цель.** Получить воспроизводимый solution и согласованные технические контракты.

**User stories.**

- Как разработчик, я могу локально поднять API, Web и PostgreSQL одной documented командой.
- Как архитектор, я вижу утвержденные boundaries, glossary и ADR.
- Как QA, я могу запустить smoke test пустого приложения.

**Backend tasks.** Создать solution/projects; request correlation; global error model; configuration validation; health endpoints; базовый application pipeline.

**Frontend tasks.** Создать Blazor app; route skeleton; global error/loading boundary; theme tokens из прототипа; accessibility baseline.

**Worker/integration tasks.** Создать Worker host template; heartbeat/metrics contract; protocol simulator strategy.

**Database tasks.** PostgreSQL schemas baseline; migration runner; migration history; seed framework.

**Tests.** Build, unit template, PostgreSQL integration test, health smoke, browser start smoke.

**Demo result.** Базовая shell работает, API/DB/worker видны в health.

**Definition of done.** CI green; clean checkout запускается; ADR и local runbook в repo; secrets отсутствуют в config files.

### Sprint 2 — Identity, roles, scopes и shell

**Цель.** Реализовать безопасный вход и навигацию по effective permissions.

**User stories.**

- Пользователь входит и видит доступные сервисы.
- Администратор назначает простую роль и scope.
- Пользователь не может открыть запрещенный URL или API.

**Backend tasks.** User/Role/PermissionScope; authentication; authorization policies; current-user endpoint; role assignment use case; last-admin guard; audit hook.

**Frontend tasks.** Header 48 px, rail 56 px, avatar menu, `/home`, forbidden/not-found states; admin/user route guards как UX, но не security source.

**Worker/integration tasks.** Session cleanup job; optional identity provider abstraction.

**Database tasks.** `iam` tables, unique constraints, seed admin/operator/engineer.

**Tests.** Positive/negative policy integration tests; direct URL e2e; last-admin protection; session revoke.

**Demo result.** Три роли видят разные navigation items и получают 403 при прямом запрещенном запросе.

**Definition of done.** Backend policy на каждом endpoint; role change audited; UI не раскрывает недоступные fixture/data.

### Sprint 3 — Locations и базовая модель оборудования

**Цель.** Создать канонический asset context без протокольного центра.

**User stories.**

- Инженер ведет дерево локаций.
- Оператор видит equipment registry/card в доступной области.
- Администратор связывает equipment с location/type.

**Backend tasks.** Location CRUD/archive; Equipment CRUD/archive; EquipmentType; relations baseline; scoped queries; первый расчет aggregate status.

**Frontend tasks.** `/locations`, `/equipment`, `/equipment/{id}`; table/tree views; context drawer; filters; empty/loading/error states.

**Worker/integration tasks.** Location aggregate recalculation skeleton.

**Database tasks.** `assets.location`, `equipment`, `equipment_type`, relation tables; hierarchy path/index strategy.

**Tests.** Scope filtering, hierarchy cycle prevention, archive behavior, route/back-forward e2e.

**Demo result.** Пользователь переходит location → equipment → card и обратно без потери контекста.

**Definition of done.** Protocol address не хранится в core Equipment fields; access scope применяется ко всем queries.

### Sprint 4 — Equipment staging и DataPoint configuration

**Цель.** Реализовать безопасное добавление источников и точек.

**User stories.**

- Инженер добавляет одну или много staging rows вручную/CSV.
- Existing ID остается `skip/review`, пока admin явно не разрешит update.
- Инженер настраивает DataPoints и protocol mappings.

**Backend tasks.** Import session/rows; structural validation; create/update/skip/draft operations; template snapshots; DataPoint/TelemetrySource APIs; secret reference model.

**Frontend tasks.** `/equipment/add`; dynamic Modbus/SNMP fields; CSV preview; row statuses/errors; Apply summary; masking/warnings; template/copy flows.

**Worker/integration tasks.** Connection test/sample poll command contracts без реального adapter.

**Database tasks.** import tables, telemetry source, data point, protocol config JSONB with schema version, secret references.

**Tests.** CSV comma/semicolon/header aliases; no delete; structural gate; template excludes identity/address/secrets; copy keeps host; Modbus Unit ID range.

**Demo result.** Валидные create/update rows применяются, invalid/skip остаются, deletions=0.

**Definition of done.** Connection/poll status не блокирует Apply; audit содержит import summary, но не secrets.

### Sprint 5 — Modbus TCP polling vertical slice

**Цель.** Получить первые реальные normalized samples.

**User stories.**

- Инженер проверяет Modbus connection и sample read.
- Оператор видит current values тестовой установки.
- Эксплуатация видит source diagnostics.

**Backend tasks.** Polling assignment service; normalized sample ingestion; current-value upsert; source state API.

**Frontend tasks.** Equipment points tab; value/unit/quality/timestamps; diagnostics states; явное представление stale данных.

**Worker/integration tasks.** Modbus adapter worker; grouped reads; timeout/backoff; simulator; orchestrator schedule/lease.

**Database tasks.** current value, source state, assignment revision, ingestion cursor.

**Tests.** Contract tests; datatype/endian/scaling; timeout; reconnect; duplicate sequence; long-run simulator smoke.

**Demo result.** Modbus simulator обновляет несколько DataPoints с source/receive timestamps.

**Definition of done.** Adapter не зависит от domain entities; bounded queues/metrics доступны; restart безопасен.

### Sprint 6 — SNMP polling и history ingestion

**Цель.** Добавить второй protocol и историческое хранение.

**User stories.**

- Инженер опрашивает SNMP v2c/v3 test source.
- Оператор открывает историю параметра.
- Администратор видит masked diagnostics.

**Backend tasks.** History query API; downsampling contract baseline; data retention config; source diagnostics normalization.

**Frontend tasks.** `/trends/tag/{tag}`; period selector; quality markers; SNMP configuration states.

**Worker/integration tasks.** SNMP adapter; OID typing; history batch writer; partition maintenance; retry policy.

**Database tasks.** partitioned historical values; indexes by datapoint/time; retention metadata.

**Tests.** SNMP auth/timeout cases; history batch/dedup; partition creation; secret masking; trend API boundaries.

**Demo result.** Modbus и SNMP values одновременно отображаются, history доступна за период.

**Definition of done.** Ни community, ни v3 secrets не возвращаются/логируются; history writer имеет backpressure metrics.

### Sprint 7 — Quality, freshness и realtime resynchronization

**Цель.** Сделать данные операционно достоверными.

**User stories.**

- Оператор различает Good/Uncertain/Bad/Stale/Offline/Initializing.
- После разрыва realtime UI показывает gap и выполняет resnapshot.
- Командные элементы блокируются при недоверенном состоянии.

**Backend tasks.** Freshness policy; quality mapper; authorized snapshot; sequence/cursor; resync endpoint; source→point state propagation.

**Frontend tasks.** Quality/freshness badges; last known styling; connection banner; `Live/Reconnecting/Gap/Resynchronizing/Offline`; reconnect UX.

**Worker/integration tasks.** Freshness evaluator; heartbeat; source offline detector; simulated gap/fault injection.

**Database tasks.** quality issue materialization; current value version/sequence.

**Tests.** Gap/reconnect SignalR integration; stale timeout; out-of-order samples; client resnapshot Playwright; no false fresh value.

**Demo result.** Отключение simulator переводит данные в stale/offline, reconnect не скрывает возможный gap.

**Definition of done.** Last value visibly non-current; every realtime channel supports sequence/snapshot.

### Sprint 8 — Alarm rules, EventRecord и AlarmOccurrence

**Цель.** Реализовать корректную alarm semantics.

**User stories.**

- Инженер публикует базовое threshold/state rule.
- Оператор получает occurrence при выполнении условия.
- Clearing условия не означает acknowledgement или incident close.

**Backend tasks.** AlarmRule revisions; EventRecord append; occurrence state machine; open/update/clear idempotency; basic rule APIs.

**Frontend tasks.** Initial `/events` list; severity/condition/quality columns; event drawer details/timeline.

**Worker/integration tasks.** Rule evaluator; exact rule revision; hysteresis/delay minimum; duplicate suppression.

**Database tasks.** rules/revisions, events, occurrences, transitions/evidence.

**Tests.** Threshold/hysteresis/state; bad quality policy; duplicate sample; restart; independent ack/condition fields.

**Demo result.** Изменение simulator открывает и clear occurrence с корректным timeline.

**Definition of done.** EventRecord, AlarmOccurrence и rule revision различаются в model/API; transitions audited.

### Sprint 9 — Диспетчер событий и операторские действия

**Цель.** Довести `/events` до рабочего operator flow.

**User stories.**

- Оператор фильтрует Realtime queue и подтверждает occurrence.
- Оператор назначает ответственного или shelve с reason/until.
- Оператор видит новые rows без прыжка списка.

**Backend tasks.** Acknowledge/assign/shelve/unshelve endpoints; bulk preflight/partial result; history read model; permission checks.

**Frontend tasks.** Realtime/History; filters/search/saved URL state; `Новых: N`; selection/bulk bar; action dialogs; linked navigation.

**Worker/integration tasks.** Shelving expiry; optional suppression baseline; realtime event projection.

**Database tasks.** acknowledgement, assignment, shelving records and indexes.

**Tests.** Required ack comment; history no actions; bulk partial; stable list; exact URL restore; permission negatives.

**Demo result.** Полный demo flow occurrence → ack → assign → shelve/unshelve с независимыми состояниями.

**Definition of done.** Все actions имеют distinct audit entries; UI labels не объединяют semantics.

### Sprint 10 — Incidents baseline и «Моя работа»

**Цель.** Связать реакцию с отдельным incident/process context.

**User stories.**

- Оператор создает incident из event без изменения occurrence.
- Координатор видит задачу в `/my-work`.
- Пользователь принимает, передает или возвращает задачу с причиной.

**Backend tasks.** Incident create/update/link; coordinator; task projection; accept/transfer/cannot actions; source authorization.

**Frontend tasks.** Incident summary/page baseline; `/my-work`; counters/type filters; task drawer; source links.

**Worker/integration tasks.** Projection consistency worker; базовые SLA age metrics без workflow automation.

**Database tasks.** incidents, links, participants/timeline, personal work projection.

**Tests.** Create incident preserves ack/condition; counters recalc; transfer removes from personal actionable count; forbidden source hidden.

**Demo result.** Event → Incident → My Work → source navigation.

**Definition of done.** My Work не владеет source state; projection rebuild test существует.

### Sprint 11 — Dashboard catalog и runtime windows

**Цель.** Реализовать адресуемый dashboard runtime на production data.

**User stories.**

- Оператор сначала видит catalog, затем возвращается к последнему dashboard.
- URL открывает конкретный DashboardWindow.
- Live/History и period видны постоянно.

**Backend tasks.** Dashboard/Window published read model; favorites/last route settings; widget data composition; access filtering.

**Frontend tasks.** `/dashboards`; `/d/{dashboard}/{screen}`; window selector; Live/History; fullscreen; right context panel.

**Worker/integration tasks.** Publication cache invalidation; widget read-model refresh.

**Database tasks.** dashboard/window/widget definitions; published revision pointers; user favorites.

**Tests.** Catalog→dashboard→service→last dashboard→all dashboards; Back/Forward exact window; inaccessible dashboard fallback.

**Demo result.** Реальные telemetry/event widgets работают в трех window types.

**Definition of done.** Runtime получает только published revision; URL является каноническим state carrier.

### Sprint 12 — Static mimic, selected context и trends

**Цель.** Реализовать связанный операторский контекст.

**User stories.**

- Оператор выбирает вентилятор/заслонку на mimic.
- Faceplate, trend и events синхронно следуют выбранному объекту.
- Первый клик не выполняет command.

**Backend tasks.** MimicDiagram revision and bindings; context data endpoint; static SVG validation/sanitization; trend-by-context API.

**Frontend tasks.** Mimic runtime; selection highlight; faceplate; short trend; linked events; fullscreen; zoom controls baseline.

**Worker/integration tasks.** Mimic publication materialization.

**Database tasks.** mimic revisions, object bindings, published pointer.

**Tests.** Selection sync; drawer independence; History blocks command preview; invalid binding; SVG security; full-screen restore.

**Demo result.** Прототипный AHU flow работает на backend data.

**Definition of done.** No direct SVG script execution; selected object state устойчив при drawer close.

### Sprint 13 — Audit, platform health и data quality

**Цель.** Обеспечить диагностику и доказуемость действий.

**User stories.**

- Администратор видит health компонентов и sources.
- Инженер находит stale/bad/missing data issues.
- Аудитор просматривает неизменяемый audit.

**Backend tasks.** Admin health aggregation; data quality issue lifecycle; audit query/read permissions; system setting override with impact preview.

**Frontend tasks.** `/admin/health`, `/admin/data-quality`, `/admin/audit`, `/admin/settings`; masked integration details.

**Worker/integration tasks.** Component health collector; data quality scanner; audit integrity check.

**Database tasks.** health observations/read models; quality issues; settings overrides; audit partitions/indexes.

**Tests.** Health ≠ building alarms; secret masking; audit append-only; policy impact; admin direct URL.

**Demo result.** Отключенный adapter отражается в health/data quality, но не создает технологическую пожарную/инженерную аварию.

**Definition of done.** Runbook links и metrics names документированы; audit export respects scope.

### Sprint 14 — Maintenance baseline

**Цель.** Реализовать независимый минимальный ТОиР-контур.

**User stories.**

- Планировщик создает MaintenanceObject без telemetry.
- Из event создается request и личная задача без ack.
- Техник выполняет checklist и следующий переход work order.

**Backend tasks.** MaintenanceObject/Plan/Request/WorkOrder; state transitions; event/equipment links; permissions.

**Frontend tasks.** `/maintenance` sections; assets/plans/calendar/requests/work-orders; work-order page/drawer; counters.

**Worker/integration tasks.** Forecast generator; overdue evaluator; My Work projection.

**Database tasks.** maintenance tables, schedule occurrences, checklist snapshots.

**Tests.** Independent object; forecast vs order vs task; mandatory checklist; one-next-transition; event ack unchanged.

**Demo result.** Maintenance plan и event-derived request проходят до work order.

**Definition of done.** Maintenance module не зависит от TelemetrySource; transitions/audit complete.

### Sprint 15 — Control mode и command preflight preview

**Цель.** Подготовить безопасную command architecture без обязательного physical execution.

**User stories.**

- Оператор включает временный control mode на текущий dashboard.
- Перед командой видит preflight и причины блокировки.
- History/gap/offline запрещают command.

**Backend tasks.** ControlSession; command definitions; preflight rules; policy token; CommandExecution draft/simulated lifecycle; audit.

**Frontend tasks.** Control indicator/timer/off; disabled explanations; command preview; reason/confirm/re-auth policy states; unknown/error states.

**Worker/integration tasks.** Gateway skeleton; simulator executor; timeout reconciliation; kill switch.

**Database tasks.** control sessions, command definitions/executions/results/idempotency.

**Tests.** Scope/time expiry; dashboard change; duplicate confirm; stale/offline/gap; unauthorized; audit phases.

**Demo result.** Command проходит до simulated completed/error/unknown, физическое устройство не меняется.

**Definition of done.** Production execution feature flag/policy off; safety review findings tracked.

### Sprint 16 — MVP hardening и пилотная приемка

**Цель.** Стабилизировать сквозной продукт и подготовить пилот.

**User stories.**

- Оператор выполняет основной shift flow без критических дефектов.
- Эксплуатация восстанавливает систему после restart/backup.
- QA прогоняет acceptance suite из REVIEW.

**Backend tasks.** Query/performance tuning; idempotency review; error consistency; retention/backup/restore; migration upgrade test.

**Frontend tasks.** Accessibility; keyboard/focus; loading/error/empty states; layout at 1920×1080; responsive fallback; UX defect fixes.

**Worker/integration tasks.** Soak/fault tests; reconnect storms; adapter backoff; backlog recovery.

**Database tasks.** Index review; partition automation; backup/restore rehearsal; seed/demo package.

**Tests.** Full integration/e2e/security/realtime/data-quality suites; load baseline; smoke release package.

**Demo result.** Production-like environment проходит end-to-end сценарий с Modbus/SNMP simulators и отказами.

**Definition of done.** All MVP gates passed; critical/high defects closed; runbooks and release notes approved.

### Sprint 17 — In-app notifications и персональные настройки (post-MVP)

**Цель.** Разделить personal inbox, subscriptions и mandatory policies.

**User stories.** Пользователь читает notification без acknowledgement; настраивает дополнительные subscriptions/channels; vacation активируется только после coverage.

**Backend tasks.** Notification delivery/read; subscriptions; schedules/quiet hours; coverage baseline.

**Frontend tasks.** Header inbox, `/notifications`, `/settings/notifications`; channel test states.

**Worker/integration tasks.** Delivery/digest worker; retry; in-app only production baseline.

**Database tasks.** policies/subscriptions/deliveries/read state/schedules.

**Tests.** Read ≠ ack; inaccessible source; critical during quiet hours; coverage acceptance.

**Demo result.** Персональный notification flow соответствует prototype decisions.

**Definition of done.** Mandatory delivery cannot be disabled; source authorization rechecked on open.

### Sprint 18 — Publication workflow и editor-ready backend (post-MVP)

**Цель.** Подготовить безопасную основу для будущих visual editors.

**User stories.** Конфигуратор сохраняет revision, validates, видит impact и публикует atomically; rollback создает новую revision.

**Backend tasks.** ConfigurationDocument/Revision; validation binding; impact; approval hook; publish/rollback; optimistic concurrency.

**Frontend tasks.** Не visual editor, а admin/config revision screens и dialogs; version history; leave-dirty UX contract prototype.

**Worker/integration tasks.** Validator/materializer; publication event; cache invalidation.

**Database tasks.** revision snapshots, validations, publications, approvals, immutable version history.

**Tests.** Unsaved/changed-after-validation cannot publish; atomic runtime pointer; rollback history; concurrent edit.

**Demo result.** JSON/SVG configuration package проходит полный lifecycle без visual editing.

**Definition of done.** Runtime остается на предыдущей revision при любой publication failure.

---

## 9. Модель данных

### 9.1. Общие правила модели

- Идентификаторы — стабильные UUID/ULID либо внутренние UUID с отдельным человекочитаемым code; UI не должен зависеть от sequence ID.
- Время хранится в UTC, а timezone применяется на presentation/schedule boundary.
- Для значимых сущностей обязательны `created_at`, `created_by`, `updated_at`, concurrency token и archive state, если удаление допустимо.
- Hard delete запрещен для audit, event/alarm lifecycle, command execution, publications и завершенных work orders.
- Protocol-specific configuration versioned и хранится отдельно от product identity.
- Secrets представлены только `secret_reference`; read API возвращает признак `configured`, но не значение.
- Access scope вычисляется по location/discipline/entity links и не хранится одним флагом `is_allowed`.
- Status и condition разделяются, когда это разные понятия.
- Все ссылки на source entity проходят authorization при чтении.

### 9.2. Identity & Access

| Сущность | Назначение | Ключевые поля | Связи | Lifecycle/status | Audit и access |
|---|---|---|---|---|---|
| `User` | Учетная запись для входа и действий | `id`, `login`, `display_name`, `email`, `status`, `identity_provider`, `external_subject`, `last_login_at` | RoleAssignments, Sessions, EmployeeProfile | Invited/Active/Blocked/Disabled | Создание, block/unblock, provider link; доступ к профилю отдельно от admin account data |
| `Role` | Именованный набор permission actions | `id`, `code`, `name`, `description`, `is_system`, `version` | Permissions, RoleAssignments | Draft/Active/Archived для custom roles | Изменения с impact preview; system role нельзя незаметно переопределить |
| `PermissionScope` | Область действия назначения | `id`, `scope_type`, `organization_id`, `location_id`, `discipline`, `entity_type`, `entity_id`, `valid_from`, `valid_to` | RoleAssignment | Active/Expired/Revoked | Источник назначения и область показываются в effective permissions |
| `RoleAssignment` | Связь User–Role–Scope | `id`, `user_id`, `role_id`, `scope_id`, `source`, `granted_by`, `valid_from/to` | User, Role, Scope | Active/Expired/Revoked | Before/after, grant source, last-scope-admin guard |
| `EmployeeProfile` | Рабочая идентичность сотрудника, отдельная от account | `user_id`, `title`, `department`, `specialization`, `about`, `preferred_contact`, `visibility_policy` | User, activities/links | Active/Hidden/Former | Пользователь редактирует только разрешенные поля; viewer-specific visibility |
| `Session` | Серверный сеанс и re-auth context | `id`, `user_id`, `device`, `started_at`, `last_seen_at`, `expires_at`, `revoked_at`, `auth_strength` | User | Active/Expired/Revoked | Завершение удаленного сеанса; данные устройства ограничены правами |

### 9.3. Locations & Asset Model

| Сущность | Назначение | Ключевые поля | Связи | Lifecycle/status | Audit и access |
|---|---|---|---|---|---|
| `Location` | Физическая иерархия организации | `id`, `code`, `name`, `parent_id`, `location_type`, `timezone`, `path`, `status` | Parent/children, Equipment, MaintenanceObject | Active/Inactive/Archived | Перемещение в иерархии, old/new parent; scope наследуется по дереву |
| `Equipment` | Пользовательская сущность устройства/установки | `id`, `code`, `name`, `equipment_type_id`, `location_id`, `discipline`, `operational_state`, `criticality`, `owner_id`, `archived_at` | DataPoints, TelemetrySources, relations, optional MaintenanceObject | Draft/Active/Disabled/Archived; operational state отдельно | Изменения identity/location/type; protocol details скрыты от operator by permission |
| `EquipmentType` | Классификация и допустимые шаблоны параметров | `id`, `code`, `name`, `discipline`, `schema_version` | Equipment, DataPoint templates | Draft/Active/Archived | Publication/versioning при влиянии на шаблоны |
| `AssetRelation` | Именованная связь между объектами | `id`, `from_type/id`, `relation_type`, `to_type/id`, `valid_from/to` | Location/Equipment/MaintenanceObject | Active/Expired | Создание/удаление связи; обе стороны должны быть доступны по scope |
| `EquipmentImportSession` | Единый staging ручных/CSV строк | `id`, `created_by`, `source_type`, `status`, `summary` | ImportRows | Open/Validated/PartiallyApplied/Completed/Cancelled | File metadata и summary; secrets не сохраняются в raw log |
| `EquipmentImportRow` | Редактируемый staging snapshot | `id`, `session_id`, `operation`, `payload`, `validation_errors`, `allow_update`, `applied_entity_id` | ImportSession, optional Equipment | Draft/Create/Update/Skip/Invalid/Applied | Кто разрешил update; отсутствует delete operation |

### 9.4. Telemetry

| Сущность | Назначение | Ключевые поля | Связи | Lifecycle/status | Audit и access |
|---|---|---|---|---|---|
| `DataPoint` | Каноническая измеряемая/состояниевая/командная точка | `id`, `equipment_id`, `code`, `name`, `value_type`, `unit`, `semantic_type`, `readable`, `writable`, `freshness_policy_id`, `enabled` | Equipment, SourceMapping, CurrentValue, AlarmRules | Draft/Active/Disabled/Archived | Mapping/scaling/unit changes versioned; read/write permission may differ |
| `TelemetrySource` | Технический endpoint и poll profile | `id`, `equipment_id`, `adapter_type`, `endpoint_masked`, `protocol_config`, `secret_reference`, `poll_profile`, `config_revision`, `enabled` | DataPoint mappings, SourceState | Draft/Active/Disabled/Error/Archived | Endpoint masked; secret never audited; activate/deactivate audited |
| `DataPointSourceMapping` | Protocol-neutral link к technical address | `id`, `data_point_id`, `source_id`, `address_payload`, `scaling`, `priority`, `revision` | DataPoint, Source | Draft/Published/Archived | Address detail доступен инженеру; exact revision в sample evidence |
| `CurrentValue` | Последнее принятое состояние точки | `data_point_id`, `typed_value`, `value_text`, `unit`, `quality`, `source_timestamp`, `received_at`, `fresh_until`, `source_id`, `sequence`, `version` | DataPoint, Source | Current/Stale/Offline represented by quality/freshness | Не редактируется пользователем; access наследуется от DataPoint/Equipment |
| `HistoricalValue` | Append-only история | `data_point_id`, `timestamp`, `received_at`, `typed_value`, `quality`, `source_id`, `sequence`, `config_revision` | DataPoint, Source | Append-only; retention/compaction | Raw row не редактируется; query permission по source DataPoint |
| `DataQuality` | Каноническая оценка доверия | `code`, `rank`, `is_usable`, `is_current`, `description` | Current/Historical values | `Good`, `Uncertain`, `Bad`, `Stale`, `Offline`, `Initializing` | Справочник versioned; UI mapping централизован |
| `FreshnessPolicy` | Правило перехода к stale/offline | `id`, `expected_interval`, `stale_after`, `offline_after`, `quality_on_timeout`, `revision` | DataPoint/Source | Draft/Published/Retired | Изменение влияет на alarms/UI и требует audit/impact |
| `SourceConnectionState` | Текущее состояние endpoint/runtime | `source_id`, `state`, `last_success_at`, `last_error_at`, `error_class`, `latency_ms`, `adapter_instance`, `config_revision` | TelemetrySource | Initializing/Online/Degraded/Offline/Disabled | Technical details scoped to engineer/admin; no secrets |

### 9.5. Events & Alarms

| Сущность | Назначение | Ключевые поля | Связи | Lifecycle/status | Audit и access |
|---|---|---|---|---|---|
| `EventRecord` | Неизменяемый факт или запись журнала | `id`, `event_type`, `severity`, `occurred_at`, `recorded_at`, `location_id`, `equipment_id`, `data_point_id`, `message`, `payload`, `source_correlation_id` | Optional AlarmOccurrence/Incident/WorkRequest | Append-only | Source evidence и creator; payload filtered by access |
| `AlarmRule` | Идентичность аварийного правила | `id`, `code`, `name`, `scope`, `enabled`, `published_revision_id` | RuleRevisions, Occurrences | Draft/Active/Disabled/Archived | Enable/disable и publication audited; access по location/discipline |
| `AlarmRuleRevision` | Exact evaluated конфигурация | `id`, `alarm_rule_id`, `version`, `condition_type`, `condition_payload`, `severity`, `delay`, `hysteresis`, `quality_policy`, `created_by` | AlarmRule, occurrences evidence | Draft/Validated/Published/Retired | Immutable после publication |
| `AlarmOccurrence` | Экземпляр/период аварийного условия | `id`, `alarm_rule_id/revision_id`, `opened_at`, `last_changed_at`, `cleared_at`, `condition_state`, `severity`, `source_event_id`, `active_assignment_id` | Events, Acks, assignments, shelving, incidents | Open/Active/Cleared; ack state отдельно | Все transitions; scope по связанному объекту |
| `AlarmAcknowledgement` | Факт подтверждения оператором | `id`, `occurrence_id`, `acknowledged_by`, `acknowledged_at`, `comment`, `policy_revision` | Occurrence | Active; возможна повторная ack policy, но не overwrite | Actor/comment/re-auth evidence; отдельное permission action |
| `AlarmAssignment` | Ответственность за reaction | `id`, `occurrence_id`, `assignee_type/id`, `assigned_by`, `assigned_at`, `ended_at`, `reason` | Occurrence/User/Team | Active/Ended | Передача сохраняет историю; access к assignee directory scoped |
| `AlarmShelving` | Временное исключение из active reaction queue | `id`, `occurrence_id`, `reason`, `starts_at`, `ends_at`, `created_by`, `ended_by` | Occurrence | Scheduled/Active/Expired/Cancelled | Reason/until mandatory; permission independent from ack |
| `AlarmSuppression` | Политическое/зависимое подавление | `id`, `occurrence_id/rule_id`, `suppression_type`, `cause`, `starts_at`, `ends_at`, `policy_revision` | Occurrence/Rule/dependency | Active/Expired/Cancelled | Источник suppression visible; не удаляет occurrence |

### 9.6. Incidents

| Сущность | Назначение | Ключевые поля | Связи | Lifecycle/status | Audit и access |
|---|---|---|---|---|---|
| `Incident` | Отдельный процесс координации ситуации | `id`, `number`, `title`, `priority`, `status`, `location_id`, `coordinator_id`, `created_at`, `closed_at`, `resolution` | Links, participants, tasks, timeline | New/Active/Monitoring/Resolved/Closed/Cancelled | Create/status/priority/coordinator/close reason; scope по links/location |
| `IncidentLink` | Связь incident с source/context | `id`, `incident_id`, `linked_type`, `linked_id`, `relation`, `created_by` | Event/Occurrence/Equipment/Document/WorkOrder | Active/Removed with history | Link/unlink audited; target authorization rechecked |
| `IncidentTimelineEntry` | Неизменяемая хронология | `id`, `incident_id`, `entry_type`, `actor_id`, `occurred_at`, `text`, `payload` | Incident | Append-only | Viewer filtering не должен менять original record |

### 9.7. Dashboards & Mimics

| Сущность | Назначение | Ключевые поля | Связи | Lifecycle/status | Audit и access |
|---|---|---|---|---|---|
| `Dashboard` | Идентичность операторского приложения | `id`, `code`, `name`, `purpose`, `location_scope`, `owner`, `published_revision_id`, `archived_at` | Windows, revisions, favorites | Draft/Published/Archived | Publication/owner/scope changes; dashboard access independent from editor permission |
| `DashboardWindow` | Каноническое адресуемое окно (`screen` alias) | `id`, `dashboard_id`, `code`, `name`, `window_type`, `order`, `configuration`, `default_context` | Widgets, optional MimicDiagram | Draft/Published via parent revision | URL code stable; changes only through revision |
| `Widget` | Runtime component with explicit inputs | `id`, `window_id`, `widget_type`, `layout`, `bindings`, `settings`, `visibility_policy` | DataPoints/queries/context | Part of revision | No hidden arbitrary widget chain; binding access validated at runtime |
| `MimicDiagram` | Versioned SVG/mimic identity | `id`, `code`, `name`, `published_revision_id`, `scope` | Revisions, bindings, windows | Draft/Published/Archived | Upload/source hash/sanitization results audited |
| `MimicObjectBinding` | Связь SVG object с Equipment/context/actions | `id`, `mimic_revision_id`, `object_key`, `equipment_id`, `data_bindings`, `command_definition_ids` | Mimic revision, Equipment | Part of immutable revision | Forbidden target не раскрывается; command binding не grants permission |
| `ConfigurationRevision` | Immutable saved snapshot | `id`, `document_type/id`, `version`, `content`, `content_hash`, `created_by`, `created_at`, `validation_id` | Dashboard/Mimic/Rules, Publication | Saved/Validated/Rejected/Published/Retired | Changed-after-validation creates new revision/hash |
| `Publication` | Atomic switch of runtime revision | `id`, `document_id`, `from_revision_id`, `to_revision_id`, `status`, `requested_by`, `published_at`, `failure` | Revision, impact/approval | Pending/Succeeded/Failed/RolledForward | Audit before/after; rollback creates new publication |

### 9.8. Maintenance / CMMS

| Сущность | Назначение | Ключевые поля | Связи | Lifecycle/status | Audit и access |
|---|---|---|---|---|---|
| `MaintenanceObject` | Самостоятельный объект обслуживания | `id`, `code`, `name`, `location_id`, `object_type`, `criticality`, `owner`, `equipment_id nullable`, `status` | Plans, requests, work orders | Active/Inactive/Archived | Link/unlink equipment; maintenance scope independent from telemetry |
| `MaintenancePlan` | Повторяющийся регламент | `id`, `maintenance_object_id`, `name`, `schedule`, `procedure_id`, `lead_time`, `enabled`, `published_revision` | Forecasts, WorkOrders | Draft/Active/Paused/Archived | Schedule/procedure changes; publish exact plan revision |
| `MaintenanceForecast` | Рассчитанная будущая работа, еще не наряд | `id`, `plan_id`, `planned_for`, `status`, `generation_version` | Optional generated WorkOrder | Planned/Converted/Skipped/Cancelled | Conversion/skip reason; access by object scope |
| `MaintenanceRequest` | Заявка/дефект от пользователя или event | `id`, `number`, `type`, `source_type/id`, `maintenance_object_id`, `priority`, `description`, `status`, `created_by` | Event, WorkOrder | New/Triaged/Accepted/Rejected/Converted/Closed | Source link and disposition; event state unchanged |
| `WorkOrder` | Исполняемая работа | `id`, `number`, `maintenance_object_id`, `plan_id`, `request_id`, `assignee`, `priority`, `status`, `planned_start/end`, `actual_start/end`, `procedure_snapshot` | Checklist, tasks, materials later | Draft/Assigned/Accepted/InProgress/OnReview/Completed/Cancelled | Every transition, assignee and checklist evidence; transition permission scoped |
| `ChecklistItem` | Обязательный пункт procedure snapshot | `id`, `work_order_id`, `order`, `text`, `required`, `result`, `completed_by/at` | WorkOrder | Pending/Completed/Failed/NotApplicable with reason | Result changes audited; required gate before transition |

### 9.9. Notifications

| Сущность | Назначение | Ключевые поля | Связи | Lifecycle/status | Audit и access |
|---|---|---|---|---|---|
| `Notification` | Доставленное пользователю сообщение | `id`, `recipient_id`, `source_type/id`, `title`, `body`, `severity`, `delivered_at`, `read_at`, `expires_at` | User, Delivery attempts | Unread/Read/Expired | Read audit optional; source access rechecked; read не меняет source |
| `NotificationPolicy` | Обязательное/ролевое правило доставки | `id`, `name`, `scope`, `condition`, `channels`, `mandatory`, `schedule_policy`, `revision` | Deliveries/subscriptions | Draft/Active/Paused/Archived | Mandatory cannot be disabled by user; publication audited |
| `NotificationSubscription` | Дополнительное личное правило | `id`, `user_id`, `scope`, `condition`, `channels`, `schedule`, `enabled` | User | Active/Paused/Deleted | User owns only allowed fields; cannot broaden source access |
| `NotificationDelivery` | Попытка по каналу | `id`, `notification_id`, `channel`, `attempt`, `status`, `provider_ref`, `sent_at`, `error_class` | Notification | Pending/Sent/Delivered/Failed/DeadLetter | Provider payload masked; retries traceable |
| `CoverageAssignment` | Замещение на отпуск/отсутствие | `id`, `from_user`, `to_user`, `period`, `status`, `accepted_at` | Users, schedule | Draft/Pending/Accepted/Rejected/Expired | Vacation policy active only after Accepted |

### 9.10. Admin, Audit и Terminals

| Сущность | Назначение | Ключевые поля | Связи | Lifecycle/status | Audit и access |
|---|---|---|---|---|---|
| `AuditEntry` | Неизменяемая запись значимого действия | `id`, `occurred_at`, `actor_type/id`, `device_id`, `action`, `object_type/id`, `scope`, `result`, `reason`, `before`, `after`, `correlation_id`, `ip/session` | Any audited object | Append-only | Только read; field-level masking; scoped export |
| `SystemSetting` | Наследуемая системная политика | `key`, `scope`, `value`, `source_scope`, `is_override`, `revision` | Impact/Publication | Draft/Active/Retired | Before/after, impacted objects, preserved exceptions |
| `PlatformComponent` | Регистр наблюдаемых компонентов | `id`, `component_type`, `instance`, `version`, `environment` | Health observations | Registered/Retired | Admin only |
| `HealthObservation` | Техническая health snapshot | `component_id`, `state`, `observed_at`, `last_success`, `metrics`, `detail` | Component | Healthy/Degraded/Unhealthy/Unknown | Не создает building AlarmOccurrence по умолчанию |
| `DataQualityIssue` | Диагностируемая проблема потока данных | `id`, `issue_type`, `source_id/data_point_id`, `severity`, `opened_at`, `status`, `assigned_to`, `resolved_at`, `evidence` | Source/Point | Open/Assigned/Investigating/Resolved/Ignored with reason | Assignment/resolution audited; access by source scope |
| `Terminal` | Физическая панель/device identity | `id`, `device_identity_id`, `name`, `location_id`, `status`, `profile_id`, `dashboard_assignment`, `last_seen_at`, `config_version` | KioskProfile, Enrollment, Audit | Pending/Active/Blocked/Revoked/Offline | Block/revoke/rebind; device identity immutable after enrollment |
| `KioskProfile` | Общая политика kiosk | `id`, `name`, `auth_policy`, `allowed_actions`, `control_timeout`, `shell_mode`, `revision` | Terminals | Draft/Active/Archived | Profile change impacts many terminals; no user identity included |
| `EnrollmentToken` | Одноразовое присоединение terminal | `id`, `code_hash`, `expires_at`, `status`, `requested_metadata`, `approved_by`, `terminal_id` | Terminal | Issued/Used/Expired/Revoked | Code plaintext not stored; approval audited |

### 9.11. Индексы, partitioning и retention

Минимальные требования:

- `HistoricalValue`: partition by UTC time; index `(data_point_id, timestamp desc)`; optional BRIN по timestamp для больших partitions;
- `EventRecord`: partition by occurred date при объеме; indexes по location, equipment, severity, type;
- `AlarmOccurrence`: partial index активных occurrences; уникальная идемпотентная связь с rule/state key;
- `AuditEntry`: time partitions, indexes actor/object/correlation; append-only DB role;
- `Notification`: recipient + unread partial index;
- `WorkOrder`: status/assignee/due date indexes;
- `CurrentValue`: primary key `data_point_id`; update batch with version/sequence guard;
- retention задается отдельно для raw history, aggregated history, events, audit, notifications и diagnostics;
- audit/command/publication retention не сокращается без отдельной policy и нормативной проверки.

---

## 10. API map

### 10.1. Общие требования API

- REST endpoints используют versioned contract policy; первая версия может находиться под `/api`, но breaking change требует `/api/v2` или explicit media version.
- Все mutating requests поддерживают correlation ID; опасные/idempotent operations — `Idempotency-Key`.
- List endpoints имеют paging, filter, sort и stable cursor там, где поток меняется часто.
- Ошибки возвращают единый `problem details` contract с machine code, correlation ID и field errors.
- ETag/concurrency token обязателен для editable configuration.
- Authorization выполняется до чтения/изменения target; filtering не заменяет action check.
- Realtime является ускоренной доставкой committed state, а не альтернативой REST snapshot.

### 10.2. Identity and access

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/auth` | Login/logout/re-auth integration | sign-in callback, logout, re-auth challenge/confirm |
| `/api/me` | Текущий пользователь | profile, effective permissions, preferences, sessions |
| `/api/users` | Accounts и employee directory | list/get/create/invite/block; viewer-filtered profile |
| `/api/roles` | Roles и permissions | list/get/create/update/archive; impact preview |
| `/api/permission-scopes` | Areas/scopes | list/create/validate scope |
| `/api/role-assignments` | Назначения | grant/revoke/expire; last-admin protection |
| `/api/access/effective` | Explain access | permissions for user/object/action and assignment source |
| `/api/sessions` | Сеансы | list own/admin scoped, revoke |

### 10.3. Locations and equipment

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/locations` | Physical hierarchy | tree/list/get/create/update/move/archive; aggregate state |
| `/api/equipment` | Registry/card | list/get/create/update/archive; parameters/events/links summaries |
| `/api/equipment-types` | Classification | list/get/manage templates/schema revisions |
| `/api/asset-relations` | Named graph links | list by object, create, expire/remove |
| `/api/equipment-imports` | Universal staging | create session; upload CSV; add/edit rows; validate; diagnostics; apply; summary |
| `/api/equipment-templates` | Safe configuration templates | list/create/apply/delete; no identity/address/secrets |

### 10.4. Telemetry and values

Целевое имя — `/api/values`, а не `/api/tag-values`, потому что product entity — `DataPoint`, а «tag» является допустимым техническим alias.

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/telemetry-sources` | Endpoint/poll configuration | list/get/create/update/enable/disable; masked config |
| `/api/data-points` | DataPoint definitions | list/get/create/update/archive; mappings; freshness policy |
| `/api/values/current` | Current state | batch by point/equipment/location/dashboard context; snapshot cursor |
| `/api/values/history` | Time series | query point(s), range, resolution, quality; export later |
| `/api/telemetry/diagnostics` | Informational diagnostics | connection test, sample poll, source state, adapter detail |
| `/api/data-quality` | Quality read API alias | point/source summary; canonical admin group below |

### 10.5. Events and alarms

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/events` | Immutable event records and query | list/get; realtime snapshot; history filters; linked context |
| `/api/alarm-rules` | Rule identities/revisions | list/get/create revision/validate/publish/enable/disable |
| `/api/alarms` | Alarm occurrence facade | list active/history/get; preferred internal name `alarm-occurrences` |
| `/api/alarm-occurrences` | Canonical occurrence API | list/get/timeline; active counts; bulk preflight |
| `/api/alarm-occurrences/{id}/acknowledgements` | Ack | create acknowledgement with comment/re-auth evidence |
| `/api/alarm-occurrences/{id}/assignments` | Responsibility | assign/transfer/end |
| `/api/alarm-occurrences/{id}/shelvings` | Temporary shelving | create/cancel; reason/until mandatory |
| `/api/alarm-occurrences/{id}/suppressions` | Suppression | create/cancel/query source, permission controlled |
| `/api/alarm-occurrences/bulk-preflight` | Safe bulk actions | classify eligible/ineligible and reasons |

### 10.6. Incidents

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/incidents` | Incident lifecycle | list/get/create/update priority/status/coordinator |
| `/api/incidents/{id}/links` | Context links | add/remove/list events, occurrences, equipment, work orders, docs |
| `/api/incidents/{id}/participants` | Participants | add/remove/role change |
| `/api/incidents/{id}/timeline` | Timeline | list/add comment/action record |
| `/api/incidents/{id}/close` | Explicit close | preflight, resolution, close; no alarm side effect |

### 10.7. Dashboards and mimics

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/dashboards` | Catalog and runtime metadata | list/get published; favorites; last-accessed route |
| `/api/dashboards/{id}/windows` | Addressable windows | list/get published window by code; context schema |
| `/api/widgets` | Widget definitions/data composition | get published definition; authorized data query |
| `/api/mimics` | Mimic metadata/runtime | get published SVG package/bindings; sanitize status |
| `/api/dashboard-context` | Explicit context composition | resolve selected equipment, linked trend/events/faceplate |
| `/api/configuration-revisions` | Saved versions | create/get/list/validate/impact |
| `/api/publications` | Atomic publication | publish/get status/rollback-as-new-publication |

### 10.8. Maintenance

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/maintenance/objects` | Maintenance registry | list/get/create/update/archive/link equipment |
| `/api/maintenance/plans` | Preventive plans | list/get/create revision/activate/pause |
| `/api/maintenance/forecast` | Calculated schedule | list/regenerate/skip/convert to work order |
| `/api/maintenance/requests` | Requests/defects | create/list/triage/accept/reject/convert |
| `/api/maintenance/work-orders` | Work order lifecycle | list/get/create/assign/accept/start/submit/complete/cancel |
| `/api/maintenance/work-orders/{id}/checklist` | Procedure execution | complete/fail/not-applicable with evidence |
| `/api/maintenance/procedures` | Versioned procedures | list/get/create revision/publish |

### 10.9. Notifications and personal work

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/notifications` | Personal inbox | list/get/mark read/mark all read/open source after recheck |
| `/api/notification-settings` | Personal schedule/channels | get/update optional settings; test channel |
| `/api/notification-subscriptions` | Additional rules | list/create/update/delete/pause |
| `/api/notification-policies` | Admin mandatory policies | list/revision/publish/impact |
| `/api/coverage-assignments` | Vacation coverage | request/accept/reject/cancel |
| `/api/my-work` | Projection of actionable items | list/counts/filters/get source link |
| `/api/my-work/{id}/actions` | Personal task decisions | accept/transfer/cannot with reason |

### 10.10. Admin, health, audit and terminals

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/admin/settings` | System settings | effective/inherited values; preview override; apply/reset |
| `/api/admin/integrations` | Connections | list/get masked/test/enable/disable |
| `/api/health` | Machine endpoints | `/live`, `/ready`, optional component summary for ops |
| `/api/health/components` | Admin health model | list/get history/current state |
| `/api/data-quality/issues` | Data quality workflow | list/get/assign/resolve/ignore with reason |
| `/api/audit` | Read-only audit | filter/get/export scoped; no update/delete |
| `/api/terminals` | Terminal registry | list/get/block/unblock/revoke/rebind/heartbeat detail |
| `/api/kiosk-profiles` | Shared profiles | list/get/create revision/publish/archive |
| `/api/terminal-enrollments` | Enrollment | issue/status/approve/revoke; one-time token |
| `/api/terminals/{id}/assignment` | Independent assignment | set dashboard/window; get effective config |

### 10.11. Commanding

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/control-sessions` | Temporary control mode | create/preflight, get current, extend if policy, terminate |
| `/api/commands/definitions` | Available commands | list by target and current effective policy |
| `/api/commands/preflight` | Full safety check | rights, scope, source state, quality, freshness, interlocks, reason, confirmation, re-auth/second user |
| `/api/commands/executions` | Execute and track | create with idempotency, get status/timeline, cancel if supported |
| `/api/commands/executions/{id}/confirmations` | Confirmation evidence | add user/second-user/re-auth confirmation |

### 10.12. Reports and documents

| Endpoint group | Назначение | Ключевые операции |
|---|---|---|
| `/api/reports` | Definitions | list/get allowed reports |
| `/api/report-runs` | Async report generation | create/get status/download/cancel |
| `/api/documents` | Files and metadata | list/upload/download/version/link, later antivirus status |
| `/api/shift-log` | Shift journal | list/create/edit own draft/finalize/hand over |

### 10.13. SignalR hubs

| Hub | Назначение | Основные сообщения |
|---|---|---|
| `/hubs/telemetry` | Current values and source states | subscribe/unsubscribe; snapshot cursor; `CurrentValueChanged`, `QualityChanged`, `SourceStateChanged`, `Gap`, `ResyncRequired` |
| `/hubs/events` | Event/alarm queue | `EventCreated`, `OccurrenceChanged`, `Acknowledged`, `Assigned`, `Shelved`, counts |
| `/hubs/work` | My Work, incidents, maintenance | `TaskChanged`, `IncidentChanged`, `WorkOrderChanged`, counters |
| `/hubs/notifications` | Personal inbox | `NotificationDelivered`, `ReadStateChanged`, badge count |
| `/hubs/platform` | Admin health/data quality | `ComponentHealthChanged`, `DataQualityIssueChanged`, publication status |
| `/hubs/commands` | Control and execution status | `ControlSessionChanged`, `CommandExecutionChanged`, `CommandRejected` |

Каждая subscription проходит серверную authorization. Изменение permission или session revoke должно закрывать/пересчитывать активные subscriptions.

---

## 11. Frontend routes

### 11.1. Общие правила маршрутизации

- Production application использует нормальные server-supported URLs; hash routing остается только свойством статического прототипа.
- URL хранит устойчивое состояние: выбранный service view, dashboard/window, filters, time range и entity ID, если это безопасно.
- Browser Back/Forward должен восстанавливать рабочий контекст без повторного опасного действия.
- Открытие прямой ссылки повторно проверяет authorization и существование сущности.
- Entity, обычно открываемая в drawer, может иметь direct route; shell восстанавливается, а сущность открывается в правой панели.
- Fullscreen runtime является display mode, а не отдельной моделью данных.
- `/settings/*` и `/admin/*` разделены на уровне route space, authorization policies и navigation.

### 11.2. Карта маршрутов

| Route | Тип экрана | Основные компоненты | Права | Релиз |
|---|---|---|---|---|
| `/home` | Оперативный Рабочий стол | Assigned widgets, attention, map, My Work, quick links | Authenticated + widget source scopes | MVP |
| `/me` | Страница текущего сотрудника | Profile header, status, professional context, activities, links | Own profile; field visibility policy | MVP baseline |
| `/users/{userId}` | Страница коллеги | Viewer-filtered profile, contact/mention/assign actions | Directory + field/object scopes | MVP baseline |
| `/notifications` | Полный personal inbox | Message list, read/unread, source links, filters | Own deliveries only | Post-MVP/basic MVP optional |
| `/locations` | Location workspace | Tree/map/list, aggregate state, filters, drawer | Location view scope | MVP |
| `/locations/{id}` | Location detail | Summary, child assets, alarms, dashboards, maintenance | Location view scope | MVP/later detail |
| `/dashboards` | Dashboard catalog | Search, filters, favorites, cards, last-used behavior | Dashboard view scope | MVP |
| `/d/{dashboard}/{screen}` | Dashboard runtime | Window toolbar, Live/History, widgets/mimic, period, drawer | Dashboard + each bound source scope | MVP |
| `/events` | Диспетчер событий | Realtime/History, working list, filters, bulk bar, drawer | Event/occurrence view + action permissions | MVP |
| `/events?view={id}` | Saved/service view | Same dispatcher with URL state; `incidents` is service subview | As above | MVP for main views; incidents subview later/baseline |
| `/events/{id}` | Direct event link | `/events` shell + selected row/drawer | Event scope | MVP |
| `/incidents/{id}` | Incident workspace | Summary, status, coordinator, links, timeline, tasks | Incident view/action scope | Baseline MVP, extended later |
| `/equipment` | Equipment registry | Table/tree, filters, state/quality, drawer | Equipment view scope | MVP |
| `/equipment/{id}` | Equipment default detail | Summary, state, values, links, actions | Equipment view | MVP |
| `/equipment/{id}/{section}` | Sectioned equipment page | Overview, points, history, events, documents, diagnostics | Section-specific permissions | MVP subset/later |
| `/equipment/add` | Universal staging editor | Manual rows, CSV, dynamic protocol fields, validation, diagnostics, Apply | Equipment configure; update requires stronger permission | MVP |
| `/trends` | Trend workspace | Point selection, period, chart, quality marks, saved views | DataPoint history scope | MVP baseline |
| `/trends/{view}` | Saved/direct trend | Restored point set and time context | Same | Later/baseline direct tag route |
| `/my-work` | Personal working queue | Counters, type filter, task list, drawer, source links | Own/assigned tasks | MVP |
| `/maintenance` | ТОиР overview | Counters, sections, due/overdue summary | Maintenance view scope | MVP baseline |
| `/maintenance/assets` | MaintenanceObject registry | List/tree, independent objects, links | Maintenance object scope | MVP |
| `/maintenance/plans` | Plans | Plan list, status, schedule, procedure | Planner/configure permission | MVP baseline |
| `/maintenance/calendar` | Forecast/calendar | Week/list mode, forecast vs work orders | Maintenance view | MVP baseline |
| `/maintenance/requests` | Requests | Queue, source context, triage | Request view/action | MVP |
| `/maintenance/defects` | Defects | Defect queue and relations | Maintenance scope | Later |
| `/maintenance/work-orders` | Work-order queue | Counters, filters, status/assignee/due | Work-order view | MVP |
| `/maintenance/work-orders/{id}` | Work-order workspace | Next action, checklist, timeline, links | Transition-specific permission | MVP |
| `/maintenance/procedures` | Procedures | Versioned checklists/instructions | Planner/configurator | Later |
| `/maintenance/resources` | Resources | People/material capacity | Planner | Later |
| `/maintenance/reports` | Maintenance analytics | KPI/report links | Manager/planner | Later |
| `/schedules` | Operational schedules/scenarios | List/calendar, status, next runs | Schedule view/manage | Later |
| `/schedules/{id}` | Schedule detail | Conditions, actions, exceptions, revision | Manage permission | Later |
| `/reports` | Report catalog/runs | Definitions, parameters, jobs, downloads | Report scope | Later |
| `/reports/{id}` | Report detail | Parameters, run history, result | Report scope | Later |
| `/shift-log` | Shift journal | Entries, handover, linked contexts | Shift role/scope | Later or optional MVP |
| `/builder/dashboards/{id}` | Dashboard Editor | Shell, canvas, properties, draft/validate/impact/publish/versions | Dashboard edit/publish separated | Later |
| `/builder/mimics/{id}` | SVG Editor | Shell, SVG canvas, bindings, draft/validate/publish | Mimic edit/publish separated | Later |
| `/settings` | Personal settings landing | Local navigation, effective personal values | Own settings | MVP baseline |
| `/settings/profile` | Editable profile settings | Allowed fields, read-only sources, save | Own profile fields | MVP baseline |
| `/settings/privacy` | Visibility preview | Effective visibility, “view as employee” | Own settings | Later/baseline |
| `/settings/home` | Limited home personalization | Available widgets, reset org layout | Own allowed layout | MVP baseline |
| `/settings/interface` | UI preferences | Language, timezone, density, theme, units, panel behavior | Own settings | Later |
| `/settings/security` | Personal security | Sessions, MFA/password depending IdP | Own sessions/security | MVP session list; rest later |
| `/settings/favorites` | Favorites/recent links | Folders, remove/reorder, access recheck | Own links | Later |
| `/settings/notifications` | Notification settings landing | Local navigation | Own settings | Post-MVP |
| `/settings/notifications/effective` | Effective policy | Mandatory read-only + optional summary | Own | Post-MVP |
| `/settings/notifications/subscriptions` | Personal subscriptions | Create/edit/pause/delete | Own, source scope constrained | Post-MVP |
| `/settings/notifications/schedule` | Schedule/absence | Work hours, quiet hours, vacation, coverage | Own + coverage accept | Post-MVP |
| `/settings/notifications/channels` | Channels | Address/status/test, optional enable | Own optional channels | Post-MVP |
| `/admin` | Admin landing | System sections/status | Admin scope | MVP baseline |
| `/admin/settings` | System settings | Effective/inherited values, overrides, impact | System/location admin as scoped | MVP |
| `/admin/settings/connections` | Integrations | Masked endpoints, state, test, enable | Integration admin | MVP |
| `/admin/users` | Accounts and roles | User/role views, effective permissions, assignment preview | Identity admin | MVP |
| `/admin/notifications` | Mandatory notification policies | Policies, channels, escalation | Notification admin | Later |
| `/admin/terminals` | Terminal registry in normal shell | Device identity, profile, assignment, status | Terminal admin | Later/baseline model |
| `/admin/terminals/enroll` | Enrollment approval | One-time code, device data, location/profile/dashboard | Terminal admin | Later |
| `/admin/health` | Platform health | Component queue, counters, filters, detail | Platform admin/ops | MVP |
| `/admin/data-quality` | Data quality | Issues, evidence, source links, assign/resolve | Platform/engineering admin | MVP |
| `/admin/audit` | Read-only audit | Filters, detail, before/after, correlation | Audit reader scope | MVP |
| `/terminal/enroll` | Standalone terminal enrollment | Code, device metadata, waiting state | Unregistered terminal token | Later |
| Kiosk runtime | Standalone assigned runtime | Assigned dashboard only, terminal status/control indicator | Valid device identity/profile/assignment | Later production; prototype path not canonical |

### 11.3. Route-level acceptance rules

- `/home`, `/me` и `/settings/*` не перенаправляются друг в друга.
- `/dashboards` всегда является каталогом; nav item может открыть последний доступный dashboard, но URL не меняет каноническую модель.
- `/events?view=incidents` не создает второй сервис в navigation.
- `/events/{id}` открывает `/events` с event drawer, а не отдельный несвязанный layout.
- `/admin/users` не заменяет `/users/{userId}`.
- `/admin/terminals` имеет обычную shell; kiosk runtime и `/terminal/enroll` не показывают header/rail.
- Route query state очищается или нормализуется, если пользователь потерял доступ.

---

## 12. Testing and acceptance

### 12.1. Test strategy

| Уровень | Назначение | Инструмент/подход |
|---|---|---|
| Unit | Инварианты, state transitions, validation, policies, mapping | .NET test framework, deterministic clocks |
| Contract | gRPC/protobuf adapter/gateway compatibility | Golden messages, consumer/provider suites |
| Integration | PostgreSQL, API, workers, SignalR, outbox | Disposable test DB, real migrations, protocol simulators |
| E2E | Критические пользовательские маршруты и browser state | Playwright |
| Security | Authorization, IDOR, secrets, sessions, command policies | API negative matrix + browser checks + threat cases |
| Realtime | Gap, sequence, reconnect, snapshot, backlog | Multi-client integration/fault tests |
| Data quality | Mapping, stale/offline, out-of-order, source errors | Simulator scenarios and deterministic time |
| Command safety | Preflight, idempotency, result unknown, revoke | Gateway simulator/fault injection |
| Load/soak | Polling, history, rules, SignalR fan-out | Dedicated load harness; profiler/metrics |
| Operational | Backup/restore, restart, migration, retention | Staging runbooks and automated smoke |

### 12.2. Manual QA checklist

Manual acceptance сохраняется для визуальной ясности и эксплуатационной семантики:

- shell не расходует лишнюю высоту; один header, rail и одна context panel;
- глобальная локация и service context понятны;
- transitions между dashboard/event/equipment сохраняют контекст;
- различия `/home`, `/me`, `/settings/*`, `/admin/*` очевидны;
- control mode заметен и отличим от view-only;
- quality не маскируется alarm colors;
- Live/History и period видны постоянно;
- operator видит timestamp/freshness и понимает last-known state;
- settings, editors, maintenance и admin воспринимаются как полноценные workspaces;
- dangerous actions имеют понятный preflight и причины блокировки;
- kiosk не содержит произвольной навигации;
- 1920×1080 является основным acceptance viewport; narrower fallback не должен ломать доступность.

### 12.3. Smoke tests

Release smoke suite должна занимать минуты и проверять:

1. migrations применились;
2. API `/health/live` и `/health/ready` успешны;
3. test user входит;
4. `/home`, `/equipment`, `/events`, `/dashboards`, `/admin/health` открываются по правам;
5. Modbus simulator дает current values;
6. SNMP simulator дает current values;
7. history содержит sample;
8. SignalR доставляет update;
9. rule открывает occurrence;
10. acknowledgement записывается и аудируется;
11. static dashboard/mimic загружается из published revision;
12. maintenance work order открывается;
13. backup/restore smoke выполняется в scheduled operational suite.

### 12.4. Integration tests

Обязательные наборы:

- migrations from previous release and clean install;
- scoped repository/API queries;
- outbox commit/delivery/retry/idempotency;
- polling assignment lease and reassignment;
- adapter contract normalization;
- current value sequence guard and out-of-order handling;
- history batching, dedup и partition rollover;
- freshness transition with fake clock;
- alarm rule open/update/clear after worker restart;
- acknowledgement/assignment/shelving independent transactions;
- incident creation preserving occurrence state;
- My Work projection rebuild;
- publication failure leaves old runtime pointer;
- work-order transition and checklist gate;
- terminal block/revoke invalidates runtime/control later.

### 12.5. Security/access tests

Нужна матрица `role × scope × entity × action × route/API`.

Критические проверки:

- hidden navigation item плюс прямой API/URL;
- IDOR: подстановка чужого `equipmentId`, `eventId`, `userId`, `terminalId`;
- role assignment outside admin scope;
- last scope admin removal;
- employee position does not grant admin/command rights;
- dashboard access does not automatically grant all bound DataPoints;
- notification source authorization on every open;
- SVG/script injection and unsafe external references;
- CSV formula injection on export and malicious headers/content;
- secret values absent in API, logs, audit and browser DOM;
- session revoke disconnects SignalR and control session;
- re-auth/second-user evidence cannot be reused outside command/deadline;
- terminal identity block/revoke affects one terminal only;
- no queued command while offline.

### 12.6. Realtime tests

- initial snapshot and updates have monotonic sequence;
- packet/message loss produces visible gap;
- reconnect triggers resnapshot before `Live`;
- duplicate delivery is idempotent;
- out-of-order updates do not roll back current state;
- permission change removes subscriptions;
- server restart and scale-out do not lose authorized state;
- high fan-out does not block ingestion;
- UI preserves last known value but marks it stale/offline;
- commands disabled during `Gap`, `Resynchronizing`, `Offline`.

### 12.7. Data quality tests

- Good/Uncertain/Bad mapping for each adapter outcome;
- Initializing before first valid sample;
- Stale after freshness threshold;
- Offline after source threshold;
- source timestamp in future/too old;
- wrong datatype/scaling/unit;
- missing mapping;
- repeated constant value is not automatically “good” without source trust;
- source reconnect resets state only after accepted sample/snapshot;
- data quality issue assignment does not alter alarm acknowledgement.

### 12.8. Command preflight tests

Для каждой command definition тестируется matrix:

- control mode off/on/expired;
- correct/incorrect dashboard scope;
- allowed/forbidden user action;
- source online/offline/degraded;
- quality Good/Uncertain/Bad/Stale/Offline;
- expected value/version matched/mismatched;
- interlock active/inactive;
- reason missing/present;
- confirmation missing/present;
- re-auth required/expired;
- second user required/same user/different authorized user;
- duplicate idempotency key;
- timeout before transmit;
- transmitted but result lost → `unknown`, not success;
- logout/revoke/gap terminates control session;
- no automatic retry after reconnect.

### 12.9. Kiosk tests

- `/admin/terminals` uses normal shell;
- enrollment/kiosk runtime hides global header, rail, events, inbox and profile;
- one-time code expires and cannot be reused;
- profile and dashboard assignment change independently;
- each terminal has unique device identity;
- blocking one terminal does not block others sharing profile;
- PIN appears only for profile requiring employee identification;
- no-PIN action is audited only by terminal identity;
- offline/blocked/revoked identity sees no trusted assigned runtime and cannot command;
- config version change invalidates pending command preview;
- terminal assignment authorization filters dashboard sources;
- no delayed button press or queued command is replayed after reconnect.

### 12.10. REVIEW.md → Playwright automation

Следующие сценарии должны стать обязательными Playwright tests. Идентификаторы рекомендуется использовать как traceability IDs в backlog и CI.

| ID | Автоматизируемый сценарий |
|---|---|
| `E2E-SHELL-001` | Header/rail/context drawer присутствуют, второй постоянный header отсутствует; основные размеры проверяются visual snapshot на 1920×1080 |
| `E2E-NAV-001` | Catalog → dashboard → events/equipment → nav «Дашборды» возвращает к last dashboard → кнопка «Все дашборды» возвращает catalog |
| `E2E-PERSONAL-001` | `/home`, `/me`, `/settings/profile` имеют разные contents и не redirect друг в друга |
| `E2E-ACCESS-001` | Non-admin не видит admin nav и получает forbidden по прямому `/admin/settings` и admin API |
| `E2E-NOTIF-001` | Header inbox отличается от Events; mark read не меняет occurrence acknowledgement |
| `E2E-NOTIF-002` | `/notifications` открывает только authorized source; inaccessible source показывает safe unavailable state |
| `E2E-HOME-001` | User hides/returns allowed home widget and resets organization layout; no template/copy page flow |
| `E2E-WORK-001` | My Work находится в avatar menu, не дублируется в rail/header; counters change after transfer/return and type filter works |
| `E2E-EQ-IMPORT-001` | Manual and CSV rows share one staging; operations create/update/skip/draft visible; existing defaults to skip/review |
| `E2E-EQ-IMPORT-002` | Apply processes only structurally valid create/update regardless of connection/poll; invalid/skip remain; delete count zero |
| `E2E-EQ-COPY-001` | Modbus/SNMP copy retains host; Unit ID sequence offered only for Modbus |
| `E2E-EQ-TEMPLATE-001` | Template excludes ID/name/host/Unit ID/secrets; SNMP v2c `public` warning and masking visible |
| `E2E-DASH-URL-001` | URL and Back/Forward restore exact Combined/Mimic/Widgets window |
| `E2E-DASH-CONTEXT-001` | Selecting fan/damper synchronizes highlight, faceplate, trend and linked events |
| `E2E-CMD-SELECT-001` | First symbol click only selects target; command preview contains selected target and no execution occurred |
| `E2E-DASH-TIME-001` | Live/History always visible; History blocks command preview |
| `E2E-QUALITY-001` | Alarm state and each quality state render independently without semantic collision |
| `E2E-EVENT-SEM-001` | condition, acknowledgement, assignment and incident fields change independently |
| `E2E-EVENT-HISTORY-001` | History disables ack, assignment, shelving, bulk, create request and create incident |
| `E2E-INCIDENT-001` | Create incident from event preserves acknowledgement and condition; link appears |
| `E2E-EVENT-FILTER-001` | State/location/assignee filters work; drawer links to equipment/mimic/tag history |
| `E2E-MAINT-001` | Maintenance counters filter one queue; forecast, work order and personal task have distinct identifiers/routes |
| `E2E-MAINT-ASSET-001` | Standalone maintenance object without connection opens; optional equipment link works both ways |
| `E2E-MAINT-CHECK-001` | Required incomplete checklist blocks submit/review transition |
| `E2E-MAINT-EVENT-001` | Request from event carries source context and preserves acknowledgement/condition |
| `E2E-PUB-001` | Local dirty, saved draft and published revision are visibly distinct |
| `E2E-PUB-002` | Publish blocked before save+validation of exact draft; impact preview lists windows/terminals |
| `E2E-PUB-003` | Dashboard and Mimic revision histories are independent; linked mimic publication explicit |
| `E2E-PUB-004` | Rollback creates new revision; leaving dirty editor prompts save/discard/stay |
| `E2E-IAM-001` | Account card shows effective services, scope and source, distinct from employee page |
| `E2E-IAM-002` | Role/scope change requires preview; last scope admin removal blocked |
| `E2E-ADMIN-SETTING-001` | Inherited source, override, affected workplaces and exceptions shown before apply/reset |
| `E2E-ADMIN-CONN-001` | Integration secret absent; informational test creates audit entry |
| `E2E-ADMIN-HEALTH-001` | Health counters filter system queue; platform health/data quality remain separate from building events |
| `E2E-AUDIT-001` | Audit route has no edit/delete controls and direct mutating API does not exist/returns method not allowed |
| `E2E-KIOSK-001` | Admin terminals in shell; kiosk runtime has no shell/navigation |
| `E2E-KIOSK-002` | Shared profile does not replace unique identity; blocking one terminal leaves another active |
| `E2E-KIOSK-003` | Profile and startup dashboard assigned independently; runtime shows assigned dashboard |
| `E2E-KIOSK-004` | Enrollment path consumes one-time code and creates identity |
| `E2E-KIOSK-005` | PIN shown only for employee-PIN profile; no-PIN audit attribution uses terminal identity |
| `E2E-KIOSK-006` | Offline/blocked/revoked block all commands and do not replay pending actions |

Часть проверок REVIEW остается manual/visual: понятность терминологии, заметность control mode, достаточность информации в карточках и отсутствие лишней визуальной высоты. Эти пункты должны иметь UX review checklist на каждом release candidate.

### 12.11. Definition of Done проекта

Feature считается готовой только если:

- domain semantics и permission action определены;
- API contract и error cases документированы;
- database migration и rollback/forward strategy подготовлены;
- audit requirements реализованы;
- telemetry/realtime behavior при gap/error определено;
- unit/integration tests проходят;
- критический user flow покрыт Playwright, если он есть в таблице выше;
- logs/metrics/health добавлены;
- secrets и sensitive data review пройдены;
- accessibility и keyboard path проверены;
- документация/runbook обновлены;
- demo показывает committed vertical slice, а не mock вместо незавершенного backend.

---

## 13. Риски и спорные места

### 13.1. Реестр рисков

| Риск | Последствие | Mitigation | Контрольный сигнал |
|---|---|---|---|
| Слишком широкий scope | Невозможность завершить промышленный vertical slice | MVP gates, отдельные later epics, не более одного нового сложного context за release train | Рост WIP, много «80% готово», нет сквозного demo |
| Dashboard Editor слишком рано | Команда тратит месяцы на конструктор без доверенной telemetry model | Runtime first; configuration packages/revisions; editor только после stable runtime | Editor stories опережают quality/events/runtime |
| Смешивание event/alarm/incident | Неверные статусы, аудит и действия оператора | Разные tables/API/state machines и tests на независимость | Один enum/status меняет сразу condition/ack/incident |
| Опасные команды | Риск воздействия на объект и ложного успеха | View-only default, control session, preflight, idempotency, unknown result, allowlist, pilot gate | Команда вызывается прямо из widget API без gateway/preflight |
| Сложность RBAC | Утечки или блокировка работы | Role+scope model, explain effective permission, negative matrix, last-admin guard | UI-only checks, flat checkbox matrix без scope/source |
| Telemetry load | Poll drift, DB saturation, stale UI | Batching, bounded queues, partitions, metrics, load tests, separate runtime | Backlog age/GC/DB write latency растут без лимита |
| Premature C++ | Два toolchain, сложный deployment, потеря скорости разработки | .NET baseline, profiling, stable process contract, rewrite decision report | Решение основано на предположении, а не trace/metrics |
| Разрастание UI | Потеря рабочей области и контекста | One shell, seven screen patterns, one drawer, route/state standards | Новые постоянные header/toolbars и разные list patterns |
| Недостаточная auditability | Нельзя доказать действие и восстановить причину | Mandatory audit pipeline, before/after/reason/correlation, append-only storage | Изменение конфигурации/прав без audit entry |
| Смешивание ТОиР с equipment | Нельзя вести неподключенные объекты; lifecycle становится неправильным | Separate MaintenanceObject/context, optional link | WorkOrder FK только на Equipment; нет standalone object |
| Kiosk security | Shared panel становится общей учетной записью или сохраняет команды offline | Unique device identity, profile separate, block/revoke, no replay, employee PIN policy | Один token/profile используется как identity всех panels |
| Неверная freshness semantics | Last-known value выглядит актуальным | Central quality/freshness policy и visual tests | UI показывает число без timestamp/quality |
| Realtime как source of truth | Gap создает неверное состояние | REST snapshot + sequence + resync; SignalR only delivery | Client продолжает после reconnect без snapshot |
| Partial publication | Runtime получает несовместимую смесь конфигураций | Immutable revision, validation hash, atomic published pointer | Separate widget rows применяются по одной |
| Secret leakage | Компрометация устройств | Secret reference/provider, masking, logging filters, security tests | Community/password виден в API/audit/DOM |
| History growth | Стоимость/отказ БД | Retention, partitions, batch ingestion, query limits | Неограниченные raw rows и full-range queries |
| Alarm storm | Оператор теряет рабочую очередь | Hysteresis/delay, dependency/suppression later, stable list, grouping | Thousands occurrences from one source transition |
| Непрозрачный data quality | Technical outage выглядит building alarm | Separate admin quality context and links | Platform errors попадают в `/events` как технологические alarms |
| Overuse JSONB | Невозможные constraints/query/index/migrations | Core columns/relations typed; JSONB only versioned payload/config | Status, rights или links хранятся только в arbitrary JSON |
| Monolith без модульных границ | Cross-context coupling и невозможность extraction | Project/module boundaries, dependency tests, schema ownership | Adapter ссылается на incidents/work orders или UI DTO |
| Слишком ранние microservices | Operational overhead и distributed consistency | Modular monolith; extract only load/lifecycle boundary | Каждый context имеет отдельную БД/queue до MVP |
| Недостаточная эксплуатационная готовность | Пилот трудно поддерживать | Health, metrics, runbooks, backup/restore, soak tests | Успех определяется только UI demo |

### 13.2. Сомнительно / требует обсуждения

Ниже перечислены вопросы, которые исходные документы намеренно не фиксируют количественно или окончательно. Их нельзя закрывать скрытым архитектурным предположением.

1. **Пилотный объем:** количество локаций, equipment, DataPoints, poll intervals, history retention и concurrent users.
2. **Availability/SLO:** допустимый downtime API, telemetry lag, event delivery latency, RPO/RTO.
3. **Identity provider:** локальные accounts, корпоративный OIDC/AD или комбинированная схема; требования MFA/re-auth.
4. **Deployment topology:** один сервер, central cluster, isolated OT network, DMZ, edge sites и outbound connectivity.
5. **PostgreSQL extensions:** достаточно ли native partitioning или допустим специализированный timeseries extension; решение только после sizing.
6. **Protocol scope MVP:** обязательна ли SNMP v3 полностью; какие Modbus datatypes/endian/scaling реально нужны.
7. **SNMP traps:** входят ли они в ближайший release или только polling.
8. **Alarm rule catalog:** threshold, discrete state, rate-of-change, missing-data, schedule masks, dependencies и priorities.
9. **Command pilot:** конкретные equipment/commands, interlocks, verification method, second-user policy и regulatory constraints.
10. **Incident baseline:** какие статусы и обязательные поля нужны MVP, есть ли SLA/escalation.
11. **ТОиР baseline:** точный work-order status flow, required checklist evidence, acceptance role, numbering.
12. **Document storage:** допустимые типы/размеры, retention, antivirus и external repository integration.
13. **Reports:** обязательные отчеты первого года; не следует строить report designer без списка.
14. **Notification providers:** какие каналы реально нужны, кто обеспечивает delivery receipts и legal consent.
15. **Kiosk enrollment:** сертификаты, TPM/device binding, rotation, factory reset и offline behavior.
16. **Configuration approvals:** нужна ли схема four-eyes для dashboard/mimic/rules и кто может publish.
17. **Data ownership:** кто отвечает за quality issue, protocol mapping, alarm rules и location structure.
18. **Audit retention/export:** нормативный срок, неизменяемое внешнее хранилище, требования электронной подписи.
19. **Localization/timezones:** одна локаль на MVP или multi-language; отображение смен и schedules на распределенных объектах.
20. **Browser/workstation policy:** поддерживаемые браузеры, kiosk OS, screen profiles и обновление клиентов.

Для каждого вопроса следует создать ADR/Decision Record с owner, due date и impact на backlog.

### 13.3. Антирешения

Не допускается без отдельного обоснования:

- начинать проект с C++;
- размещать protocol driver в ASP.NET controller/process;
- вызывать C++ DLL через P/Invoke как основной production boundary;
- создавать один `Alarm` с полями event/ack/incident/work order;
- хранить «последнее значение» без quality/timestamps;
- считать скрытую кнопку проверкой прав;
- исполнять command из browser напрямую;
- retry command после неизвестного результата или reconnect;
- использовать draft runtime;
- публиковать отдельные widgets/rows частями;
- хранить SNMP community/password в audit/log;
- превращать My Work в владельца исходных tasks;
- делать MaintenanceObject обязательным child Equipment;
- создавать второй global header для каждого service;
- переносить весь prototype state/localStorage behavior в production data model;
- выбирать Kafka, Kubernetes, Redis, event sourcing, CQRS или microservices без измеримой проблемы, которую они решают.

---

## 14. Итоговые рекомендации

### 14.1. Что делать первым

1. Утвердить glossary, pilot sizing и permission matrix.
2. Создать solution/CI/PostgreSQL/observability baseline.
3. Реализовать server-side Identity & Access и единую shell.
4. Построить Location–Equipment–DataPoint–TelemetrySource model.
5. Получить сквозные Modbus/SNMP samples с quality/freshness и history.
6. Реализовать SignalR snapshot/gap/resync.
7. Разделить и внедрить EventRecord, AlarmRule, AlarmOccurrence.
8. Довести Диспетчер событий до рабочего operator flow.
9. Подключить dashboard/static mimic runtime к published revisions.
10. Добавить базовые incidents, My Work, ТОиР, audit, health и data quality.
11. Провести hardening/soak/security и только затем открывать pilot command path.

### 14.2. Что отложить

- visual Dashboard Editor и SVG Editor;
- arbitrary scripting/widgets;
- сложный CMMS;
- mobile offline app;
- production external notification channels/escalation;
- wallboard playlists и production kiosk enrollment;
- OPC UA/BACnet до согласования реальных источников;
- edge gateway до появления удаленной площадки/offline requirement;
- report designer;
- C++ implementation;
- multi-tenant UX;
- общее adaptive/touch redesign.

### 14.3. Где оставить интерфейсы под C++

Стабильные service/process contracts нужны у:

- Telemetry Polling Runtime;
- Modbus Adapter;
- SNMP Adapter;
- future OPC UA Adapter;
- future BACnet Adapter;
- Edge Gateway;
- Command Execution Gateway;
- History Ingestion;
- Alarm/Rule Evaluation Engine.

Контракты должны быть protocol-neutral на входе/выходе центральной системы, versioned и покрыты contract tests. Business authorization, incident/maintenance workflow, dashboard publication и audit остаются C#/.NET.

### 14.4. Документы, которые необходимо поддерживать

| Документ | Назначение | Когда обновлять |
|---|---|---|
| Product concept | Product scope, entities, principles | При изменении продуктового смысла |
| WEB_INTERFACE_SPECIFICATION | Routes, states, interactions | При изменении UX/route model |
| Technical specification and roadmap | Architecture, MVP, phases, tests | На каждом крупном planning cycle |
| Glossary/domain semantics | Канонические термины и lifecycle | До merge изменения сущности/state |
| Context map | Ownership and integrations | При изменении module boundary |
| ADR/Decision records | Обоснование ключевых technical choices | До реализации irreversible decision |
| API contracts/OpenAPI | HTTP and error contracts | Автоматически/с каждым API change |
| gRPC/protobuf contracts | Adapter/gateway/edge boundaries | Versioned с compatibility review |
| Data dictionary | Tables, fields, retention, sensitivity | С каждой migration |
| Permission matrix | Role/scope/action rules | С каждым новым action/route |
| Audit catalog | Какие действия и поля аудируются | С каждым mutating use case |
| Alarm rule catalog | Rule types/quality behavior | С каждым новым evaluator type |
| Command safety case | Commands, interlocks, policy, evidence | До каждого production command rollout |
| Test traceability matrix | Requirement → tests | На каждом sprint/release |
| Operational runbooks | Deploy, backup, restore, incidents | Перед pilot и после operational change |
| Capacity/load report | Workload and bottlenecks | До scale/C++ decision |

### 14.5. Вопросы владельцу продукта

Приоритет уточнения:

1. пилотные площадки и количественный профиль данных;
2. обязательные operator roles/scopes;
3. набор alarm rules и severity policy MVP;
4. минимальный incident lifecycle;
5. минимальный maintenance lifecycle;
6. список production commands и safety policy;
7. identity provider и MFA/re-auth;
8. retention/audit requirements;
9. обязательные dashboards/mimics для пилота;
10. rollout order extended features.

### 14.6. Финальное архитектурное решение

Для первого промышленного релиза рекомендуется:

- **не микросервисная платформа**, а модульный C#/.NET backend с четкими bounded contexts;
- **Blazor Web client + ASP.NET Core API + SignalR**;
- **PostgreSQL** с context schemas, native partitioning и transactional outbox;
- **отдельные .NET Worker/process boundaries** для polling, protocol adapters, ingestion и command gateway;
- **runtime-first** подход к dashboards/mimics;
- **read-only production posture** до прохождения отдельного command safety gate;
- **C++ только после production metrics**, без изменения business contracts.

Критерий успеха MVP: оператор получает достоверный realtime-контекст, корректно обрабатывает alarm occurrence, переходит к связанному equipment/dashboard/history, а система сохраняет права, качество, lifecycle и audit при отказах и reconnect. Количество реализованных экранов само по себе не является критерием готовности.

---

## Приложение A. Минимальная трассировка требований

| Продуктовый инвариант | Архитектурная реализация | Основная проверка |
|---|---|---|
| Context, не protocol | AssetModel отдельно от adapters | Equipment API не содержит обязательных Modbus/SNMP полей |
| Value metadata | TelemetrySample/CurrentValue contract | Data quality and UI tests |
| Last known ≠ current | Freshness evaluator + UI state | Stale/offline E2E |
| Event ≠ occurrence ≠ incident | Separate modules/tables/API | State independence integration/E2E |
| Ack ≠ assign ≠ clear | Separate records/actions | Event action matrix |
| View-only default | ControlSession service | Command preflight suite |
| Draft не меняет runtime | Published revision pointer | Publication failure/dirty tests |
| ТОиР independent | MaintenanceObject optional Equipment link | Standalone asset E2E |
| Terminal ≠ user | Device identity + KioskProfile | Kiosk identity tests |
| Hidden UI ≠ authorization | ASP.NET policies/scoped queries | Negative security matrix |
| Personal ≠ admin settings | Route/module split | Direct URL tests |
| Health ≠ building alarm | Admin read model | Health/events separation tests |

## Приложение B. MVP exit checklist

- [ ] Glossary и context map утверждены.
- [ ] Все MVP routes имеют backend authorization.
- [ ] Modbus TCP и SNMP vertical slices работают на simulators/pilot devices.
- [ ] Current/history values содержат unit, quality, timestamps, freshness/source.
- [ ] Realtime gap/resync доказан тестом.
- [ ] EventRecord, AlarmOccurrence и Incident разделены.
- [ ] Ack/assignment/clearing/shelving не смешиваются.
- [ ] Dashboard/static mimic читают только published revision.
- [ ] MaintenanceObject существует без Equipment.
- [ ] Audit append-only и не содержит secrets.
- [ ] Platform health и data quality доступны отдельно.
- [ ] Command execution production-off до safety gate.
- [ ] Playwright critical suite проходит стабильно.
- [ ] Backup/restore, migration, retention и runbooks проверены.
- [ ] Load baseline зафиксирован для будущих scale/C++ решений.

