(() => {
  "use strict";

  const app = document.getElementById("app");
  const modalRoot = document.getElementById("modal-root");
  const toastRoot = document.getElementById("toast-root");

  const defaultHomeWidgets = ["metrics", "attention", "map", "work", "quick"];
  const homeWidgetDefinitions = [
    { id: "metrics", title: "Оперативные показатели", description: "Реакции, аварии, доступность оборудования и личная очередь" },
    { id: "attention", title: "Ситуации, требующие реакции", description: "Приоритетные неподтверждённые события" },
    { id: "map", title: "Карта комплекса", description: "Краткое состояние корпусов и ключевых площадок" },
    { id: "work", title: "Моя работа", description: "Назначенные задачи, решения и сроки" },
    { id: "quick", title: "Быстрый доступ", description: "Избранные рабочие переходы" },
  ];

  const state = {
    route: routeFromHash(),
    preserveDrawerOnRouteChange: false,
    navOpen: false,
    location: "Главный комплекс",
    controlEnabled: false,
    controlRemaining: 0,
    controlTimer: null,
    controlScopeDashboardId: null,
    workFullscreen: false,
    zoom: 100,
    dashboardMode: "live",
    dashboardSelectedObjectId: null,
    drawer: null,
    drawerTab: "details",
    eventMode: "realtime",
    eventView: "requires",
    eventSeverity: "all",
    eventCondition: "all",
    eventLocation: "all",
    eventAssignment: "all",
    eventSearch: "",
    selectedEvents: new Set(),
    pendingEvents: 1,
    equipmentView: "table",
    maintenanceFilter: "all",
    maintenanceCalendarMode: "calendar",
    workFilter: "all",
    workType: "all",
    notificationScenario: "now",
    notificationSchedulePreset: "work",
    vacationState: "draft",
    subscriptions: { hvac: true, ups: true, ppr: true },
    notificationChannels: { push: true, sms: true, email: true, messenger: false },
    channelTests: {},
    adminIdentityView: "users",
    adminHealthFilter: "all",
    adminAuditFilter: "all",
    adminSettingOverride: "",
    adminConnectionTests: {},
    kioskControlTerminalId: null,
    kioskControlRemaining: 0,
    kioskControlExpiresAt: 0,
    kioskControlTimer: null,
    kioskAuthenticatedEmployee: "",
    kioskPendingCommand: null,
    editorCanvasOnly: false,
    editorProfile: "desktop",
    pendingEditorNavigation: null,
    editorDocuments: {
      "/builder/dashboards/hvac": {
        kind: "dashboard",
        id: "hvac",
        name: "Дашборд · Климат корпуса A",
        working: { elementName: "Мнемосхема П1", source: "Мнемосхема AHU-P1 · опубликована v4" },
        savedDraft: { elementName: "Мнемосхема П1", source: "Мнемосхема AHU-P1 · опубликована v4" },
        dirty: false,
        workingSerial: 1,
        savedSerial: 1,
        validatedSerial: 0,
        publishedSerial: 0,
        draftSerial: 8,
        publishedVersion: 7,
        versions: [
          { version: 7, author: "Алексей Смирнов", time: "Сегодня · 11:40", note: "Текущая операторская версия", snapshot: { elementName: "Мнемосхема П1", source: "Мнемосхема AHU-P1 · опубликована v4" } },
          { version: 6, author: "Анна Волкова", time: "Вчера · 16:15", note: "До изменения состава окна", snapshot: { elementName: "Обзор П1", source: "Мнемосхема AHU-P1 · опубликована v3" } },
        ],
      },
      "/builder/mimics/ahu-p1": {
        kind: "mimic",
        id: "ahu-p1",
        name: "Мнемосхема · Установка П1",
        working: { elementName: "Вентилятор П1", source: "Оборудование AHU-P1" },
        savedDraft: { elementName: "Вентилятор П1", source: "Оборудование AHU-P1" },
        dirty: false,
        workingSerial: 1,
        savedSerial: 1,
        validatedSerial: 0,
        publishedSerial: 0,
        draftSerial: 5,
        publishedVersion: 4,
        versions: [
          { version: 4, author: "Алексей Смирнов", time: "Сегодня · 10:25", note: "Текущая мнемосхема", snapshot: { elementName: "Вентилятор П1", source: "Оборудование AHU-P1" } },
          { version: 3, author: "Сергей Петров", time: "10.07 · 18:05", note: "До обновления привязки", snapshot: { elementName: "Вентилятор притока", source: "Оборудование AHU-P1" } },
        ],
      },
    },
    lastDashboardRoute: safeGet("dispatcher.prototype.v3.lastDashboardRoute", safeGet("dispatcher.prototype.v2.lastDashboard", "") ? `/d/${safeGet("dispatcher.prototype.v2.lastDashboard")}/overview` : ""),
    dashboardFilter: "all",
    dashboardSearch: "",
    dashboardCatalogScroll: 0,
    favoriteDashboards: new Set(safeList("dispatcher.prototype.v2.favoriteDashboards", ["main", "energy"])),
    homeWidgets: new Set(safeList("dispatcher.prototype.v3.homeWidgets", defaultHomeWidgets).filter((id) => defaultHomeWidgets.includes(id))),
    profileStatus: safeGet("dispatcher.prototype.v3.profileStatus", "Доступен"),
    profileStatusUntil: safeGet("dispatcher.prototype.v3.profileStatusUntil", "До конца смены"),
    privacyPreview: false,
    closePanelOnNavigate: safeGet("dispatcher.prototype.v3.closePanelOnNavigate", "true") !== "false",
    endedSessions: new Set(),
    headerMenu: null,
    readPersonalNotifications: new Set(safeList("dispatcher.prototype.v4.readPersonalNotifications", ["PN-003"])),
    equipmentDraftSequence: 2,
    equipmentDrafts: [{ draftKey: "draft-1", id: "AHU-P3", name: "Приточная установка П3", type: "Вентустановка", location: "Корпус C · Венткамера 1", protocol: "Modbus TCP", host: "192.168.30.43", port: "502", unitId: "1", timeout: "1500", snmpVersion: "v2c", community: "public", snmpUsername: "", authSecret: "", privacySecret: "", source: "manual", allowUpdate: false }],
    selectedEquipmentDraftKey: "draft-1",
    equipmentDiagnostics: {},
    equipmentCommunityVisible: false,
    equipmentSelectedTemplateId: "template-modbus",
    equipmentTemplates: [
      { id: "template-modbus", name: "Modbus TCP · типовой", config: { type: "Вентустановка", location: "Корпус A", protocol: "Modbus TCP", port: "502", timeout: "1500" } },
      { id: "template-snmp", name: "SNMP v2c · сетевое", config: { type: "Коммутатор", location: "ЦОД", protocol: "SNMP", port: "161", snmpVersion: "v2c" } },
    ],
  };

  const currentUser = { id: "alexey", name: "Алексей Смирнов", initials: "АС", isAdmin: true };

  const navItems = [
    { route: "/home", label: "Главная", icon: "home" },
    { route: "/dashboards", action: "go-dashboards", label: "Дашборды", icon: "dashboard" },
    { route: "/events", label: "Диспетчер событий", icon: "alert", badge: () => reactionCount() },
    { route: "/equipment", label: "Оборудование", icon: "equipment" },
    { route: "/maintenance", label: "ТОиР и ППР", icon: "work" },
    { route: "/builder/dashboards/hvac", label: "Конструктор", icon: "editor" },
    { route: "/admin/settings", label: "Системные настройки", icon: "settings", adminOnly: true },
  ];

  const iconPaths = {
    menu: '<path d="M4 7h16M4 12h16M4 17h16"/>',
    home: '<path d="m3 11 9-8 9 8"/><path d="M5 10v10h14V10M9 20v-6h6v6"/>',
    dashboard: '<rect x="3" y="3" width="7" height="7" rx="1"/><rect x="14" y="3" width="7" height="4" rx="1"/><rect x="14" y="11" width="7" height="10" rx="1"/><rect x="3" y="14" width="7" height="7" rx="1"/>',
    alert: '<path d="M12 3 2.8 19h18.4L12 3Z"/><path d="M12 9v4M12 17h.01"/>',
    equipment: '<rect x="4" y="3" width="16" height="18" rx="2"/><path d="M8 7h8M8 11h8M8 15h4M16 15h.01"/>',
    work: '<path d="M9 6V4h6v2M4 7h16v13H4z"/><path d="M4 12h16M10 12v2h4v-2"/>',
    bell: '<path d="M18 8a6 6 0 0 0-12 0c0 7-3 7-3 9h18c0-2-3-2-3-9M10 21h4"/>',
    mail: '<rect x="3" y="5" width="18" height="14" rx="2"/><path d="m4 7 8 6 8-6"/>',
    editor: '<path d="M4 20h16M6 16l10-10 2 2L8 18H6v-2Z"/><path d="m14 8 2 2"/>',
    search: '<circle cx="11" cy="11" r="7"/><path d="m20 20-4-4"/>',
    chevron: '<path d="m9 18 6-6-6-6"/>',
    close: '<path d="m6 6 12 12M18 6 6 18"/>',
    command: '<path d="M12 3v8M7 5.8a8 8 0 1 0 10 0"/>',
    filter: '<path d="M3 5h18l-7 8v6l-4 2v-8L3 5Z"/>',
    fullscreen: '<path d="M8 3H3v5M16 3h5v5M8 21H3v-5M16 21h5v-5"/>',
    user: '<circle cx="12" cy="8" r="4"/><path d="M4 21a8 8 0 0 1 16 0"/>',
    health: '<path d="M3 12h4l2-6 4 12 2-6h6"/>',
    plus: '<path d="M12 5v14M5 12h14"/>',
    minus: '<path d="M5 12h14"/>',
    reset: '<path d="M4 12a8 8 0 1 0 2.3-5.7L4 8"/><path d="M4 3v5h5"/>',
    check: '<path d="m4 12 5 5L20 6"/>',
    clock: '<circle cx="12" cy="12" r="9"/><path d="M12 7v5l3 2"/>',
    lock: '<rect x="5" y="10" width="14" height="11" rx="2"/><path d="M8 10V7a4 4 0 0 1 8 0v3"/>',
    link: '<path d="M10 13a5 5 0 0 0 7.1 0l2-2a5 5 0 0 0-7.1-7.1l-1.1 1.1"/><path d="M14 11a5 5 0 0 0-7.1 0l-2 2A5 5 0 0 0 12 20.1l1.1-1.1"/>',
    table: '<path d="M3 5h18v14H3zM3 10h18M9 5v14"/>',
    tree: '<path d="M6 3v18M6 7h7M6 12h7M6 17h7"/><rect x="13" y="5" width="7" height="4" rx="1"/><rect x="13" y="10" width="7" height="4" rx="1"/><rect x="13" y="15" width="7" height="4" rx="1"/>',
    more: '<circle cx="5" cy="12" r="1"/><circle cx="12" cy="12" r="1"/><circle cx="19" cy="12" r="1"/>',
    eye: '<path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6S2 12 2 12Z"/><circle cx="12" cy="12" r="2.5"/>',
    panel: '<path d="M3 4h18v16H3zM9 4v16M15 4v16"/>',
    settings: '<circle cx="12" cy="12" r="3"/><path d="M19.4 15a1.7 1.7 0 0 0 .3 1.9l.1.1-2.8 2.8-.1-.1a1.7 1.7 0 0 0-1.9-.3 1.7 1.7 0 0 0-1 1.6v.2h-4V21a1.7 1.7 0 0 0-1-1.6 1.7 1.7 0 0 0-1.9.3l-.1.1L4.2 17l.1-.1a1.7 1.7 0 0 0 .3-1.9A1.7 1.7 0 0 0 3 14H2.8v-4H3a1.7 1.7 0 0 0 1.6-1 1.7 1.7 0 0 0-.3-1.9L4.2 7 7 4.2l.1.1A1.7 1.7 0 0 0 9 4.6a1.7 1.7 0 0 0 1-1.6v-.2h4V3a1.7 1.7 0 0 0 1 1.6 1.7 1.7 0 0 0 1.9-.3l.1-.1L19.8 7l-.1.1a1.7 1.7 0 0 0-.3 1.9 1.7 1.7 0 0 0 1.6 1h.2v4H21a1.7 1.7 0 0 0-1.6 1Z"/>',
    star: '<path d="m12 3 2.8 5.7 6.2.9-4.5 4.4 1.1 6.2-5.6-2.9-5.6 2.9 1.1-6.2L3 9.6l6.2-.9L12 3Z"/>',
    logout: '<path d="M10 4H5v16h5M14 8l4 4-4 4M8 12h10"/>',
    history: '<path d="M3 12a9 9 0 1 0 3-6.7L3 8"/><path d="M3 3v5h5M12 7v5l3 2"/>',
  };

  const dashboards = [
    { id: "main", name: "Климат · Корпус A", purpose: "Основной операторский", location: "Корпус A", system: "ОВиК", owner: "Группа эксплуатации", updated: "Сегодня · 13:42", alarms: 1, theme: "hvac", kind: "combined", windows: [{ id: "overview", name: "Установка П1", type: "Combined", description: "Мнемосхема и связанный контекст" }, { id: "mimic", name: "Мнемосхема П1", type: "Mimic", description: "Основная технологическая схема" }, { id: "widgets", name: "Параметры П1", type: "Widgets", description: "Состояния, тренды и события" }], metrics: [["Температура притока", "18,7 °C"], ["Давление", "286 Па"], ["Установки online", "8 / 9"], ["Требуют реакции", "1"]] },
    { id: "energy", name: "Энергетика · Главный комплекс", purpose: "Баланс и вводы", location: "Все локации", system: "Энергетика", owner: "Энергослужба", updated: "Сегодня · 14:05", alarms: 2, theme: "energy", kind: "widgets", metrics: [["Текущая мощность", "486 кВт"], ["Пик сегодня", "612 кВт"], ["Вводы в норме", "3 / 4"], ["ИБП от батареи", "1"]] },
    { id: "fire", name: "Пожарная безопасность", purpose: "Состояние зон и автоматики", location: "Главный комплекс", system: "АПС", owner: "Пожарная служба", updated: "Сегодня · 14:28", alarms: 1, theme: "fire", kind: "widgets", metrics: [["Активные тревоги", "1"], ["Зоны в норме", "47 / 48"], ["Автоматика", "Активна"], ["Неисправности", "0"]] },
    { id: "datacenter", name: "ЦОД · Инфраструктура", purpose: "Сеть, питание и климат", location: "ЦОД", system: "IT/OT", owner: "Сетевая служба", updated: "Сегодня · 14:17", alarms: 2, theme: "datacenter", kind: "widgets", metrics: [["Стойки online", "24 / 24"], ["PUE", "1,47"], ["Температура", "23,8 °C"], ["Сетевые аварии", "2"]] },
    { id: "security", name: "СКУД и периметр", purpose: "Доступ и охранные зоны", location: "Все локации", system: "Безопасность", owner: "Служба безопасности", updated: "Сегодня · 14:26", alarms: 0, theme: "security", kind: "widgets", metrics: [["Точки доступа", "38"], ["Online", "100 %"], ["Посетители", "17"], ["Тревоги", "0"]] },
    { id: "maintenance", name: "ТОиР · Сегодня", purpose: "Работы и просрочки", location: "Главный комплекс", system: "ТОиР", owner: "Служба эксплуатации", updated: "Сегодня · 13:58", alarms: 0, theme: "maintenance", kind: "widgets", metrics: [["Работы сегодня", "12"], ["Просрочено", "3"], ["На приёмке", "2"], ["Без исполнителя", "1"]] },
  ];

  const dashboardObjects = {
    "AHU-P1": {
      id: "AHU-P1", name: "Приточный вентилятор П1", type: "Вентилятор", location: "Корпус A · Венткамера 2",
      mode: "Автомат", controlOwner: "Контроллер П1", processState: "Остановлен", quality: "good", freshness: "2 с назад",
      primaryValue: "0", unit: "об/мин", secondaryValue: "Температура 18,7 °C · давление 286 Па", alarm: "Нет подтверждения работы",
      eventIds: ["EV-002"], historyRoute: "/trends/tag/ahu-p1-supply-temp", points: "0,49 20,45 40,43 60,38 80,34 100,31 120,28 140,23 160,19 180,15 200,12 220,10",
      command: "Перевести в автоматический режим",
    },
    "AHU-P1-DAMPER": {
      id: "AHU-P1-DAMPER", name: "Приточная заслонка П1", type: "Исполнительный механизм", location: "Корпус A · Венткамера 2",
      mode: "Автомат", controlOwner: "Контроллер П1", processState: "Открыта", quality: "uncertain", freshness: "5 с назад",
      primaryValue: "62", unit: "%", secondaryValue: "Задание 65 % · обратная связь нестабильна", alarm: null,
      eventIds: [], historyRoute: "/trends/tag/ahu-p1-pressure", points: "0,34 20,32 40,35 60,30 80,31 100,29 120,26 140,28 160,23 180,25 200,21 220,22",
      command: "Установить положение 65 %",
    },
  };

  const people = [
    {
      id: "alexey", name: "Алексей Смирнов", initials: "АС", title: "Дежурный диспетчер", department: "Служба эксплуатации", location: "Главный комплекс · диспетчерская", manager: "Анна Волкова",
      presence: "Online", availability: "Доступен", availabilityUntil: "До конца смены", workState: "На смене до 20:00",
      phone: "+7 ••• •••-42-18", email: "a.smirnov@dispatcher.local", preferredContact: safeGet("dispatcher.prototype.v3.profileContact", "Рабочий телефон"),
      about: safeGet("dispatcher.prototype.v3.profileAbout", "Координация смены и первичная реакция на инженерные события."), specialization: safeGet("dispatcher.prototype.v3.profileSpecialization", "ОВиК, энергетика, координация аварийных работ"),
      responsibilities: ["Корпуса A и B", "Диспетчер событий", "Передача смены"], qualifications: ["III группа по электробезопасности", "Допуск к оперативным переключениям"],
      tasks: ["Принять координацию по EV-001", "Согласовать окно работ QF-12", "Провести приёмку ТО вентилятора П2"],
      pinned: [["Климат · Корпус A", "/d/main/overview", "Дашборд"], ["Инструкция дежурной смены", "", "Регламент"], ["Журнал передачи смены", "", "Документ"]],
      activity: [["14:29", "Принял назначение по пожарному инциденту"], ["13:42", "Добавил комментарий к работе WO-241"], ["08:03", "Принял смену"]],
    },
    {
      id: "anna", name: "Анна Волкова", initials: "АВ", title: "Руководитель эксплуатации", department: "Служба эксплуатации", location: "Главный комплекс · административный корпус", manager: "Директор по эксплуатации",
      presence: "Online", availability: "Занята", availabilityUntil: "До 15:30", workState: "Рабочий день до 18:00",
      phone: "+7 ••• •••-10-04", email: "a.volkova@dispatcher.local", preferredContact: "Корпоративный мессенджер",
      about: "Организация эксплуатации инженерных систем и контроль выполнения ТОиР.", specialization: "Эксплуатация зданий, планирование ресурсов, технический аудит",
      responsibilities: ["Главный комплекс", "ТОиР и ППР", "Согласование публикаций"], qualifications: ["Ответственная за электрохозяйство", "Промышленная безопасность"],
      tasks: ["Согласовать план ППР на август", "Проверить отчёт по доступности оборудования"],
      pinned: [["Контроль эксплуатации", "/d/maintenance/overview", "Дашборд"], ["Регламент эскалации", "", "Регламент"]],
      activity: [["14:12", "Согласовала окно работ по ИБП-2"], ["11:30", "Опубликовала отчёт по эксплуатации"], ["09:15", "Назначила проверку просроченных работ"]],
    },
    {
      id: "sergey", name: "Сергей Петров", initials: "СП", title: "Инженер ОВиК", department: "Служба ОВиК", location: "Корпус A · инженерная зона", manager: "Анна Волкова",
      presence: "Offline", availability: "На объекте", availabilityUntil: "До 16:00", workState: "Выездная работа",
      phone: "+7 ••• •••-33-72", email: "s.petrov@dispatcher.local", preferredContact: "Рабочий телефон",
      about: "Эксплуатация вентиляции, отопления и холодоснабжения.", specialization: "Вентустановки, ИТП, автоматика ОВиК",
      responsibilities: ["ОВиК корпуса A", "Венткамеры 1–4"], qualifications: ["IV группа по электробезопасности"],
      tasks: ["Диагностика вентилятора П1"], pinned: [["ОВиК · Корпус A", "/d/main/overview", "Дашборд"]], activity: [["12:48", "Принял наряд на диагностику П1"], ["10:20", "Завершил обход венткамер"]],
    },
  ];

  const adminAccounts = [
    { id: "ACC-001", personId: "alexey", login: "a.smirnov", name: "Алексей Смирнов", status: "Активна", identitySource: "Корпоративный каталог", groups: ["Дежурная смена"], role: "Диспетчер", scope: "Главный комплекс", services: ["Главная и дашборды", "Диспетчер событий", "Оборудование"], actions: ["Просмотр", "Acknowledgement", "Управление по политике"], permissionSource: "Группа «Дежурная смена»", temporary: "Нет", conflicts: "Нет" },
    { id: "ACC-002", personId: "anna", login: "a.volkova", name: "Анна Волкова", status: "Активна", identitySource: "Корпоративный каталог", groups: ["Руководители эксплуатации"], role: "Администратор организации", scope: "Вся организация", services: ["Все эксплуатационные сервисы", "Системные настройки", "Аудит"], actions: ["Администрирование", "Публикация", "Назначение прав"], permissionSource: "Прямое назначение", temporary: "Нет", conflicts: "Нет", lastScopeAdmin: true },
    { id: "ACC-003", personId: "sergey", login: "s.petrov", name: "Сергей Петров", status: "Активна", identitySource: "Корпоративный каталог", groups: ["Служба ОВиК"], role: "Инженер", scope: "Корпус A · ОВиК", services: ["Дашборды ОВиК", "Оборудование", "ТОиР и ППР"], actions: ["Просмотр", "Диагностика", "Выполнение нарядов"], permissionSource: "Группа «Служба ОВиК»", temporary: "Доступ к Корпусу B до 18.07", conflicts: "Нет" },
    { id: "ACC-004", personId: "", login: "contractor.04", name: "Внешний подрядчик", status: "Приостановлена", identitySource: "Локальная учётная запись", groups: ["Подрядчики"], role: "Наблюдатель", scope: "Корпус C · ИТП", services: ["Назначенный дашборд", "Назначенные наряды"], actions: ["Просмотр"], permissionSource: "Временное назначение", temporary: "Истекло 14.07", conflicts: "Истёкшее назначение" },
  ];

  const adminRoles = [
    { id: "ROLE-ADMIN", name: "Администратор организации", users: 1, scope: "Вся организация", services: "Все сервисы и системные настройки", actions: "Назначение прав, публикация, аудит", source: "Системная роль" },
    { id: "ROLE-DISPATCHER", name: "Диспетчер", users: 6, scope: "Назначенные локации", services: "Дашборды, события, оборудование", actions: "Просмотр, acknowledgement, команды по политике", source: "Роль организации" },
    { id: "ROLE-ENGINEER", name: "Инженер", users: 11, scope: "Назначенные системы и локации", services: "Оборудование, история, ТОиР", actions: "Диагностика, работы, разрешённое управление", source: "Роль организации" },
    { id: "ROLE-VIEWER", name: "Наблюдатель", users: 3, scope: "Явно назначенные объекты", services: "Назначенные дашборды", actions: "Только просмотр", source: "Системная роль" },
  ];

  const adminIntegrations = [
    { id: "INT-001", name: "Корпоративный каталог", type: "Identity provider", purpose: "Вход и синхронизация учётных записей", status: "Норма", endpoint: "ldaps://directory.•••:636", owner: "ИТ-служба", lastSuccess: "14:28", lastError: "Нет" },
    { id: "INT-002", name: "Корпоративная почта", type: "SMTP", purpose: "Доставка email-оповещений", status: "Норма", endpoint: "smtp://mail.•••:587", owner: "ИТ-служба", lastSuccess: "14:29", lastError: "Нет" },
    { id: "INT-003", name: "SMS-шлюз", type: "HTTPS API", purpose: "Резервная доставка критических оповещений", status: "Деградация", endpoint: "https://sms.•••/v2", owner: "Служба связи", lastSuccess: "14:18", lastError: "Ответ дольше 8 с" },
    { id: "INT-004", name: "CMMS webhook", type: "Webhook", purpose: "Передача согласованных заявок", status: "Выключено", endpoint: "https://cmms.•••/dispatcher", owner: "Служба эксплуатации", lastSuccess: "Не выполнялся", lastError: "Отключено политикой" },
  ];

  const platformHealthIssues = [
    { id: "PH-001", category: "connections", title: "SMS-шлюз отвечает с задержкой", status: "Деградация", type: "warning", impact: "Резервная SMS-доставка; primary web/push работают", cause: "Внешний endpoint превышает ожидаемое время ответа", lastSuccess: "14:18", scope: "Платформа · доставка", integrationId: "INT-003" },
    { id: "PH-002", category: "devices", title: "Одно устройство не передаёт данные", status: "Требует проверки", type: "critical", impact: "UPS-02 · значения Offline", cause: "Нет ответа при последних циклах опроса", lastSuccess: "13:57", scope: "ЦОД · стойка 2", equipmentId: "UPS-02" },
    { id: "PH-003", category: "configuration", title: "Два черновика конфигурации содержат ошибки", status: "Не опубликовано", type: "warning", impact: "Runtime продолжает использовать действующие revisions", cause: "Неполные привязки в черновиках", lastSuccess: "Runtime v7 доступен", scope: "Конструктор" },
    { id: "PH-004", category: "jobs", title: "Фоновая архивация отстаёт на 3 минуты", status: "Наблюдение", type: "warning", impact: "Realtime не затронут; history догоняет очередь", cause: "Повышенная нагрузка на хранилище", lastSuccess: "14:26", scope: "Платформа · история" },
  ];

  const dataQualityIssues = [
    { id: "DQ-001", category: "Stale / Bad", title: "14 параметров имеют устаревшее качество", object: "Корпус A · ОВиК", status: "Назначено", assignedTo: "Сергей Петров", detail: "Состояние качества не считается аварией и не меняет acknowledgement событий." },
    { id: "DQ-002", category: "История", title: "Разрыв истории 4 минуты", object: "UPS-02.BatteryCharge", status: "Новая", assignedTo: "Не назначено", detail: "Realtime и история проверяются раздельно." },
    { id: "DQ-003", category: "Диапазоны", title: "Единицы не совпадают с шаблоном", object: "Датчик температуры T-17", status: "На проверке", assignedTo: "Инженер КИПиА", detail: "Публикация исправления требует отдельной проверки конфигурации." },
    { id: "DQ-004", category: "Время", title: "Источник отстаёт на 38 секунд", object: "SNMP · SW-CORE-01", status: "Новая", assignedTo: "Не назначено", detail: "Рассинхронизация отмечается отдельно от потери связи." },
  ];

  const adminAuditEntries = [
    { id: "AUD-104", time: "14:42", actor: "Алексей Смирнов", action: "Опубликована revision", object: "Дашборд «Климат корпуса A» · v8", result: "Успех", category: "configuration", summary: "Целая revision применена после проверки; runtime не получал смешанное состояние." },
    { id: "AUD-103", time: "14:18", actor: "Система", action: "Диагностика подключения", object: "SMS-шлюз", result: "Предупреждение", category: "integration", summary: "Endpoint ответил, но превысил ожидаемое время. Секреты в запись не включены." },
    { id: "AUD-102", time: "13:55", actor: "Анна Волкова", action: "Изменено назначение", object: "Сергей Петров · Корпус B", result: "Успех", category: "access", summary: "Добавлен временный доступ до 18.07; источник — прямое назначение." },
    { id: "AUD-101", time: "09:04", actor: "Система", action: "Вход отклонён", object: "contractor.04", result: "Отказ", category: "security", summary: "Учётная запись приостановлена. Техническая запись не отображается на странице сотрудника." },
  ];

  const kioskProfiles = [
    {
      id: "KIOSK-CLIMATE", name: "Климат помещений", sharedProfile: "KIOSK-BUILDING",
      authPolicy: "employee-pin", authLabel: "PIN сотрудника перед управлением", controlTimeout: 120,
      allowedActions: ["Просмотр", "Уставка температуры", "Освещение"], offlineBehavior: "Stale-данные; команды запрещены",
    },
    {
      id: "KIOSK-LIMITED", name: "Ограниченное управление", sharedProfile: "KIOSK-BUILDING",
      authPolicy: "none", authLabel: "Без персональной идентификации для ограниченных команд", controlTimeout: 90,
      allowedActions: ["Просмотр", "Освещение"], offlineBehavior: "Stale-данные; команды запрещены",
    },
  ];

  const kioskCommandDefinitions = {
    "temperature-down": { label: "Уставка температуры −1 °C", capability: "Уставка температуры" },
    "temperature-up": { label: "Уставка температуры +1 °C", capability: "Уставка температуры" },
    "lighting-toggle": { label: "Переключить освещение", capability: "Освещение" },
  };

  const kioskTerminals = [
    { id: "TERM-001", deviceIdentity: "DEV-8F2A", name: "Панель переговорной 2.04", location: "Корпус A · 2 этаж", profileId: "KIOSK-CLIMATE", dashboardRoute: "/d/main/overview", status: "Активен", online: true, lastSeen: "Сейчас", currentScreen: "overview", configVersion: "v12", resolution: "1920×1080", sync: "Синхронизирован" },
    { id: "TERM-002", deviceIdentity: "DEV-91C4", name: "Панель кабинета 2.07", location: "Корпус A · 2 этаж", profileId: "KIOSK-CLIMATE", dashboardRoute: "/d/main/overview", status: "Активен", online: false, lastSeen: "12 мин назад", currentScreen: "overview", configVersion: "v12", resolution: "1920×1080", sync: "Нет связи" },
    { id: "TERM-003", deviceIdentity: "DEV-A17D", name: "Панель холла", location: "Корпус A · 1 этаж", profileId: "KIOSK-LIMITED", dashboardRoute: "/d/main/overview", status: "Заблокирован", online: true, lastSeen: "Сейчас", currentScreen: "overview", configVersion: "v8", resolution: "1920×1080", sync: "Заблокирован" },
  ];

  const terminalEnrollment = {
    code: "482715", status: "Ожидает подтверждения", requestedAt: "14:46", deviceIdentity: "DEV-CANDIDATE-04",
    deviceInfo: "Chromium kiosk · Linux", resolution: "1920×1080", approvedTerminalId: "",
  };

  const settingsSections = [
    { id: "profile", label: "Профиль", icon: "user", route: "/settings/profile" },
    { id: "privacy", label: "Видимость", icon: "eye", route: "/settings/privacy" },
    { id: "home", label: "Рабочий стол", icon: "home", route: "/settings/home" },
    { id: "interface", label: "Интерфейс", icon: "panel", route: "/settings/interface" },
    { id: "security", label: "Безопасность", icon: "lock", route: "/settings/security" },
    { id: "favorites", label: "Избранное", icon: "star", route: "/settings/favorites" },
    { id: "notifications", label: "Оповещения", icon: "bell", route: "/settings/notifications/effective" },
  ];

  const adminSections = [
    { id: "settings", label: "Общие", icon: "settings", route: "/admin/settings" },
    { id: "users", label: "Пользователи и роли", icon: "user", route: "/admin/users" },
    { id: "integrations", label: "Подключения", icon: "link", route: "/admin/settings/connections" },
    { id: "terminals", label: "Терминалы", icon: "panel", route: "/admin/terminals" },
    { id: "health", label: "Состояние платформы", icon: "health", route: "/admin/health" },
    { id: "data-quality", label: "Качество данных", icon: "table", route: "/admin/data-quality" },
    { id: "audit", label: "Аудит", icon: "history", route: "/admin/audit" },
  ];

  const personalNotifications = [
    {
      id: "PN-001", type: "Авария", severity: "warning", time: "14:26", title: "Протечка устранена, требуется подтверждение",
      meta: "EV-003 · Корпус C", eventId: "EV-003",
    },
    {
      id: "PN-002", type: "Оборудование", severity: "medium", time: "14:08", title: "ИБП-2 работает от батареи",
      meta: "Подписка «Энергетика» · заряд 76 %", equipmentId: "UPS-02", tagId: "ups-2-battery",
    },
    {
      id: "PN-003", type: "Восстановление", severity: "good", time: "12:30", title: "Параметр вернулся в личный диапазон",
      meta: "П1 · давление 286 Па", dashboardRoute: "/d/main/overview", tagId: "ahu-p1-pressure",
    },
  ];

  const tagHistories = {
    "ahu-p1-supply-temp": { tag: "AHU-P1.SupplyTemp", title: "Температура приточного воздуха", unit: "°C", current: "21,4", min: "18,2", average: "19,7", max: "21,6", updated: "14:30:12", dashboardRoute: "/d/main/overview", points: "0,49 20,45 40,43 60,38 80,34 100,31 120,28 140,23 160,19 180,15 200,12 220,10" },
    "ups-2-battery": { tag: "UPS-2.BatteryCharge", title: "Заряд батареи ИБП-2", unit: "%", current: "76", min: "76", average: "88", max: "100", updated: "14:30:09", dashboardRoute: "/d/energy/overview", points: "0,8 20,8 40,9 60,9 80,10 100,12 120,17 140,23 160,29 180,35 200,41 220,47" },
    "ahu-p1-pressure": { tag: "AHU-P1.SupplyPressure", title: "Давление приточного воздуха", unit: "Па", current: "286", min: "271", average: "282", max: "301", updated: "14:29:58", dashboardRoute: "/d/main/overview", points: "0,32 20,28 40,30 60,22 80,25 100,18 120,20 140,14 160,19 180,17 200,21 220,18" },
  };

  const events = [
    {
      id: "EV-001", kind: "Авария", category: "Пожарная безопасность", severity: "critical", condition: "Активна", ack: false,
      ackRequired: true, ackNoteRequired: true, assignment: "Не назначено", time: "14:28:42", duration: "01:32", location: "Корпус B · 4 этаж",
      source: "Извещатель 4-17", message: "Обнаружен дым в переговорной 4.12", value: "8,2 % obsc/m", quality: "good", shelvable: false,
      equipmentId: "FIRE-417", dashboardRoute: "/d/fire/overview", incidentId: "INC-001",
      timeline: [["14:28", "Получено аварийное состояние"], ["14:28", "Запущено дымоудаление"], ["14:29", "Push отправлен дежурной смене"]],
    },
    {
      id: "EV-002", kind: "Авария", category: "Климат", severity: "high", condition: "Активна", ack: true,
      ackRequired: true, assignment: "Алексей Смирнов", time: "14:21:09", duration: "09:05", location: "Корпус A · Венткамера 2",
      source: "П1 · Вентилятор", message: "Нет подтверждения работы приточного вентилятора", value: "0 об/мин", quality: "good", shelvable: true,
      equipmentId: "AHU-P1", dashboardRoute: "/d/main/overview", historyRoute: "/trends/tag/ahu-p1-supply-temp",
      timeline: [["14:21", "Условие стало активным"], ["14:22", "Подтвердил А. Смирнов"], ["14:23", "Назначено А. Смирнову"]],
    },
    {
      id: "EV-003", kind: "Авария", category: "Водоснабжение", severity: "medium", condition: "Норма", ack: false,
      ackRequired: true, assignment: "Не назначено", time: "14:12:18", duration: "00:47", location: "Корпус C · Техэтаж",
      source: "Датчик протечки WL-08", message: "Протечка устранена, требуется подтверждение", value: "Сухо", quality: "good", shelvable: false,
      timeline: [["14:12", "Обнаружена протечка"], ["14:13", "Состояние вернулось к норме"]],
    },
    {
      id: "EV-004", kind: "Авария", category: "Климат", severity: "medium", condition: "Активна", ack: true,
      ackRequired: true, assignment: "Пусконаладка", time: "13:48:02", duration: "42:28", location: "Корпус B · ИТП",
      source: "Контур отопления 2", message: "Температура обратной воды выше задания", value: "71,4 °C", quality: "good", shelvable: true,
      shelvedUntil: "15:00", shelveReason: "Пусконаладочные работы", timeline: [["13:48", "Условие стало активным"], ["13:50", "Отложено до 15:00: пусконаладка"]],
    },
    {
      id: "EV-005", kind: "Авария", category: "Сеть", severity: "low", condition: "Активна", ack: true,
      ackRequired: true, assignment: "Сетевая служба", time: "12:40:11", duration: "01:50", location: "ЦОД · Стойка R14",
      source: "Коммутатор SW-14", message: "Порт 17 недоступен в окне обслуживания", value: "Link down", quality: "stale", shelvable: false,
      equipmentId: "SW-14", dashboardRoute: "/d/datacenter/overview",
      suppression: "Окно обслуживания NET-204 до 18:00", timeline: [["12:40", "Связь потеряна"], ["12:40", "Suppression применён политикой ТОиР"]],
    },
    {
      id: "EV-006", kind: "Событие", category: "СКУД", severity: "info", condition: null, ack: null,
      ackRequired: false, assignment: null, time: "14:26:55", duration: "—", location: "Корпус A · Главный вход",
      source: "Турникет T-01", message: "Проход сотрудника по карте", value: "Разрешено", quality: "good", shelvable: false,
      timeline: [["14:26", "Проход разрешён контроллером СКУД"]],
    },
    {
      id: "EV-007", kind: "Авария", category: "Система", severity: "high", condition: "Активна", ack: false,
      ackRequired: true, assignment: "Не назначено", time: "14:17:33", duration: "12:57", location: "Платформа",
      source: "SNMP Collector 2", message: "Источник данных недоступен", value: "Timeout", quality: "bad", shelvable: true,
      dashboardRoute: "/d/datacenter/overview",
      timeline: [["14:17", "Превышен timeout опроса"], ["14:18", "Резервный маршрут уведомления не сработал"]],
    },
  ];

  const pendingEvent = {
    id: "EV-009", kind: "Авария", category: "Энергетика", severity: "critical", condition: "Активна", ack: false,
    ackRequired: true, ackNoteRequired: false, assignment: "Не назначено", time: "14:30:12", duration: "00:18", location: "ГРЩ · Ввод 1",
    source: "Автомат QF-01", message: "Отключён основной ввод питания", value: "0 A", quality: "good", shelvable: false, dashboardRoute: "/d/energy/overview",
    timeline: [["14:30", "Получено аварийное состояние"]],
  };

  const incidents = [
    { id: "INC-001", title: "Пожарная тревога · Корпус B", priority: "Критический", status: "Назначен", location: "Корпус B · 4 этаж", coordinator: "Алексей Смирнов", created: "14:29", eventIds: ["EV-001"] },
  ];

  const equipment = [
    { id: "AHU-P1", name: "Приточная установка П1", type: "Вентустановка", location: "Корпус A · Венткамера 2", connection: "На связи", quality: "good", status: "Авария", alarms: 1, ppr: "18 июля", owner: "Служба ОВиК", params: [["Температура притока", "18,7 °C", "good"], ["Давление", "286 Па", "good"], ["Вентилятор", "0 об/мин", "bad"]] },
    { id: "UPS-02", name: "ИБП-2", type: "ИБП", location: "ЦОД · Электрощитовая", connection: "На связи", quality: "good", status: "Норма", alarms: 0, ppr: "2 августа", owner: "Энергослужба", params: [["Нагрузка", "43 %", "good"], ["Батарея", "98 %", "good"], ["Температура", "24,1 °C", "good"]] },
    { id: "SW-14", name: "Коммутатор SW-14", type: "Коммутатор", location: "ЦОД · Стойка R14", connection: "Частично", quality: "stale", status: "Обслуживание", alarms: 1, ppr: "Сегодня", owner: "Сетевая служба", params: [["Доступность", "97,8 %", "stale"], ["CPU", "34 %", "good"], ["Порт 17", "Offline", "bad"]] },
    { id: "FIRE-417", name: "Извещатель 4-17", type: "Пожарный извещатель", location: "Корпус B · 4 этаж", connection: "На связи", quality: "good", status: "Авария", alarms: 1, ppr: "12 сентября", owner: "Пожарная служба", params: [["Задымление", "8,2 %", "bad"], ["Питание", "Норма", "good"], ["Связь", "Норма", "good"]] },
    { id: "MTR-2", name: "Счётчик ввода 2", type: "Электросчётчик", location: "ГРЩ", connection: "Нет связи", quality: "offline", status: "Недоступно", alarms: 1, ppr: "29 июля", owner: "Энергослужба", params: [["Мощность", "—", "offline"], ["Напряжение", "—", "offline"], ["Последние данные", "14:09", "stale"]] },
  ];

  const maintenanceAssets = [
    { id: "MA-001", name: "Приточная установка П1", type: "Вентустановка", location: "Корпус A · Венткамера 2", owner: "Служба ОВиК", linkState: "Связан с устройством", equipmentId: "AHU-P1", passport: "INV-ОВиК-001 · введена 2021" },
    { id: "MA-002", name: "Щит ЩР-12", type: "Распределительный щит", location: "Корпус C · 1 этаж", owner: "Энергослужба", linkState: "Только объект ТОиР", equipmentId: null, passport: "INV-ЭЛ-112 · введён 2018" },
    { id: "MA-003", name: "ИБП-2", type: "ИБП", location: "ЦОД · Электрощитовая", owner: "Энергослужба", linkState: "Связь требует проверки", equipmentId: "UPS-02", passport: "INV-ЦОД-022 · введён 2020" },
    { id: "MA-004", name: "Извещатель 4-17", type: "Пожарный извещатель", location: "Корпус B · 4 этаж", owner: "Пожарная служба", linkState: "Связан с устройством", equipmentId: "FIRE-417", passport: "INV-АПС-417 · установлен 2024" },
  ];

  const maintenancePlans = [
    { id: "P-001", assetId: "MA-001", name: "Ежемесячное ТО установки П1", trigger: "Каждый месяц", status: "Активен", nextDue: "18 июля", nextDates: ["18 июля", "18 августа", "18 сентября"], procedure: "Регламент ОВиК-01", assignee: "Служба ОВиК" },
    { id: "P-002", assetId: "MA-002", name: "Квартальное обслуживание ЩР-12", trigger: "Каждые 3 месяца", status: "Активен", nextDue: "Сегодня · 15:00", nextDates: ["15 июля", "15 октября", "15 января"], procedure: "Регламент ЭЛ-04", assignee: "Энергослужба" },
    { id: "P-003", assetId: "MA-003", name: "Годовая проверка ИБП-2", trigger: "Раз в год", status: "Приостановлен", nextDue: "2 августа", nextDates: ["2 августа", "2 августа 2027"], procedure: "Регламент UPS-02", assignee: "Сервис ИБП" },
  ];

  const maintenanceWorkOrders = [
    { id: "WO-201", planId: "P-002", assetId: "MA-002", title: "Проверка и протяжка соединений ЩР-12", dueDate: "2026-07-15", dayIndex: 2, time: "15:00–16:00", status: "Назначен", assignee: "Алексей Смирнов", priority: "Обычный", today: true, safety: "III группа · отключение не требуется", materials: "Комплект маркировки · в наличии", checklist: [["Визуальный осмотр", false, true], ["Контроль затяжки", false, true], ["Фото результата", false, false]], timeline: [["10:00", "Наряд назначен Алексею Смирнову"]] },
    { id: "WO-202", planId: "P-001", assetId: "MA-001", title: "ТО приточной установки П1", dueDate: "2026-07-16", dayIndex: 3, time: "09:00–11:00", status: "В работе", assignee: "Сергей Петров", priority: "Высокий", safety: "Допуск ОВиК действителен", materials: "Фильтр F7 · в наличии", checklist: [["Остановить установку", true, true], ["Заменить фильтр", false, true], ["Проверить вибрацию", false, true]], timeline: [["13:20", "Работа начата Сергеем Петровым"], ["09:00", "Наряд принят"]] },
    { id: "WO-203", planId: "P-003", assetId: "MA-003", title: "Приёмка проверки батарей ИБП-2", dueDate: "2026-07-15", dayIndex: 2, time: "16:00–16:30", status: "На приёмке", assignee: "Сервис ИБП", priority: "Обычный", today: true, safety: "Работа без отключения нагрузки", materials: "Не требуются", checklist: [["Протокол измерений приложен", true, true], ["Фото маркировки", true, true]], timeline: [["14:10", "Результат передан на приёмку"]] },
    { id: "WO-204", planId: null, assetId: "MA-004", title: "Проверка пожарного извещателя 4-17", dueDate: "2026-07-14", dayIndex: 1, time: "12:00–13:00", status: "Просрочен", assignee: "Не назначен", priority: "Высокий", overdue: true, safety: "Требуется согласование с пожарным постом", materials: "Тестер извещателей · не зарезервирован", checklist: [["Согласовать тест", false, true], ["Выполнить проверку", false, true]], timeline: [["Вчера", "Срок выполнения истёк"]] },
    { id: "WO-205", planId: null, assetId: "MA-001", title: "Осмотр приводных ремней П1", dueDate: "2026-07-13", dayIndex: 0, time: "10:00–10:30", status: "Выполнен", assignee: "Служба ОВиК", priority: "Обычный", checklist: [["Осмотр выполнен", true, true]], safety: "Без дополнительных требований", materials: "Не требуются", timeline: [["13 июля", "Работа выполнена"]] },
  ];

  const maintenanceForecasts = [
    { id: "FC-301", planId: "P-003", assetId: "MA-003", dayIndex: 5, dueDate: "2026-07-18", time: "10:00", status: "Прогноз", title: "Годовая проверка ИБП-2" },
  ];

  const maintenanceRequests = [
    { id: "MR-101", type: "Заявка", object: "Насос Н2", location: "ИТП", description: "Повышенный шум при работе", priority: "Средний", status: "Новая", source: "Сменный журнал", eventId: null },
    { id: "DF-101", type: "Дефект", object: "Приточная установка П1", location: "Корпус A", description: "Износ приводного ремня", priority: "Высокий", status: "На оценке", source: "WO-205", eventId: null },
  ];

  const tasks = [
    { id: "T-101", type: "Инцидент", action: "Принять координацию", object: "Пожарная тревога EV-001", location: "Корпус B", priority: "Критический", due: "Сейчас", status: "Назначено", from: "Дежурная смена", assignedToMe: true, requiresDecision: true, eventId: "EV-001", incidentId: "INC-001", timeline: [["14:29", "Назначено текущему пользователю"]] },
    { id: "T-102", type: "Наряд", action: "Провести приёмку", object: "Проверка батарей ИБП-2", location: "ЦОД", priority: "Обычный", due: "Сегодня · 16:00", status: "На приёмке", from: "Сервис ИБП", assignedToMe: true, today: true, equipmentId: "UPS-02", workOrderId: "WO-203", timeline: [["14:10", "Исполнитель передал результат"]] },
    { id: "T-103", type: "Согласование", action: "Согласовать окно работ", object: "Замена автомата QF-12", location: "ГРЩ", priority: "Высокий", due: "Просрочено 2 ч", status: "Ожидает решения", from: "Энергослужба", assignedToMe: true, overdue: true, requiresDecision: true, sourceRoute: "/equipment", timeline: [["Вчера", "Отправлено на согласование"]] },
    { id: "T-104", type: "Дефект", action: "Уточнить приоритет", object: "Шум насоса Н2", location: "ИТП", priority: "Средний", due: "Завтра", status: "Назначено", from: "Диспетчер", assignedToMe: true, requestId: "MR-101", sourceRoute: "/maintenance/requests", timeline: [["13:05", "Создано из сменного журнала"]] },
    { id: "T-105", type: "Публикация", action: "Проверить дашборд", object: "Климат · Корпус B", location: "Корпус B", priority: "Обычный", due: "Сегодня · 17:00", status: "Ожидает решения", from: "Инженер ОВиК", assignedToMe: true, today: true, requiresDecision: true, sourceRoute: "/builder/dashboards/hvac", timeline: [["11:44", "Версия 7 отправлена на проверку"]] },
    { id: "T-106", type: "Событие", action: "Проверить устранение протечки", object: "EV-003 · Датчик WL-08", location: "Корпус C", priority: "Средний", due: "Сейчас", status: "Назначено", from: "Диспетчер событий", assignedToMe: true, requiresDecision: true, eventId: "EV-003", timeline: [["14:14", "Создано из события без acknowledgement"]] },
    { id: "T-107", type: "ППР", action: "Подтвердить готовность к ППР", object: "Щит ЩР-12", location: "Корпус C", priority: "Обычный", due: "Сегодня · 15:00", status: "Назначено", from: "Служба эксплуатации", assignedToMe: true, today: true, workOrderId: "WO-201", timeline: [["10:00", "Назначено по плану ППР"]] },
  ];

  const subscriptions = [
    { id: "hvac", title: "Климат · средняя важность и выше", meta: "Корпуса A и B · Email · рабочее время" },
    { id: "ups", title: "ИБП · высокая важность", meta: "Все локации · Push · всегда" },
    { id: "ppr", title: "Ежедневная сводка ТОиР", meta: "Email · каждый день в 08:00" },
  ];

  function safeGet(key, fallback = "") {
    try { return localStorage.getItem(key) || fallback; } catch { return fallback; }
  }

  function safeSet(key, value) {
    try { localStorage.setItem(key, value); } catch { /* file:// may disable storage by policy */ }
  }

  function safeList(key, fallback = []) {
    try {
      const value = JSON.parse(localStorage.getItem(key));
      return Array.isArray(value) ? value : fallback;
    } catch { return fallback; }
  }

  function routeFromHash() {
    const value = location.hash.replace(/^#/, "");
    return value.startsWith("/") ? value : "/home";
  }

  function icon(name, label = "") {
    return `<svg class="icon" viewBox="0 0 24 24" aria-hidden="${label ? "false" : "true"}">${label ? `<title>${escapeHtml(label)}</title>` : ""}${iconPaths[name] || iconPaths.more}</svg>`;
  }

  function escapeHtml(value) {
    return String(value).replace(/[&<>'"]/g, (char) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", "'": "&#39;", '"': "&quot;" }[char]));
  }

  function isEditorRoute(route = state.route) {
    return /^\/builder\/(dashboards|mimics)\/[^/]+$/.test(route) && !route.includes("/home-");
  }

  function cloneEditorSnapshot(snapshot) {
    return { elementName: snapshot.elementName, source: snapshot.source };
  }

  function editorDocumentFromRoute(route = state.route, create = true) {
    if (!isEditorRoute(route)) return null;
    if (!state.editorDocuments[route] && create) {
      const match = route.match(/^\/builder\/(dashboards|mimics)\/([^/]+)$/);
      const kind = match?.[1] === "mimics" ? "mimic" : "dashboard";
      const id = match?.[2] || "new";
      const snapshot = { elementName: kind === "mimic" ? "Новый символ" : "Новый виджет", source: "Источник не выбран" };
      state.editorDocuments[route] = {
        kind,
        id,
        name: kind === "mimic" ? `Мнемосхема · ${id}` : `Дашборд · ${id}`,
        working: cloneEditorSnapshot(snapshot),
        savedDraft: cloneEditorSnapshot(snapshot),
        dirty: false,
        workingSerial: 1,
        savedSerial: 1,
        validatedSerial: 0,
        publishedSerial: 0,
        draftSerial: 1,
        publishedVersion: 0,
        versions: [],
      };
    }
    return state.editorDocuments[route] || null;
  }

  function editorHasCurrentValidation(document) {
    return Boolean(document && !document.dirty && document.validatedSerial === document.savedSerial);
  }

  function editorHasUnpublishedDraft(document) {
    return Boolean(document && document.savedSerial !== document.publishedSerial);
  }

  function navigate(route, preserveDrawer = false, force = false) {
    const document = editorDocumentFromRoute(state.route, false);
    if (!force && route !== state.route && document?.dirty) {
      showEditorLeaveWarning(route, preserveDrawer);
      return false;
    }
    if (!preserveDrawer) state.drawer = null;
    state.headerMenu = null;
    state.preserveDrawerOnRouteChange = preserveDrawer;
    state.navOpen = false;
    if (`#${route}` === location.hash) {
      state.route = route;
      state.preserveDrawerOnRouteChange = false;
      render();
    } else {
      location.hash = route;
    }
    return true;
  }

  function routePath(route = state.route) {
    return route.split("?")[0];
  }

  function kioskProfile(profileId) {
    return kioskProfiles.find((item) => item.id === profileId) || null;
  }

  function kioskTerminal(terminalId) {
    return kioskTerminals.find((item) => item.id === terminalId) || null;
  }

  function kioskTerminalIdFromRoute(route = state.route) {
    const match = route.match(/[?&]terminal=([^&]*)/);
    if (!match) return "";
    try { return decodeURIComponent(match[1]); } catch { return ""; }
  }

  function isKioskRuntimeRoute(route = state.route) {
    return /^\/d\/[^/]+\/[^/?]+$/.test(routePath(route)) && /[?&]terminal(?:=[^&]*)?(?:&|$)/.test(route);
  }

  function kioskRuntimeContext(route = state.route) {
    if (!isKioskRuntimeRoute(route)) return null;
    const terminal = kioskTerminal(kioskTerminalIdFromRoute(route));
    const profile = terminal ? kioskProfile(terminal.profileId) : null;
    return { terminal, profile, dashboardRoute: routePath(route), assignmentValid: Boolean(terminal && profile && terminal.dashboardRoute === routePath(route)) };
  }

  function kioskRuntimeRoute(terminal) {
    return terminal ? `${terminal.dashboardRoute}?terminal=${encodeURIComponent(terminal.id)}` : "/terminal/enroll";
  }

  function dashboardRouteParts(route = state.route) {
    const match = routePath(route).match(/^\/d\/([^/]+)\/([^/]+)$/);
    return match ? { id: match[1], screen: match[2] } : null;
  }

  function dashboardWindowsFor(dashboard) {
    return dashboard?.windows || [{ id: "overview", name: "Обзор", type: "Widgets", description: dashboard?.purpose || "Основной экран" }];
  }

  function isValidDashboardRoute(route) {
    const parts = dashboardRouteParts(route);
    const dashboard = parts ? dashboards.find((item) => item.id === parts.id) : null;
    return Boolean(dashboard && dashboardWindowsFor(dashboard).some((item) => item.id === parts.screen));
  }

  function validLastDashboardRoute() {
    return isValidDashboardRoute(state.lastDashboardRoute) ? state.lastDashboardRoute : "";
  }

  function lastDashboardId() {
    return dashboardRouteParts(validLastDashboardRoute())?.id || "";
  }

  function personFromRoute(route = state.route) {
    if (route === "/me") return people.find((person) => person.id === "alexey") || null;
    const match = route.match(/^\/users\/([^/]+)$/);
    return match ? people.find((person) => person.id === match[1]) || null : null;
  }

  function tagHistoryFromRoute(route = state.route) {
    const match = route.match(/^\/trends\/tag\/([^/]+)$/);
    return match ? tagHistories[match[1]] || null : null;
  }

  function unreadPersonalNotificationCount() {
    return personalNotifications.filter((item) => !state.readPersonalNotifications.has(item.id)).length;
  }

  function persistPersonalNotificationReadState() {
    safeSet("dispatcher.prototype.v4.readPersonalNotifications", JSON.stringify([...state.readPersonalNotifications]));
  }

  function markPersonalNotificationRead(id) {
    if (!personalNotifications.some((item) => item.id === id)) return;
    state.readPersonalNotifications.add(id);
    persistPersonalNotificationReadState();
  }

  function currentSettingsSection() {
    if (state.route === "/settings") return "profile";
    if (state.route.startsWith("/settings/notifications")) return "notifications";
    return state.route.split("/")[2] || "profile";
  }

  function isSupportedSettingsRoute(route) {
    if (route === "/settings") return true;
    if (settingsSections.some((item) => item.route === route)) return true;
    return /^\/settings\/notifications\/(effective|subscriptions|schedule|channels)$/.test(route) || route === "/settings/notifications";
  }

  function currentAdminSection(route = state.route) {
    if (route === "/admin") return "settings";
    if (route === "/admin/terminals/enroll") return "terminals";
    return adminSections.find((item) => item.route === route)?.id || "unknown";
  }

  function isSupportedAdminRoute(route) {
    return route === "/admin" || route === "/admin/terminals/enroll" || adminSections.some((item) => item.route === route);
  }

  function isActive(route) {
    if (route === "/dashboards") return state.route === "/dashboards" || state.route.startsWith("/d/");
    if (route === "/equipment") return state.route === "/equipment" || state.route.startsWith("/equipment/");
    if (route === "/maintenance") return state.route === "/maintenance" || state.route.startsWith("/maintenance/");
    if (route.startsWith("/d/")) return state.route.startsWith("/d/");
    if (route.startsWith("/settings/")) return state.route === "/settings" || state.route.startsWith("/settings/");
    if (route === "/admin/settings") return state.route === "/admin" || state.route.startsWith("/admin/");
    if (route.startsWith("/builder/")) return state.route.startsWith("/builder/");
    return state.route === route;
  }

  function routeTitle() {
    if (state.route === "/terminal/enroll") return "Регистрация терминала";
    if (isKioskRuntimeRoute(state.route)) return kioskRuntimeContext()?.terminal?.name || "Kiosk";
    if (state.route === "/home") return "Главная";
    if (state.route === "/me") return "Моя страница";
    if (state.route.startsWith("/users/")) return personFromRoute()?.name || "Сотрудник не найден";
    if (state.route === "/settings/profile" || state.route === "/settings") return "Настройки профиля";
    if (state.route.startsWith("/settings/notifications")) return "Настройка оповещений";
    if (state.route.startsWith("/settings/")) return "Личные настройки";
    if (state.route === "/admin" || state.route.startsWith("/admin/")) return adminSections.find((item) => item.id === currentAdminSection())?.label || "Администрирование";
    if (state.route === "/dashboards") return "Дашборды";
    if (state.route.startsWith("/d/")) return currentDashboard()?.name || "Дашборд не найден";
    if (state.route.startsWith("/events")) return "Диспетчер событий";
    if (state.route === "/equipment/add") return "Добавление оборудования";
    if (state.route.startsWith("/equipment")) return "Оборудование";
    if (state.route.startsWith("/maintenance")) return "ТОиР и ППР";
    if (state.route === "/notifications") return "Личные оповещения";
    if (state.route.startsWith("/my-work")) return "Моя работа";
    if (state.route.startsWith("/trends/tag/")) return tagHistoryFromRoute()?.title || "История тега";
    if (state.route.startsWith("/builder/")) return "Конструктор";
    return "Страница не найдена";
  }

  function currentMaintenanceSection(route = state.route) {
    if (route === "/maintenance") return "overview";
    if (route.startsWith("/maintenance/assets")) return "assets";
    if (route.startsWith("/maintenance/plans")) return "plans";
    if (route.startsWith("/maintenance/calendar")) return "calendar";
    if (route.startsWith("/maintenance/requests") || route.startsWith("/maintenance/defects")) return "requests";
    if (route.startsWith("/maintenance/work-orders")) return "work-orders";
    return "unknown";
  }

  function isSupportedMaintenanceRoute(route) {
    return currentMaintenanceSection(route) !== "unknown" && (route === "/maintenance" || /^\/maintenance\/(assets|plans|calendar|requests|defects|work-orders)$/.test(route) || /^\/maintenance\/work-orders\/[^/]+$/.test(route));
  }

  function currentDashboardId() {
    return dashboardRouteParts()?.id || "";
  }

  function currentDashboard() {
    return dashboards.find((item) => item.id === currentDashboardId()) || null;
  }

  function currentDashboardWindow() {
    const dashboard = currentDashboard();
    const screen = dashboardRouteParts()?.screen;
    return dashboardWindowsFor(dashboard).find((item) => item.id === screen) || null;
  }

  function selectedDashboardObject() {
    return state.dashboardSelectedObjectId ? dashboardObjects[state.dashboardSelectedObjectId] || null : null;
  }

  function rememberDashboardRoute(route) {
    if (!isValidDashboardRoute(route) || isKioskRuntimeRoute(route)) return;
    state.lastDashboardRoute = routePath(route);
    safeSet("dispatcher.prototype.v3.lastDashboardRoute", routePath(route));
  }

  function reactionCount() {
    return events.filter((item) => item.ackRequired && !item.ack).length + state.pendingEvents;
  }

  function formatTimer(seconds) {
    const minutes = Math.floor(seconds / 60).toString().padStart(2, "0");
    const rest = (seconds % 60).toString().padStart(2, "0");
    return `${minutes}:${rest}`;
  }

  function badge(label, type = "neutral") {
    return `<span class="badge ${type}">${escapeHtml(label)}</span>`;
  }

  function qualityBadge(value) {
    const labels = { good: "Good", uncertain: "Uncertain", bad: "Bad", stale: "Stale", offline: "Offline", initializing: "Initializing" };
    return `<span class="quality ${value}">${labels[value] || value}</span>`;
  }

  function renderPersonalNotificationActions(item, isRead) {
    const actions = [];
    if (item.eventId && events.some((event) => event.id === item.eventId)) actions.push(`<button data-action="open-personal-notification-event" data-id="${item.id}" data-event-id="${item.eventId}">Событие</button>`);
    else if (item.equipmentId && equipment.some((entry) => entry.id === item.equipmentId)) actions.push(`<button data-action="open-personal-notification-equipment" data-id="${item.id}" data-equipment-id="${item.equipmentId}">Устройство</button>`);
    else if (item.dashboardRoute && isValidDashboardRoute(item.dashboardRoute)) actions.push(`<button data-action="open-personal-notification-dashboard" data-id="${item.id}" data-route="${item.dashboardRoute}">Дашборд</button>`);
    if (item.tagId && tagHistories[item.tagId]) actions.push(`<button data-action="open-personal-notification-history" data-id="${item.id}" data-tag="${item.tagId}">История тега</button>`);
    if (!isRead) actions.push(`<button data-action="mark-personal-notification" data-id="${item.id}" title="Отметить прочитанным">Прочитано</button>`);
    return actions.join("");
  }

  function renderPersonalNotification(item) {
    const isRead = state.readPersonalNotifications.has(item.id);
    return `<article class="personal-notification ${isRead ? "read" : "unread"}"><div class="personal-notification-main"><span class="notification-status ${item.severity}"></span><div><strong>${escapeHtml(item.title)}</strong><span>${escapeHtml(item.type)} · ${escapeHtml(item.meta)} · ${escapeHtml(item.time)}</span></div></div><div class="personal-notification-actions">${renderPersonalNotificationActions(item, isRead)}</div></article>`;
  }

  function renderPersonalNotificationsMenu() {
    const unread = unreadPersonalNotificationCount();
    return `<section class="header-dropdown personal-notifications-menu" id="personal-notifications-menu" aria-label="Личные оповещения">
      <div class="header-dropdown-title"><div><strong>Личные оповещения</strong><span>${unread ? `Непрочитанных: ${unread}` : "Все прочитано"}</span></div>${unread ? `<button class="text-link" data-action="mark-all-personal-notifications">Прочитать все</button>` : ""}</div>
      <div class="personal-notification-list">${personalNotifications.map(renderPersonalNotification).join("")}</div>
      <div class="header-dropdown-footer"><span>Отдельно от общего журнала событий</span><div class="header-dropdown-footer-actions"><button data-nav="/notifications">Все</button><button data-nav="/settings/notifications/effective">Настроить</button></div></div>
    </section>`;
  }

  function renderUserMenu() {
    return `<section class="header-dropdown user-menu" id="user-menu" aria-label="Меню пользователя"><div class="user-menu-summary"><span class="avatar large">${currentUser.initials}</span><div><strong>${currentUser.name}</strong><span>Дежурный диспетчер</span></div></div><nav class="user-menu-links"><button data-nav="/me">${icon("user")}<span>Моя страница</span></button><button data-nav="/my-work">${icon("work")}<span>Моя работа</span><span class="user-menu-badge">${tasks.filter(isTaskActionable).length}</span></button><button data-nav="/settings/notifications/effective">${icon("bell")}<span>Настройка оповещений</span></button><button data-nav="/settings/profile">${icon("settings")}<span>Настройки профиля</span></button><button class="logout" data-action="request-logout">${icon("logout")}<span>Выход</span></button></nav></section>`;
  }

  function renderHeader() {
    return `
      <header class="app-header">
        <button class="brand-button" data-action="toggle-nav" aria-label="Открыть навигацию">D</button>
        <button class="header-location" data-action="cycle-location" title="Сменить локацию">${icon("equipment")}<span>${escapeHtml(state.location)}</span>${icon("chevron")}</button>
        <div class="header-divider"></div>
        <div class="header-service"><span>${escapeHtml(routeTitle())}</span></div>
        <div class="header-spacer"></div>
        <button class="header-action search" data-action="global-search" title="Поиск · Ctrl+K">${icon("search")}<span class="optional-label">Поиск</span></button>
        ${currentUser.isAdmin ? `<button class="header-action health" data-nav="/admin/health" title="Состояние платформы"><span class="status-dot"></span><span class="optional-label">Связь</span></button>` : `<div class="header-action health" title="Платформа доступна"><span class="status-dot"></span><span class="optional-label">Связь</span></div>`}
        <button class="header-action" data-action="go-events" title="Общий журнал событий · требуют реакции">${icon("alert")}<span class="header-count">${reactionCount()}</span></button>
        <div class="header-menu-anchor"><button class="header-action compact" data-action="toggle-personal-notifications" title="Личные оповещения" aria-haspopup="true" aria-expanded="${state.headerMenu === "notifications"}" aria-controls="personal-notifications-menu">${icon("mail")}${unreadPersonalNotificationCount() ? `<span class="header-count personal">${unreadPersonalNotificationCount()}</span>` : ""}</button>${state.headerMenu === "notifications" ? renderPersonalNotificationsMenu() : ""}</div>
        <button class="header-action ${state.controlEnabled ? "control-on" : ""}" data-action="control-toggle" title="Временный режим управления">
          ${icon("command")}<span class="optional-label control-time">${state.controlEnabled ? formatTimer(state.controlRemaining) : "Управление"}</span>
        </button>
        <div class="header-menu-anchor"><button class="header-action profile" data-action="toggle-user-menu" title="Активный пользователь: ${currentUser.name}" aria-haspopup="true" aria-expanded="${state.headerMenu === "user"}" aria-controls="user-menu"><span class="optional-label">${currentUser.name}</span><span class="avatar">${currentUser.initials}</span></button>${state.headerMenu === "user" ? renderUserMenu() : ""}</div>
      </header>`;
  }

  function renderNav() {
    return `
      <nav class="nav-rail ${state.navOpen ? "open" : ""}" aria-label="Основная навигация">
        <div class="nav-list">
          ${navItems.filter((item) => !item.adminOnly || currentUser.isAdmin).map((item) => `
            <button class="nav-item ${isActive(item.route) ? "active" : ""}" ${item.action ? `data-action="${item.action}"` : `data-nav="${item.route}"`} title="${item.label}">
              ${icon(item.icon)}<span class="nav-label">${item.label}</span>${item.badge && item.badge() ? `<span class="nav-badge">${item.badge()}</span>` : ""}
            </button>`).join("")}
        </div>
      </nav>`;
  }

  function renderShell(content) {
    return `<div class="app-shell">${renderHeader()}<div class="body-shell">${renderNav()}<main class="workspace" id="workspace">${content}</main>${renderDrawer()}</div></div>`;
  }

  function renderHome() {
    const activeAlarms = events.filter((item) => item.kind === "Авария" && item.condition === "Активна").length + state.pendingEvents;
    const show = (id) => state.homeWidgets.has(id);
    const leftSections = [
      show("attention") ? `<section class="panel">
        <div class="panel-head"><h2 class="panel-title">Ситуации, требующие реакции</h2><button class="button small ghost" data-nav="/events">Все события ${icon("chevron")}</button></div>
        <div class="attention-list">
          ${events.filter((item) => item.ackRequired && !item.ack).slice(0, 4).map((item) => `
            <div class="attention-row" data-action="open-event" data-id="${item.id}" tabindex="0">
              <span class="severity-bar ${item.severity}"></span><div><div class="row-title">${escapeHtml(item.message)}</div><div class="row-meta">${escapeHtml(item.location)} · ${escapeHtml(item.source)}</div></div><div class="time">${item.time.slice(0,5)}</div>
            </div>`).join("")}
        </div>
      </section>` : "",
      show("map") ? `<section class="panel">
        <div class="panel-head"><h2 class="panel-title">Карта комплекса</h2><span class="muted">3 корпуса · ЦОД · ГРЩ</span></div>
        <div class="location-plan">
          <button class="building a" data-nav="/d/main/overview">Корпус A<span class="building-status">1 авария · 99% online</span></button>
          <button class="building b" data-action="open-event" data-id="EV-001">Корпус B<span class="building-status">Критическая ситуация</span></button>
          <button class="building c" data-nav="/equipment">ЦОД<span class="building-status">Обслуживание сети</span></button>
        </div>
      </section>` : "",
    ].filter(Boolean).join("");
    const rightSections = [
      show("work") ? `<section class="panel">
        <div class="panel-head"><h2 class="panel-title">Моя работа</h2><button class="button small ghost" data-nav="/my-work">Открыть очередь ${icon("chevron")}</button></div>
        <div class="work-list">
          ${tasks.slice(0, 4).map((item) => `<div class="work-row" data-action="open-task" data-id="${item.id}" tabindex="0"><div><div class="row-title">${escapeHtml(item.action)}</div><div class="row-meta">${escapeHtml(item.type)} · ${escapeHtml(item.object)}</div></div><span class="badge ${item.priority === "Критический" ? "critical" : item.priority === "Высокий" ? "high" : "neutral"}">${escapeHtml(item.due)}</span></div>`).join("")}
        </div>
      </section>` : "",
      show("quick") ? `<section class="panel"><div class="panel-head"><h2 class="panel-title">Быстрый доступ</h2></div><div class="quick-grid">
        <button class="quick-card" data-nav="/d/main/overview"><strong>Климат · Корпус A</strong><div class="row-meta">Основной операторский дашборд</div></button>
        <button class="quick-card" data-nav="/events"><strong>Требуют реакции</strong><div class="row-meta">Системное представление событий</div></button>
        <button class="quick-card" data-nav="/equipment"><strong>Оборудование offline</strong><div class="row-meta">1 устройство · 1 stale</div></button>
        <button class="quick-card" data-nav="/settings/notifications/effective"><strong>Мои оповещения</strong><div class="row-meta">Effective policy и график</div></button>
      </div></section>` : "",
    ].filter(Boolean).join("");
    return `
      <section class="page working-desk" aria-label="Главная — Рабочий стол">
        <h1 class="sr-only">Рабочий стол</h1>
        <div class="page-content home-dashboard-content">
          ${show("metrics") ? `<div class="metric-grid">
            <article class="metric alert"><div class="metric-label"><span>Требуют реакции</span>${icon("alert")}</div><div class="metric-value">${reactionCount()}</div><div class="metric-note">2 критических · 3 неподтверждённых</div></article>
            <article class="metric warn"><div class="metric-label"><span>Активные аварии</span>${icon("health")}</div><div class="metric-value">${activeAlarms}</div><div class="metric-note">По 4 инженерным системам</div></article>
            <article class="metric"><div class="metric-label"><span>Оборудование online</span>${icon("equipment")}</div><div class="metric-value">98,7<span class="value-unit">%</span></div><div class="metric-note">1 устройство offline · 1 stale</div></article>
            <article class="metric"><div class="metric-label"><span>Моя работа</span>${icon("work")}</div><div class="metric-value">5</div><div class="metric-note">1 просрочено · 2 сегодня</div></article>
          </div>` : ""}
          ${leftSections || rightSections ? `<div class="home-grid"><div class="home-stack">${leftSections}</div><div class="home-stack">${rightSections}</div></div>` : `<div class="empty-state panel"><div><strong>Рабочий стол пуст</strong><p>Добавьте разрешённые блоки в разделе «Настройки → Рабочий стол».</p><button class="button primary" data-nav="/settings/home">Настроить Рабочий стол</button></div></div>`}
        </div>
      </section>`;
  }

  function filteredDashboards() {
    let result = [...dashboards];
    if (state.dashboardFilter === "favorites") result = result.filter((item) => state.favoriteDashboards.has(item.id));
    if (state.dashboardFilter === "recent") result = lastDashboardId() ? result.filter((item) => item.id === lastDashboardId()) : [];
    if (state.dashboardSearch) {
      const query = state.dashboardSearch.toLowerCase();
      result = result.filter((item) => [item.name, item.location, item.system, item.owner].some((value) => value.toLowerCase().includes(query)));
    }
    return result;
  }

  function renderDashboardPreview(item) {
    return `<div class="dashboard-preview ${item.theme}"><div class="preview-top"><span></span><span></span><span></span></div><div class="preview-main"></div><div class="preview-grid"><span></span><span></span><span></span></div></div>`;
  }

  function renderDashboardCatalog() {
    const items = filteredDashboards();
    const lastId = lastDashboardId();
    const lastDashboard = dashboards.find((item) => item.id === lastId);
    return `<section class="page dashboard-catalog-page"><div class="page-toolbar"><div class="toolbar-title">Дашборды</div><input class="field" value="${escapeHtml(state.dashboardSearch)}" data-change="dashboard-search" placeholder="Поиск · Enter" aria-label="Поиск дашбордов"><div class="segmented"><button class="${state.dashboardFilter === "all" ? "active" : ""}" data-action="dashboard-filter" data-value="all">Все</button><button class="${state.dashboardFilter === "favorites" ? "active" : ""}" data-action="dashboard-filter" data-value="favorites">Избранные</button><button class="${state.dashboardFilter === "recent" ? "active" : ""}" data-action="dashboard-filter" data-value="recent">Недавние</button></div><div class="toolbar-spacer"></div><span class="muted">Доступно: ${dashboards.length}</span><button class="button small" data-nav="/builder/dashboards/new">${icon("plus")} Создать</button></div><div class="page-content"><div class="catalog-summary"><div><strong>${state.dashboardFilter === "favorites" ? "Избранные дашборды" : state.dashboardFilter === "recent" ? "Последний активный" : "Все доступные дашборды"}</strong><div class="row-meta">Каталог остаётся доступен через кнопку «Все дашборды» в runtime.</div></div>${lastDashboard ? `<button class="button small" data-nav="${escapeHtml(validLastDashboardRoute())}">Продолжить: ${escapeHtml(lastDashboard.name)}</button>` : badge("Первое посещение", "neutral")}</div>${items.length ? `<div class="dashboard-catalog-grid">${items.map((item) => `<article class="dashboard-card ${lastId === item.id ? "last-active" : ""}">${renderDashboardPreview(item)}<div class="dashboard-card-body"><div class="dashboard-card-heading"><div><h2>${escapeHtml(item.name)}</h2><p>${escapeHtml(item.purpose)}</p></div><button class="favorite-button ${state.favoriteDashboards.has(item.id) ? "active" : ""}" data-action="toggle-dashboard-favorite" data-id="${item.id}" title="Избранное">${icon("star")}</button></div><div class="dashboard-meta"><span>${icon("equipment")} ${escapeHtml(item.location)}</span><span>${escapeHtml(item.system)}</span><span>${escapeHtml(item.owner)}</span></div><div class="dashboard-card-footer">${item.alarms ? badge(`${item.alarms} активн.`, item.alarms > 1 ? "critical" : "high") : badge("Норма", "good")}<span class="muted">${escapeHtml(item.updated)}</span><div class="toolbar-spacer"></div>${lastId === item.id ? badge("Последний активный", "medium") : ""}<button class="button primary small" data-action="open-dashboard" data-id="${item.id}">Открыть</button></div></div></article>`).join("")}</div>` : `<div class="empty-state panel">Ничего не найдено. Измените поиск или выбранное представление.</div>`}</div></section>`;
  }

  function renderMimic() {
    const fanSelected = state.dashboardSelectedObjectId === "AHU-P1";
    const damperSelected = state.dashboardSelectedObjectId === "AHU-P1-DAMPER";
    const history = state.dashboardMode === "history";
    return `
      <section class="mimic-card ${history ? "history-mode" : ""}" aria-label="Мнемосхема приточной установки П1">
        <div class="mimic-titlebar">${badge(history ? "History · срез 14:00" : "Live · П1", history ? "warning" : "good")}${badge("Авария вентилятора", "high")}<span class="muted">${history ? "Период 13:30–14:30" : "Обновлено 2 с назад"}</span></div>
        <div class="mimic-stage">
          <svg class="mimic-svg" style="transform:scale(${state.zoom / 100})" viewBox="0 0 1200 600" role="img" aria-label="Схема вентиляционной установки. Вентилятор в аварии, заслонка имеет качество Uncertain.">
            <defs><pattern id="grid" width="30" height="30" patternUnits="userSpaceOnUse"><path class="mimic-grid" d="M30 0H0V30" fill="none"/></pattern></defs>
            <rect width="1200" height="600" fill="url(#grid)"/>
            <rect class="room" x="70" y="100" width="1060" height="400" rx="12"/><text class="room-label" x="90" y="128">ВЕНТКАМЕРА 2 · ПРИТОЧНАЯ УСТАНОВКА П1</text>
            <path class="duct" d="M145 300H1030"/><path class="duct-inner" d="M145 300H1030"/><path class="flow" d="M145 300H1030"/>
            <g class="equipment-node alarm ${fanSelected ? "selected" : ""}" data-action="dashboard-select-object" data-id="AHU-P1" tabindex="0" aria-label="Выбрать приточный вентилятор П1" transform="translate(230 300)"><circle class="selection-ring" r="62"/><circle class="equipment-body" r="53"/><g class="fan-blades stopped"><path d="M0-36c18 0 21 15 12 28L0 0ZM36 0c0 18-15 21-28 12L0 0ZM0 36c-18 0-21-15-12-28L0 0ZM-36 0c0-18 15-21 28-12L0 0Z" fill="#ff5d73" opacity=".76"/></g><circle r="7" fill="#071119"/><circle class="quality-marker good" cx="42" cy="-42" r="9"/><text y="75">Вентилятор</text><text y="91" fill="#ff8798">0 об/мин</text></g>
            <g transform="translate(430 250)"><rect x="-55" y="-62" width="110" height="124" rx="5" fill="#142d3a" stroke="#4d7084"/><path d="M-38-45 38 45M-38-15 38 75M-38-75 38 15" stroke="#55a6ff" stroke-width="5" opacity=".8"/><text class="room-label" text-anchor="middle" y="85">Калорифер</text></g>
            <g transform="translate(610 300)"><rect x="-55" y="-62" width="110" height="124" rx="5" fill="#142d3a" stroke="#4d7084"/><path d="M-38-44H38M-38-22H38M-38 0H38M-38 22H38M-38 44H38" stroke="#7897a7" stroke-width="3"/><text class="room-label" text-anchor="middle" y="85">Фильтр</text></g>
            <g class="equipment-node ${damperSelected ? "selected" : ""}" data-action="dashboard-select-object" data-id="AHU-P1-DAMPER" tabindex="0" aria-label="Выбрать приточную заслонку П1" transform="translate(800 300)"><circle class="selection-ring" r="68"/><circle class="equipment-body" r="58"/><path d="M-45-45 45 45M45-45-45 45" stroke="#f4b84a" stroke-width="9"/><circle class="quality-marker uncertain" cx="46" cy="-46" r="9"/><text y="85">Заслонка 62%</text></g>
            <g transform="translate(1000 225)"><text class="sensor-value" text-anchor="middle">18,7 °C</text><text class="sensor-label" text-anchor="middle" y="18">T приточного воздуха</text></g>
            <g transform="translate(1000 375)"><text class="sensor-value" text-anchor="middle">286 Па</text><text class="sensor-label" text-anchor="middle" y="18">Давление в канале</text></g>
            <path class="pipe" d="M430 185V150H540"/><circle cx="545" cy="150" r="8" fill="#55a6ff"/><text class="sensor-label" x="565" y="154">Теплоноситель 74,2 °C</text>
          </svg>
        </div>
        <div class="zoom-controls" aria-label="Масштаб мнемосхемы">
          <button data-action="zoom-minus" title="Уменьшить">${icon("minus")}</button><span class="zoom-value">${state.zoom}%</span><button data-action="zoom-plus" title="Увеличить">${icon("plus")}</button>
          <button data-action="zoom-set" data-value="100" title="100%">100%</button><button data-action="zoom-set" data-value="90" title="Вписать">Вписать</button><button data-action="zoom-set" data-value="110" title="По ширине">По ширине</button><button data-action="zoom-set" data-value="100" title="Сбросить к авторскому виду">${icon("reset")}</button>
        </div>
      </section>`;
  }

  function sparkline(points = "0,50 20,44 40,48 60,30 80,34 100,25 120,31 140,18 160,22 180,15 200,19 220,12") {
    return `<svg class="sparkline" viewBox="0 0 220 65" preserveAspectRatio="none" aria-hidden="true"><path class="grid" d="M0 16H220M0 32H220M0 48H220"/><polyline class="line" points="${points}"/></svg>`;
  }

  function renderDashboardObjectSelector(selected) {
    return `<article class="widget"><div class="widget-head"><strong>Объекты экрана</strong><span class="muted">Выбор задаёт общий контекст</span></div><div class="dashboard-object-list">${Object.values(dashboardObjects).map((item) => `<button class="dashboard-object-choice ${selected?.id === item.id ? "selected" : ""}" data-action="dashboard-select-object" data-id="${item.id}"><span><strong>${escapeHtml(item.name)}</strong><small>${escapeHtml(item.type)} · ${escapeHtml(item.processState)}</small></span>${qualityBadge(item.quality)}</button>`).join("")}</div></article>`;
  }

  function renderDashboardStateWidget(item) {
    if (!item) return `<article class="widget"><div class="widget-head"><strong>Состояние оборудования</strong>${badge("Объект не выбран", "neutral")}</div><div class="dashboard-widget-empty"><strong>Выберите объект на мнемосхеме</strong><span>Выбор обновит эту карточку, тренд и связанные события. Первый клик не выполняет команду.</span></div></article>`;
    return `<article class="widget"><div class="widget-head"><strong>${escapeHtml(item.name)}</strong><div class="chip-row">${qualityBadge(item.quality)}${item.alarm ? badge("Активная авария", "high") : badge("Без аварий", "good")}</div></div><div class="value-row"><div><span class="value-number">${escapeHtml(item.primaryValue)}</span><span class="value-unit">${escapeHtml(item.unit)}</span></div><span class="muted">${escapeHtml(item.freshness)}</span></div><div class="detail-grid"><div class="detail-field"><span>Состояние</span><strong>${escapeHtml(item.processState)}</strong></div><div class="detail-field"><span>Режим</span><strong>${escapeHtml(item.mode)}</strong></div><div class="detail-field"><span>Управляет</span><strong>${escapeHtml(item.controlOwner)}</strong></div><div class="detail-field"><span>Контекст</span><strong>${state.dashboardMode === "history" ? "Исторический срез" : "Live"}</strong></div></div><div class="drawer-actions"><button class="button small" data-action="open-dashboard-faceplate" data-id="${item.id}">Открыть faceplate</button></div></article>`;
  }

  function renderDashboardTrendWidget(item) {
    if (!item) return `<article class="widget"><div class="widget-head"><strong>Короткий тренд</strong>${badge("Нет контекста", "neutral")}</div><div class="dashboard-widget-empty"><span>Тренд появится после выбора объекта.</span></div></article>`;
    return `<article class="widget"><div class="widget-head"><strong>Короткий тренд · ${escapeHtml(item.primaryValue)} ${escapeHtml(item.unit)}</strong>${qualityBadge(item.quality)}</div><div class="row-meta">${escapeHtml(item.name)} · ${state.dashboardMode === "history" ? "13:30–14:30" : "последний час"}</div>${sparkline(item.points)}<div class="drawer-actions"><button class="button small ghost" data-nav="${item.historyRoute}">Открыть историю ${icon("chevron")}</button></div></article>`;
  }

  function renderDashboardEventsWidget(item) {
    if (!item) return `<article class="widget large"><div class="widget-head"><strong>Связанные события</strong>${badge("Нет контекста", "neutral")}</div><div class="dashboard-widget-empty"><span>Список обновится вместе с выбранным объектом.</span></div></article>`;
    const related = item.eventIds.map((id) => events.find((event) => event.id === id)).filter(Boolean);
    return `<article class="widget large"><div class="widget-head"><strong>Связанные события · ${escapeHtml(item.name)}</strong><button class="button small ghost" data-nav="/events">Открыть диспетчер ${icon("chevron")}</button></div>${related.length ? `<div class="attention-list">${related.map((event) => `<div class="attention-row" data-action="open-event" data-id="${event.id}" tabindex="0"><span class="severity-bar ${event.severity}"></span><div><div class="row-title">${escapeHtml(event.message)}</div><div class="row-meta">${event.time} · ${escapeHtml(event.source)}</div></div>${event.ack ? badge("Подтверждено", "neutral") : badge("Не подтверждено", "high")}</div>`).join("")}</div>` : `<div class="dashboard-widget-empty"><strong>Связанных событий нет</strong><span>${state.dashboardMode === "history" ? "В выбранном периоде записи не найдены." : "Текущее состояние не требует реакции."}</span></div>`}</article>`;
  }

  function renderQualityStatesWidget() {
    return `<article class="widget"><div class="widget-head"><strong>Состояния данных</strong><span class="muted">Единый словарь</span></div><div class="quality-state-list">${["good", "uncertain", "bad", "stale", "offline", "initializing"].map((quality) => qualityBadge(quality)).join("")}</div><p class="row-meta">Авария и качество отображаются независимо: плохое качество не скрывает активное условие.</p></article>`;
  }

  function renderDashboardWindowContent(screen, selected) {
    if (screen === "mimic") return `<div class="page-content dashboard-content dashboard-mimic-content">${renderMimic()}</div>`;
    if (screen === "widgets") return `<div class="page-content dashboard-content"><div class="widget-grid dashboard-widgets-only">${renderDashboardObjectSelector(selected)}${renderDashboardStateWidget(selected)}${renderDashboardTrendWidget(selected)}${renderDashboardEventsWidget(selected)}${renderQualityStatesWidget()}</div></div>`;
    return `<div class="page-content dashboard-content">${renderMimic()}<div class="widget-grid">${renderDashboardStateWidget(selected)}${renderDashboardTrendWidget(selected)}${renderDashboardEventsWidget(selected)}</div></div>`;
  }

  function renderGenericDashboard(dashboard) {
    const relatedEvents = events.filter((item) => item.category === dashboard.system || dashboard.system.includes(item.category)).slice(0, 3);
    const visibleEvents = relatedEvents.length ? relatedEvents : events.slice(0, 3);
    return `<section class="page">
      <div class="page-toolbar">
        <button class="button small ghost catalog-return" data-nav="/dashboards">${icon("dashboard")}<span>Все дашборды</span></button>
        <div class="toolbar-title">${escapeHtml(dashboard.name)}</div><span class="toolbar-subtitle">${escapeHtml(dashboard.purpose)}</span><div class="toolbar-spacer"></div>
        ${dashboard.alarms ? badge(`${dashboard.alarms} требуют внимания`, dashboard.alarms > 1 ? "critical" : "high") : badge("Системы в норме", "good")}
      </div>
      <div class="page-content dashboard-content">
        <div class="metric-grid">${dashboard.metrics.map(([label, value], index) => `<article class="metric ${dashboard.alarms && index === 3 ? "warn" : ""}"><div class="metric-label"><span>${escapeHtml(label)}</span>${index === 3 ? icon("alert") : icon("health")}</div><div class="metric-value">${escapeHtml(value)}</div><div class="metric-note">Актуально · 2 с назад</div></article>`).join("")}</div>
        <div class="generic-dashboard-grid">
          <section class="panel generic-trend"><div class="panel-head"><h2 class="panel-title">Состояние системы</h2><span class="muted">${escapeHtml(dashboard.location)}</span></div><div class="generic-chart">${sparkline("0,44 20,41 40,42 60,35 80,39 100,29 120,31 140,24 160,27 180,20 200,23 220,17")}<div class="generic-chart-caption"><span>00:00</span><span>06:00</span><span>12:00</span><span>Сейчас</span></div></div></section>
          <section class="panel"><div class="panel-head"><h2 class="panel-title">Последние события</h2><button class="button small ghost" data-nav="/events">Открыть диспетчер ${icon("chevron")}</button></div><div class="attention-list">${visibleEvents.map((item) => `<div class="attention-row" data-action="open-event" data-id="${item.id}" tabindex="0"><span class="severity-bar ${item.severity}"></span><div><div class="row-title">${escapeHtml(item.message)}</div><div class="row-meta">${escapeHtml(item.location)} · ${escapeHtml(item.source)}</div></div><div class="time">${item.time.slice(0, 5)}</div></div>`).join("")}</div></section>
        </div>
      </div>
    </section>`;
  }

  function renderDashboard() {
    const dashboard = currentDashboard();
    if (dashboard.id !== "main") return renderGenericDashboard(dashboard);
    const window = currentDashboardWindow();
    const selected = selectedDashboardObject();
    const history = state.dashboardMode === "history";
    return `<section class="page">
      <div class="page-toolbar">
        <button class="button small ghost catalog-return" data-nav="/dashboards">${icon("dashboard")}<span>Все дашборды</span></button>
        <div class="toolbar-title">${escapeHtml(dashboard.name)}</div><button class="button small" data-action="dashboard-windows">${icon("panel")} ${escapeHtml(window.name)}</button>
        <div class="segmented" aria-label="Режим времени"><button class="${history ? "" : "active"}" data-action="dashboard-mode" data-value="live">Live</button><button class="${history ? "active" : ""}" data-action="dashboard-mode" data-value="history">History</button></div>${history ? badge("13:30–14:30", "warning") : ""}<div class="toolbar-spacer"></div>
        ${badge("1 активная авария", "high")}
        <button class="button small" data-action="command-preview" data-id="${selected?.id || ""}" ${history || !selected ? "disabled" : ""} title="${history ? "Команды запрещены в History" : selected ? `Команда для ${escapeHtml(selected.name)}` : "Сначала выберите объект"}">${icon("command")} Команда</button>
        ${window.id === "widgets" ? "" : `<button class="icon-button" data-action="enter-work-fullscreen" title="Мнемосхема на весь экран">${icon("fullscreen")}</button>`}
      </div>
      ${renderDashboardWindowContent(window.id, selected)}
    </section>`;
  }

  function renderWorkFullscreen() {
    const selected = selectedDashboardObject();
    return `<div class="work-fullscreen">${renderMimic()}<div class="fullscreen-hud"><strong>П1 · Мнемосхема</strong>${badge(state.dashboardMode === "history" ? "History" : "Live", state.dashboardMode === "history" ? "warning" : "good")}<span class="status-dot"></span><span class="muted">Связь</span><span class="header-count">${reactionCount()}</span>${selected ? `<span class="muted">${escapeHtml(selected.name)} · ${escapeHtml(selected.id)}</span>` : badge("Объект не выбран", "neutral")}${state.controlEnabled ? badge(`Управление ${formatTimer(state.controlRemaining)}`, "good") : ""}<button class="button small" data-action="exit-work-fullscreen">Выйти · Esc</button></div></div>`;
  }

  function renderTerminalEnroll() {
    const approved = terminalEnrollment.status === "Активен" ? kioskTerminal(terminalEnrollment.approvedTerminalId) : null;
    return `<main class="kiosk-runtime" id="workspace" aria-label="Регистрация терминала"><div class="kiosk-content kiosk-enrollment"><div class="panel"><div class="panel-head"><h1 class="panel-title">D · Регистрация терминала</h1>${badge(approved ? "Активен" : "Ожидает подтверждения", approved ? "good" : "warning")}</div><div class="panel-body">${approved ? `<div class="policy-card"><strong>Терминал зарегистрирован</strong><div class="policy-meta">Device identity активна, одноразовый код больше не действует.</div></div><div class="preflight"><div class="preflight-row"><span>Терминал</span><strong>${escapeHtml(approved.name)}</strong></div><div class="preflight-row"><span>Профиль</span><strong>${escapeHtml(kioskProfile(approved.profileId)?.name || "—")}</strong></div></div><div class="settings-actions"><button class="button primary" data-nav="${escapeHtml(kioskRuntimeRoute(approved))}">Открыть назначенный дашборд</button></div>` : `<p>Сообщите код администратору. На панели нельзя выбрать организацию, пользователя или произвольный дашборд.</p><div class="readonly-value"><strong>${escapeHtml(terminalEnrollment.code.replace(/(\d{3})(\d{3})/, "$1 $2"))}</strong><span>Одноразовый код · запрос ${escapeHtml(terminalEnrollment.requestedAt)}</span></div><div class="preflight"><div class="preflight-row"><span>Устройство</span><strong>${escapeHtml(terminalEnrollment.deviceInfo)}</strong></div><div class="preflight-row"><span>Разрешение</span><strong>${escapeHtml(terminalEnrollment.resolution)}</strong></div></div>`}</div></div></div></main>`;
  }

  function renderKioskDenied(title, message, terminal = null) {
    return `<main class="kiosk-runtime" id="workspace" aria-label="Kiosk недоступен"><div class="kiosk-content kiosk-enrollment"><div class="panel"><div class="panel-head"><h1 class="panel-title">D · Kiosk</h1>${badge(terminal?.status || "Недоступен", terminal?.status === "Отозван" ? "critical" : "warning")}</div><div class="panel-body"><div class="policy-card"><strong>${escapeHtml(title)}</strong><div class="policy-meta">${escapeHtml(message)}</div></div>${terminal ? `<div class="preflight"><div class="preflight-row"><span>Терминал</span><strong>${escapeHtml(terminal.id)}</strong></div><div class="preflight-row"><span>Device identity</span><strong>${escapeHtml(terminal.deviceIdentity)}</strong></div></div>` : ""}</div></div></div></main>`;
  }

  function renderKioskRuntime() {
    const context = kioskRuntimeContext();
    if (!context?.terminal) return renderKioskDenied("Терминал не распознан", "Device identity отсутствует или недействительна.");
    const { terminal, profile, assignmentValid } = context;
    if (terminal.status !== "Активен") return renderKioskDenied("Доступ панели заблокирован", `Состояние device identity: ${terminal.status}.`, terminal);
    if (!assignmentValid || !profile) return renderKioskDenied("Назначение недействительно", "Текущий URL не входит в effective profile терминала.", terminal);
    const dashboard = dashboards.find((item) => item.id === dashboardRouteParts(terminal.dashboardRoute)?.id);
    if (!dashboard) return renderKioskDenied("Дашборд недоступен", "Назначенная опубликованная версия не найдена.", terminal);
    const controlEnabled = kioskControlIsActive(context);
    const commandsEnabled = terminal.online && controlEnabled && profile.authPolicy !== "deny";
    const quality = terminal.online ? "good" : "stale";
    const freshness = terminal.online ? "Обновлено 2 с назад" : `Последняя связь: ${terminal.lastSeen}`;
    const metricCards = dashboard.metrics.slice(0, 4).map(([label, value]) => `<article class="metric"><div class="metric-label"><span>${escapeHtml(label)}</span>${qualityBadge(quality)}</div><div class="metric-value">${escapeHtml(value)}</div><div class="metric-note">${escapeHtml(freshness)}</div></article>`).join("");
    const temperatureControl = profile.allowedActions.includes("Уставка температуры");
    const lightingControl = profile.allowedActions.includes("Освещение");
    return `<main class="kiosk-runtime" id="workspace" aria-label="Kiosk-панель"><h1 class="sr-only">${escapeHtml(dashboard.name)} · kiosk runtime</h1><div class="kiosk-content"><div class="metric-grid">${metricCards}</div><div class="widget-grid"><article class="widget"><div class="widget-head"><strong>Климат</strong>${badge(terminal.online ? "Live" : "Offline", terminal.online ? "good" : "critical")}</div><div class="drawer-actions">${temperatureControl ? `<button class="button" data-action="kiosk-command-preview" data-command-id="temperature-down" ${commandsEnabled ? "" : "disabled"}>Уставка −1 °C</button><button class="button" data-action="kiosk-command-preview" data-command-id="temperature-up" ${commandsEnabled ? "" : "disabled"}>Уставка +1 °C</button>` : `<span class="muted">Изменение уставки не разрешено профилем.</span>`}</div></article><article class="widget"><div class="widget-head"><strong>Освещение</strong><span class="muted">Effective profile</span></div><div class="drawer-actions">${lightingControl ? `<button class="button" data-action="kiosk-command-preview" data-command-id="lighting-toggle" ${commandsEnabled ? "" : "disabled"}>Включить / выключить</button>` : `<span class="muted">Управление освещением не разрешено профилем.</span>`}</div></article></div></div>${terminal.online ? "" : `<div class="kiosk-offline" role="alert" aria-live="assertive"><strong>Нет связи</strong><span>Значения устарели, команды заблокированы. Нажатия не ставятся в очередь.</span></div>`}<div class="fullscreen-hud kiosk-hud"><strong>D · ${escapeHtml(terminal.name)}</strong>${badge(dashboard.name, "neutral")}<span class="status-dot ${terminal.online ? "" : "offline"}"></span><span class="muted">${terminal.online ? "Связь" : "Offline"}</span>${controlEnabled ? `<span class="badge good kiosk-control-time">Управление ${formatTimer(state.kioskControlRemaining)}</span>` : badge("Только просмотр", "neutral")}<span class="toolbar-spacer"></span>${controlEnabled ? `<button class="button small" data-action="kiosk-control-stop">Заблокировать управление</button>` : `<button class="button small primary" data-action="kiosk-control-request" ${terminal.online && profile.authPolicy !== "deny" ? "" : "disabled"}>Включить управление</button>`}</div></main>`;
  }

  function filteredEvents() {
    let result = [...events];
    if (state.eventView === "requires") result = result.filter((item) => item.ackRequired && !item.ack);
    if (state.eventView === "active") result = result.filter((item) => item.kind === "Авария" && item.condition === "Активна");
    if (state.eventView === "unacked") result = result.filter((item) => item.ackRequired && item.ack === false);
    if (state.eventView === "assigned") result = result.filter((item) => item.assignment === currentUser.name);
    if (state.eventView === "incidents") result = result.filter((item) => Boolean(item.incidentId));
    if (state.eventView === "shelved") result = result.filter((item) => item.shelvedUntil || item.suppression);
    if (state.eventSeverity !== "all") result = result.filter((item) => item.severity === state.eventSeverity);
    if (state.eventCondition === "active") result = result.filter((item) => item.condition === "Активна");
    if (state.eventCondition === "cleared") result = result.filter((item) => item.condition === "Норма");
    if (state.eventLocation !== "all") result = result.filter((item) => item.location.startsWith(state.eventLocation));
    if (state.eventAssignment === "unassigned") result = result.filter((item) => item.assignment === "Не назначено");
    if (state.eventAssignment === "mine") result = result.filter((item) => item.assignment === currentUser.name);
    if (state.eventSearch) {
      const query = state.eventSearch.toLowerCase();
      result = result.filter((item) => [item.message, item.location, item.source, item.id].some((value) => value.toLowerCase().includes(query)));
    }
    return result;
  }

  function activeEventFilterCount() {
    return [state.eventSeverity, state.eventCondition, state.eventLocation, state.eventAssignment].filter((value) => value !== "all").length;
  }

  function requireRealtimeEventAction() {
    if (state.eventMode === "realtime") return true;
    toast("История доступна только для чтения", "Вернитесь в Realtime для изменения состояния события.");
    return false;
  }

  function renderEvents() {
    const result = filteredEvents();
    return `<section class="list-page">
      <div class="page-toolbar">
        <div class="toolbar-title">Диспетчер событий</div>
        <select class="select" data-change="event-view" aria-label="Сохранённое представление">
          <option value="requires" ${state.eventView === "requires" ? "selected" : ""}>Требуют реакции</option><option value="all" ${state.eventView === "all" ? "selected" : ""}>Все события</option><option value="active" ${state.eventView === "active" ? "selected" : ""}>Активные аварии</option><option value="unacked" ${state.eventView === "unacked" ? "selected" : ""}>Неподтверждённые</option><option value="assigned" ${state.eventView === "assigned" ? "selected" : ""}>Назначенные мне</option><option value="incidents" ${state.eventView === "incidents" ? "selected" : ""}>Связаны с инцидентом</option><option value="shelved" ${state.eventView === "shelved" ? "selected" : ""}>Shelved и suppressed</option>
        </select>
        <div class="segmented"><button class="${state.eventMode === "realtime" ? "active" : ""}" data-action="event-mode" data-value="realtime">Realtime</button><button class="${state.eventMode === "history" ? "active" : ""}" data-action="event-mode" data-value="history">История</button></div>${state.eventMode === "history" ? badge("Период · 24 часа", "warning") : ""}
        <input class="field hide-narrow" value="${escapeHtml(state.eventSearch)}" data-change="event-search" placeholder="Поиск · Enter" aria-label="Поиск событий">
        <select class="select hide-narrow" data-change="event-severity" aria-label="Важность"><option value="all">Все уровни</option><option value="critical" ${state.eventSeverity === "critical" ? "selected" : ""}>Критические</option><option value="high" ${state.eventSeverity === "high" ? "selected" : ""}>Высокие</option><option value="medium" ${state.eventSeverity === "medium" ? "selected" : ""}>Средние</option></select>
        <button class="button small" data-action="open-event-filters">${icon("filter")} Фильтры</button><div class="toolbar-spacer"></div><span class="muted">${result.length} записей</span><button class="icon-button" title="Колонки">${icon("table")}</button>
      </div>
      <div class="list-body">
        <div class="table-wrap"><table class="working-table"><thead><tr><th class="check-cell"></th><th class="severity-cell">Важность</th><th class="state-cell">Состояние</th><th class="time-cell">Время</th><th>Сообщение</th><th class="location-cell">Локация</th><th class="location-cell">Источник</th><th class="owner-cell">Ответственный</th></tr></thead><tbody>
          ${result.map((item) => `<tr class="${state.drawer?.kind === "events" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-event" data-id="${item.id}" tabindex="0">
            <td class="check-cell">${state.eventMode === "realtime" ? `<input type="checkbox" data-action="toggle-event-selection" data-id="${item.id}" ${state.selectedEvents.has(item.id) ? "checked" : ""} aria-label="Выбрать ${item.id}">` : ""}</td>
            <td>${badge(item.severity === "critical" ? "Критическая" : item.severity === "high" ? "Высокая" : item.severity === "medium" ? "Средняя" : item.severity === "low" ? "Низкая" : "Инфо", item.severity)}</td>
            <td>${escapeHtml(item.kind)} · ${item.condition ? `${escapeHtml(item.condition)} · ${item.ack ? "Ack" : "Не ack"}` : "—"}${item.shelvedUntil ? ` · Shelved` : item.suppression ? ` · Suppressed` : ""}${item.incidentId ? ` · ${escapeHtml(item.incidentId)}` : ""}</td>
            <td class="time">${item.time.slice(0,5)}</td><td class="primary-cell">${escapeHtml(item.message)}</td><td>${escapeHtml(item.location)}</td><td>${escapeHtml(item.source)}</td><td>${escapeHtml(item.assignment || "—")}</td>
          </tr>`).join("") || `<tr><td colspan="8"><div class="empty-state">По текущему представлению записей нет</div></td></tr>`}
        </tbody></table></div>
        ${state.eventMode === "realtime" && state.pendingEvents ? `<div class="new-items"><button class="button primary" data-action="apply-pending-events">Новых: ${state.pendingEvents} · Показать</button></div>` : ""}
        ${state.eventMode === "realtime" && state.selectedEvents.size ? `<div class="bulk-bar"><strong>Выбрано: ${state.selectedEvents.size}</strong><button class="button small" data-action="bulk-preview">Предпросмотр действия</button><button class="icon-button" data-action="clear-event-selection">${icon("close")}</button></div>` : ""}
      </div>
    </section>`;
  }

  function equipmentPprLabel(item) {
    const asset = maintenanceAssets.find((entry) => entry.equipmentId === item.id);
    const plan = asset ? maintenancePlans.find((entry) => entry.assetId === asset.id) : null;
    return plan?.nextDue || item.ppr;
  }

  function renderEquipment() {
    return `<section class="list-page"><div class="page-toolbar"><div class="toolbar-title">Оборудование</div><select class="select"><option>Все доступные</option><option>Нет связи</option><option>С активными авариями</option></select><input class="field hide-narrow" placeholder="Поиск оборудования"><button class="button small">${icon("filter")} Фильтры</button><div class="toolbar-spacer"></div><div class="segmented"><button class="${state.equipmentView === "table" ? "active" : ""}" data-action="equipment-view" data-value="table">${icon("table")} Таблица</button><button class="${state.equipmentView === "tree" ? "active" : ""}" data-action="equipment-view" data-value="tree">${icon("tree")} Дерево</button></div><button class="button small primary" data-nav="/equipment/add">${icon("plus")} Добавить</button></div>
      <div class="list-body">${state.equipmentView === "tree" ? renderEquipmentTree() : `<div class="table-wrap"><table class="working-table"><thead><tr><th>Оборудование</th><th class="location-cell">Тип</th><th class="location-cell">Локация</th><th class="state-cell">Связь</th><th class="state-cell">Статус</th><th class="time-cell">Аварии</th><th class="state-cell">Ближайший ППР</th><th class="owner-cell">Ответственный</th></tr></thead><tbody>${equipment.map((item) => `<tr class="${state.drawer?.kind === "equipment" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-equipment" data-id="${item.id}" tabindex="0"><td class="primary-cell">${escapeHtml(item.name)}<div class="row-meta">${item.id}</div></td><td>${escapeHtml(item.type)}</td><td>${escapeHtml(item.location)}</td><td>${qualityBadge(item.quality)}</td><td>${badge(item.status, item.status === "Авария" ? "critical" : item.status === "Норма" ? "good" : "neutral")}</td><td>${item.alarms || "—"}</td><td>${escapeHtml(equipmentPprLabel(item))}</td><td>${escapeHtml(item.owner)}</td></tr>`).join("")}</tbody></table></div>`}</div>
    </section>`;
  }

  function maintenanceAsset(id) {
    return maintenanceAssets.find((item) => item.id === id) || null;
  }

  function maintenancePlan(id) {
    return maintenancePlans.find((item) => item.id === id) || null;
  }

  function maintenanceStatusBadge(status) {
    const type = status === "Просрочен" ? "critical" : status === "На приёмке" || status === "В работе" ? "warning" : status === "Выполнен" ? "good" : status === "Прогноз" ? "neutral" : "medium";
    return badge(status, type);
  }

  function maintenanceSectionSelect() {
    const section = currentMaintenanceSection();
    const options = [["overview", "/maintenance", "Обзор"], ["assets", "/maintenance/assets", "Объекты"], ["plans", "/maintenance/plans", "Планы ППР"], ["calendar", "/maintenance/calendar", "Календарь"], ["requests", "/maintenance/requests", "Заявки и дефекты"], ["work-orders", "/maintenance/work-orders", "Наряды"]];
    return `<select class="select maintenance-section-select" data-change="maintenance-section" aria-label="Раздел ТОиР">${options.map(([id, route, label]) => `<option value="${route}" ${section === id ? "selected" : ""}>${label}</option>`).join("")}</select>`;
  }

  function renderMaintenanceShell(content, extra = "") {
    return `<section class="page maintenance-page"><div class="page-toolbar"><div class="toolbar-title">ТОиР и ППР</div>${maintenanceSectionSelect()}${extra}<div class="toolbar-spacer"></div><span class="muted">Главный комплекс</span></div><div class="page-content">${content}</div></section>`;
  }

  function maintenanceOrdersForFilter(id = state.maintenanceFilter) {
    const active = maintenanceWorkOrders.filter((item) => item.status !== "Выполнен" && item.status !== "Закрыт");
    if (id === "overdue") return active.filter((item) => item.status === "Просрочен");
    if (id === "today") return active.filter((item) => item.today);
    if (id === "unassigned") return active.filter((item) => item.assignee === "Не назначен");
    if (id === "acceptance") return active.filter((item) => item.status === "На приёмке");
    return active;
  }

  function renderMaintenanceOrderTable(items) {
    return `<div class="table-wrap"><table class="working-table maintenance-orders-table"><thead><tr><th class="state-cell">Наряд</th><th>Работа</th><th class="location-cell">Объект</th><th class="location-cell">Локация</th><th class="state-cell">Срок</th><th class="state-cell">Состояние</th><th class="owner-cell">Исполнитель</th></tr></thead><tbody>${items.length ? items.map((item) => { const asset = maintenanceAsset(item.assetId); return `<tr class="${state.drawer?.kind === "maintenance-work-order" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-maintenance-work-order" data-id="${item.id}" tabindex="0"><td>${escapeHtml(item.id)}</td><td class="primary-cell">${escapeHtml(item.title)}</td><td>${escapeHtml(asset?.name || "Объект недоступен")}</td><td>${escapeHtml(asset?.location || "—")}</td><td>${escapeHtml(item.dueDate)} · ${escapeHtml(item.time)}</td><td>${maintenanceStatusBadge(item.status)}</td><td>${escapeHtml(item.assignee)}</td></tr>`; }).join("") : `<tr><td colspan="7"><div class="empty-state">Нет работ по выбранному условию</div></td></tr>`}</tbody></table></div>`;
  }

  function renderMaintenanceOverview() {
    const counters = [["overdue", "Просрочено", "critical"], ["today", "Сегодня", "warning"], ["unassigned", "Требует назначения", "medium"], ["acceptance", "Ожидает приёмки", "good"]];
    const items = maintenanceOrdersForFilter();
    const content = `<div class="metric-grid">${counters.map(([id, label, type]) => `<button class="metric ${type === "critical" ? "alert" : type === "warning" ? "warn" : ""} ${state.maintenanceFilter === id ? "active" : ""}" data-action="maintenance-filter" data-value="${id}" aria-pressed="${state.maintenanceFilter === id}"><div class="metric-label"><span>${label}</span></div><div class="metric-value">${maintenanceOrdersForFilter(id).length}</div><div class="metric-note">Фильтровать общую очередь</div></button>`).join("")}</div><section class="panel"><div class="panel-head"><h2 class="panel-title">Ближайшие работы · ${items.length}</h2><button class="button small ghost" data-action="maintenance-filter" data-value="all">Сбросить</button></div>${renderMaintenanceOrderTable(items)}</section>`;
    return renderMaintenanceShell(content);
  }

  function renderMaintenanceAssets() {
    const rows = maintenanceAssets.map((item) => { const plan = maintenancePlans.find((entry) => entry.assetId === item.id); const linkType = item.linkState === "Связан с устройством" ? "good" : item.linkState === "Связь требует проверки" ? "warning" : "neutral"; return `<tr class="${state.drawer?.kind === "maintenance-asset" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-maintenance-asset" data-id="${item.id}" tabindex="0"><td class="primary-cell">${escapeHtml(item.name)}<div class="row-meta">${escapeHtml(item.id)}</div></td><td>${escapeHtml(item.type)}</td><td>${escapeHtml(item.location)}</td><td>${badge(item.linkState, linkType)}</td><td>${escapeHtml(plan?.nextDue || "Нет активного плана")}</td><td>${escapeHtml(item.owner)}</td></tr>`; }).join("");
    return renderMaintenanceShell(`<section class="panel"><div class="panel-head"><h2 class="panel-title">Объекты обслуживания · ${maintenanceAssets.length}</h2><span class="muted">Телеметрия необязательна</span></div><div class="table-wrap"><table class="working-table"><thead><tr><th>Объект</th><th class="location-cell">Тип</th><th class="location-cell">Локация</th><th class="owner-cell">Связь с диспетчеризацией</th><th class="state-cell">Ближайший ППР</th><th class="owner-cell">Ответственный</th></tr></thead><tbody>${rows}</tbody></table></div></section>`);
  }

  function renderMaintenancePlans() {
    const rows = maintenancePlans.map((item) => { const asset = maintenanceAsset(item.assetId); return `<tr class="${state.drawer?.kind === "maintenance-plan" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-maintenance-plan" data-id="${item.id}" tabindex="0"><td>${escapeHtml(item.id)}</td><td class="primary-cell">${escapeHtml(item.name)}</td><td>${escapeHtml(asset?.name || "—")}</td><td>${escapeHtml(item.trigger)}</td><td>${escapeHtml(item.nextDue)}</td><td>${badge(item.status, item.status === "Активен" ? "good" : "warning")}</td><td>${escapeHtml(item.assignee)}</td></tr>`; }).join("");
    return renderMaintenanceShell(`<section class="panel"><div class="panel-head"><h2 class="panel-title">Планы ППР · ${maintenancePlans.length}</h2><span class="muted">Редактор плана будет проработан позднее</span></div><div class="table-wrap"><table class="working-table"><thead><tr><th class="state-cell">План</th><th>Назначение</th><th class="location-cell">Объект</th><th class="state-cell">Запуск</th><th class="state-cell">Ближайшая дата</th><th class="state-cell">Состояние</th><th class="owner-cell">Исполнитель</th></tr></thead><tbody>${rows}</tbody></table></div></section>`);
  }

  function renderMaintenanceCalendar() {
    const extra = `<div class="segmented"><button class="${state.maintenanceCalendarMode === "calendar" ? "active" : ""}" data-action="maintenance-calendar-mode" data-value="calendar">Неделя</button><button class="${state.maintenanceCalendarMode === "list" ? "active" : ""}" data-action="maintenance-calendar-mode" data-value="list">Список</button></div>`;
    if (state.maintenanceCalendarMode === "list") return renderMaintenanceShell(`<section class="panel"><div class="panel-head"><h2 class="panel-title">Неделя 13–19 июля</h2>${badge("Прогноз: 1", "neutral")}</div>${renderMaintenanceOrderTable(maintenanceWorkOrders)}</section>`, extra);
    const days = ["Пн 13", "Вт 14", "Ср 15", "Чт 16", "Пт 17", "Сб 18", "Вс 19"];
    const columns = days.map((day, dayIndex) => { const orders = maintenanceWorkOrders.filter((item) => item.dayIndex === dayIndex); const forecasts = maintenanceForecasts.filter((item) => item.dayIndex === dayIndex); const slots = [...orders.map((item) => ({ kind: "order", item })), ...forecasts.map((item) => ({ kind: "forecast", item }))]; return `<section class="maintenance-day"><strong>${day}</strong><div class="maintenance-day-slots">${slots.map(({ kind, item }) => `<button class="maintenance-slot ${state.drawer?.id === item.id ? "selected" : ""}" data-action="${kind === "order" ? "open-maintenance-work-order" : "open-maintenance-forecast"}" data-id="${item.id}"><span>${escapeHtml(item.time)}</span><strong>${escapeHtml(item.title)}</strong>${maintenanceStatusBadge(item.status)}</button>`).join("") || `<span class="muted">Нет работ</span>`}</div></section>`; }).join("");
    return renderMaintenanceShell(`<section class="panel"><div class="panel-head"><h2 class="panel-title">Неделя 13–19 июля</h2><span class="muted">Прогноз и созданный наряд различаются текстом</span></div><div class="maintenance-calendar">${columns}</div></section>`, extra);
  }

  function renderMaintenanceRequests() {
    const rows = maintenanceRequests.map((item) => `<tr class="${state.drawer?.kind === "maintenance-request" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-maintenance-request" data-id="${item.id}" tabindex="0"><td>${badge(item.type, item.type === "Дефект" ? "warning" : "neutral")}</td><td class="primary-cell">${escapeHtml(item.description)}</td><td>${escapeHtml(item.object)}</td><td>${escapeHtml(item.location)}</td><td>${badge(item.priority, item.priority === "Высокий" ? "high" : "neutral")}</td><td>${escapeHtml(item.status)}</td><td>${escapeHtml(item.source)}</td></tr>`).join("");
    return renderMaintenanceShell(`<section class="panel"><div class="panel-head"><h2 class="panel-title">Заявки и дефекты · ${maintenanceRequests.length}</h2><span class="muted">Отдельно от событий и аварий</span></div><div class="table-wrap"><table class="working-table"><thead><tr><th class="state-cell">Тип</th><th>Описание</th><th class="location-cell">Объект</th><th class="location-cell">Локация</th><th class="state-cell">Приоритет</th><th class="state-cell">Состояние</th><th class="owner-cell">Источник</th></tr></thead><tbody>${rows}</tbody></table></div></section>`);
  }

  function renderMaintenanceWorkOrders() {
    return renderMaintenanceShell(`<section class="panel"><div class="panel-head"><h2 class="panel-title">Наряды · ${maintenanceWorkOrders.length}</h2><span class="muted">Открытие строки показывает следующее допустимое действие</span></div>${renderMaintenanceOrderTable(maintenanceWorkOrders)}</section>`);
  }

  function renderMaintenance() {
    const direct = state.route.match(/^\/maintenance\/work-orders\/([^/]+)$/);
    if (direct) {
      if (!maintenanceWorkOrders.some((item) => item.id === direct[1])) return renderNotFound("Наряд не найден или недоступен.");
      state.drawer = { kind: "maintenance-work-order", id: direct[1] };
    }
    const section = currentMaintenanceSection();
    if (section === "assets") return renderMaintenanceAssets();
    if (section === "plans") return renderMaintenancePlans();
    if (section === "calendar") return renderMaintenanceCalendar();
    if (section === "requests") return renderMaintenanceRequests();
    if (section === "work-orders") return renderMaintenanceWorkOrders();
    return renderMaintenanceOverview();
  }

  function renderEquipmentTree() {
    return `<div class="page-content"><section class="panel"><div class="panel-head"><h2 class="panel-title">Главный комплекс</h2><span class="muted">${equipment.length} объектов в демонстрации</span></div><div class="attention-list">${["Корпус A", "Корпус B", "ЦОД", "ГРЩ"].map((locationName) => `<div><div class="panel-head"><strong>${locationName}</strong></div>${equipment.filter((item) => item.location.startsWith(locationName)).map((item) => `<div class="attention-row" data-action="open-equipment" data-id="${item.id}" tabindex="0"><span class="severity-bar ${item.status === "Авария" ? "critical" : ""}"></span><div><div class="row-title">${item.name}</div><div class="row-meta">${item.type} · ${item.connection}</div></div>${qualityBadge(item.quality)}</div>`).join("") || `<div class="panel-body muted">Нет объектов в демонстрационном наборе</div>`}</div>`).join("")}</div></section></div>`;
  }

  function renderEquipmentAdd() {
    return renderEquipmentDraftWorkspace();
  }

  function nextEquipmentDraftKey() {
    const key = `draft-${state.equipmentDraftSequence}`;
    state.equipmentDraftSequence += 1;
    return key;
  }

  function uniqueEquipmentDraftValue(base, field) {
    const used = new Set([...state.equipmentDrafts.map((item) => item[field]), ...equipment.map((item) => item[field])]);
    let index = 1;
    let candidate = field === "id" ? `${base}-COPY-${index}` : `${base} (копия ${index})`;
    while (used.has(candidate)) { index += 1; candidate = field === "id" ? `${base}-COPY-${index}` : `${base} (копия ${index})`; }
    return candidate;
  }

  function createEquipmentDraft(seed = {}) {
    const sequence = state.equipmentDraftSequence;
    return {
      draftKey: nextEquipmentDraftKey(), id: `NEW-${String(sequence).padStart(3, "0")}`, name: `Новое устройство ${sequence}`,
      type: "Устройство", location: "Главный комплекс", protocol: "Modbus TCP", host: "", port: "502", unitId: "1", timeout: "1500",
      snmpVersion: "v2c", community: "public", snmpUsername: "", authSecret: "", privacySecret: "", source: "manual", allowUpdate: false, ...seed,
    };
  }

  function selectedEquipmentDraft() {
    return state.equipmentDrafts.find((item) => item.draftKey === state.selectedEquipmentDraftKey) || null;
  }

  function equipmentDraftSignature(draft) {
    if (!draft) return "";
    return JSON.stringify({
      id: draft.id, name: draft.name, type: draft.type, location: draft.location,
      protocol: draft.protocol, host: draft.host, port: draft.port, unitId: draft.unitId,
      timeout: draft.timeout, snmpVersion: draft.snmpVersion, community: draft.community,
      snmpUsername: draft.snmpUsername, authSecret: draft.authSecret, privacySecret: draft.privacySecret,
    });
  }

  function equipmentDraftValidation(draft) {
    const errors = {};
    const required = (field, message = "Обязательное поле") => { if (!String(draft[field] || "").trim()) errors[field] = message; };
    ["id", "name", "type", "location", "host", "port"].forEach((field) => required(field));
    if (draft.id && state.equipmentDrafts.filter((item) => item.id === draft.id).length > 1) errors.id = "ID повторяется в staging";
    const port = Number(draft.port);
    if (draft.port && (!Number.isInteger(port) || port < 1 || port > 65535)) errors.port = "Port: целое 1–65535";
    if (draft.protocol === "Modbus TCP") {
      required("unitId", "Unit ID обязателен"); required("timeout", "Timeout обязателен");
      const unitId = Number(draft.unitId); const timeout = Number(draft.timeout);
      if (draft.unitId && (!Number.isInteger(unitId) || unitId < 1 || unitId > 247)) errors.unitId = "Unit ID: целое 1–247";
      if (draft.timeout && (!Number.isFinite(timeout) || timeout <= 0)) errors.timeout = "Timeout должен быть больше 0";
    } else if (draft.protocol === "SNMP") {
      if (!['v2c', 'v3'].includes(draft.snmpVersion)) errors.snmpVersion = "Выберите v2c или v3";
      if (draft.snmpVersion === "v2c") required("community", "Community обязателен");
      else { required("snmpUsername", "Username обязателен"); required("authSecret", "Auth secret обязателен"); required("privacySecret", "Privacy secret обязателен"); }
    } else errors.protocol = "Поддерживаются только Modbus TCP и SNMP";
    return errors;
  }

  function equipmentDiagnosticValidation(draft) {
    const errors = equipmentDraftValidation(draft);
    const diagnosticFields = new Set(["protocol", "host", "port"]);
    if (draft.protocol === "Modbus TCP") {
      diagnosticFields.add("unitId");
      diagnosticFields.add("timeout");
    } else if (draft.protocol === "SNMP") {
      diagnosticFields.add("snmpVersion");
      if (draft.snmpVersion === "v2c") diagnosticFields.add("community");
      else {
        diagnosticFields.add("snmpUsername");
        diagnosticFields.add("authSecret");
        diagnosticFields.add("privacySecret");
      }
    }
    return Object.fromEntries(Object.entries(errors).filter(([field]) => diagnosticFields.has(field)));
  }

  function equipmentDraftReview(draft) {
    const errors = equipmentDraftValidation(draft);
    const valid = Object.keys(errors).length === 0;
    const existing = equipment.some((item) => item.id === draft.id);
    if (!valid) return { draft, errors, valid: false, operation: "draft", review: "Исправьте структурные ошибки", type: "critical" };
    if (existing && !draft.allowUpdate) return { draft, errors, valid: true, operation: "skip", review: "Existing: требуется явный update", type: "warning", existing: true };
    if (existing) return { draft, errors, valid: true, operation: "update", review: "Update разрешён администратором", type: "warning", existing: true };
    return { draft, errors, valid: true, operation: "create", review: "Новый объект", type: "good", existing: false };
  }

  function normalizeEquipmentCsvHeader(value) {
    const key = String(value || "").replace(/^\uFEFF/, "").trim().toLowerCase().replace(/[\s_-]+/g, "");
    const aliases = { id: "id", name: "name", title: "name", type: "type", location: "location", protocol: "protocol", host: "host", ip: "host", port: "port", unitid: "unitId", timeout: "timeout", version: "snmpVersion", snmpversion: "snmpVersion", community: "community", username: "snmpUsername", snmpusername: "snmpUsername", authsecret: "authSecret", privacysecret: "privacySecret" };
    return aliases[key] || key;
  }

  function parseEquipmentCsvLine(line, delimiter) {
    const values = [];
    let current = "";
    let quoted = false;
    for (let index = 0; index < line.length; index += 1) {
      const char = line[index];
      if (char === '"') {
        if (quoted && line[index + 1] === '"') { current += '"'; index += 1; }
        else quoted = !quoted;
      } else if (char === delimiter && !quoted) {
        values.push(current.trim()); current = "";
      } else current += char;
    }
    values.push(current.trim());
    return values;
  }

  function parseEquipmentCsv(text) {
    const lines = String(text || "").split(/\r?\n/).map((line) => line.trim()).filter(Boolean);
    if (lines.length < 2) return [];
    const delimiter = (lines[0].match(/;/g) || []).length >= (lines[0].match(/,/g) || []).length ? ";" : ",";
    const headers = parseEquipmentCsvLine(lines[0], delimiter).map(normalizeEquipmentCsvHeader);
    if (!headers.includes("id") && !headers.includes("name")) return [];
    return lines.slice(1).map((line) => {
      const values = parseEquipmentCsvLine(line, delimiter);
      const row = {}; headers.forEach((header, index) => { row[header] = values[index] || ""; });
      const rawProtocol = String(row.protocol || "").trim();
      const normalizedProtocol = rawProtocol.toLowerCase();
      const snmpProtocols = ["snmp", "snmp v2c", "snmpv2c", "snmp v3", "snmpv3"];
      const protocol = snmpProtocols.includes(normalizedProtocol) ? "SNMP" : ["modbus", "modbus tcp", "modbustcp"].includes(normalizedProtocol) ? "Modbus TCP" : rawProtocol;
      const snmpVersion = row.snmpVersion || (normalizedProtocol.includes("v3") ? "v3" : "v2c");
      return createEquipmentDraft({ ...row, protocol, port: row.port || (protocol === "SNMP" ? "161" : protocol === "Modbus TCP" ? "502" : ""), unitId: row.unitId || "1", timeout: row.timeout || "1500", snmpVersion, community: row.community || (protocol === "SNMP" && snmpVersion === "v2c" ? "public" : ""), source: "csv", allowUpdate: false });
    });
  }

  function readEquipmentCsv(file) {
    if (!file) return;
    const reader = new FileReader();
    reader.onload = () => {
      const drafts = parseEquipmentCsv(reader.result);
      if (!drafts.length) { toast("CSV не импортирован", "Нужны header и хотя бы одна строка; delimiter comma или semicolon."); return; }
      state.equipmentDrafts.push(...drafts); state.selectedEquipmentDraftKey = drafts[0].draftKey; state.equipmentCommunityVisible = false; render();
      toast("CSV добавлен в staging", `Строк: ${drafts.length}. Existing остаётся skip/review; удалений нет.`);
    };
    reader.onerror = () => toast("Ошибка чтения CSV", "Файл не был изменён и staging сохранён.");
    reader.readAsText(file, "UTF-8");
  }

  function equipmentTemplateConfig(draft) {
    return { type: draft.type, location: draft.location, protocol: draft.protocol, port: draft.port, timeout: draft.timeout, snmpVersion: draft.snmpVersion, snmpUsername: draft.snmpUsername };
  }

  function showEquipmentCopyModal() {
    const draft = selectedEquipmentDraft(); if (!draft) return;
    openModal("Копировать черновик", `<label class="form-label">Количество<input id="equipment-copy-quantity" class="field" type="number" min="1" max="50" value="2"></label>${draft.protocol === "Modbus TCP" ? `<label class="form-label">Начальный Unit ID · необязательно<input id="equipment-copy-unit-start" class="field" type="number" min="1" max="247" placeholder="Оставить исходный"></label>` : ""}<p class="muted">ID и названия получают уникальные draft-суффиксы. IP/host никогда не увеличивается автоматически${draft.protocol === "SNMP" ? " и копируется без изменений" : "; при заданном старте Unit ID увеличивается последовательно"}.</p>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="equipment-copy-confirm">Создать копии</button>`);
  }

  function showEquipmentTemplateSaveModal() {
    const draft = selectedEquipmentDraft(); if (!draft) return;
    openModal("Сохранить как шаблон", `<label class="form-label">Название шаблона<input id="equipment-template-name" class="field" value="${escapeHtml(`${draft.type} · ${draft.protocol}`)}"></label><p class="muted">Не сохраняются ID, название устройства, IP/host, Unit ID, community и другие секреты.</p>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="equipment-template-save-confirm">Сохранить</button>`);
  }

  function renderEquipmentDraftInput(draft, errors, field, label, options = {}) {
    const error = errors[field];
    const type = options.type || "text";
    return `<label class="form-label draft-field">${label}<input class="field ${error ? "invalid" : ""}" type="${type}" ${options.inputmode ? `inputmode="${options.inputmode}"` : ""} ${type === "password" ? "autocomplete=\"new-password\"" : ""} data-change="equipment-draft-field" data-field="${field}" value="${escapeHtml(draft[field] || "")}">${error ? `<span class="field-error">${escapeHtml(error)}</span>` : ""}</label>`;
  }

  function renderEquipmentDraftForm(review) {
    const { draft, errors } = review;
    const diagnostic = state.equipmentDiagnostics[draft.draftKey] || {};
    const unsupportedProtocol = draft.protocol !== "Modbus TCP" && draft.protocol !== "SNMP";
    const diagnosticBadge = (status, testingLabel, successLabel, failureLabel) => !status ? "" : status === "testing" ? badge(testingLabel, "warning") : status === "failure" ? badge(failureLabel, "critical") : badge(successLabel, "good");
    const diagnosticDetails = [diagnostic.connectionDetail ? `Связь: ${diagnostic.connectionDetail}` : "", diagnostic.pollDetail ? `Опрос: ${diagnostic.pollDetail}` : ""].filter(Boolean);
    const diagnosticMarkup = `<div class="equipment-diagnostic"><p>Проверка связи и пробный опрос информационные и не влияют на Apply.</p><div class="equipment-add-actions"><button class="button small" data-action="equipment-draft-connection">Проверить связь</button><button class="button small" data-action="equipment-draft-poll">Пробный опрос</button>${diagnosticBadge(diagnostic.connection, "Связь…", "Связь OK", "Ошибка связи")}${diagnosticBadge(diagnostic.poll, "Опрос…", "Данные получены", "Ошибка опроса")}</div>${diagnosticDetails.length ? `<p class="${diagnostic.connection === "failure" || diagnostic.poll === "failure" ? "form-error" : "settings-note"}">${escapeHtml(diagnosticDetails.join(" · "))}</p>` : ""}${diagnostic.poll === "success" ? `<div class="poll-preview"><span>Temperature 21,4 · Status Running · качество good</span></div>` : ""}</div>`;
    const protocolFields = draft.protocol === "SNMP"
      ? `${renderEquipmentDraftInput(draft, errors, "host", "Host / IP")}${renderEquipmentDraftInput(draft, errors, "port", "Port", { inputmode: "numeric" })}<label class="form-label draft-field">SNMP version<select class="select ${errors.snmpVersion ? "invalid" : ""}" data-change="equipment-draft-snmp-version"><option value="v2c" ${draft.snmpVersion === "v2c" ? "selected" : ""}>v2c</option><option value="v3" ${draft.snmpVersion === "v3" ? "selected" : ""}>v3</option></select>${errors.snmpVersion ? `<span class="field-error">${escapeHtml(errors.snmpVersion)}</span>` : ""}</label>${draft.snmpVersion === "v2c" ? `<label class="form-label draft-field">Community<div class="secret-field"><input class="field ${errors.community ? "invalid" : ""}" type="${state.equipmentCommunityVisible ? "text" : "password"}" autocomplete="new-password" data-change="equipment-draft-field" data-field="community" value="${escapeHtml(draft.community || "")}"><button class="icon-button" data-action="equipment-toggle-community" title="Показать или скрыть community">${icon("eye")}</button></div>${errors.community ? `<span class="field-error">${escapeHtml(errors.community)}</span>` : ""}${draft.community === "public" ? `<span class="public-community-warning">Community public используется по умолчанию и небезопасен для production.</span>` : ""}</label>` : `${renderEquipmentDraftInput(draft, errors, "snmpUsername", "Username")}${renderEquipmentDraftInput(draft, errors, "authSecret", "Auth secret", { type: "password" })}${renderEquipmentDraftInput(draft, errors, "privacySecret", "Privacy secret", { type: "password" })}`}`
      : `${renderEquipmentDraftInput(draft, errors, "host", "Host / IP")}${renderEquipmentDraftInput(draft, errors, "port", "Port", { inputmode: "numeric" })}${renderEquipmentDraftInput(draft, errors, "unitId", "Unit ID", { inputmode: "numeric" })}${renderEquipmentDraftInput(draft, errors, "timeout", "Timeout, ms", { inputmode: "numeric" })}`;
    return `<aside class="panel equipment-draft-form-panel"><div class="panel-head"><h2 class="panel-title">Параметры черновика</h2>${badge(review.operation, review.operation === "create" ? "good" : review.operation === "update" ? "warning" : "critical")}</div><div class="panel-body"><div class="equipment-draft-form">${renderEquipmentDraftInput(draft, errors, "id", "Идентификатор")}${renderEquipmentDraftInput(draft, errors, "name", "Название")}${renderEquipmentDraftInput(draft, errors, "type", "Тип")}${renderEquipmentDraftInput(draft, errors, "location", "Локация")}<label class="form-label draft-field">Протокол<select class="select ${errors.protocol ? "invalid" : ""}" data-change="equipment-draft-protocol">${unsupportedProtocol ? `<option value="${escapeHtml(draft.protocol)}" selected disabled>Неподдерживаемый: ${escapeHtml(draft.protocol || "не задан")}</option>` : ""}<option ${draft.protocol === "Modbus TCP" ? "selected" : ""}>Modbus TCP</option><option ${draft.protocol === "SNMP" ? "selected" : ""}>SNMP</option></select>${errors.protocol ? `<span class="field-error">${escapeHtml(errors.protocol)}</span>` : ""}</label>${protocolFields}</div>${review.existing ? `<div class="draft-review-action"><strong>Existing ID</strong><span>По умолчанию строка остаётся skip/review.</span><button class="button small ${draft.allowUpdate ? "danger" : ""}" data-action="equipment-draft-toggle-update" data-id="${draft.draftKey}">${draft.allowUpdate ? "Запретить update" : "Разрешить update"}</button></div>` : ""}${diagnosticMarkup}<div class="settings-actions"><button class="button" data-action="equipment-copy-open">Копировать</button><button class="button" data-action="equipment-template-save">Сохранить как шаблон</button><button class="button danger" data-action="equipment-draft-delete">Удалить черновик</button></div></div></aside>`;
  }

  function renderEquipmentDraftWorkspace() {
    const reviews = state.equipmentDrafts.map(equipmentDraftReview);
    const selected = reviews.find((item) => item.draft.draftKey === state.selectedEquipmentDraftKey) || reviews[0] || null;
    const actionable = reviews.filter((item) => item.valid && (item.operation === "create" || item.operation === "update"));
    const counts = { create: reviews.filter((item) => item.operation === "create").length, update: reviews.filter((item) => item.operation === "update").length, skip: reviews.filter((item) => item.operation === "skip").length, draft: reviews.filter((item) => item.operation === "draft").length };
    const template = state.equipmentTemplates.find((item) => item.id === state.equipmentSelectedTemplateId) || state.equipmentTemplates[0];
    const rows = reviews.map((item) => `<tr class="equipment-draft-row ${item.draft.draftKey === state.selectedEquipmentDraftKey ? "selected" : ""} ${item.valid ? "" : "invalid"}" data-action="equipment-draft-select" data-id="${item.draft.draftKey}" tabindex="0"><td class="primary-cell">${escapeHtml(item.draft.name || "Без названия")}<div class="row-meta">${escapeHtml(item.draft.id || "ID не задан")} · ${escapeHtml(item.draft.source)}</div></td><td>${escapeHtml(item.draft.protocol)}<div class="row-meta">${escapeHtml(item.draft.host || "Host не задан")}:${escapeHtml(item.draft.port || "—")}</div></td><td>${badge(item.operation, item.operation === "create" ? "good" : item.operation === "update" ? "warning" : "critical")}</td><td><span class="review-text ${item.type}">${escapeHtml(item.review)}</span></td><td>${Object.keys(item.errors).length ? `${Object.keys(item.errors).length} ошибок` : "Структура валидна"}</td></tr>`).join("");
    return `<section class="page equipment-add-page"><div class="page-toolbar"><button class="button small" data-nav="/equipment">${icon("chevron")} К реестру</button><div class="toolbar-title">Staging оборудования</div><div class="toolbar-spacer"></div><button class="button small" data-action="equipment-draft-add">${icon("plus")} Добавить черновик</button><label class="button small equipment-csv-button">Импорт CSV<input class="sr-only" type="file" accept=".csv,text/csv" data-change="equipment-csv"></label><button class="button small primary" data-action="equipment-drafts-apply" ${actionable.length ? "" : "disabled"}>Apply valid (${actionable.length})</button></div><div class="page-content equipment-add-content"><section class="panel equipment-template-bar"><div class="panel-body"><strong>Шаблоны</strong><select class="select" data-change="equipment-template-select">${state.equipmentTemplates.map((item) => `<option value="${item.id}" ${item.id === template?.id ? "selected" : ""}>${escapeHtml(item.name)}</option>`).join("")}</select><button class="button small" data-action="equipment-template-apply" ${template ? "" : "disabled"}>${selected ? "Применить к выбранному" : "Создать черновик из шаблона"}</button><button class="button small danger" data-action="equipment-template-delete" ${template ? "" : "disabled"}>Удалить шаблон</button><span class="muted">Шаблон исключает ID, имя, IP/host, Unit ID и секреты.</span></div></section><div class="equipment-staging-summary">${badge(`create ${counts.create}`, "good")}${badge(`update ${counts.update}`, "warning")}${badge(`skip ${counts.skip}`, "critical")}${badge(`draft ${counts.draft}`, "neutral")}<span>CSV: header-based, comma/semicolon. Existing = skip/review до явного update. Missing from CSV не удаляет оборудование.</span></div><div class="equipment-add-workspace"><section class="panel equipment-staging-panel"><div class="table-wrap"><table class="working-table"><thead><tr><th>Черновик</th><th>Подключение</th><th>Action</th><th>Review</th><th>Validation</th></tr></thead><tbody>${rows || `<tr><td colspan="5"><div class="empty-state">Добавьте черновик или выберите CSV.</div></td></tr>`}</tbody></table></div></section>${selected ? renderEquipmentDraftForm(selected) : `<aside class="panel"><div class="empty-state">Выберите строку staging.</div></aside>`}</div></div></section>`;
  }

  function renderPersonPage(person, isSelf) {
    const availability = isSelf ? state.profileStatus : person.availability;
    const availabilityUntil = isSelf ? state.profileStatusUntil : person.availabilityUntil;
    const manager = person.manager === "Анна Волкова" ? `<button class="text-link" data-nav="/users/anna">Анна Волкова</button>` : escapeHtml(person.manager);
    const toolbarActions = isSelf
      ? `<button class="button small primary" data-nav="/settings/profile">${icon("settings")} Редактировать</button><button class="button small" data-nav="/settings/privacy">${icon("eye")} Как меня видят</button>`
      : `<button class="button small primary" data-action="contact-person" data-id="${person.id}">Связаться</button><button class="button small" data-action="mention-person" data-id="${person.id}">Упомянуть</button><button class="button small" data-action="assign-person" data-id="${person.id}">Назначить задачу</button>`;
    const statusControl = isSelf
      ? `<div class="person-status-controls"><label>Доступность<select class="select" data-change="person-status"><option ${availability === "Доступен" ? "selected" : ""}>Доступен</option><option ${availability === "Занят" ? "selected" : ""}>Занят</option><option ${availability === "Не беспокоить" ? "selected" : ""}>Не беспокоить</option><option ${availability === "На объекте" ? "selected" : ""}>На объекте</option></select></label><label>Срок<select class="select" data-change="person-status-until"><option ${availabilityUntil === "До конца смены" ? "selected" : ""}>До конца смены</option><option ${availabilityUntil === "1 час" ? "selected" : ""}>1 час</option><option ${availabilityUntil === "4 часа" ? "selected" : ""}>4 часа</option></select></label></div>`
      : `<div class="person-status-summary"><strong>${escapeHtml(availability)}</strong><span>${escapeHtml(availabilityUntil)}</span></div>`;
    return `<section class="page user-profile-page">
      <div class="page-toolbar"><div class="toolbar-title">${isSelf ? "Моя страница" : escapeHtml(person.name)}</div><span class="toolbar-subtitle">Рабочая идентичность сотрудника</span><div class="toolbar-spacer"></div>${toolbarActions}</div>
      <div class="page-content user-profile-content"><div class="user-profile-layout">
        <aside class="user-profile-sidebar">
          <section class="panel identity-panel"><div class="panel-body"><div class="person-identity"><div class="person-avatar">${escapeHtml(person.initials)}</div><div><h1>${escapeHtml(person.name)}</h1><p>${escapeHtml(person.title)}</p></div></div><div class="person-status-row">${badge(person.presence, person.presence === "Online" ? "good" : "neutral")}${badge(availability, availability === "Доступен" ? "good" : availability === "Занят" ? "warning" : "medium")}${badge(person.workState, "neutral")}</div>${statusControl}</div></section>
          <section class="panel"><div class="panel-head"><h2 class="panel-title">Контакты и организация</h2></div><div class="panel-body profile-facts"><div><span>Подразделение</span><strong>${escapeHtml(person.department)}</strong></div><div><span>Локация</span><strong>${escapeHtml(person.location)}</strong></div><div><span>Руководитель</span><strong>${manager}</strong></div><div><span>Телефон</span><strong>${escapeHtml(person.phone)}</strong></div><div><span>Email</span><strong>${escapeHtml(person.email)}</strong></div><div><span>Предпочтительный способ</span><strong>${escapeHtml(person.preferredContact)}</strong></div></div></section>
          <section class="panel"><div class="panel-head"><h2 class="panel-title">Ответственность и квалификация</h2></div><div class="panel-body"><div class="tag-list">${person.responsibilities.map((item) => `<span class="chip">${escapeHtml(item)}</span>`).join("")}</div><div class="qualification-list">${person.qualifications.map((item) => `<div>${icon("check")}<span>${escapeHtml(item)}</span></div>`).join("")}</div></div></section>
        </aside>
        <div class="user-profile-main">
          <section class="panel wide"><div class="panel-head"><h2 class="panel-title">О сотруднике</h2>${badge("Организационный профиль", "neutral")}</div><div class="panel-body"><p class="profile-about">${escapeHtml(person.about)}</p><div class="profile-specialization"><span>Специализация</span><strong>${escapeHtml(person.specialization)}</strong></div></div></section>
          <section class="panel wide"><div class="panel-head"><h2 class="panel-title">Текущая работа</h2><span class="muted">Только доступные вам объекты</span></div><div class="profile-list">${person.tasks.map((item, index) => `<div class="profile-list-row"><div><strong>${escapeHtml(item)}</strong><span>${index === 0 ? "Требует внимания сегодня" : "Рабочий контекст"}</span></div>${badge(index === 0 ? "В работе" : "Назначено", index === 0 ? "warning" : "neutral")}</div>`).join("")}</div></section>
          <section class="panel"><div class="panel-head"><h2 class="panel-title">Закреплённое</h2></div><div class="profile-list">${person.pinned.map(([title, route, type]) => route ? `<button class="profile-list-row interactive" data-nav="${route}"><div><strong>${escapeHtml(title)}</strong><span>${escapeHtml(type)}</span></div>${icon("chevron")}</button>` : `<div class="profile-list-row"><div><strong>${escapeHtml(title)}</strong><span>${escapeHtml(type)}</span></div></div>`).join("")}</div></section>
          <section class="panel"><div class="panel-head"><h2 class="panel-title">Рабочая активность</h2></div><div class="profile-activity">${person.activity.map(([time, text]) => `<div><span>${escapeHtml(time)}</span><p>${escapeHtml(text)}</p></div>`).join("")}</div></section>
        </div>
      </div></div>
    </section>`;
  }

  function settingsNavigation() {
    const current = currentSettingsSection();
    return `<nav class="settings-nav" aria-label="Разделы настроек">${settingsSections.map((item) => `<button class="${current === item.id ? "active" : ""}" data-nav="${item.route}">${icon(item.icon)} ${item.label}</button>`).join("")}</nav><label class="settings-selector-label"><span class="sr-only">Раздел настроек</span><select class="select settings-selector" data-change="settings-route">${settingsSections.map((item) => `<option value="${item.route}" ${current === item.id ? "selected" : ""}>${item.label}</option>`).join("")}</select></label>`;
  }

  function settingsPanel(title, body, extra = "") {
    return `<section class="panel settings-panel"><div class="panel-head"><h2 class="panel-title">${title}</h2>${extra}</div><div class="panel-body">${body}</div></section>`;
  }

  function renderProfileSettings() {
    const person = people[0];
    const readonlyRow = (label, value, source) => `<div class="settings-row"><div><strong>${label}</strong><span>Управляется организацией</span></div><div class="readonly-value"><strong>${escapeHtml(value)}</strong><span>${icon("lock")} Источник: ${escapeHtml(source)}</span></div></div>`;
    return `${settingsPanel("Данные профиля", `<p class="settings-note">Организационные поля доступны только для чтения. Разрешённые сведения изменяются здесь, а не на «Моей странице».</p><div class="settings-rows">${readonlyRow("ФИО", person.name, "Корпоративный каталог")}${readonlyRow("Должность", person.title, "Кадровая система")}${readonlyRow("Подразделение", person.department, "Кадровая система")}${readonlyRow("Руководитель", person.manager, "Организационная структура")}</div>`, badge("4 внешних поля", "neutral"))}
      ${settingsPanel("Разрешённые поля", `<div class="settings-form"><label class="form-label">Кратко о себе<textarea id="profile-about" rows="3">${escapeHtml(person.about)}</textarea></label><label class="form-label">Специализация<input id="profile-specialization" class="field" value="${escapeHtml(person.specialization)}"></label><label class="form-label">Предпочтительный способ связи<select id="profile-contact" class="select"><option ${person.preferredContact === "Рабочий телефон" ? "selected" : ""}>Рабочий телефон</option><option ${person.preferredContact === "Корпоративный мессенджер" ? "selected" : ""}>Корпоративный мессенджер</option><option ${person.preferredContact === "Email" ? "selected" : ""}>Email</option></select></label></div><div class="settings-actions"><button class="button primary" data-action="save-profile-settings">Сохранить изменения</button></div>`)}`;
  }

  function renderPrivacySettings() {
    const previewPerson = people[0];
    return `${settingsPanel("Видимость страницы", `<p class="settings-note">Пользователь может только сузить видимость там, где это разрешено. Права на рабочие объекты проверяются отдельно при каждом открытии.</p><div class="settings-rows"><div class="settings-row"><div><strong>Рабочие контакты</strong><span>Телефон и email</span></div><select class="select"><option>Сотрудники организации</option><option>Только моя служба</option></select></div><div class="settings-row"><div><strong>График и отсутствие</strong><span>Смена, отпуск и замещение</span></div><select class="select"><option>По служебной необходимости</option><option>Только руководитель</option></select></div><div class="settings-row"><div><strong>Специализация</strong><span>Профессиональный контекст</span></div><select class="select"><option>Сотрудники организации</option><option>Только моя служба</option></select></div></div>`)}
      ${settingsPanel("Предварительный просмотр", `<div class="preview-toolbar"><span class="muted">Режим просмотра</span><div class="segmented"><button class="${!state.privacyPreview ? "active" : ""}" data-action="privacy-preview" data-value="self">Владелец</button><button class="${state.privacyPreview ? "active" : ""}" data-action="privacy-preview" data-value="employee">Сотрудник</button></div></div><div class="privacy-preview"><div class="person-avatar small">${previewPerson.initials}</div><div><strong>${previewPerson.name}</strong><span>${previewPerson.title} · ${previewPerson.department}</span><p>${state.privacyPreview ? "Сотрудник видит рабочие контакты, специализацию и только общие доступные объекты. Личные каналы, сеансы и права скрыты." : "Вы видите все разрешённые собственные поля и источники данных."}</p></div></div>`, badge(state.privacyPreview ? "Как сотрудник" : "Собственный вид", "medium"))}`;
  }

  function renderHomeSettings() {
    return `${settingsPanel("Рабочий стол", `<p class="settings-note">Исходную компоновку назначила организация. Можно скрывать и возвращать только разрешённые блоки; страница другого сотрудника не копируется.</p><div class="settings-rows">${homeWidgetDefinitions.map((item) => `<div class="settings-row"><div><strong>${item.title}</strong><span>${item.description}</span></div><button class="switch ${state.homeWidgets.has(item.id) ? "on" : ""}" data-action="toggle-home-widget" data-id="${item.id}" aria-label="${state.homeWidgets.has(item.id) ? "Скрыть" : "Показать"}: ${item.title}"></button></div>`).join("")}</div><div class="settings-actions"><button class="button" data-action="reset-home-widgets">${icon("reset")} Восстановить компоновку организации</button><button class="button primary" data-nav="/home">Открыть Рабочий стол</button></div>`, badge(`${state.homeWidgets.size} из ${homeWidgetDefinitions.length}`, "good"))}`;
  }

  function renderInterfaceSettings() {
    return settingsPanel("Интерфейс", `<div class="settings-rows"><div class="settings-row"><div><strong>Тема</strong><span>Основная тема рабочего места</span></div><select class="select"><option>Тёмная операторская</option><option disabled>Светлая — следующая итерация</option></select></div><div class="settings-row"><div><strong>Плотность</strong><span>Таблицы и формы</span></div><select class="select"><option>Компактная</option><option>Стандартная</option></select></div><div class="settings-row"><div><strong>Часовой пояс</strong><span>Время событий и отчётов</span></div><select class="select"><option>Europe/Moscow · UTC+3</option></select></div><div class="settings-row"><div><strong>Поведение панелей</strong><span>Контекстная панель закрывается при переходе</span></div><button class="switch ${state.closePanelOnNavigate ? "on" : ""}" data-action="toggle-panel-autoclose" aria-pressed="${state.closePanelOnNavigate}" aria-label="Закрывать панель при переходе"></button></div></div><div class="settings-actions"><button class="button primary" data-action="save-interface-settings">Сохранить</button></div>`);
  }

  function renderSecuritySettings() {
    const sessions = [{ id: "current", title: "Этот компьютер · Edge", meta: "Главный комплекс · активен сейчас", current: true }, { id: "tablet", title: "Планшет диспетчера · Chrome", meta: "Корпус A · 12 июля, 18:42" }].filter((item) => !state.endedSessions.has(item.id));
    return `${settingsPanel("Безопасность и сеансы", `<p class="settings-note">MFA и пароль управляются корпоративным identity provider. Здесь показаны только активные сеансы Dispatcher.</p><div class="profile-list">${sessions.map((item) => `<div class="profile-list-row"><div><strong>${escapeHtml(item.title)}</strong><span>${escapeHtml(item.meta)}</span></div>${item.current ? badge("Текущий", "good") : `<button class="button small danger" data-action="end-session" data-id="${item.id}">Завершить</button>`}</div>`).join("") || `<div class="panel-body muted">Других активных сеансов нет.</div>`}</div>`, badge("MFA включена", "good"))}`;
  }

  function renderFavoritesSettings() {
    const lastRoute = validLastDashboardRoute();
    return `${settingsPanel("Избранные дашборды", `<div class="profile-list">${dashboards.map((item) => `<div class="profile-list-row"><div><strong>${escapeHtml(item.name)}</strong><span>${escapeHtml(item.location)} · ${escapeHtml(item.system)}</span></div><button class="favorite-button ${state.favoriteDashboards.has(item.id) ? "active" : ""}" data-action="toggle-dashboard-favorite" data-id="${item.id}" title="Избранное">${icon("star")}</button></div>`).join("")}</div>`)}${settingsPanel("Недавнее", lastRoute ? `<div class="settings-actions start"><button class="button" data-nav="${lastRoute}">Продолжить последний дашборд</button><button class="button" data-nav="/dashboards">Открыть каталог</button></div>` : `<p class="settings-note">Недавних дашбордов пока нет.</p>`)}`;
  }

  function renderSettingsContent() {
    const section = currentSettingsSection();
    if (section === "privacy") return renderPrivacySettings();
    if (section === "home") return renderHomeSettings();
    if (section === "interface") return renderInterfaceSettings();
    if (section === "security") return renderSecuritySettings();
    if (section === "favorites") return renderFavoritesSettings();
    if (section === "notifications") return renderNotifications();
    return renderProfileSettings();
  }

  function renderSettings() {
    const current = settingsSections.find((item) => item.id === currentSettingsSection()) || settingsSections[0];
    return `<section class="page settings-page"><div class="page-toolbar"><div class="toolbar-title">Личные настройки</div><span class="toolbar-subtitle">${escapeHtml(current.label)}</span><div class="toolbar-spacer"></div>${current.id === "notifications" ? badge("Критические доставляются", "good") : badge("Текущий пользователь", "neutral")}</div><div class="page-content settings-page-content"><div class="settings-layout">${settingsNavigation()}<div class="settings-content">${renderSettingsContent()}</div></div></div></section>`;
  }

  function adminNavigation() {
    const section = currentAdminSection();
    return `<nav class="settings-nav" aria-label="Разделы системных настроек">${adminSections.map((item) => `<button class="${section === item.id ? "active" : ""}" data-nav="${item.route}">${icon(item.icon)} ${escapeHtml(item.label)}</button>`).join("")}</nav>`;
  }

  function renderAdminShell(content) {
    const current = adminSections.find((item) => item.id === currentAdminSection()) || adminSections[0];
    return `<section class="page settings-page system-settings-page"><div class="page-toolbar"><div class="toolbar-title">Администрирование</div><span class="toolbar-subtitle">${escapeHtml(current.label)}</span><div class="toolbar-spacer"></div>${badge("Администратор", "warning")}</div><div class="page-content settings-page-content"><div class="settings-layout">${adminNavigation()}<div class="settings-content">${content}</div></div></div></section>`;
  }

  function renderAdminSettings() {
    const effectiveControlTimeout = state.adminSettingOverride || "3 минуты";
    const source = state.adminSettingOverride ? "Локальное переопределение организации" : "Базовая политика платформы";
    return `${settingsPanel("Организация", `<p class="settings-note">Эти параметры действуют для всей организации и не относятся к личному профилю Алексея Смирнова.</p><div class="settings-rows"><div class="settings-row"><div><strong>Название</strong><span>Отображается в служебных разделах</span></div><input class="field" value="Главный комплекс"></div><div class="settings-row"><div><strong>Основной часовой пояс</strong><span>Системное время журналов и расписаний</span></div><select class="select"><option>Europe/Moscow · UTC+3</option></select></div><div class="settings-row"><div><strong>Язык по умолчанию</strong><span>Для новых пользователей</span></div><select class="select"><option>Русский</option></select></div></div><div class="settings-actions"><button class="button primary" data-action="save-system-settings">Сохранить системные параметры</button></div>`, badge("Одна организация", "medium"))}${settingsPanel("Наследуемая политика", `<div class="settings-rows"><div class="settings-row"><div><strong>Время режима управления</strong><span>Источник: ${escapeHtml(source)}</span></div><div class="readonly-value"><strong>${escapeHtml(effectiveControlTimeout)}</strong><span>${state.adminSettingOverride ? "Override действует" : "Наследуется"}</span></div></div></div><div class="settings-actions"><button class="button" data-action="admin-setting-preview">Preview изменения</button>${state.adminSettingOverride ? `<button class="button" data-action="admin-setting-reset">Вернуть наследование</button>` : ""}</div><p class="settings-note">Перед применением показываются затронутые рабочие места и исключения.</p>`, badge(state.adminSettingOverride ? "Override" : "Inherited", state.adminSettingOverride ? "warning" : "neutral"))}${settingsPanel("Границы", `<div class="settings-rows"><div class="settings-row"><div><strong>Личные настройки</strong><span>Профиль, интерфейс и оповещения</span></div><strong>Не изменяются здесь</strong></div><div class="settings-row"><div><strong>Kiosk и терминалы</strong><span>Device identity, политика доступа и назначение дашбордов</span></div><button class="button small" data-nav="/admin/terminals">Открыть реестр</button></div></div>`)}`;
  }

  function renderAdminUsers() {
    const usersView = state.adminIdentityView === "users";
    const rows = usersView
      ? adminAccounts.map((item) => `<tr class="${state.drawer?.kind === "admin-account" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-admin-account" data-id="${item.id}" tabindex="0"><td class="primary-cell">${escapeHtml(item.name)}<div class="row-meta">${escapeHtml(item.login)} · ${escapeHtml(item.identitySource)}</div></td><td>${badge(item.status, item.status === "Активна" ? "good" : "warning")}</td><td>${escapeHtml(item.role)}</td><td>${escapeHtml(item.scope)}</td><td>${escapeHtml(item.temporary)}</td></tr>`).join("")
      : adminRoles.map((item) => `<tr class="${state.drawer?.kind === "admin-role" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-admin-role" data-id="${item.id}" tabindex="0"><td class="primary-cell">${escapeHtml(item.name)}<div class="row-meta">${escapeHtml(item.source)}</div></td><td>${item.users}</td><td>${escapeHtml(item.scope)}</td><td>${escapeHtml(item.services)}</td><td>${escapeHtml(item.actions)}</td></tr>`).join("");
    return `${settingsPanel("Учётные записи и effective permissions", `<div class="preview-toolbar"><div class="segmented"><button class="${usersView ? "active" : ""}" data-action="admin-identity-view" data-value="users">Пользователи</button><button class="${usersView ? "" : "active"}" data-action="admin-identity-view" data-value="roles">Роли</button></div><span class="muted">Должность сотрудника не является ролью доступа</span></div><div class="table-wrap"><table class="working-table"><thead><tr>${usersView ? "<th>Учётная запись</th><th>Состояние</th><th>Роль</th><th>Область</th><th>Временный доступ</th>" : "<th>Роль</th><th>Пользователи</th><th>Допустимая область</th><th>Сервисы</th><th>Действия</th>"}</tr></thead><tbody>${rows}</tbody></table></div>`, badge(usersView ? `${adminAccounts.length} учётные записи` : `${adminRoles.length} роли`, "neutral"))}<p class="settings-note">Выберите строку: административная карточка откроется справа и не заменит страницу сотрудника.</p>`;
  }

  function renderAdminIntegrations() {
    const rows = adminIntegrations.map((item) => `<tr class="${state.drawer?.kind === "admin-integration" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-admin-integration" data-id="${item.id}" tabindex="0"><td class="primary-cell">${escapeHtml(item.name)}<div class="row-meta">${escapeHtml(item.type)}</div></td><td>${escapeHtml(item.purpose)}</td><td>${badge(item.status, item.status === "Норма" ? "good" : item.status === "Деградация" ? "warning" : "neutral")}</td><td>${escapeHtml(item.lastSuccess)}</td><td>${escapeHtml(item.owner)}</td></tr>`).join("");
    return `${settingsPanel("Подключения и источники", `<p class="settings-note">Показана только эксплуатационная диагностика. Endpoint маскируется, секреты и реальные сетевые вызовы отсутствуют.</p><div class="table-wrap"><table class="working-table"><thead><tr><th>Подключение</th><th>Назначение</th><th>Состояние</th><th>Последний успех</th><th>Владелец</th></tr></thead><tbody>${rows}</tbody></table></div>`, badge("Representative", "neutral"))}`;
  }

  function kioskDashboardName(route) {
    const dashboard = dashboards.find((item) => item.id === dashboardRouteParts(route)?.id);
    return dashboard?.name || "Дашборд недоступен";
  }

  function renderAdminTerminalEnroll() {
    if (terminalEnrollment.status === "Активен") {
      const terminal = kioskTerminal(terminalEnrollment.approvedTerminalId);
      return `${settingsPanel("Терминал зарегистрирован", `<div class="policy-card"><strong>Одноразовый код погашен</strong><div class="policy-meta">Создана отдельная device identity. Пользовательская учётная запись для панели не создавалась.</div></div><div class="preflight"><div class="preflight-row"><span>Терминал</span><strong>${escapeHtml(terminal?.name || "—")}</strong></div><div class="preflight-row"><span>Device identity</span><strong>${escapeHtml(terminal?.deviceIdentity || "—")}</strong></div><div class="preflight-row"><span>Профиль</span><strong>${escapeHtml(kioskProfile(terminal?.profileId)?.name || "—")}</strong></div></div><div class="settings-actions"><button class="button" data-nav="/admin/terminals">К реестру</button>${terminal ? `<button class="button primary" data-nav="${escapeHtml(kioskRuntimeRoute(terminal))}">Открыть kiosk runtime</button>` : ""}</div>`, badge("Активен", "good"))}`;
    }
    return `${settingsPanel("Подтверждение регистрации", `<p class="settings-note">Символический pairing без сертификатов и backend. Код используется один раз.</p><div class="settings-form"><label class="form-label">Одноразовый код<input id="terminal-enrollment-code" class="field" value="${escapeHtml(terminalEnrollment.code)}"></label><label class="form-label">Имя панели<input id="terminal-enrollment-name" class="field" value="Панель переговорной 3.02"></label><label class="form-label">Локация<input id="terminal-enrollment-location" class="field" value="Корпус A · 3 этаж"></label><label class="form-label">Общий профиль<select id="terminal-enrollment-profile" class="select">${kioskProfiles.map((profile) => `<option value="${profile.id}">${escapeHtml(profile.name)} · ${escapeHtml(profile.authLabel)}</option>`).join("")}</select></label><label class="form-label">Стартовый дашборд<select id="terminal-enrollment-dashboard" class="select">${dashboards.map((dashboard) => `<option value="/d/${dashboard.id}/overview">${escapeHtml(dashboard.name)}</option>`).join("")}</select></label></div><div class="preflight"><div class="preflight-row"><span>Устройство</span><strong>${escapeHtml(terminalEnrollment.deviceInfo)}</strong></div><div class="preflight-row"><span>Кандидат identity</span><strong>${escapeHtml(terminalEnrollment.deviceIdentity)}</strong></div><div class="preflight-row"><span>Разрешение</span><strong>${escapeHtml(terminalEnrollment.resolution)}</strong></div><div class="preflight-row"><span>Запрос</span><strong>${escapeHtml(terminalEnrollment.requestedAt)}</strong></div></div><div class="settings-actions"><button class="button" data-nav="/admin/terminals">Отмена</button><button class="button primary" data-action="admin-terminal-enroll-confirm">Подтвердить и назначить</button></div>`, badge("Ожидает подтверждения", "warning"))}`;
  }

  function renderAdminTerminals() {
    if (state.route === "/admin/terminals/enroll") return renderAdminTerminalEnroll();
    const rows = kioskTerminals.map((terminal) => {
      const profile = kioskProfile(terminal.profileId);
      const statusType = terminal.status === "Активен" ? "good" : terminal.status === "Отозван" ? "critical" : "warning";
      return `<tr class="${state.drawer?.kind === "admin-terminal" && state.drawer.id === terminal.id ? "selected" : ""}" data-action="open-admin-terminal" data-id="${terminal.id}" tabindex="0"><td class="primary-cell">${escapeHtml(terminal.name)}<div class="row-meta">${escapeHtml(terminal.id)} · ${escapeHtml(terminal.deviceIdentity)}</div></td><td>${escapeHtml(terminal.location)}</td><td>${escapeHtml(profile?.name || "—")}</td><td>${escapeHtml(kioskDashboardName(terminal.dashboardRoute))}</td><td>${badge(terminal.status, statusType)}</td><td>${terminal.online ? badge("Online", "good") : badge("Offline", "critical")}</td><td>${escapeHtml(terminal.lastSeen)}</td><td>${escapeHtml(terminal.currentScreen)}</td><td>${escapeHtml(terminal.configVersion)}</td><td>${escapeHtml(terminal.resolution)}</td><td>${escapeHtml(terminal.sync)}</td></tr>`;
    }).join("");
    return `${settingsPanel("Реестр kiosk-панелей", `<div class="preview-toolbar"><span class="muted">Общий профиль конфигурации · отдельная device identity каждой панели</span><button class="button primary small" data-nav="/admin/terminals/enroll">Зарегистрировать терминал</button></div><div class="table-wrap"><table class="working-table"><thead><tr><th>Терминал</th><th>Локация</th><th>Профиль</th><th>Дашборд</th><th>Identity</th><th>Связь</th><th>Последнее появление</th><th>Экран</th><th>Конфигурация</th><th>Разрешение</th><th>Синхронизация</th></tr></thead><tbody>${rows}</tbody></table></div>`, badge(`${kioskTerminals.length} панели`, "neutral"))}<p class="settings-note">Выберите строку для просмотра effective profile, запуска ограниченного runtime, блокировки или отзыва identity.</p>`;
  }

  function healthIssuesForFilter() {
    if (state.adminHealthFilter === "operations") return platformHealthIssues.filter((item) => item.category === "configuration" || item.category === "jobs");
    return state.adminHealthFilter === "all" ? platformHealthIssues : platformHealthIssues.filter((item) => item.category === state.adminHealthFilter);
  }

  function renderAdminHealth() {
    const counters = [["all", "Все проблемы"], ["connections", "Подключения"], ["devices", "Устройства offline"], ["operations", "Конфигурация и jobs"]];
    const rows = healthIssuesForFilter().map((item) => `<tr class="${state.drawer?.kind === "admin-health" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-admin-health" data-id="${item.id}" tabindex="0"><td>${badge(item.status, item.type)}</td><td class="primary-cell">${escapeHtml(item.title)}</td><td>${escapeHtml(item.scope)}</td><td>${escapeHtml(item.impact)}</td><td>${escapeHtml(item.lastSuccess)}</td></tr>`).join("");
    return `<div class="metric-grid compact-grid">${counters.map(([id, label]) => { const count = id === "all" ? platformHealthIssues.length : id === "operations" ? platformHealthIssues.filter((item) => item.category === "configuration" || item.category === "jobs").length : platformHealthIssues.filter((item) => item.category === id).length; return `<button class="metric ${state.adminHealthFilter === id ? "active" : ""}" data-action="admin-health-filter" data-value="${id}" aria-pressed="${state.adminHealthFilter === id}"><div class="metric-label"><span>${label}</span></div><div class="metric-value">${count}</div><div class="metric-note">Фильтр единой очереди</div></button>`; }).join("")}</div>${settingsPanel("Проблемы платформы", `<p class="settings-note">Это системная диагностика, а не аварии здания и не acknowledgement в Диспетчере событий.</p><div class="table-wrap"><table class="working-table"><thead><tr><th>Состояние</th><th>Проблема</th><th>Область</th><th>Влияние</th><th>Последний успех</th></tr></thead><tbody>${rows || `<tr><td colspan="5" class="muted">Нет проблем в выбранной категории.</td></tr>`}</tbody></table></div>`)}`;
  }

  function renderAdminDataQuality() {
    const rows = dataQualityIssues.map((item) => `<tr class="${state.drawer?.kind === "admin-data-quality" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-admin-data-quality" data-id="${item.id}" tabindex="0"><td>${badge(item.category, "neutral")}</td><td class="primary-cell">${escapeHtml(item.title)}</td><td>${escapeHtml(item.object)}</td><td>${escapeHtml(item.assignedTo)}</td><td>${escapeHtml(item.status)}</td></tr>`).join("");
    return `${settingsPanel("Центр качества данных", `<p class="settings-note">Quality, realtime, history и аварийные occurrence остаются разными состояниями.</p><div class="table-wrap"><table class="working-table"><thead><tr><th>Категория</th><th>Проблема</th><th>Объект</th><th>Ответственный</th><th>Состояние</th></tr></thead><tbody>${rows}</tbody></table></div>`, badge(`${dataQualityIssues.length} проблемы`, "warning"))}`;
  }

  function filteredAdminAudit() {
    if (state.adminAuditFilter === "success") return adminAuditEntries.filter((item) => item.result === "Успех");
    if (state.adminAuditFilter === "problem") return adminAuditEntries.filter((item) => item.result !== "Успех");
    return adminAuditEntries;
  }

  function renderAdminAudit() {
    const rows = filteredAdminAudit().map((item) => `<tr class="${state.drawer?.kind === "admin-audit" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-admin-audit" data-id="${item.id}" tabindex="0"><td>${escapeHtml(item.time)}</td><td>${escapeHtml(item.actor)}</td><td class="primary-cell">${escapeHtml(item.action)}</td><td>${escapeHtml(item.object)}</td><td>${badge(item.result, item.result === "Успех" ? "good" : item.result === "Отказ" ? "critical" : "warning")}</td></tr>`).join("");
    return `${settingsPanel("Технический аудит", `<div class="preview-toolbar"><label class="form-label">Результат<select class="select" data-change="admin-audit-filter"><option value="all" ${state.adminAuditFilter === "all" ? "selected" : ""}>Все</option><option value="success" ${state.adminAuditFilter === "success" ? "selected" : ""}>Успех</option><option value="problem" ${state.adminAuditFilter === "problem" ? "selected" : ""}>Предупреждение / отказ</option></select></label><span class="muted">Только чтение · без редактирования и удаления</span></div><div class="table-wrap"><table class="working-table"><thead><tr><th>Время</th><th>Исполнитель</th><th>Действие</th><th>Объект</th><th>Результат</th></tr></thead><tbody>${rows}</tbody></table></div>`, badge(`${filteredAdminAudit().length} записи`, "neutral"))}`;
  }

  function renderAdmin() {
    const section = currentAdminSection();
    if (section === "users") return renderAdminShell(renderAdminUsers());
    if (section === "integrations") return renderAdminShell(renderAdminIntegrations());
    if (section === "terminals") return renderAdminShell(renderAdminTerminals());
    if (section === "health") return renderAdminShell(renderAdminHealth());
    if (section === "data-quality") return renderAdminShell(renderAdminDataQuality());
    if (section === "audit") return renderAdminShell(renderAdminAudit());
    return renderAdminShell(renderAdminSettings());
  }

  function renderTagHistory(history) {
    return `<section class="page tag-history-page"><div class="page-toolbar"><div class="toolbar-title">История тега</div><span class="toolbar-subtitle">${escapeHtml(history.tag)}</span><div class="toolbar-spacer"></div><button class="button small" data-nav="${history.dashboardRoute}">${icon("dashboard")} Открыть дашборд</button></div><div class="page-content tag-history-content"><div class="metric-grid"><article class="metric"><div class="metric-label"><span>Текущее значение</span>${icon("health")}</div><div class="metric-value">${escapeHtml(history.current)} <span class="value-unit">${escapeHtml(history.unit)}</span></div><div class="metric-note">Обновлено ${escapeHtml(history.updated)}</div></article><article class="metric"><div class="metric-label"><span>Минимум · 24 ч</span></div><div class="metric-value">${escapeHtml(history.min)} <span class="value-unit">${escapeHtml(history.unit)}</span></div></article><article class="metric"><div class="metric-label"><span>Среднее · 24 ч</span></div><div class="metric-value">${escapeHtml(history.average)} <span class="value-unit">${escapeHtml(history.unit)}</span></div></article><article class="metric"><div class="metric-label"><span>Максимум · 24 ч</span></div><div class="metric-value">${escapeHtml(history.max)} <span class="value-unit">${escapeHtml(history.unit)}</span></div></article></div><section class="panel tag-history-chart"><div class="panel-head"><h2 class="panel-title">${escapeHtml(history.title)}</h2>${badge("Демонстрационные данные", "neutral")}</div><div class="panel-body">${sparkline(history.points)}<div class="tag-history-axis"><span>24 часа назад</span><span>Сейчас</span></div><p class="settings-note">Переход открыт из личного оповещения. Общий Диспетчер событий остаётся отдельным журналом событий и аварий.</p></div></section></div></section>`;
  }

  function renderNotificationsInbox() {
    const unread = unreadPersonalNotificationCount();
    return `<section class="page notification-inbox-page"><div class="page-toolbar"><div class="toolbar-title">Личные оповещения</div><span class="toolbar-subtitle">${unread ? `Непрочитанных: ${unread}` : "Все прочитано"}</span><div class="toolbar-spacer"></div>${unread ? `<button class="button small" data-action="mark-all-personal-notifications">Прочитать все</button>` : ""}<button class="button small" data-nav="/settings/notifications/effective">Настроить доставку</button></div><div class="page-content"><section class="panel"><div class="panel-head"><h2 class="panel-title">Адресовано вам</h2>${badge(`${personalNotifications.length} сообщения`, "neutral")}</div><div class="personal-notification-list notification-inbox-list">${personalNotifications.map(renderPersonalNotification).join("")}</div><div class="panel-body"><p class="settings-note">Прочтение относится только к личному inbox и не подтверждает событие, аварию или принятие задачи.</p></div></section></div></section>`;
  }

  function renderNotFound(message = "Запрошенная страница не существует или недоступна.") {
    return `<section class="page"><div class="page-toolbar"><div class="toolbar-title">Страница не найдена</div></div><div class="page-content"><div class="empty-state panel"><div><strong>Не удалось открыть страницу</strong><p>${escapeHtml(message)}</p><button class="button primary" data-nav="/home">На Главную</button></div></div></div></section>`;
  }

  function renderAccessDenied() {
    return `<section class="page"><div class="page-toolbar"><div class="toolbar-title">Нет доступа</div></div><div class="page-content"><div class="empty-state panel"><div><strong>Недостаточно прав</strong><p>Административный раздел доступен только уполномоченным пользователям.</p><button class="button primary" data-nav="/home">На Главную</button></div></div></div></section>`;
  }

  function currentNotificationTab() {
    const tab = state.route.split("/").pop();
    return ["effective", "subscriptions", "schedule", "channels"].includes(tab) ? tab : "effective";
  }

  function notificationNav() {
    const tabs = [["effective", "Что я получаю", "eye"], ["subscriptions", "Мои подписки", "bell"], ["schedule", "Расписание и отсутствие", "clock"], ["channels", "Каналы", "link"]];
    return `<nav class="subsettings-nav" aria-label="Разделы оповещений">${tabs.map(([id, label, iconName]) => `<button class="${currentNotificationTab() === id ? "active" : ""}" data-nav="/settings/notifications/${id}">${icon(iconName)} ${label}</button>`).join("")}</nav>`;
  }

  function renderNotifications() {
    return `${notificationNav()}${renderNotificationTab()}`;
  }

  function renderNotificationTab() {
    const tab = currentNotificationTab();
    if (tab === "subscriptions") return `<div class="page-intro"><div><h1>Мои подписки</h1><p>Дополняют обязательный набор и не могут его уменьшить.</p></div><button class="button primary" data-action="new-subscription">${icon("plus")} Новая подписка</button></div>${subscriptions.map((item) => `<div class="subscription-row"><div><div class="policy-title">${escapeHtml(item.title)}</div><div class="policy-meta">${escapeHtml(item.meta)}</div></div><button class="switch ${state.subscriptions[item.id] ? "on" : ""}" data-action="toggle-subscription" data-id="${item.id}" aria-label="Переключить подписку"></button></div>`).join("")}`;
    if (tab === "schedule") return renderNotificationSchedule();
    if (tab === "channels") return renderNotificationChannels();
    return renderEffectivePolicy();
  }

  function renderEffectivePolicy() {
    const scenarioText = state.notificationScenario === "night" ? "Личные HVAC-уведомления приостановлены quiet hours; critical остаются активными." : state.notificationScenario === "vacation" ? "Личные подписки приостановлены; обязательный получатель — Сергей Петров." : "Рабочее время: личные и обязательные правила активны.";
    return `<div class="page-intro"><div><h1>Что я получаю</h1><p>${scenarioText}</p></div></div>
      <div class="scenario-strip"><strong>Сценарий:</strong><div class="segmented"><button class="${state.notificationScenario === "now" ? "active" : ""}" data-action="notification-scenario" data-value="now">Сейчас</button><button class="${state.notificationScenario === "night" ? "active" : ""}" data-action="notification-scenario" data-value="night">Ночь</button><button class="${state.notificationScenario === "vacation" ? "active" : ""}" data-action="notification-scenario" data-value="vacation">Отпуск</button></div><span class="muted">Europe/Moscow</span></div>
      <article class="policy-card"><div class="policy-head"><div><div class="policy-title">Пожарные события · critical</div><div class="policy-meta">Обязательная политика · роль «Дежурный диспетчер» · два совпавших правила</div></div>${badge("Обязательно", "critical")}${icon("lock")}</div><div class="route-steps"><div class="route-step"><strong>Сразу</strong><div class="policy-meta">Push · Алексей Смирнов</div></div><span class="route-arrow">→</span><div class="route-step"><strong>Через 2 мин</strong><div class="policy-meta">SMS · если нет ack</div></div><span class="route-arrow">→</span><div class="route-step"><strong>Через 5 мин</strong><div class="policy-meta">Дежурная группа</div></div></div></article>
      <article class="policy-card"><div class="policy-head"><div><div class="policy-title">Климат · medium+</div><div class="policy-meta">Личная подписка · Email · только рабочее время</div></div>${state.notificationScenario === "now" ? badge("Активна", "good") : badge("Приостановлена", "warning")}</div></article>
      <article class="policy-card"><div class="policy-head"><div><div class="policy-title">Объяснение effective policy</div><div class="policy-meta">MP-FIRE + MP-BUILDING → один Push без дубля + SMS fallback. Личная digest-настройка не задерживает critical-доставку.</div></div></div></article>`;
  }

  function renderNotificationSchedule() {
    const status = state.vacationState === "draft" ? "Черновик" : state.vacationState === "pending" ? "Ожидает принятия" : "Активен";
    const scheduleLabel = state.notificationSchedulePreset === "always" ? "Всегда" : "Рабочее время";
    const nextSwitch = state.notificationSchedulePreset === "always" ? "Переключений нет" : "Следующее переключение сегодня в 18:00";
    return `<div class="page-intro"><div><h1>Расписание и отсутствие</h1><p>Итоговый получатель всегда рассчитывается в Europe/Moscow.</p></div></div><div class="scenario-strip"><strong>Личный график:</strong><div class="segmented"><button class="${state.notificationSchedulePreset === "always" ? "active" : ""}" data-action="notification-schedule-preset" data-value="always">Всегда</button><button class="${state.notificationSchedulePreset === "work" ? "active" : ""}" data-action="notification-schedule-preset" data-value="work">Рабочее время</button><button disabled>Пользовательский · позже</button></div><span class="muted">Europe/Moscow</span></div><div class="schedule-grid"><article class="schedule-card"><strong>${scheduleLabel}</strong><div>${state.notificationSchedulePreset === "always" ? "24 × 7" : "Пн–Пт · 08:00–18:00"}</div><div class="policy-meta">${nextSwitch}</div></article><article class="schedule-card"><strong>Quiet hours</strong><div>22:00–07:00</div><div class="policy-meta">Не влияют на mandatory critical</div></article><article class="schedule-card"><strong>Замещение</strong><div>Сергей Петров</div><div class="policy-meta">Группа: дежурные инженеры</div></article></div>
      <article class="policy-card"><div class="policy-head"><div><div class="policy-title">Отпуск · 20–31 июля 2026</div><div class="policy-meta">Дополнительные подписки будут приостановлены. Обязательные обязанности перейдут Сергею Петрову только после принятия.</div>${state.vacationState === "active" ? `<div class="policy-meta">Итог: личные подписки приостановлены · обязанности переданы · разрыва покрытия нет.</div>` : ""}</div>${badge(status, state.vacationState === "active" ? "good" : "warning")}</div><div class="drawer-actions">${state.vacationState === "draft" ? `<button class="button primary" data-action="request-vacation">Отправить заместителю</button>` : state.vacationState === "pending" ? `<button class="button primary" data-action="accept-vacation">Имитировать принятие</button>` : `<button class="button" disabled>${icon("check")} Покрытие подтверждено</button>`}</div></article>`;
  }

  function renderNotificationChannels() {
    const channels = [
      ["push", "Push в Dispatcher", "Подтверждён · обязательный основной", true],
      ["sms", "SMS · +7 *** ***-12-40", "Подтверждён · обязательный резерв", true],
      ["email", "Email · a.smirnov@company.ru", "Подтверждён · дополнительный", false],
      ["messenger", "Корпоративный мессенджер", "Не подтверждён", false],
    ];
    return `<div class="page-intro"><div><h1>Каналы</h1><p>Канал настраивается отдельно от подписок.</p></div></div>${channels.map(([id, title, meta, mandatory]) => `<div class="channel-row"><div><div class="policy-title">${title}</div><div class="policy-meta">${meta}${state.channelTests[id] ? ` · ${state.channelTests[id]}` : " · последний тест 14 июля"}</div></div>${mandatory ? `${badge("Обязательный", "critical")}${icon("lock")}` : ""}<button class="button small" data-action="test-channel" data-id="${id}">Отправить тест</button>${mandatory ? "" : `<button class="switch ${state.notificationChannels[id] ? "on" : ""}" data-action="toggle-channel" data-id="${id}" aria-label="Переключить канал"></button>`}</div>`).join("")}`;
  }

  function isTaskActionable(item) {
    return Boolean(item?.assignedToMe) && item.status !== "Выполнено" && item.status !== "Возвращено" && !item.status.startsWith("Передано:");
  }

  function tasksForWorkCounter(id) {
    const active = tasks.filter(isTaskActionable);
    if (id === "overdue") return active.filter((item) => item.overdue);
    if (id === "today") return active.filter((item) => item.today);
    if (id === "decision") return active.filter((item) => item.requiresDecision);
    if (id === "assigned") return active;
    return tasks;
  }

  function filteredTasks() {
    const byCounter = tasksForWorkCounter(state.workFilter);
    return state.workType === "all" ? byCounter : byCounter.filter((item) => item.type === state.workType);
  }

  function renderTaskSourceActions(item) {
    const actions = [];
    if (item.workOrderId && maintenanceWorkOrders.some((entry) => entry.id === item.workOrderId)) actions.push(`<button class="button" data-action="open-maintenance-work-order" data-id="${item.workOrderId}">Наряд ${item.workOrderId}</button>`);
    if (item.requestId && maintenanceRequests.some((entry) => entry.id === item.requestId)) actions.push(`<button class="button" data-action="open-maintenance-request" data-id="${item.requestId}">Заявка ${item.requestId}</button>`);
    if (item.eventId && events.some((event) => event.id === item.eventId)) actions.push(`<button class="button" data-action="open-task-event-source" data-event-id="${item.eventId}">Событие ${item.eventId}</button>`);
    if (item.incidentId && incidents.some((incident) => incident.id === item.incidentId)) actions.push(`<button class="button" data-action="open-incident" data-id="${item.incidentId}">Инцидент ${item.incidentId}</button>`);
    if (item.equipmentId && equipment.some((entry) => entry.id === item.equipmentId)) actions.push(`<button class="button" data-action="open-equipment" data-id="${item.equipmentId}">Оборудование</button>`);
    if (item.sourceRoute && !item.workOrderId && !item.requestId) actions.push(`<button class="button" data-nav="${item.sourceRoute}">Открыть источник</button>`);
    return actions.join("");
  }

  function renderMyWork() {
    const counters = [["overdue", "Просрочено", "critical"], ["today", "Сегодня", "warning"], ["decision", "Требует решения", "medium"], ["assigned", "Назначено мне", "good"]];
    const taskTypes = [...new Set(tasks.map((item) => item.type))];
    const rows = filteredTasks();
    return `<section class="page"><div class="page-toolbar"><div class="toolbar-title">Моя работа</div><select class="select" data-change="work-type" aria-label="Тип задачи"><option value="all">Все типы</option>${taskTypes.map((type) => `<option value="${escapeHtml(type)}" ${state.workType === type ? "selected" : ""}>${escapeHtml(type)}</option>`).join("")}</select><div class="toolbar-spacer"></div><span class="muted">Единая очередь · по сроку</span></div><div class="page-content"><div class="metric-grid">${counters.map(([id, title, type]) => `<button class="metric ${type === "critical" ? "alert" : type === "warning" ? "warn" : ""} ${state.workFilter === id ? "active" : ""}" data-action="work-filter" data-value="${id}" aria-pressed="${state.workFilter === id}"><div class="metric-label"><span>${title}</span></div><div class="metric-value">${tasksForWorkCounter(id).length}</div><div class="metric-note">Показать в очереди</div></button>`).join("")}</div><section class="panel"><div class="panel-head"><h2 class="panel-title">Единая очередь · ${rows.length}</h2><button class="button small ghost" data-action="work-filter" data-value="all">Сбросить счётчик</button></div><div class="table-wrap"><table class="working-table"><thead><tr><th class="state-cell">Тип</th><th>Требуемое действие</th><th class="location-cell">Объект</th><th class="location-cell">Локация</th><th class="state-cell">Приоритет</th><th class="state-cell">Срок</th><th class="state-cell">Состояние</th><th class="state-cell">Назначил</th></tr></thead><tbody>${rows.length ? rows.map((item) => `<tr class="${state.drawer?.kind === "task" && state.drawer.id === item.id ? "selected" : ""}" data-action="open-task" data-id="${item.id}" tabindex="0"><td>${escapeHtml(item.type)}</td><td class="primary-cell">${escapeHtml(item.action)}</td><td>${escapeHtml(item.object)}</td><td>${escapeHtml(item.location)}</td><td>${badge(item.priority, item.priority === "Критический" ? "critical" : item.priority === "Высокий" ? "high" : "neutral")}</td><td>${escapeHtml(item.due)}</td><td>${escapeHtml(item.status)}</td><td>${escapeHtml(item.from)}</td></tr>`).join("") : `<tr><td colspan="8" class="muted">Нет задач по выбранным условиям.</td></tr>`}</tbody></table></div></section></div></section>`;
  }

  function editorSnapshotEquals(left, right) {
    return left.elementName === right.elementName && left.source === right.source;
  }

  function markEditorDirty(field, value) {
    const document = editorDocumentFromRoute();
    if (!document || !(field in document.working) || document.working[field] === value) return;
    document.working[field] = value;
    document.workingSerial += 1;
    document.dirty = !editorSnapshotEquals(document.working, document.savedDraft);
  }

  function refreshEditorStateIndicators() {
    const editorDocument = editorDocumentFromRoute();
    if (!editorDocument) return;
    const draftState = editorDocument.dirty ? badge("Несохранено", "critical") : badge(`Черновик d${editorDocument.draftSerial}`, "warning");
    const validationState = editorHasCurrentValidation(editorDocument) ? badge("Проверено", "good") : badge("Не проверено", "neutral");
    const draftNode = document.querySelector("[data-editor-draft-state]");
    const validationNode = document.querySelector("[data-editor-validation-state]");
    if (draftNode) draftNode.innerHTML = draftState;
    if (validationNode) validationNode.innerHTML = validationState;
    document.querySelectorAll("[data-editor-preview-name]").forEach((node) => { node.textContent = editorDocument.working.elementName; });
  }

  function saveEditorDraft(document = editorDocumentFromRoute(), notify = true) {
    if (!document) return false;
    if (!document.dirty) {
      if (notify) toast("Черновик уже сохранён", `Draft d${document.draftSerial} не изменился.`);
      return true;
    }
    document.savedDraft = cloneEditorSnapshot(document.working);
    document.savedSerial = document.workingSerial;
    document.draftSerial += 1;
    document.dirty = false;
    if (notify) {
      render();
      toast("Черновик сохранён", `Draft d${document.draftSerial}; runtime v${document.publishedVersion} не изменён.`);
    }
    return true;
  }

  function showEditorLeaveWarning(route, preserveDrawer = false) {
    const document = editorDocumentFromRoute();
    if (!document) return;
    state.pendingEditorNavigation = { route, preserveDrawer };
    openModal("Есть несохранённые изменения", `<p>Изменения ещё не попали в черновик <strong>d${document.draftSerial}</strong>.</p><p class="muted">Опубликованная версия v${document.publishedVersion} и операторский экран не менялись.</p>`, `<button class="button" data-action="editor-leave-stay">Остаться</button><button class="button danger" data-action="editor-leave-discard">Выйти без сохранения</button><button class="button primary" data-action="editor-leave-save">Сохранить черновик и выйти</button>`);
  }

  function continueEditorNavigation(mode) {
    const pending = state.pendingEditorNavigation;
    const document = editorDocumentFromRoute();
    if (!pending || !document) { closeModal(); return; }
    if (mode === "save") saveEditorDraft(document, false);
    else if (mode === "discard") {
      document.working = cloneEditorSnapshot(document.savedDraft);
      document.workingSerial = document.savedSerial;
      document.dirty = false;
    }
    state.pendingEditorNavigation = null;
    closeModal();
    navigate(pending.route, pending.preserveDrawer, true);
  }

  function validateEditorDraft() {
    const document = editorDocumentFromRoute();
    if (!document) return;
    if (document.dirty) {
      toast("Проверка не начата", "Сначала сохраните локальные изменения в черновик.");
      return;
    }
    const nameMissing = !document.savedDraft.elementName.trim();
    document.validatedSerial = nameMissing ? -1 : document.savedSerial;
    render();
    openModal(nameMissing ? "Проверка: публикация заблокирована" : "Проверка черновика завершена", `<div class="preflight"><div class="preflight-row"><span>Черновик</span><strong>d${document.draftSerial}</strong></div><div class="preflight-row"><span>Ошибки</span><strong>${nameMissing ? 1 : 0}</strong></div><div class="preflight-row"><span>Предупреждения</span><strong>2 · не блокируют</strong></div></div>${nameMissing ? `<div class="policy-card"><strong>Название выбранного элемента не задано</strong><div class="policy-meta">Исправьте поле в inspector и повторите проверку.</div><div class="drawer-actions"><button class="button small" data-action="editor-focus-name">Перейти к полю</button></div></div>` : `<div class="policy-card"><strong>Проверить состояния качества</strong><div class="policy-meta">В preview показаны Normal, Alarm и Offline/Bad.</div></div><div class="policy-card"><strong>Touch-профиль не проверялся</strong><div class="policy-meta">Текущий прототип квалифицируется только для 1920×1080.</div></div>`}`, `<button class="button primary" data-action="close-modal">Закрыть</button>`);
  }

  function showEditorPreview() {
    const document = editorDocumentFromRoute();
    if (!document) return;
    openModal(`Preview · ${escapeHtml(document.name)}`, `<div class="preflight"><div class="preflight-row"><span>Сценарий</span><strong>Live normal</strong></div><div class="preflight-row"><span>Дополнительно</span><strong>Alarm · Offline/Bad · Нет права</strong></div><div class="preflight-row"><span>Версия данных</span><strong>${document.dirty ? "Локальные изменения" : `Черновик d${document.draftSerial}`}</strong></div></div><div class="policy-card"><strong>Preview безопасен</strong><div class="policy-meta">Физические команды и изменение runtime исключены.</div></div>`, `<button class="button primary" data-action="close-modal">Закрыть</button>`);
  }

  function showEditorVersions() {
    const document = editorDocumentFromRoute();
    if (!document) return;
    const rows = document.versions.length ? document.versions.map((revision) => `<div class="policy-card"><div class="preflight-row"><span>Revision v${revision.version}</span><strong>${revision.version === document.publishedVersion ? "Текущая" : "Архивная"}</strong></div><div class="policy-meta">${escapeHtml(revision.note)} · ${escapeHtml(revision.author)} · ${escapeHtml(revision.time)}</div>${revision.version === document.publishedVersion ? "" : `<div class="drawer-actions"><button class="button small" data-action="editor-rollback-preview" data-version="${revision.version}">Откатить новой версией</button></div>`}</div>`).join("") : `<div class="empty-state">Опубликованных версий ещё нет.</div>`;
    openModal("Версии", `<div class="preflight"><div class="preflight-row"><span>Runtime</span><strong>v${document.publishedVersion}</strong></div><div class="preflight-row"><span>Редактор</span><strong>${document.dirty ? "Есть несохранённые изменения" : `Черновик d${document.draftSerial} сохранён`}</strong></div></div><div class="filter-stack">${rows}</div>`, `<button class="button primary" data-action="close-modal">Закрыть</button>`);
  }

  function showEditorPublishPreview() {
    const document = editorDocumentFromRoute();
    if (!document) return;
    if (document.dirty) { toast("Публикация заблокирована", "Сначала сохраните локальные изменения в черновик."); return; }
    if (!editorHasCurrentValidation(document)) { toast("Публикация заблокирована", "Проверьте текущий сохранённый черновик."); return; }
    if (!editorHasUnpublishedDraft(document)) { toast("Нет изменений для публикации", `Runtime уже использует v${document.publishedVersion}.`); return; }
    const nextVersion = document.publishedVersion + 1;
    const impact = document.kind === "dashboard"
      ? `<div class="preflight-row"><span>Затронуто</span><strong>3 окна · 2 терминала</strong></div><div class="preflight-row"><span>Мнемосхема</span><strong>AHU-P1 · закреплена v4</strong></div>`
      : `<div class="preflight-row"><span>Использование</span><strong>1 дашборд · 2 терминала</strong></div><div class="preflight-row"><span>Безопасное состояние</span><strong>Offline/Bad задано</strong></div>`;
    openModal(`Опубликовать v${nextVersion}?`, `<p>Будет опубликована одна целая неизменяемая revision. Runtime не получит смешанное состояние.</p><div class="preflight"><div class="preflight-row"><span>Сущность</span><strong>${document.kind === "dashboard" ? "Dashboard" : "SVG-мнемосхема"}</strong></div><div class="preflight-row"><span>Переход</span><strong>v${document.publishedVersion} → v${nextVersion}</strong></div>${impact}<div class="preflight-row"><span>Роли</span><strong>Оператор · инженер</strong></div></div>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="editor-publish-confirm">Опубликовать v${nextVersion}</button>`);
  }

  function publishEditorRevision() {
    const document = editorDocumentFromRoute();
    if (!document || document.dirty || !editorHasCurrentValidation(document) || !editorHasUnpublishedDraft(document)) { closeModal(); toast("Публикация отменена", "Состояние черновика изменилось; повторите проверку."); return; }
    const nextVersion = document.publishedVersion + 1;
    document.publishedVersion = nextVersion;
    document.publishedSerial = document.savedSerial;
    document.versions.unshift({ version: nextVersion, author: currentUser.name, time: `Сегодня · ${new Date().toLocaleTimeString("ru-RU", { hour: "2-digit", minute: "2-digit" })}`, note: `Опубликован черновик d${document.draftSerial}`, snapshot: cloneEditorSnapshot(document.savedDraft) });
    closeModal();
    render();
    toast(`Опубликована v${nextVersion}`, "Runtime переключается только на целую готовую revision.");
  }

  function showEditorRollbackPreview(version) {
    const document = editorDocumentFromRoute();
    const source = document?.versions.find((revision) => revision.version === version);
    if (!document || !source || version === document.publishedVersion) return;
    if (document.dirty) { toast("Откат заблокирован", "Сначала сохраните либо отмените локальные изменения."); return; }
    const nextVersion = document.publishedVersion + 1;
    openModal("Откат новой версией", `<p>История не перезаписывается. Содержимое v${version} станет основой новой revision v${nextVersion}.</p><div class="preflight"><div class="preflight-row"><span>Текущий runtime</span><strong>v${document.publishedVersion}</strong></div><div class="preflight-row"><span>Основа</span><strong>v${version}</strong></div><div class="preflight-row"><span>Результат</span><strong>Новая v${nextVersion}</strong></div><div class="preflight-row"><span>Черновик d${document.draftSerial}</span><strong>Сохраняется отдельно</strong></div><div class="preflight-row"><span>Старая история</span><strong>Сохраняется</strong></div></div>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="editor-rollback-confirm" data-version="${version}">Создать и опубликовать v${nextVersion}</button>`);
  }

  function publishEditorRollback(version) {
    const document = editorDocumentFromRoute();
    const source = document?.versions.find((revision) => revision.version === version);
    if (!document || !source || version === document.publishedVersion || document.dirty) { closeModal(); return; }
    const nextVersion = document.publishedVersion + 1;
    const snapshot = cloneEditorSnapshot(source.snapshot);
    document.publishedSerial = -nextVersion;
    document.publishedVersion = nextVersion;
    document.versions.unshift({ version: nextVersion, author: currentUser.name, time: `Сегодня · ${new Date().toLocaleTimeString("ru-RU", { hour: "2-digit", minute: "2-digit" })}`, note: `Откат новой revision на основе v${version}`, snapshot: cloneEditorSnapshot(snapshot) });
    closeModal();
    render();
    toast(`Опубликована v${nextVersion}`, `Создана на основе v${version}; история и черновик сохранены.`);
  }

  function renderEditor() {
    const document = editorDocumentFromRoute();
    const mimicEditor = document.kind === "mimic";
    const routeOther = mimicEditor ? "/builder/dashboards/hvac" : "/builder/mimics/ahu-p1";
    const draftState = document.dirty ? badge("Несохранено", "critical") : badge(`Черновик d${document.draftSerial}`, "warning");
    const validationState = editorHasCurrentValidation(document) ? badge("Проверено", "good") : badge("Не проверено", "neutral");
    const dashboardCanvas = `<div class="editor-widget-grid"><div class="editor-widget wide"><strong data-editor-preview-name>${escapeHtml(document.working.elementName)}</strong><div class="row-meta">${escapeHtml(document.working.source)} · ссылка на опубликованную SVG-сущность</div></div><div class="editor-widget"><strong>Температура</strong></div><div class="editor-widget"><strong>Давление</strong></div><div class="editor-widget wide"><strong>События установки</strong></div></div>`;
    return `<div class="editor-shell"><header class="editor-toolbar"><div class="editor-zone"><button class="icon-button" data-nav="/home" title="Выйти">${icon("close")}</button><span class="editor-name">${escapeHtml(document.name)}</span>${badge(`Опубл. v${document.publishedVersion}`, "neutral")}<span data-editor-draft-state>${draftState}</span><span data-editor-validation-state>${validationState}</span></div><div class="editor-zone center"><button class="icon-button" title="Undo будет реализован позднее" disabled>↶</button><button class="icon-button" title="Redo будет реализован позднее" disabled>↷</button></div><div class="editor-zone end"><button class="button small hide-narrow" data-nav="${routeOther}">${mimicEditor ? "Dashboard Editor" : "SVG Editor"}</button><button class="button small" data-action="editor-canvas-only">${icon("eye")} ${state.editorCanvasOnly ? "Показать панели" : "Только холст"}</button><button class="button small hide-narrow" data-action="editor-preview">Preview</button><button class="button small hide-narrow" data-action="editor-versions">Версии</button><button class="button small" data-action="editor-save-draft">Сохранить</button><button class="button small" data-action="editor-validate">Проверить</button><button class="button primary small" data-action="editor-publish">Опубликовать v${document.publishedVersion + 1}</button></div></header>
      <div class="editor-workspace ${state.editorCanvasOnly ? "canvas-only" : ""}"><aside class="editor-panel"><div class="segmented"><button class="active">${mimicEditor ? "Символы" : "Виджеты"}</button><button>${mimicEditor ? "Слои" : "Экраны"}</button></div><div class="subsection"><h3>${mimicEditor ? "Библиотека символов" : "Базовые виджеты"}</h3><div class="library-list">${(mimicEditor ? ["Вентилятор", "Заслонка", "Фильтр", "Датчик", "Труба"] : ["Значение", "Состояние оборудования", "Тренд", "События", "Навигация"]).map((item) => `<div class="library-item">${icon(mimicEditor ? "equipment" : "dashboard")} ${item}</div>`).join("")}</div></div></aside><main class="editor-canvas-wrap"><div class="editor-canvas">${mimicEditor ? `<div class="editor-mimic"><strong data-editor-preview-name>${escapeHtml(document.working.elementName)}</strong><br>SVG-canvas · отдельная версионируемая сущность<br><span class="muted">Unknown/Bad не выглядит как нормальное состояние</span></div>` : dashboardCanvas}</div></main><aside class="editor-panel right"><div class="inspector-section"><h3>Свойства выбранного элемента</h3><label class="form-label">Название<input class="field" value="${escapeHtml(document.working.elementName)}" data-input="editor-property" data-field="elementName"></label></div><div class="inspector-section"><h3>Данные</h3><label class="form-label">Источник<select class="select"><option>${escapeHtml(document.working.source)}</option></select></label>${mimicEditor ? "" : `<div class="drawer-actions"><button class="button small" data-nav="/builder/mimics/ahu-p1">Редактировать мнемосхему</button></div>`}</div><div class="inspector-section"><h3>Отображение</h3><label class="form-label">Preview-состояние<select class="select"><option>Normal</option><option>Alarm</option><option>Offline / Bad</option><option>Нет права</option></select></label></div><div class="policy-card"><strong>Runtime: опубликована v${document.publishedVersion}</strong><div class="policy-meta">Черновик и локальные изменения не меняют операторский экран.</div></div></aside></div></div>`;
  }

  function renderDashboardWindowList() {
    const dashboard = currentDashboard();
    const activeWindow = currentDashboardWindow();
    return `<div class="dashboard-window-list">${dashboardWindowsFor(dashboard).map((window) => `<button class="dashboard-window-item ${window.id === activeWindow?.id ? "active" : ""}" data-nav="/d/${dashboard.id}/${window.id}" ${window.id === activeWindow?.id ? "aria-current=page" : ""}>${icon(window.type === "Mimic" ? "eye" : window.type === "Combined" ? "panel" : "dashboard")}<span><strong>${escapeHtml(window.name)}</strong><small>${escapeHtml(window.type)} · ${escapeHtml(window.description)}</small></span>${icon("chevron")}</button>`).join("")}</div><p class="settings-note">Каждое окно имеет собственную ссылку; Back/Forward восстанавливают выбранное окно.</p>`;
  }

  function renderDashboardObjectDetail(item) {
    if (!item) return `<div class="empty-state">Объект больше не доступен в текущем окне.</div>`;
    const history = state.dashboardMode === "history";
    const related = item.eventIds.map((id) => events.find((event) => event.id === id)).filter(Boolean);
    return `<div class="detail-hero"><div class="chip-row">${qualityBadge(item.quality)}${item.alarm ? badge("Активная авария", "high") : badge("Без аварий", "good")}${badge(history ? "History" : "Live", history ? "warning" : "good")}</div><h2>${escapeHtml(item.name)}</h2><div class="muted">${escapeHtml(item.type)} · ${escapeHtml(item.location)}</div></div><div class="detail-grid"><div class="detail-field"><span>Значение</span><strong>${escapeHtml(item.primaryValue)} ${escapeHtml(item.unit)}</strong></div><div class="detail-field"><span>Свежесть</span><strong>${escapeHtml(item.freshness)}</strong></div><div class="detail-field"><span>Режим</span><strong>${escapeHtml(item.mode)}</strong></div><div class="detail-field"><span>Управляет</span><strong>${escapeHtml(item.controlOwner)}</strong></div></div><div class="subsection"><h3>Контекст</h3><div class="preflight-row"><span>Состояние</span><strong>${escapeHtml(item.processState)}</strong></div><div class="preflight-row"><span>Дополнительно</span><strong>${escapeHtml(item.secondaryValue)}</strong></div>${item.alarm ? `<div class="preflight-row"><span>Авария</span><strong>${escapeHtml(item.alarm)}</strong></div>` : ""}</div><div class="subsection"><h3>Короткий тренд</h3>${sparkline(item.points)}</div><div class="subsection"><h3>Связанные события</h3>${related.length ? related.map((event) => `<button class="dashboard-related-event" data-action="open-event" data-id="${event.id}"><span>${escapeHtml(event.message)}</span>${badge(event.severity === "high" ? "Высокая" : event.severity, event.severity)}</button>`).join("") : `<p class="muted">Связанных событий нет.</p>`}</div><div class="drawer-actions"><button class="button primary" data-action="command-preview" data-id="${item.id}" ${history ? "disabled" : ""}>${icon("command")} ${history ? "Недоступно в History" : "Команда"}</button>${item.id === "AHU-P1" ? `<button class="button" data-action="open-equipment" data-id="AHU-P1">Карточка оборудования</button>` : `<button class="button" data-nav="/equipment">Реестр оборудования</button>`}<button class="button" data-nav="${item.historyRoute}">История</button><button class="button" data-nav="/events">Диспетчер событий</button></div>`;
  }

  function renderMaintenanceAssetDetail(item) {
    const plan = maintenancePlans.find((entry) => entry.assetId === item.id);
    const orders = maintenanceWorkOrders.filter((entry) => entry.assetId === item.id);
    const linkedEquipment = item.equipmentId ? equipment.find((entry) => entry.id === item.equipmentId) : null;
    const linkType = item.linkState === "Связан с устройством" ? "good" : item.linkState === "Связь требует проверки" ? "warning" : "neutral";
    return `<div class="detail-hero"><div class="chip-row">${badge(item.linkState, linkType)}</div><h2>${escapeHtml(item.name)}</h2><div class="muted">${escapeHtml(item.type)} · ${escapeHtml(item.location)}</div></div><div class="detail-grid"><div class="detail-field"><span>Паспорт</span><strong>${escapeHtml(item.passport)}</strong></div><div class="detail-field"><span>Ответственный</span><strong>${escapeHtml(item.owner)}</strong></div><div class="detail-field"><span>Ближайший ППР</span><strong>${escapeHtml(plan?.nextDue || "Не запланирован")}</strong></div><div class="detail-field"><span>Работы</span><strong>${orders.length}</strong></div></div>${item.linkState === "Только объект ТОиР" ? `<div class="policy-card"><strong>Телеметрия не требуется</strong><div class="policy-meta">Это самостоятельный объект обслуживания. Отсутствие устройства диспетчеризации не является ошибкой.</div></div>` : ""}<div class="drawer-actions">${linkedEquipment ? `<button class="button" data-action="open-equipment" data-id="${linkedEquipment.id}">Устройство ${linkedEquipment.id}</button>` : ""}${plan ? `<button class="button" data-action="open-maintenance-plan" data-id="${plan.id}">План ${plan.id}</button>` : ""}${orders[0] ? `<button class="button" data-action="open-maintenance-work-order" data-id="${orders[0].id}">Наряд ${orders[0].id}</button>` : ""}</div>`;
  }

  function renderMaintenancePlanDetail(item) {
    const asset = maintenanceAsset(item.assetId);
    return `<div class="detail-hero"><div class="chip-row">${badge(item.status, item.status === "Активен" ? "good" : "warning")}${badge("План, не наряд", "neutral")}</div><h2>${escapeHtml(item.name)}</h2><div class="muted">${escapeHtml(asset?.name || "Объект недоступен")} · ${escapeHtml(item.trigger)}</div></div><div class="detail-grid"><div class="detail-field"><span>Следующая дата</span><strong>${escapeHtml(item.nextDue)}</strong></div><div class="detail-field"><span>Регламент</span><strong>${escapeHtml(item.procedure)}</strong></div><div class="detail-field"><span>Исполнитель</span><strong>${escapeHtml(item.assignee)}</strong></div><div class="detail-field"><span>Состояние</span><strong>${escapeHtml(item.status)}</strong></div></div><div class="subsection"><h3>Preview ближайших дат</h3>${item.nextDates.map((date) => `<div class="preflight-row"><span>Плановая дата</span><strong>${escapeHtml(date)}</strong></div>`).join("")}</div><div class="policy-card"><strong>Граница прототипа</strong><div class="policy-meta">Редактирование периодичности, ресурсов и правил генерации нарядов будет проработано отдельно.</div></div>${asset ? `<div class="drawer-actions"><button class="button" data-action="open-maintenance-asset" data-id="${asset.id}">Объект обслуживания</button></div>` : ""}`;
  }

  function renderMaintenanceForecastDetail(item) {
    const asset = maintenanceAsset(item.assetId);
    const plan = maintenancePlan(item.planId);
    return `<div class="detail-hero"><div class="chip-row">${badge("Прогноз", "neutral")}${badge("Наряд не создан", "warning")}</div><h2>${escapeHtml(item.title)}</h2><div class="muted">${escapeHtml(asset?.name || "Объект недоступен")}</div></div><div class="detail-grid"><div class="detail-field"><span>Дата</span><strong>${escapeHtml(item.dueDate)}</strong></div><div class="detail-field"><span>Время</span><strong>${escapeHtml(item.time)}</strong></div><div class="detail-field"><span>План</span><strong>${escapeHtml(plan?.id || "—")}</strong></div><div class="detail-field"><span>Состояние</span><strong>Прогноз</strong></div></div><div class="policy-card"><strong>Это ещё не наряд</strong><div class="policy-meta">Назначение исполнителя и выполнение начнутся только после создания наряда.</div></div>${plan ? `<div class="drawer-actions"><button class="button" data-action="open-maintenance-plan" data-id="${plan.id}">Открыть план</button></div>` : ""}`;
  }

  function renderMaintenanceRequestDetail(item) {
    const sourceEvent = item.eventId ? events.find((entry) => entry.id === item.eventId) : null;
    return `<div class="detail-hero"><div class="chip-row">${badge(item.type, item.type === "Дефект" ? "warning" : "neutral")}${badge(item.status, "medium")}</div><h2>${escapeHtml(item.description)}</h2><div class="muted">${escapeHtml(item.object)} · ${escapeHtml(item.location)}</div></div><div class="detail-grid"><div class="detail-field"><span>Приоритет</span><strong>${escapeHtml(item.priority)}</strong></div><div class="detail-field"><span>Источник</span><strong>${escapeHtml(item.source)}</strong></div><div class="detail-field"><span>Состояние</span><strong>${escapeHtml(item.status)}</strong></div><div class="detail-field"><span>ID</span><strong>${escapeHtml(item.id)}</strong></div></div>${sourceEvent ? `<div class="policy-card"><strong>Создано из события ${sourceEvent.id}</strong><div class="policy-meta">Контекст перенесён; acknowledgement и состояние события не изменены.</div></div><div class="drawer-actions"><button class="button" data-action="open-task-event-source" data-event-id="${sourceEvent.id}">Открыть событие</button></div>` : ""}`;
  }

  function requiredChecklistComplete(item) {
    return item.checklist.every(([, done, required]) => !required || done);
  }

  function nextWorkOrderAction(item) {
    if (item.status === "Просрочен") return "Назначить мне";
    if (item.status === "Назначен") return "Принять наряд";
    if (item.status === "Принят") return "Начать работу";
    if (item.status === "В работе") return "Передать на приёмку";
    if (item.status === "На приёмке") return "Принять результат";
    return "";
  }

  function renderMaintenanceWorkOrderDetail(item) {
    const asset = maintenanceAsset(item.assetId);
    const plan = maintenancePlan(item.planId);
    const linkedEquipment = asset?.equipmentId ? equipment.find((entry) => entry.id === asset.equipmentId) : null;
    const action = nextWorkOrderAction(item);
    const checklistBlocked = item.status === "В работе" && !requiredChecklistComplete(item);
    const directRoute = `/maintenance/work-orders/${item.id}`;
    return `<div class="detail-hero"><div class="chip-row">${maintenanceStatusBadge(item.status)}${badge(item.priority, item.priority === "Высокий" ? "high" : "neutral")}</div><h2>${escapeHtml(item.title)}</h2><div class="muted">${escapeHtml(asset?.name || "Объект недоступен")} · ${escapeHtml(asset?.location || "—")}</div></div><div class="policy-card"><strong>Маршрут наряда</strong><div class="policy-meta">Назначен → Принят → В работе → На приёмке → Выполнен</div></div><div class="detail-grid"><div class="detail-field"><span>Окно</span><strong>${escapeHtml(item.dueDate)} · ${escapeHtml(item.time)}</strong></div><div class="detail-field"><span>Исполнитель</span><strong>${escapeHtml(item.assignee)}</strong></div><div class="detail-field"><span>Безопасность</span><strong>${escapeHtml(item.safety)}</strong></div><div class="detail-field"><span>Материалы</span><strong>${escapeHtml(item.materials)}</strong></div></div><div class="subsection"><h3>Чек-лист</h3><div class="maintenance-checklist">${item.checklist.map(([label, done, required], index) => `<button class="maintenance-check ${done ? "done" : ""}" data-action="toggle-work-order-check" data-id="${item.id}" data-index="${index}" ${item.status !== "В работе" ? "disabled" : ""}><span>${done ? "✓" : "○"}</span><strong>${escapeHtml(label)}</strong>${required ? `<small>Обязательно</small>` : `<small>Необязательно</small>`}</button>`).join("")}</div>${checklistBlocked ? `<p class="settings-note">Обязательные пункты должны быть выполнены перед передачей на приёмку.</p>` : ""}</div>${action ? `<div class="drawer-actions"><button class="button primary" data-action="advance-work-order" data-id="${item.id}" ${checklistBlocked ? "disabled" : ""}>${escapeHtml(action)}</button></div>` : `<div class="drawer-actions">${badge("Следующих действий нет", "neutral")}</div>`}<div class="subsection"><h3>Связанный контекст</h3><div class="drawer-actions">${state.route !== directRoute ? `<button class="button" data-nav="${directRoute}">Отдельная ссылка</button>` : ""}${linkedEquipment ? `<button class="button" data-action="open-equipment" data-id="${linkedEquipment.id}">Оборудование</button>` : ""}${plan ? `<button class="button" data-action="open-maintenance-plan" data-id="${plan.id}">План ${plan.id}</button>` : ""}<button class="button" data-nav="/my-work">Моя работа</button></div></div><div class="subsection"><h3>Активность</h3><div class="timeline">${item.timeline.map(([time, text]) => `<div class="timeline-item"><div class="timeline-time">${escapeHtml(time)}</div><div class="timeline-text">${escapeHtml(text)}</div></div>`).join("")}</div></div>`;
  }

  function advanceMaintenanceWorkOrder(id) {
    const item = maintenanceWorkOrders.find((entry) => entry.id === id);
    if (!item) return;
    if (item.status === "В работе" && !requiredChecklistComplete(item)) { toast("Чек-лист не завершён", "Выполните обязательные пункты перед передачей на приёмку."); return; }
    const previous = item.status;
    if (item.status === "Просрочен") { item.status = "Назначен"; item.assignee = currentUser.name; }
    else if (item.status === "Назначен") item.status = "Принят";
    else if (item.status === "Принят") item.status = "В работе";
    else if (item.status === "В работе") item.status = "На приёмке";
    else if (item.status === "На приёмке") item.status = "Выполнен";
    else return;
    item.timeline.unshift(["14:35", `${previous} → ${item.status} · ${currentUser.name}`]);
    if (item.status === "Выполнен") tasks.filter((task) => task.workOrderId === item.id && isTaskActionable(task)).forEach((task) => { task.status = "Выполнено"; task.assignedToMe = false; task.timeline.unshift(["14:35", `Завершено из наряда ${item.id}`]); });
    render();
    toast("Состояние наряда изменено", `${item.id}: ${previous} → ${item.status}.`);
  }

  function renderAdminAccountDetail(item) {
    const person = item.personId ? people.find((entry) => entry.id === item.personId) : null;
    return `<div class="detail-hero"><div class="chip-row">${badge("Учётная запись", "neutral")}${badge(item.status, item.status === "Активна" ? "good" : "warning")}${item.conflicts === "Нет" ? badge("Без конфликтов", "good") : badge(item.conflicts, "warning")}</div><h2>${escapeHtml(item.name)}</h2><div class="muted">${escapeHtml(item.login)} · ${escapeHtml(item.identitySource)}</div></div><div class="detail-grid"><div class="detail-field"><span>Роль доступа</span><strong>${escapeHtml(item.role)}</strong></div><div class="detail-field"><span>Область</span><strong>${escapeHtml(item.scope)}</strong></div><div class="detail-field"><span>Группы</span><strong>${escapeHtml(item.groups.join(", "))}</strong></div><div class="detail-field"><span>Временный доступ</span><strong>${escapeHtml(item.temporary)}</strong></div></div><div class="subsection"><h3>Effective permissions</h3>${item.services.map((service) => `<div class="preflight-row"><span>${escapeHtml(service)}</span><strong>${escapeHtml(item.permissionSource)}</strong></div>`).join("")}<div class="policy-card"><strong>Разрешённые действия</strong><div class="policy-meta">${escapeHtml(item.actions.join(" · "))}</div></div></div>${item.lastScopeAdmin ? `<div class="policy-card"><strong>Последний администратор области</strong><div class="policy-meta">Снятие административной роли блокируется до назначения замены.</div></div>` : ""}<div class="drawer-actions"><button class="button primary" data-action="admin-assignment-preview" data-id="${item.id}">Изменить роль и область</button>${person ? `<button class="button" data-nav="/users/${person.id}">Страница сотрудника</button>` : ""}<button class="button" data-nav="/admin/audit">Технический аудит</button></div>`;
  }

  function renderAdminRoleDetail(item) {
    return `<div class="detail-hero"><div class="chip-row">${badge("Роль безопасности", "neutral")}${badge(`${item.users} пользователей`, "medium")}</div><h2>${escapeHtml(item.name)}</h2><div class="muted">${escapeHtml(item.source)}</div></div><div class="detail-grid"><div class="detail-field"><span>Допустимая область</span><strong>${escapeHtml(item.scope)}</strong></div><div class="detail-field"><span>Пользователи</span><strong>${item.users}</strong></div></div><div class="policy-card"><strong>Сервисы</strong><div class="policy-meta">${escapeHtml(item.services)}</div></div><div class="policy-card"><strong>Действия</strong><div class="policy-meta">${escapeHtml(item.actions)}</div></div><p class="settings-note">В этой итерации каталог ролей доступен только для чтения. Плоская матрица прав не используется.</p>`;
  }

  function renderAdminIntegrationDetail(item) {
    const tested = state.adminConnectionTests[item.id];
    const issue = platformHealthIssues.find((entry) => entry.integrationId === item.id);
    return `<div class="detail-hero"><div class="chip-row">${badge(item.type, "neutral")}${badge(item.status, item.status === "Норма" ? "good" : item.status === "Деградация" ? "warning" : "neutral")}</div><h2>${escapeHtml(item.name)}</h2><div class="muted">${escapeHtml(item.purpose)}</div></div><div class="detail-grid"><div class="detail-field"><span>Endpoint</span><strong>${escapeHtml(item.endpoint)}</strong></div><div class="detail-field"><span>Владелец</span><strong>${escapeHtml(item.owner)}</strong></div><div class="detail-field"><span>Последний успех</span><strong>${escapeHtml(item.lastSuccess)}</strong></div><div class="detail-field"><span>Последняя ошибка</span><strong>${escapeHtml(item.lastError)}</strong></div></div>${tested ? `<div class="policy-card"><strong>${escapeHtml(tested)}</strong><div class="policy-meta">Информационная проверка прототипа; сетевой вызов не выполнялся.</div></div>` : ""}<div class="policy-card"><strong>Секреты скрыты</strong><div class="policy-meta">Пароли, токены и строки подключения не отображаются и не попадают в аудит.</div></div><div class="drawer-actions"><button class="button primary" data-action="admin-test-integration" data-id="${item.id}" ${item.status === "Выключено" ? "disabled" : ""}>Проверить</button>${issue ? `<button class="button" data-action="open-admin-health" data-id="${issue.id}">Проблема ${issue.id}</button>` : ""}</div>`;
  }

  function renderAdminHealthDetail(item) {
    return `<div class="detail-hero"><div class="chip-row">${badge("Платформа", "neutral")}${badge(item.status, item.type)}</div><h2>${escapeHtml(item.title)}</h2><div class="muted">${escapeHtml(item.scope)}</div></div><div class="detail-grid"><div class="detail-field"><span>Влияние</span><strong>${escapeHtml(item.impact)}</strong></div><div class="detail-field"><span>Последний успех</span><strong>${escapeHtml(item.lastSuccess)}</strong></div></div><div class="policy-card"><strong>Причина</strong><div class="policy-meta">${escapeHtml(item.cause)}</div></div><p class="settings-note">Системная проблема не является аварийным occurrence здания.</p><div class="drawer-actions">${item.integrationId ? `<button class="button" data-action="open-admin-integration" data-id="${item.integrationId}">Подключение</button>` : ""}${item.equipmentId ? `<button class="button" data-action="open-equipment" data-id="${item.equipmentId}">Оборудование</button>` : ""}<button class="button" data-nav="/admin/audit">Аудит</button></div>`;
  }

  function renderAdminDataQualityDetail(item) {
    return `<div class="detail-hero"><div class="chip-row">${badge(item.category, "neutral")}${badge(item.status, item.status === "Новая" ? "warning" : "medium")}</div><h2>${escapeHtml(item.title)}</h2><div class="muted">${escapeHtml(item.object)}</div></div><div class="detail-grid"><div class="detail-field"><span>Ответственный</span><strong>${escapeHtml(item.assignedTo)}</strong></div><div class="detail-field"><span>Состояние</span><strong>${escapeHtml(item.status)}</strong></div></div><div class="policy-card"><strong>Граница состояния</strong><div class="policy-meta">${escapeHtml(item.detail)}</div></div><div class="drawer-actions"><button class="button primary" data-action="admin-assign-data-quality" data-id="${item.id}" ${item.assignedTo === currentUser.name ? "disabled" : ""}>Назначить мне</button></div>`;
  }

  function renderAdminAuditDetail(item) {
    return `<div class="detail-hero"><div class="chip-row">${badge("Audit · read-only", "neutral")}${badge(item.result, item.result === "Успех" ? "good" : item.result === "Отказ" ? "critical" : "warning")}</div><h2>${escapeHtml(item.action)}</h2><div class="muted">${escapeHtml(item.id)} · ${escapeHtml(item.time)}</div></div><div class="detail-grid"><div class="detail-field"><span>Исполнитель</span><strong>${escapeHtml(item.actor)}</strong></div><div class="detail-field"><span>Объект</span><strong>${escapeHtml(item.object)}</strong></div><div class="detail-field"><span>Категория</span><strong>${escapeHtml(item.category)}</strong></div><div class="detail-field"><span>Результат</span><strong>${escapeHtml(item.result)}</strong></div></div><div class="policy-card"><strong>Контекст изменения</strong><div class="policy-meta">${escapeHtml(item.summary)}</div></div><p class="settings-note">Запись нельзя изменить или удалить из интерфейса.</p>`;
  }

  function renderAdminTerminalDetail(terminal) {
    const profile = kioskProfile(terminal.profileId);
    const active = terminal.status === "Активен";
    const revoked = terminal.status === "Отозван";
    return `<div class="detail-hero"><div class="chip-row">${badge("Device identity", "neutral")}${badge(terminal.status, active ? "good" : revoked ? "critical" : "warning")}${terminal.online ? badge("Online", "good") : badge("Offline", "critical")}</div><h2>${escapeHtml(terminal.name)}</h2><div class="muted">${escapeHtml(terminal.id)} · ${escapeHtml(terminal.deviceIdentity)}</div></div><div class="detail-grid"><div class="detail-field"><span>Локация</span><strong>${escapeHtml(terminal.location)}</strong></div><div class="detail-field"><span>Последнее появление</span><strong>${escapeHtml(terminal.lastSeen)}</strong></div><div class="detail-field"><span>Конфигурация</span><strong>${escapeHtml(terminal.configVersion)} · ${escapeHtml(terminal.sync)}</strong></div><div class="detail-field"><span>Разрешение</span><strong>${escapeHtml(terminal.resolution)}</strong></div></div><div class="subsection"><h3>Effective profile</h3><div class="preflight-row"><span>Общий профиль · профиль терминала</span><strong>${escapeHtml(profile?.sharedProfile || "—")}</strong></div><div class="preflight-row"><span>Дашборд · назначение</span><strong>${escapeHtml(kioskDashboardName(terminal.dashboardRoute))}</strong></div><div class="preflight-row"><span>Авторизация · профиль терминала</span><strong>${escapeHtml(profile?.authLabel || "—")}</strong></div><div class="preflight-row"><span>Timeout управления · профиль терминала</span><strong>${profile?.controlTimeout || 0} с</strong></div><div class="preflight-row"><span>Offline · политика организации</span><strong>${escapeHtml(profile?.offlineBehavior || "—")}</strong></div></div><div class="policy-card"><strong>Граница идентичности</strong><div class="policy-meta">Профиль может быть общим для нескольких панелей. Блокировка и отзыв применяются только к этой device identity; отдельная учётная запись пользователя не создаётся.</div></div><div class="drawer-actions"><button class="button primary" data-nav="${escapeHtml(kioskRuntimeRoute(terminal))}">Открыть runtime</button>${revoked ? "" : `<button class="button" data-action="admin-terminal-rebind" data-id="${terminal.id}">Изменить назначение</button><button class="button" data-action="admin-terminal-state-preview" data-id="${terminal.id}" data-value="${active ? "Заблокирован" : "Активен"}">${active ? "Заблокировать" : "Разблокировать"}</button><button class="button danger" data-action="admin-terminal-state-preview" data-id="${terminal.id}" data-value="Отозван">Отозвать identity</button>`}</div>`;
  }

  function renderDrawer() {
    if (!state.drawer) return "";
    let title = "Подробности";
    let subtitle = "";
    let body = "";
    let tabs = "";
    if (state.drawer.kind === "dashboard-windows") {
      title = currentDashboard()?.name || "Дашборд";
      subtitle = "Окна дашборда";
      body = renderDashboardWindowList();
    } else if (state.drawer.kind === "dashboard-object") {
      const item = dashboardObjects[state.drawer.id];
      title = item?.name || "Выбранный объект";
      subtitle = item?.id || "Контекст недоступен";
      body = renderDashboardObjectDetail(item);
    } else if (state.drawer.kind === "events") {
      const item = events.find((event) => event.id === state.drawer.id);
      title = item ? item.id : "Фильтры событий";
      subtitle = item ? item.source : "Активное представление";
      tabs = `<div class="drawer-tabs"><button class="tab-button ${state.drawerTab === "details" ? "active" : ""}" data-action="drawer-tab" data-value="details" ${item ? "" : "disabled"}>Подробности</button><button class="tab-button ${state.drawerTab === "filters" ? "active" : ""}" data-action="drawer-tab" data-value="filters">Фильтры <span class="chip">${activeEventFilterCount()}</span></button></div>`;
      body = state.drawerTab === "filters" ? renderEventFilters() : renderEventDetail(item);
    } else if (state.drawer.kind === "equipment") {
      const item = equipment.find((entry) => entry.id === state.drawer.id);
      title = item.name; subtitle = item.id; body = renderEquipmentDetail(item);
    } else if (state.drawer.kind === "task") {
      const item = tasks.find((entry) => entry.id === state.drawer.id);
      title = item.action; subtitle = `${item.type} · ${item.id}`; body = renderTaskDetail(item);
    } else if (state.drawer.kind === "maintenance-asset") {
      const item = maintenanceAsset(state.drawer.id);
      if (!item) return "";
      title = item.name; subtitle = `Объект обслуживания · ${item.id}`; body = renderMaintenanceAssetDetail(item);
    } else if (state.drawer.kind === "maintenance-plan") {
      const item = maintenancePlan(state.drawer.id);
      if (!item) return "";
      title = item.name; subtitle = `План ППР · ${item.id}`; body = renderMaintenancePlanDetail(item);
    } else if (state.drawer.kind === "maintenance-forecast") {
      const item = maintenanceForecasts.find((entry) => entry.id === state.drawer.id);
      if (!item) return "";
      title = item.title; subtitle = `Прогноз · ${item.id}`; body = renderMaintenanceForecastDetail(item);
    } else if (state.drawer.kind === "maintenance-request") {
      const item = maintenanceRequests.find((entry) => entry.id === state.drawer.id);
      if (!item) return "";
      title = item.description; subtitle = `${item.type} · ${item.id}`; body = renderMaintenanceRequestDetail(item);
    } else if (state.drawer.kind === "maintenance-work-order") {
      const item = maintenanceWorkOrders.find((entry) => entry.id === state.drawer.id);
      if (!item) return "";
      title = item.title; subtitle = `Наряд · ${item.id}`; body = renderMaintenanceWorkOrderDetail(item);
    } else if (state.drawer.kind === "admin-account") {
      const item = adminAccounts.find((entry) => entry.id === state.drawer.id);
      if (!item) return "";
      title = item.name; subtitle = `Учётная запись · ${item.id}`; body = renderAdminAccountDetail(item);
    } else if (state.drawer.kind === "admin-role") {
      const item = adminRoles.find((entry) => entry.id === state.drawer.id);
      if (!item) return "";
      title = item.name; subtitle = `Роль · ${item.id}`; body = renderAdminRoleDetail(item);
    } else if (state.drawer.kind === "admin-integration") {
      const item = adminIntegrations.find((entry) => entry.id === state.drawer.id);
      if (!item) return "";
      title = item.name; subtitle = `Подключение · ${item.id}`; body = renderAdminIntegrationDetail(item);
    } else if (state.drawer.kind === "admin-health") {
      const item = platformHealthIssues.find((entry) => entry.id === state.drawer.id);
      if (!item) return "";
      title = item.title; subtitle = `Состояние платформы · ${item.id}`; body = renderAdminHealthDetail(item);
    } else if (state.drawer.kind === "admin-data-quality") {
      const item = dataQualityIssues.find((entry) => entry.id === state.drawer.id);
      if (!item) return "";
      title = item.title; subtitle = `Качество данных · ${item.id}`; body = renderAdminDataQualityDetail(item);
    } else if (state.drawer.kind === "admin-audit") {
      const item = adminAuditEntries.find((entry) => entry.id === state.drawer.id);
      if (!item) return "";
      title = item.action; subtitle = `Аудит · ${item.id}`; body = renderAdminAuditDetail(item);
    } else if (state.drawer.kind === "admin-terminal") {
      const item = kioskTerminal(state.drawer.id);
      if (!item) return "";
      title = item.name; subtitle = `Терминал · ${item.id}`; body = renderAdminTerminalDetail(item);
    }
    return `<div class="drawer-backdrop" data-action="close-drawer"></div><aside class="context-drawer" aria-label="Контекстная панель"><div class="drawer-header"><div class="drawer-heading"><strong>${escapeHtml(title)}</strong><span>${escapeHtml(subtitle)}</span></div><button class="icon-button" data-action="close-drawer" aria-label="Закрыть">${icon("close")}</button></div>${tabs}<div class="drawer-body">${body}</div></aside>`;
  }

  function renderEventFilters() {
    return `<div class="filter-stack"><label class="form-label">Важность<select class="select" data-change="event-severity"><option value="all">Все уровни</option><option value="critical" ${state.eventSeverity === "critical" ? "selected" : ""}>Критическая</option><option value="high" ${state.eventSeverity === "high" ? "selected" : ""}>Высокая</option><option value="medium" ${state.eventSeverity === "medium" ? "selected" : ""}>Средняя</option><option value="low" ${state.eventSeverity === "low" ? "selected" : ""}>Низкая</option></select></label><label class="form-label">Состояние условия<select class="select" data-change="event-condition"><option value="all">Все</option><option value="active" ${state.eventCondition === "active" ? "selected" : ""}>Активные</option><option value="cleared" ${state.eventCondition === "cleared" ? "selected" : ""}>Возвращены к норме</option></select></label><label class="form-label">Локация<select class="select" data-change="event-location"><option value="all">Все локации</option>${["Корпус A", "Корпус B", "Корпус C", "ЦОД", "ГРЩ", "Платформа"].map((location) => `<option value="${location}" ${state.eventLocation === location ? "selected" : ""}>${location}</option>`).join("")}</select></label><label class="form-label">Ответственный<select class="select" data-change="event-assignment"><option value="all">Все</option><option value="unassigned" ${state.eventAssignment === "unassigned" ? "selected" : ""}>Не назначено</option><option value="mine" ${state.eventAssignment === "mine" ? "selected" : ""}>Назначено мне</option></select></label><div class="drawer-actions"><button class="button primary" data-action="apply-event-filters">Применить</button><button class="button" data-action="reset-event-filters">Сбросить</button></div></div>`;
  }

  function renderEventDetail(item) {
    if (!item) return `<div class="empty-state">Выберите запись в списке</div>`;
    const readonly = state.eventMode === "history";
    const incident = item.incidentId ? incidents.find((entry) => entry.id === item.incidentId) : null;
    const maintenanceRequest = item.maintenanceRequestId ? maintenanceRequests.find((entry) => entry.id === item.maintenanceRequestId) : null;
    const instruction = item.kind === "Событие" ? "Запись носит информационный характер; acknowledgement неприменим." : item.ack === false ? "Проверьте источник и подтвердите, что состояние принято в работу." : item.condition === "Активна" ? "Состояние подтверждено; контролируйте устранение и ответственного." : "Условие вернулось к норме; завершите применимые действия по occurrence.";
    return `<div class="detail-hero"><div class="chip-row">${badge(item.kind, "neutral")}${badge(item.severity === "critical" ? "Критическая" : item.severity === "high" ? "Высокая" : item.severity === "medium" ? "Средняя" : item.severity === "low" ? "Низкая" : "Информация", item.severity)}${item.condition ? badge(item.condition, item.condition === "Активна" ? "critical" : "good") : badge("Точечный факт", "neutral")}${item.ack === false ? badge("Не подтверждено", "warning") : item.ack === true ? badge("Подтверждено", "good") : ""}${readonly ? badge("История · read-only", "warning") : ""}</div><h2>${escapeHtml(item.message)}</h2><div class="muted">${escapeHtml(item.location)} · ${escapeHtml(item.source)}</div></div>
      ${item.suppression ? `<div class="policy-card"><strong>Suppressed политикой</strong><div class="policy-meta">${escapeHtml(item.suppression)} · владелец: Сетевая служба</div></div>` : ""}
      ${item.shelvedUntil ? `<div class="policy-card"><strong>Shelved до ${item.shelvedUntil}</strong><div class="policy-meta">Причина: ${escapeHtml(item.shelveReason)}</div></div>` : ""}
      ${incident ? `<div class="policy-card"><strong>Связанный инцидент · ${escapeHtml(incident.id)}</strong><div class="policy-meta">${escapeHtml(incident.status)} · координатор: ${escapeHtml(incident.coordinator)}</div></div>` : ""}
      ${maintenanceRequest ? `<div class="policy-card"><strong>Связанная заявка ТОиР · ${escapeHtml(maintenanceRequest.id)}</strong><div class="policy-meta">${escapeHtml(maintenanceRequest.status)} · acknowledgement и состояние аварии независимы</div></div>` : ""}
      <div class="detail-grid"><div class="detail-field"><span>Значение</span><strong>${escapeHtml(item.value)}</strong></div><div class="detail-field"><span>Качество</span><strong>${qualityBadge(item.quality)}</strong></div><div class="detail-field"><span>Ответственный</span><strong>${escapeHtml(item.assignment || "—")}</strong></div><div class="detail-field"><span>Возникло</span><strong>${item.time}</strong></div></div>
      <div class="policy-card"><strong>Рекомендуемое действие</strong><div class="policy-meta">${escapeHtml(instruction)}</div></div>
      <div class="drawer-actions">${readonly ? `<button class="button primary" data-action="event-mode" data-value="realtime">Открыть текущее состояние</button>` : item.ack === false ? `<button class="button primary" data-action="ack-event" data-id="${item.id}">Подтвердить</button>` : item.assignment === "Не назначено" ? `<button class="button primary" data-action="assign-event" data-id="${item.id}">Назначить мне</button>` : ""}${!readonly && item.shelvedUntil ? `<button class="button" data-action="unshelve-event" data-id="${item.id}">Unshelve</button>` : ""}${!readonly && item.shelvable && !item.shelvedUntil && !item.suppression ? `<button class="button" data-action="shelve-event" data-id="${item.id}">Shelve</button>` : ""}${!readonly ? incident ? `<button class="button" data-action="open-incident" data-id="${incident.id}">Открыть ${incident.id}</button>` : `<button class="button" data-action="create-incident" data-id="${item.id}">Создать инцидент</button>` : ""}${!readonly ? maintenanceRequest ? `<button class="button" data-action="open-maintenance-request" data-id="${maintenanceRequest.id}">Открыть ${maintenanceRequest.id}</button>` : `<button class="button" data-action="create-work-request" data-id="${item.id}">Создать заявку</button>` : ""}</div>
      <div class="subsection"><h3>Связанный контекст</h3><div class="drawer-actions">${item.equipmentId ? `<button class="button" data-action="open-equipment" data-id="${item.equipmentId}">Оборудование</button>` : ""}${item.dashboardRoute ? `<button class="button" data-nav="${item.dashboardRoute}">Мнемосхема</button>` : ""}${item.historyRoute ? `<button class="button" data-nav="${item.historyRoute}">История параметра</button>` : ""}</div></div>
      <div class="subsection"><h3>Хронология</h3><div class="timeline">${item.timeline.map(([time, text]) => `<div class="timeline-item"><div class="timeline-time">${escapeHtml(time)}</div><div class="timeline-text">${escapeHtml(text)}</div></div>`).join("")}</div></div>`;
  }

  function renderEquipmentDetail(item) {
    const maintenanceItem = maintenanceAssets.find((entry) => entry.equipmentId === item.id);
    return `<div class="detail-hero"><div class="chip-row">${qualityBadge(item.quality)}${badge(item.status, item.status === "Авария" ? "critical" : item.status === "Норма" ? "good" : "neutral")}</div><h2>${escapeHtml(item.name)}</h2><div class="muted">${escapeHtml(item.type)} · ${escapeHtml(item.location)}</div></div><div class="detail-grid"><div class="detail-field"><span>Связь</span><strong>${escapeHtml(item.connection)}</strong></div><div class="detail-field"><span>Ответственный</span><strong>${escapeHtml(item.owner)}</strong></div><div class="detail-field"><span>Активные аварии</span><strong>${item.alarms}</strong></div><div class="detail-field"><span>Ближайший ППР</span><strong>${escapeHtml(equipmentPprLabel(item))}</strong></div></div><div class="subsection"><h3>Ключевые параметры</h3>${item.params.map(([name, value, quality]) => `<div class="preflight-row"><span>${escapeHtml(name)}</span><strong>${escapeHtml(value)} · ${qualityBadge(quality)}</strong></div>`).join("")}</div><div class="drawer-actions"><button class="button primary" data-action="command-preview" data-id="${item.id}">${icon("command")} Команда</button>${maintenanceItem ? `<button class="button" data-action="open-maintenance-asset" data-id="${maintenanceItem.id}">ТОиР и ППР</button>` : ""}<button class="button" data-nav="/d/main/overview">Мнемосхема</button><button class="button" data-nav="/events">События</button></div>`;
  }

  function renderTaskDetail(item) {
    const actionable = isTaskActionable(item);
    const sourceActions = renderTaskSourceActions(item);
    return `<div class="detail-hero"><div class="chip-row">${badge(item.type, "neutral")}${badge(item.priority, item.priority === "Критический" ? "critical" : item.priority === "Высокий" ? "high" : "neutral")}</div><h2>${escapeHtml(item.action)}</h2><div class="muted">${escapeHtml(item.object)} · ${escapeHtml(item.location)}</div></div><div class="detail-grid"><div class="detail-field"><span>Срок</span><strong>${escapeHtml(item.due)}</strong></div><div class="detail-field"><span>Состояние</span><strong>${escapeHtml(item.status)}</strong></div><div class="detail-field"><span>Назначил</span><strong>${escapeHtml(item.from)}</strong></div><div class="detail-field"><span>Объект</span><strong>${escapeHtml(item.object)}</strong></div></div>${sourceActions ? `<div class="subsection"><h3>Источник задачи</h3><div class="drawer-actions">${sourceActions}</div></div>` : ""}<div class="drawer-actions">${actionable && item.status !== "Принято" ? `<button class="button primary" data-action="accept-task" data-id="${item.id}">Принять</button>` : ""}${actionable ? `<button class="button" data-action="transfer-task" data-id="${item.id}">Передать</button><button class="button danger" data-action="cannot-task" data-id="${item.id}">Не могу выполнить</button>` : badge("Больше не назначена вам", "neutral")}</div><div class="subsection"><h3>Активность</h3><div class="timeline">${item.timeline.map(([time, text]) => `<div class="timeline-item"><div class="timeline-time">${escapeHtml(time)}</div><div class="timeline-text">${escapeHtml(text)}</div></div>`).join("")}</div></div>`;
  }

  function render() {
    state.route = routeFromHash();
    const kioskContext = kioskRuntimeContext();
    if (state.kioskControlTerminalId && (!kioskContext?.terminal || !kioskContext.profile || !kioskContext.assignmentValid || kioskContext.terminal.id !== state.kioskControlTerminalId || kioskContext.terminal.status !== "Активен" || !kioskContext.terminal.online || state.kioskControlExpiresAt <= Date.now())) clearKioskControl();
    if (isKioskRuntimeRoute() && state.controlEnabled) {
      state.controlEnabled = false; state.controlRemaining = 0; state.controlScopeDashboardId = null; clearInterval(state.controlTimer); state.controlTimer = null;
    }
    if (state.controlEnabled && state.controlScopeDashboardId && currentDashboardId() !== state.controlScopeDashboardId) {
      state.controlEnabled = false;
      state.controlRemaining = 0;
      state.controlScopeDashboardId = null;
      clearInterval(state.controlTimer);
      state.controlTimer = null;
    }
    if (isValidDashboardRoute(state.route)) rememberDashboardRoute(state.route);
    const supportedBuilderRoute = isEditorRoute(state.route);
    document.body.classList.toggle("touch-preview", supportedBuilderRoute && state.editorProfile === "touch");
    if (state.route === "/terminal/enroll") app.innerHTML = renderTerminalEnroll();
    else if (isKioskRuntimeRoute(state.route)) app.innerHTML = renderKioskRuntime();
    else if (state.workFullscreen && isValidDashboardRoute(state.route)) app.innerHTML = renderWorkFullscreen();
    else {
      let content;
      if (supportedBuilderRoute) content = renderEditor();
      else if (state.route === "/home") content = renderHome();
      else if (state.route === "/me") content = renderPersonPage(people[0], true);
      else if (state.route.startsWith("/users/")) {
        const person = personFromRoute();
        content = person ? renderPersonPage(person, false) : renderNotFound("Сотрудник не найден или недоступен.");
      }
      else if (state.route === "/dashboards") content = renderDashboardCatalog();
      else if (state.route.startsWith("/d/")) content = isValidDashboardRoute(state.route) ? renderDashboard() : renderNotFound("Дашборд или экран не найден.");
      else if (state.route.startsWith("/events")) content = renderEvents();
      else if (state.route === "/equipment/add") content = renderEquipmentAdd();
      else if (state.route.startsWith("/equipment")) content = renderEquipment();
      else if (state.route.startsWith("/maintenance")) content = isSupportedMaintenanceRoute(state.route) ? renderMaintenance() : renderNotFound("Раздел ТОиР не найден.");
      else if (state.route === "/notifications") content = renderNotificationsInbox();
      else if (state.route === "/my-work") content = renderMyWork();
      else if (state.route === "/settings" || state.route.startsWith("/settings/")) content = isSupportedSettingsRoute(state.route) ? renderSettings() : renderNotFound("Раздел настроек не найден.");
      else if (state.route === "/admin" || state.route.startsWith("/admin/")) content = !currentUser.isAdmin ? renderAccessDenied() : isSupportedAdminRoute(state.route) ? renderAdmin() : renderNotFound("Административный раздел не найден.");
      else if (state.route.startsWith("/trends/tag/")) {
        const history = tagHistoryFromRoute();
        content = history ? renderTagHistory(history) : renderNotFound("Тег не найден или недоступен.");
      }
      else content = renderNotFound();
      app.innerHTML = renderShell(content);
      if (state.route === "/dashboards" && state.dashboardCatalogScroll) {
        requestAnimationFrame(() => {
          const catalog = document.querySelector(".dashboard-catalog-page");
          if (catalog) catalog.scrollTop = state.dashboardCatalogScroll;
        });
      }
    }
    document.title = `Диспетчер — ${routeTitle()}`;
  }

  function openModal(title, body, footer = "") {
    modalRoot.innerHTML = `<div class="modal-backdrop" data-action="close-modal"><section class="modal" role="dialog" aria-modal="true" aria-labelledby="modal-title"><div class="modal-header"><h2 id="modal-title">${title}</h2><button class="icon-button" data-action="close-modal" aria-label="Закрыть">${icon("close")}</button></div><div class="modal-body">${body}</div>${footer ? `<div class="modal-footer">${footer}</div>` : ""}</section></div>`;
    setTimeout(() => modalRoot.querySelector("input, textarea, select, button")?.focus(), 0);
  }

  function closeModal() {
    modalRoot.innerHTML = "";
    state.pendingEditorNavigation = null;
    state.kioskPendingCommand = null;
  }

  function toast(title, message) {
    const id = `toast-${Date.now()}`;
    toastRoot.insertAdjacentHTML("beforeend", `<div class="toast" id="${id}"><div><strong>${escapeHtml(title)}</strong><span>${escapeHtml(message)}</span></div><button data-action="close-toast" data-id="${id}">×</button></div>`);
    setTimeout(() => document.getElementById(id)?.remove(), 5000);
  }

  function addAdminAudit(action, object, result, category, summary) {
    adminAuditEntries.unshift({ id: `AUD-${105 + adminAuditEntries.length}`, time: new Date().toLocaleTimeString("ru-RU", { hour: "2-digit", minute: "2-digit" }), actor: currentUser.name, action, object, result, category, summary });
  }

  function approveTerminalEnrollment() {
    if (!currentUser.isAdmin || terminalEnrollment.status === "Активен") return;
    const code = document.getElementById("terminal-enrollment-code")?.value.replace(/\s/g, "") || "";
    const name = document.getElementById("terminal-enrollment-name")?.value.trim() || "";
    const location = document.getElementById("terminal-enrollment-location")?.value.trim() || "";
    const profile = kioskProfile(document.getElementById("terminal-enrollment-profile")?.value);
    const dashboardRoute = document.getElementById("terminal-enrollment-dashboard")?.value || "";
    if (code !== terminalEnrollment.code || !name || !location || !profile || !isValidDashboardRoute(dashboardRoute)) { toast("Регистрация не подтверждена", "Проверьте одноразовый код, имя, локацию, профиль и стартовый дашборд."); return; }
    const terminal = {
      id: `TERM-${String(kioskTerminals.length + 1).padStart(3, "0")}`, deviceIdentity: "DEV-C04E", name, location,
      profileId: profile.id, dashboardRoute, status: "Активен", online: true, lastSeen: "Сейчас",
      currentScreen: dashboardRouteParts(dashboardRoute)?.screen || "overview", configVersion: "v1", resolution: terminalEnrollment.resolution, sync: "Синхронизирован",
    };
    kioskTerminals.push(terminal);
    terminalEnrollment.status = "Активен";
    terminalEnrollment.approvedTerminalId = terminal.id;
    addAdminAudit("Зарегистрирован терминал", terminal.name, "Успех", "terminal", `${terminal.deviceIdentity}; профиль ${profile.id}; дашборд ${dashboardRoute}; одноразовый код погашен.`);
    render();
    toast("Терминал зарегистрирован", `${terminal.id}: создана отдельная device identity.`);
  }

  function showAdminTerminalRebind(id) {
    const terminal = kioskTerminal(id);
    if (!currentUser.isAdmin || !terminal || terminal.status === "Отозван") return;
    openModal("Изменить назначение терминала", `<div class="preflight"><div class="preflight-row"><span>Терминал</span><strong>${escapeHtml(terminal.name)}</strong></div><div class="preflight-row"><span>Device identity</span><strong>${escapeHtml(terminal.deviceIdentity)}</strong></div></div><label class="form-label">Общий профиль<select id="admin-terminal-profile" class="select">${kioskProfiles.map((profile) => `<option value="${profile.id}" ${profile.id === terminal.profileId ? "selected" : ""}>${escapeHtml(profile.name)} · ${escapeHtml(profile.authLabel)}</option>`).join("")}</select></label><label class="form-label">Стартовый дашборд<select id="admin-terminal-dashboard" class="select">${dashboards.map((dashboard) => { const route = `/d/${dashboard.id}/overview`; return `<option value="${route}" ${route === terminal.dashboardRoute ? "selected" : ""}>${escapeHtml(dashboard.name)}</option>`; }).join("")}</select></label><p class="muted">Профиль безопасности и стартовый дашборд назначаются независимо. Пользовательская учётная запись панели не создаётся.</p>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="admin-terminal-rebind-confirm" data-id="${terminal.id}">Применить</button>`);
  }

  function applyAdminTerminalRebind(id) {
    const terminal = kioskTerminal(id);
    const profile = kioskProfile(document.getElementById("admin-terminal-profile")?.value);
    const dashboardRoute = document.getElementById("admin-terminal-dashboard")?.value || "";
    if (!currentUser.isAdmin || !terminal || terminal.status === "Отозван" || !profile || !isValidDashboardRoute(dashboardRoute)) { closeModal(); return; }
    const previous = `${terminal.profileId} · ${terminal.dashboardRoute}`;
    terminal.profileId = profile.id;
    terminal.dashboardRoute = dashboardRoute;
    terminal.currentScreen = dashboardRouteParts(dashboardRoute)?.screen || "overview";
    terminal.configVersion = `v${Number(terminal.configVersion.replace(/\D/g, "")) + 1}`;
    terminal.sync = terminal.online ? "Синхронизирован" : "Ожидает связи";
    addAdminAudit("Изменено назначение терминала", terminal.name, "Успех", "terminal", `${previous} → ${profile.id} · ${dashboardRoute}; device identity не изменялась.`);
    closeModal();
    render();
    toast("Назначение изменено", `${terminal.name}: ${profile.name}.`);
  }

  function showAdminTerminalStatePreview(id, nextStatus) {
    const terminal = kioskTerminal(id);
    if (!currentUser.isAdmin || !terminal || terminal.status === "Отозван" || !["Активен", "Заблокирован", "Отозван"].includes(nextStatus)) return;
    const destructive = nextStatus === "Отозван";
    openModal(destructive ? "Отозвать device identity?" : `${nextStatus === "Активен" ? "Разблокировать" : "Заблокировать"} терминал?`, `<div class="preflight"><div class="preflight-row"><span>Терминал</span><strong>${escapeHtml(terminal.name)}</strong></div><div class="preflight-row"><span>Было</span><strong>${escapeHtml(terminal.status)}</strong></div><div class="preflight-row"><span>Станет</span><strong>${escapeHtml(nextStatus)}</strong></div></div><p class="muted">Общий профиль и другие панели не изменятся. Активный kiosk runtime этой identity будет закрыт.</p>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button ${destructive ? "danger" : "primary"}" data-action="admin-terminal-state-confirm" data-id="${terminal.id}" data-value="${escapeHtml(nextStatus)}">Подтвердить</button>`);
  }

  function applyAdminTerminalState(id, nextStatus) {
    const terminal = kioskTerminal(id);
    if (!currentUser.isAdmin || !terminal || terminal.status === "Отозван" || !["Активен", "Заблокирован", "Отозван"].includes(nextStatus)) { closeModal(); return; }
    const previous = terminal.status;
    terminal.status = nextStatus;
    if (nextStatus === "Отозван") terminal.online = false;
    terminal.sync = nextStatus === "Активен" ? (terminal.online ? "Синхронизирован" : "Ожидает связи") : nextStatus;
    if (state.kioskControlTerminalId === terminal.id) clearKioskControl();
    addAdminAudit(nextStatus === "Отозван" ? "Отозвана device identity" : nextStatus === "Активен" ? "Разблокирован терминал" : "Заблокирован терминал", terminal.name, "Успех", "terminal", `${previous} → ${nextStatus}; общий профиль не изменён.`);
    closeModal();
    render();
    toast("Состояние терминала изменено", `${terminal.name}: ${nextStatus}.`);
  }

  function clearKioskControl() {
    const hadPendingCommand = Boolean(state.kioskPendingCommand);
    clearInterval(state.kioskControlTimer);
    state.kioskControlTimer = null;
    state.kioskControlTerminalId = null;
    state.kioskControlRemaining = 0;
    state.kioskControlExpiresAt = 0;
    state.kioskAuthenticatedEmployee = "";
    state.kioskPendingCommand = null;
    if (hadPendingCommand) modalRoot.innerHTML = "";
  }

  function kioskControlIsActive(context = kioskRuntimeContext()) {
    const remaining = Math.ceil((state.kioskControlExpiresAt - Date.now()) / 1000);
    state.kioskControlRemaining = Math.max(0, remaining);
    return Boolean(context?.terminal && context.profile && context.assignmentValid && context.terminal.status === "Активен" && context.terminal.online && state.kioskControlTerminalId === context.terminal.id && remaining > 0);
  }

  function kioskCommandForProfile(profile, commandId) {
    const command = kioskCommandDefinitions[commandId];
    return command && profile?.allowedActions.includes(command.capability) ? command : null;
  }

  function showKioskControlRequest() {
    const context = kioskRuntimeContext();
    const { terminal, profile, assignmentValid } = context || {};
    if (!terminal || !profile || !assignmentValid || terminal.status !== "Активен" || !terminal.online || profile.authPolicy === "deny") return;
    const needsPin = profile.authPolicy === "employee-pin";
    openModal("Включить режим управления", `<div class="preflight"><div class="preflight-row"><span>Область</span><strong>${escapeHtml(terminal.name)} · назначенный дашборд</strong></div><div class="preflight-row"><span>Timeout</span><strong>${profile.controlTimeout} с · профиль терминала</strong></div><div class="preflight-row"><span>Effective policy</span><strong>${escapeHtml(profile.authLabel)}</strong></div><div class="preflight-row"><span>Аудит</span><strong>${needsPin ? "Device identity + сотрудник" : `${escapeHtml(terminal.deviceIdentity)} · без персонального исполнителя`}</strong></div></div>${needsPin ? `<label class="form-label">PIN сотрудника<input id="kiosk-employee-pin" class="field" type="password" inputmode="numeric" maxlength="4" autocomplete="off" placeholder="4 цифры" aria-describedby="kiosk-auth-help kiosk-auth-error" aria-invalid="false"></label><div id="kiosk-auth-error" role="alert" aria-live="assertive" style="color:var(--red);margin-top:6px"></div><p id="kiosk-auth-help" class="muted">В прототипе любые 4 цифры имитируют подтверждённого сотрудника без присвоения реального имени; IAM не подключён.</p>` : `<p class="muted">Разрешены только ограниченные команды профиля. Прототип не отправляет физические команды.</p>`}`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="kiosk-control-confirm">Включить</button>`);
  }

  function startKioskControl() {
    const context = kioskRuntimeContext();
    const { terminal, profile, assignmentValid } = context || {};
    if (!terminal || !profile || !assignmentValid || terminal.status !== "Активен" || !terminal.online || profile.authPolicy === "deny") { closeModal(); return; }
    if (profile.authPolicy === "employee-pin") {
      const input = document.getElementById("kiosk-employee-pin");
      const pin = input?.value || "";
      if (!/^\d{4}$/.test(pin)) { input?.setAttribute?.("aria-invalid", "true"); const error = document.getElementById("kiosk-auth-error"); if (error) error.textContent = "Введите 4 цифры."; return; }
      state.kioskAuthenticatedEmployee = "Сотрудник подтверждён (демо)";
    } else state.kioskAuthenticatedEmployee = "";
    clearInterval(state.kioskControlTimer);
    state.kioskControlTerminalId = terminal.id;
    state.kioskControlRemaining = profile.controlTimeout;
    state.kioskControlExpiresAt = Date.now() + profile.controlTimeout * 1000;
    closeModal();
    render();
    state.kioskControlTimer = setInterval(() => {
      state.kioskControlRemaining = Math.max(0, Math.ceil((state.kioskControlExpiresAt - Date.now()) / 1000));
      const node = document.querySelector(".kiosk-control-time");
      if (node) node.textContent = `Управление ${formatTimer(state.kioskControlRemaining)}`;
      if (state.kioskControlRemaining <= 0) stopKioskControl("Режим завершён по timeout");
    }, 1000);
    toast("Управление включено", `${terminal.name}: только действия effective profile.`);
  }

  function stopKioskControl(message = "Режим управления выключен", notify = true) {
    clearKioskControl();
    render();
    if (notify) toast("Только просмотр", message);
  }

  function showKioskCommandPreview(commandId) {
    const context = kioskRuntimeContext();
    const { terminal, profile, assignmentValid } = context || {};
    const command = kioskCommandForProfile(profile, commandId);
    if (!terminal || !profile || !assignmentValid || !command || !kioskControlIsActive(context)) return;
    state.kioskPendingCommand = { terminalId: terminal.id, profileId: profile.id, dashboardRoute: terminal.dashboardRoute, configVersion: terminal.configVersion, commandId };
    const actor = state.kioskAuthenticatedEmployee ? `${state.kioskAuthenticatedEmployee} · ${terminal.deviceIdentity}` : `${terminal.deviceIdentity} · без персонального исполнителя`;
    openModal("Предпросмотр команды", `<p>Команда подготовлена, но не будет отправлена оборудованию.</p><div class="preflight"><div class="preflight-row"><span>Терминал</span><strong>${escapeHtml(terminal.name)} · ${escapeHtml(terminal.deviceIdentity)}</strong></div><div class="preflight-row"><span>Команда</span><strong>${escapeHtml(command.label)}</strong></div><div class="preflight-row"><span>Область</span><strong>${escapeHtml(kioskDashboardName(terminal.dashboardRoute))}</strong></div><div class="preflight-row"><span>Аудит</span><strong>${escapeHtml(actor)}</strong></div></div>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="kiosk-command-confirm">Подтвердить в прототипе</button>`);
  }

  function confirmKioskCommand() {
    const pending = state.kioskPendingCommand;
    const context = kioskRuntimeContext();
    const { terminal, profile, assignmentValid } = context || {};
    const command = pending ? kioskCommandForProfile(profile, pending.commandId) : null;
    const valid = Boolean(pending && terminal && profile && assignmentValid && command && kioskControlIsActive(context) && terminal.id === pending.terminalId && terminal.profileId === pending.profileId && terminal.dashboardRoute === pending.dashboardRoute && terminal.configVersion === pending.configVersion);
    if (!valid) {
      state.kioskPendingCommand = null;
      closeModal();
      toast("Команда отклонена", "Состояние терминала, назначение или время управления изменилось. Откройте preview заново.");
      return;
    }
    state.kioskPendingCommand = null;
    closeModal();
    toast("Команда не отправлена", "Проверен только kiosk preview; значения оборудования не изменены.");
  }

  function showAdminAssignmentPreview(id) {
    const account = adminAccounts.find((item) => item.id === id);
    if (!currentUser.isAdmin || !account) return;
    if (account.lastScopeAdmin) {
      openModal("Изменение заблокировано", `<div class="policy-card"><strong>Последний администратор области</strong><div class="policy-meta">Сначала назначьте другого администратора организации. Система не допускает область без ответственного администратора.</div></div><div class="preflight"><div class="preflight-row"><span>Учётная запись</span><strong>${escapeHtml(account.name)}</strong></div><div class="preflight-row"><span>Текущая роль</span><strong>${escapeHtml(account.role)}</strong></div></div>`, `<button class="button primary" data-action="close-modal">Закрыть</button>`);
      return;
    }
    openModal("Изменить роль и область", `<div class="preflight"><div class="preflight-row"><span>Учётная запись</span><strong>${escapeHtml(account.name)}</strong></div><div class="preflight-row"><span>Текущее назначение</span><strong>${escapeHtml(account.role)} · ${escapeHtml(account.scope)}</strong></div></div><label class="form-label">Роль<select id="admin-assignment-role" class="select"><option ${account.role === "Диспетчер" ? "selected" : ""}>Диспетчер</option><option ${account.role === "Инженер" ? "selected" : ""}>Инженер</option><option>Администратор локации</option><option ${account.role === "Наблюдатель" ? "selected" : ""}>Наблюдатель</option></select></label><label class="form-label">Область<select id="admin-assignment-scope" class="select"><option ${account.scope === "Главный комплекс" ? "selected" : ""}>Главный комплекс</option><option ${account.scope.includes("Корпус A") ? "selected" : ""}>Корпус A · ОВиК</option><option ${account.scope.includes("Корпус C") ? "selected" : ""}>Корпус C · ИТП</option></select></label><div class="policy-card"><strong>Effective-permissions preview</strong><div class="policy-meta">Будут пересчитаны доступные сервисы, действия и область объектов. Должность сотрудника не изменяется.</div></div>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="admin-assignment-confirm" data-id="${account.id}">Применить назначение</button>`);
  }

  function applyAdminAssignment(id) {
    const account = adminAccounts.find((item) => item.id === id);
    if (!currentUser.isAdmin || !account || account.lastScopeAdmin) { closeModal(); return; }
    const role = document.getElementById("admin-assignment-role")?.value || account.role;
    const scope = document.getElementById("admin-assignment-scope")?.value || account.scope;
    const previous = `${account.role} · ${account.scope}`;
    account.role = role;
    account.scope = scope;
    account.permissionSource = `Прямое назначение · ${currentUser.name}`;
    const preset = role === "Диспетчер"
      ? { services: ["Главная и дашборды", "Диспетчер событий", "Оборудование"], actions: ["Просмотр", "Acknowledgement", "Управление по политике"] }
      : role === "Инженер"
        ? { services: ["Дашборды области", "Оборудование", "ТОиР и ППР"], actions: ["Просмотр", "Диагностика", "Выполнение нарядов"] }
        : role === "Администратор локации"
          ? { services: ["Эксплуатационные сервисы области", "Пользователи области", "Системные настройки области"], actions: ["Назначение локальных прав", "Публикация", "Диагностика"] }
          : { services: ["Назначенные дашборды"], actions: ["Только просмотр"] };
    account.services = preset.services;
    account.actions = preset.actions;
    addAdminAudit("Изменено назначение роли", account.name, "Успех", "access", `${previous} → ${role} · ${scope}. Effective permissions пересчитаны в прототипе.`);
    closeModal();
    render();
    toast("Назначение изменено", `${account.name}: ${role} · ${scope}.`);
  }

  function showAdminSettingPreview(reset = false) {
    if (!currentUser.isAdmin) return;
    const from = state.adminSettingOverride || "3 минуты · наследуется";
    const to = reset ? "3 минуты · базовая политика" : "5 минут · override организации";
    openModal(reset ? "Вернуть наследуемое значение?" : "Применить override?", `<div class="preflight"><div class="preflight-row"><span>Параметр</span><strong>Время режима управления</strong></div><div class="preflight-row"><span>Было</span><strong>${escapeHtml(from)}</strong></div><div class="preflight-row"><span>Станет</span><strong>${escapeHtml(to)}</strong></div><div class="preflight-row"><span>Затронуто</span><strong>8 операторских рабочих мест</strong></div><div class="preflight-row"><span>Исключения</span><strong>1 kiosk-профиль не изменится</strong></div></div>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="${reset ? "admin-setting-reset-confirm" : "admin-setting-apply"}">${reset ? "Вернуть наследование" : "Применить override"}</button>`);
  }

  function applyAdminSetting(reset = false) {
    if (!currentUser.isAdmin) { closeModal(); return; }
    state.adminSettingOverride = reset ? "" : "5 минут";
    addAdminAudit(reset ? "Удалено локальное переопределение" : "Изменена системная политика", "Время режима управления", "Успех", "configuration", reset ? "Организация снова наследует базовое значение 3 минуты." : "Применён override 5 минут после impact preview; одно исключение сохранено.");
    closeModal();
    render();
    toast("Системная политика обновлена", reset ? "Восстановлено наследование базовой политики." : "Override действует для организации.");
  }

  function testAdminIntegration(id) {
    const integration = adminIntegrations.find((item) => item.id === id);
    if (!currentUser.isAdmin || !integration || integration.status === "Выключено") return;
    const result = integration.status === "Деградация" ? "Предупреждение: задержка сохраняется" : "Проверено: доступно";
    state.adminConnectionTests[id] = result;
    addAdminAudit("Диагностика подключения", integration.name, integration.status === "Деградация" ? "Предупреждение" : "Успех", "integration", `${result}. Реальный сетевой вызов и передача секретов не выполнялись.`);
    render();
    toast("Информационная проверка завершена", `${integration.name}: ${result}.`);
  }

  function assignAdminDataQuality(id) {
    const item = dataQualityIssues.find((entry) => entry.id === id);
    if (!currentUser.isAdmin || !item || item.assignedTo === currentUser.name) return;
    item.assignedTo = currentUser.name;
    item.status = "Назначено";
    addAdminAudit("Назначена проблема качества данных", item.id, "Успех", "data-quality", `${item.title}; alarm occurrence и acknowledgement не изменялись.`);
    render();
    toast("Проблема назначена", `${item.id} добавлена в ответственность ${currentUser.name}.`);
  }

  function showControlModal() {
    const dashboard = isValidDashboardRoute(state.route) ? currentDashboard() : null;
    if (dashboard && state.dashboardMode === "history") {
      openModal("Управление недоступно", `<p>Исторический режим предназначен только для анализа. Для подготовки команды вернитесь в Live.</p>`, `<button class="button primary" data-action="dashboard-mode" data-value="live">Вернуться в Live</button>`);
      return;
    }
    const scope = dashboard?.name || state.location;
    const durationMinutes = state.adminSettingOverride ? 5 : 3;
    openModal("Включить режим управления", `<p>На ${durationMinutes} минут будут доступны только разрешённые команды для выбранной области.</p><div class="preflight"><div class="preflight-row"><span>Область</span><strong>${escapeHtml(scope)}</strong></div><div class="preflight-row"><span>Политика времени</span><strong>${state.adminSettingOverride ? "Override организации" : "Базовая"}</strong></div><div class="preflight-row"><span>Аудит</span><strong>Алексей Смирнов</strong></div><div class="preflight-row"><span>Повторная авторизация</span><strong>Не требуется для демо</strong></div></div><p class="muted">Прототип не отправляет физические команды.</p>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="enable-control">Включить</button>`);
  }

  function startControl() {
    const durationMinutes = state.adminSettingOverride ? 5 : 3;
    state.controlEnabled = true; state.controlRemaining = durationMinutes * 60; state.controlScopeDashboardId = isValidDashboardRoute(state.route) ? currentDashboardId() : null; closeModal(); render();
    clearInterval(state.controlTimer);
    state.controlTimer = setInterval(() => {
      state.controlRemaining -= 1;
      document.querySelectorAll(".control-time").forEach((node) => { node.textContent = formatTimer(Math.max(0, state.controlRemaining)); });
      if (state.controlRemaining <= 0) stopControl("Режим управления завершён по timeout");
    }, 1000);
    toast("Управление включено", `Режим автоматически завершится через ${durationMinutes} минут.`);
  }

  function stopControl(message = "Режим управления выключен") {
    state.controlEnabled = false; state.controlRemaining = 0; state.controlScopeDashboardId = null; clearInterval(state.controlTimer); state.controlTimer = null; render(); toast("Режим просмотра", message);
  }

  function showCommandPreview(id = "") {
    const dashboardItem = id ? dashboardObjects[id] : null;
    const item = dashboardItem || equipment.find((entry) => entry.id === id) || null;
    const dashboardContext = isValidDashboardRoute(state.route);
    if (dashboardContext && state.dashboardMode === "history") {
      openModal("Команда недоступна в History", `<p>Исторический режим не допускает технологических команд. Выбранный объект и период сохранены.</p>`, `<button class="button" data-action="close-modal">Закрыть</button><button class="button primary" data-action="dashboard-mode" data-value="live">Вернуться в Live</button>`);
      return;
    }
    if (dashboardContext && !dashboardItem) {
      openModal("Объект не выбран", `<p>Сначала явно выберите объект на мнемосхеме или в окне «Параметры П1».</p><p class="muted">Первый клик только задаёт контекст и не выполняет команду.</p>`, `<button class="button primary" data-action="close-modal">Понятно</button>`);
      return;
    }
    if (!item) {
      openModal("Объект не выбран", `<p>Команда не может быть подготовлена без явно указанного объекта.</p>`, `<button class="button primary" data-action="close-modal">Закрыть</button>`);
      return;
    }
    if (dashboardContext && state.controlEnabled && state.controlScopeDashboardId !== currentDashboardId()) {
      state.controlEnabled = false; state.controlRemaining = 0; state.controlScopeDashboardId = null; clearInterval(state.controlTimer); state.controlTimer = null;
    }
    if (!state.controlEnabled) {
      openModal("Управление выключено", `<p>Сначала включите временный режим управления. Это защищает от случайного воздействия.</p><p class="muted">Выбранный объект: ${escapeHtml(item.name)}</p>`, `<button class="button" data-action="close-modal">Закрыть</button><button class="button primary" data-action="enable-control">Включить управление</button>`);
      return;
    }
    openModal("Предпросмотр команды", `<p>Команда подготовлена, но в прототипе не будет отправлена оборудованию.</p><div class="preflight"><div class="preflight-row"><span>Объект</span><strong>${escapeHtml(item.name)} · ${escapeHtml(item.id)}</strong></div><div class="preflight-row"><span>Команда</span><strong>${escapeHtml(dashboardItem?.command || "Перевести в автоматический режим")}</strong></div><div class="preflight-row"><span>Связь / качество</span><strong>${escapeHtml(dashboardItem ? "На связи" : item.connection)} / ${escapeHtml(item.quality)}</strong></div><div class="preflight-row"><span>Область управления</span><strong>${escapeHtml(dashboardContext ? currentDashboard().name : state.location)}</strong></div><div class="preflight-row"><span>Исполнитель</span><strong>Алексей Смирнов</strong></div></div>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="simulate-command">Подтвердить в прототипе</button>`);
  }

  function showGlobalSearch() {
    openModal("Поиск и быстрые переходы", `<input class="field" style="width:100%" placeholder="Оборудование, экран, событие, сотрудник или сервис" aria-label="Поиск"><div class="search-results"><button class="search-result" data-nav="/d/main/overview">${icon("dashboard")}<div><strong>Климат · Корпус A</strong><div class="row-meta">Дашборд · Установка П1</div></div></button><button class="search-result" data-action="open-equipment" data-id="AHU-P1">${icon("equipment")}<div><strong>Приточная установка П1</strong><div class="row-meta">Оборудование · Корпус A</div></div></button><button class="search-result" data-action="open-event" data-id="EV-001">${icon("alert")}<div><strong>Обнаружен дым в переговорной 4.12</strong><div class="row-meta">Критическая авария · EV-001</div></div></button><button class="search-result" data-nav="/users/anna">${icon("user")}<div><strong>Анна Волкова</strong><div class="row-meta">Руководитель эксплуатации · доступные рабочие сведения</div></div></button></div>`);
  }

  function showAckModal(id) {
    const item = events.find((entry) => entry.id === id);
    openModal("Подтвердить аварийное состояние", `<p>${escapeHtml(item.message)}</p><label class="form-label">Комментарий ${item.ackNoteRequired ? "· обязателен по политике" : "· необязательно"}<textarea id="ack-note" placeholder="Что проверено оператором"></textarea></label><div id="ack-error" style="color:var(--red);margin-top:6px"></div>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="confirm-ack" data-id="${id}">Подтвердить</button>`);
  }

  function showShelveModal(id) {
    openModal("Временно отложить реакцию", `<label class="form-label">Причина<textarea id="shelve-reason" placeholder="Обязательная причина"></textarea></label><label class="form-label">До<select id="shelve-until" class="select"><option>15:30</option><option>16:00</option><option>18:00</option></select></label>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="confirm-shelve" data-id="${id}">Shelve</button>`);
  }

  function showCreateIncidentModal(id) {
    if (!requireRealtimeEventAction()) return;
    const item = events.find((entry) => entry.id === id);
    if (!item) return;
    if (item.incidentId) { showIncidentSummary(item.incidentId); return; }
    const similar = incidents.find((incident) => incident.location === item.location && incident.status !== "Закрыт");
    openModal("Создать инцидент", `<p>Событие и текущий контекст будут приложены автоматически. Acknowledgement, assignment и состояние условия не изменятся.</p><div class="preflight"><div class="preflight-row"><span>Событие</span><strong>${escapeHtml(item.id)}</strong></div><div class="preflight-row"><span>Приоритет</span><strong>${escapeHtml(item.severity === "critical" ? "Критический" : item.severity === "high" ? "Высокий" : "Средний")}</strong></div><div class="preflight-row"><span>Локация / источник</span><strong>${escapeHtml(item.location)} · ${escapeHtml(item.source)}</strong></div><div class="preflight-row"><span>Похожие активные</span><strong>${similar ? escapeHtml(similar.id) : "Не найдены"}</strong></div></div><label class="form-label">Координатор<select id="incident-coordinator" class="select"><option>Алексей Смирнов</option><option>Дежурная смена</option><option>Не назначен</option></select></label>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="confirm-create-incident" data-id="${item.id}">Создать</button>`);
  }

  function nextTaskId() {
    const nextNumber = tasks.reduce((maximum, task) => Math.max(maximum, Number(task.id.split("-")[1]) || 0), 0) + 1;
    return `T-${String(nextNumber).padStart(3, "0")}`;
  }

  function createIncidentFromEvent(id) {
    if (!requireRealtimeEventAction()) return;
    const item = events.find((entry) => entry.id === id);
    if (!item || item.incidentId) return;
    const nextNumber = incidents.reduce((maximum, incident) => Math.max(maximum, Number(incident.id.split("-")[1]) || 0), 0) + 1;
    const incident = {
      id: `INC-${String(nextNumber).padStart(3, "0")}`,
      title: item.message,
      priority: item.severity === "critical" ? "Критический" : item.severity === "high" ? "Высокий" : "Средний",
      status: "Новый",
      location: item.location,
      coordinator: document.getElementById("incident-coordinator")?.value || "Не назначен",
      created: "14:32",
      eventIds: [item.id],
    };
    incidents.push(incident);
    item.incidentId = incident.id;
    item.timeline.unshift(["14:32", `Создан инцидент ${incident.id}; состояние события не изменено`]);
    const taskCreated = incident.coordinator === currentUser.name;
    if (taskCreated) tasks.push({ id: nextTaskId(), type: "Инцидент", action: "Принять координацию", object: `${incident.id} · ${item.source}`, location: item.location, priority: incident.priority, due: "Сейчас", status: "Назначено", from: "Диспетчер событий", assignedToMe: true, requiresDecision: true, eventId: item.id, incidentId: incident.id, timeline: [["14:32", `Создано при назначении координатором ${currentUser.name}`]] });
    closeModal();
    state.drawer = { kind: "events", id: item.id };
    state.drawerTab = "details";
    render();
    toast("Инцидент создан", `${incident.id} связан с ${item.id}${taskCreated ? " и добавлен в «Мою работу»" : ""}.`);
  }

  function createWorkRequestFromEvent(id) {
    if (!requireRealtimeEventAction()) return;
    const item = events.find((entry) => entry.id === id);
    if (!item) return;
    const existing = maintenanceRequests.find((request) => request.eventId === item.id);
    if (existing) { toast("Заявка уже создана", `${existing.id} доступна в разделе ТОиР.`); return; }
    const nextNumber = maintenanceRequests.reduce((maximum, request) => Math.max(maximum, Number(request.id.split("-")[1]) || 0), 100) + 1;
    const request = { id: `MR-${nextNumber}`, type: "Заявка", object: item.source, location: item.location, description: item.message, priority: item.severity === "critical" ? "Критический" : item.severity === "high" ? "Высокий" : "Средний", status: "Новая", source: item.id, eventId: item.id };
    maintenanceRequests.push(request);
    item.maintenanceRequestId = request.id;
    const task = { id: nextTaskId(), type: "Заявка", action: "Организовать проверку на объекте", object: `${item.id} · ${item.source}`, location: item.location, priority: request.priority, due: "Сегодня · 18:00", status: "Назначено", from: "Диспетчер событий", assignedToMe: true, today: true, eventId: item.id, equipmentId: item.equipmentId, requestId: request.id, timeline: [["14:32", `Создано из ${item.id}; состояние события не изменено`]] };
    tasks.push(task);
    item.timeline.unshift(["14:32", `Создана заявка ${request.id}; acknowledgement не изменён`]);
    render();
    toast("Заявка создана", `${request.id} добавлена в ТОиР, задача ${task.id} — в «Мою работу».`);
  }

  function showIncidentSummary(id) {
    const incident = incidents.find((entry) => entry.id === id);
    if (!incident) return;
    openModal(`Инцидент ${incident.id}`, `<div class="preflight"><div class="preflight-row"><span>Состояние</span><strong>${escapeHtml(incident.status)}</strong></div><div class="preflight-row"><span>Приоритет</span><strong>${escapeHtml(incident.priority)}</strong></div><div class="preflight-row"><span>Координатор</span><strong>${escapeHtml(incident.coordinator)}</strong></div><div class="preflight-row"><span>Локация</span><strong>${escapeHtml(incident.location)}</strong></div><div class="preflight-row"><span>Связанные события</span><strong>${incident.eventIds.map(escapeHtml).join(", ")}</strong></div></div><p class="muted">В этой итерации показана только связь события с отдельным процессом реагирования. Полная рабочая область инцидента будет разработана позднее.</p>`, `<button class="button primary" data-action="close-modal">Закрыть</button>`);
  }

  function showBulkPreview() {
    if (!requireRealtimeEventAction()) return;
    const chosen = events.filter((item) => state.selectedEvents.has(item.id));
    const eligible = chosen.filter((item) => item.ack === false);
    openModal("Предпросмотр массового подтверждения", `<p>Область явно ограничена выбранными строками.</p><div class="preflight"><div class="preflight-row"><span>Выбрано</span><strong>${chosen.length}</strong></div><div class="preflight-row"><span>Допустимо</span><strong style="color:var(--green)">${eligible.length}</strong></div><div class="preflight-row"><span>Недопустимо</span><strong style="color:var(--amber)">${chosen.length - eligible.length}</strong></div></div><p class="muted">Уже подтверждённые и точечные события будут пропущены; результат сохранит частичный статус.</p>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="apply-bulk-ack">Подтвердить допустимые</button>`);
  }

  function showTaskModal(kind, id) {
    const item = tasks.find((entry) => entry.id === id);
    if (!item || !isTaskActionable(item)) { toast("Действие недоступно", "Задача больше не назначена текущему пользователю."); return; }
    if (kind === "transfer") openModal("Передать задачу", `<p>${escapeHtml(item.action)}</p><label class="form-label">Новый исполнитель<select id="task-assignee" class="select"><option value="">Выберите исполнителя</option><option>Сергей Петров</option><option>Дежурная смена</option><option>Служба ОВиК</option></select></label><div id="task-error" style="color:var(--red);margin-top:6px"></div>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="confirm-transfer" data-id="${id}">Передать</button>`);
    else openModal("Не могу выполнить", `<p>${escapeHtml(item.action)}</p><label class="form-label">Причина<textarea id="task-reason" placeholder="Причина обязательна"></textarea></label><div id="task-error" style="color:var(--red);margin-top:6px"></div>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button danger" data-action="confirm-cannot" data-id="${id}">Вернуть задачу</button>`);
  }

  function showNewSubscriptionModal() {
    openModal("Новая личная подписка", `<p>Подписка только дополняет обязательные правила.</p><div class="settings-form"><label class="form-label">Где<select id="subscription-where" class="select"><option>Главный комплекс</option><option>Корпус A</option><option>Все локации</option></select></label><label class="form-label">Что<select id="subscription-what" class="select"><option>Аварии</option><option>События</option><option>Работы ППР</option></select></label><label class="form-label">Условие<select id="subscription-condition" class="select"><option>Средняя важность и выше</option><option>Высокая важность</option><option>Ежедневная сводка</option></select></label><label class="form-label">Когда<select id="subscription-when" class="select"><option>По личному графику</option><option>Всегда</option></select></label><label class="form-label">Канал<select id="subscription-channel" class="select"><option>Push</option><option>Email</option></select></label></div>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button primary" data-action="confirm-new-subscription">Добавить</button>`);
  }

  function createSubscriptionFromModal() {
    const where = document.getElementById("subscription-where")?.value || "Главный комплекс";
    const what = document.getElementById("subscription-what")?.value || "Аварии";
    const condition = document.getElementById("subscription-condition")?.value || "Средняя важность и выше";
    const when = document.getElementById("subscription-when")?.value || "По личному графику";
    const channel = document.getElementById("subscription-channel")?.value || "Push";
    const id = `personal-${Date.now()}`;
    subscriptions.push({ id, title: `${what} · ${condition}`, meta: `${where} · ${channel} · ${when}` });
    state.subscriptions[id] = true;
    closeModal();
    render();
    toast("Подписка добавлена", "Обязательные правила не изменены.");
  }

  document.addEventListener("click", (event) => {
    const navTarget = event.target.closest("[data-nav]");
    if (navTarget) { closeModal(); navigate(navTarget.dataset.nav); return; }
    const closedHeaderMenu = Boolean(state.headerMenu && !event.target.closest(".header-menu-anchor"));
    if (closedHeaderMenu) state.headerMenu = null;
    const target = event.target.closest("[data-action]");
    if (!target) { if (closedHeaderMenu) render(); return; }
    const action = target.dataset.action;
    const id = target.dataset.id;
    const route = target.dataset.route;
    if (action === "close-modal" && target.classList.contains("modal-backdrop") && event.target !== target) return;

    if (action === "toggle-nav") { state.navOpen = !state.navOpen; render(); }
    else if (action === "cycle-location") { state.location = state.location === "Главный комплекс" ? "ЦОД" : "Главный комплекс"; render(); toast("Контекст изменён", state.location); }
    else if (action === "global-search") showGlobalSearch();
    else if (action === "go-dashboards") {
      navigate(validLastDashboardRoute() || "/dashboards");
    }
    else if (action === "open-dashboard") {
      state.dashboardCatalogScroll = document.querySelector(".dashboard-catalog-page")?.scrollTop || 0;
      const route = `/d/${id}/overview`;
      rememberDashboardRoute(route);
      navigate(route);
    }
    else if (action === "dashboard-windows") { state.drawer = { kind: "dashboard-windows" }; render(); }
    else if (action === "dashboard-mode") { closeModal(); state.dashboardMode = target.dataset.value === "history" ? "history" : "live"; render(); }
    else if (action === "dashboard-select-object") {
      if (!dashboardObjects[id]) return;
      state.dashboardSelectedObjectId = id;
      state.drawer = state.workFullscreen ? null : { kind: "dashboard-object", id };
      render();
    }
    else if (action === "open-dashboard-faceplate") { if (!dashboardObjects[id]) return; state.dashboardSelectedObjectId = id; state.drawer = { kind: "dashboard-object", id }; render(); }
    else if (action === "dashboard-filter") { state.dashboardFilter = target.dataset.value; state.dashboardCatalogScroll = 0; render(); }
    else if (action === "toggle-dashboard-favorite") { state.favoriteDashboards.has(id) ? state.favoriteDashboards.delete(id) : state.favoriteDashboards.add(id); safeSet("dispatcher.prototype.v2.favoriteDashboards", JSON.stringify([...state.favoriteDashboards])); render(); }
    else if (action === "toggle-home-widget") { if (!homeWidgetDefinitions.some((item) => item.id === id)) return; state.homeWidgets.has(id) ? state.homeWidgets.delete(id) : state.homeWidgets.add(id); safeSet("dispatcher.prototype.v3.homeWidgets", JSON.stringify([...state.homeWidgets])); render(); }
    else if (action === "reset-home-widgets") { state.homeWidgets = new Set(defaultHomeWidgets); safeSet("dispatcher.prototype.v3.homeWidgets", JSON.stringify(defaultHomeWidgets)); render(); toast("Компоновка восстановлена", "Применён исходный Рабочий стол организации."); }
    else if (action === "toggle-user-menu") { state.headerMenu = state.headerMenu === "user" ? null : "user"; render(); }
    else if (action === "toggle-personal-notifications") { state.headerMenu = state.headerMenu === "notifications" ? null : "notifications"; render(); }
    else if (action === "mark-personal-notification") { markPersonalNotificationRead(id); render(); }
    else if (action === "mark-all-personal-notifications") { personalNotifications.forEach((item) => state.readPersonalNotifications.add(item.id)); persistPersonalNotificationReadState(); render(); toast("Личные оповещения прочитаны", "События и аварии не подтверждены."); }
    else if (action === "open-personal-notification-event") { markPersonalNotificationRead(id); const eventId = target.dataset.eventId; if (!events.some((item) => item.id === eventId)) { toast("Источник недоступен", "Событие не найдено или нет права просмотра."); return; } state.drawer = { kind: "events", id: eventId }; state.drawerTab = "details"; navigate("/events", true); }
    else if (action === "open-personal-notification-equipment") { markPersonalNotificationRead(id); const equipmentId = target.dataset.equipmentId; if (!equipment.some((item) => item.id === equipmentId)) { toast("Источник недоступен", "Устройство не найдено или нет права просмотра."); return; } state.drawer = { kind: "equipment", id: equipmentId }; navigate("/equipment", true); }
    else if (action === "open-personal-notification-dashboard") { markPersonalNotificationRead(id); navigate(route); }
    else if (action === "open-personal-notification-history") { markPersonalNotificationRead(id); const tag = target.dataset.tag; if (!tagHistories[tag]) { toast("История недоступна", "История тега не существует или недоступна."); return; } navigate(`/trends/tag/${tag}`); }
    else if (action === "request-logout") { state.headerMenu = null; openModal("Выйти из Dispatcher?", `<p>Текущий демонстрационный сеанс будет завершён. Реальная аутентификация в прототипе не подключена.</p>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button danger" data-action="confirm-logout">Выйти</button>`); }
    else if (action === "confirm-logout") { closeModal(); toast("Демонстрационный выход", "В рабочей системе откроется экран входа."); }
    else if (action === "privacy-preview") { state.privacyPreview = target.dataset.value === "employee"; render(); }
    else if (action === "toggle-panel-autoclose") { state.closePanelOnNavigate = !state.closePanelOnNavigate; safeSet("dispatcher.prototype.v3.closePanelOnNavigate", String(state.closePanelOnNavigate)); render(); }
    else if (action === "save-profile-settings") { const person = people[0]; person.about = document.getElementById("profile-about")?.value.trim() || person.about; person.specialization = document.getElementById("profile-specialization")?.value.trim() || person.specialization; person.preferredContact = document.getElementById("profile-contact")?.value || person.preferredContact; safeSet("dispatcher.prototype.v3.profileAbout", person.about); safeSet("dispatcher.prototype.v3.profileSpecialization", person.specialization); safeSet("dispatcher.prototype.v3.profileContact", person.preferredContact); render(); toast("Профиль обновлён", "Изменены только разрешённые пользователю поля."); }
    else if (action === "save-interface-settings") toast("Настройки интерфейса сохранены", "Параметры применены к текущему пользователю.");
    else if (action === "save-system-settings") { if (!currentUser.isAdmin) return; toast("Системные параметры сохранены", "Демонстрационное изменение относится ко всей организации."); }
    else if (action === "admin-identity-view") { if (!currentUser.isAdmin) return; state.adminIdentityView = target.dataset.value === "roles" ? "roles" : "users"; state.drawer = null; render(); }
    else if (action === "open-admin-account") { if (!currentUser.isAdmin) return; state.drawer = { kind: "admin-account", id }; if (state.route !== "/admin/users") navigate("/admin/users", true); else render(); }
    else if (action === "open-admin-role") { if (!currentUser.isAdmin) return; state.drawer = { kind: "admin-role", id }; if (state.route !== "/admin/users") navigate("/admin/users", true); else render(); }
    else if (action === "open-admin-integration") { if (!currentUser.isAdmin) return; state.drawer = { kind: "admin-integration", id }; if (state.route !== "/admin/settings/connections") navigate("/admin/settings/connections", true); else render(); }
    else if (action === "open-admin-terminal") { if (!currentUser.isAdmin || !kioskTerminal(id)) return; state.drawer = { kind: "admin-terminal", id }; if (state.route !== "/admin/terminals") navigate("/admin/terminals", true); else render(); }
    else if (action === "open-admin-health") { if (!currentUser.isAdmin) return; state.drawer = { kind: "admin-health", id }; if (state.route !== "/admin/health") navigate("/admin/health", true); else render(); }
    else if (action === "open-admin-data-quality") { if (!currentUser.isAdmin) return; state.drawer = { kind: "admin-data-quality", id }; if (state.route !== "/admin/data-quality") navigate("/admin/data-quality", true); else render(); }
    else if (action === "open-admin-audit") { if (!currentUser.isAdmin) return; state.drawer = { kind: "admin-audit", id }; if (state.route !== "/admin/audit") navigate("/admin/audit", true); else render(); }
    else if (action === "admin-assignment-preview") showAdminAssignmentPreview(id);
    else if (action === "admin-assignment-confirm") applyAdminAssignment(id);
    else if (action === "admin-setting-preview") showAdminSettingPreview(false);
    else if (action === "admin-setting-apply") applyAdminSetting(false);
    else if (action === "admin-setting-reset") showAdminSettingPreview(true);
    else if (action === "admin-setting-reset-confirm") applyAdminSetting(true);
    else if (action === "admin-test-integration") testAdminIntegration(id);
    else if (action === "admin-terminal-enroll-confirm") approveTerminalEnrollment();
    else if (action === "admin-terminal-rebind") showAdminTerminalRebind(id);
    else if (action === "admin-terminal-rebind-confirm") applyAdminTerminalRebind(id);
    else if (action === "admin-terminal-state-preview") showAdminTerminalStatePreview(id, target.dataset.value);
    else if (action === "admin-terminal-state-confirm") applyAdminTerminalState(id, target.dataset.value);
    else if (action === "admin-health-filter") { if (!currentUser.isAdmin) return; state.adminHealthFilter = target.dataset.value; state.drawer = null; render(); }
    else if (action === "admin-assign-data-quality") assignAdminDataQuality(id);
    else if (action === "end-session") { state.endedSessions.add(id); render(); toast("Сеанс завершён", "Удалённый сеанс больше не активен."); }
    else if (action === "contact-person" || action === "mention-person" || action === "assign-person") { const person = people.find((item) => item.id === id); const titles = { "contact-person": "Рабочий контакт", "mention-person": "Упоминание подготовлено", "assign-person": "Назначение подготовлено" }; toast(titles[action], `${person?.name || "Сотрудник"}: показан демонстрационный UX-путь без отправки.`); }
    else if (action === "go-events") navigate("/events");
    else if (action === "kiosk-control-request") showKioskControlRequest();
    else if (action === "kiosk-control-confirm") startKioskControl();
    else if (action === "kiosk-control-stop") stopKioskControl();
    else if (action === "kiosk-command-preview") showKioskCommandPreview(target.dataset.commandId);
    else if (action === "kiosk-command-confirm") confirmKioskCommand();
    else if (action === "control-toggle") state.controlEnabled ? stopControl() : showControlModal();
    else if (action === "enable-control") startControl();
    else if (action === "command-preview") showCommandPreview(id);
    else if (action === "simulate-command") { closeModal(); toast("Команда не отправлена", "Проверен только UX-путь и отображение preflight."); }
    else if (action === "enter-work-fullscreen") { state.workFullscreen = true; state.drawer = null; render(); }
    else if (action === "exit-work-fullscreen") { state.workFullscreen = false; render(); }
    else if (action === "zoom-plus") { state.zoom = Math.min(180, state.zoom + 10); render(); }
    else if (action === "zoom-minus") { state.zoom = Math.max(50, state.zoom - 10); render(); }
    else if (action === "zoom-set") { state.zoom = Number(target.dataset.value); render(); }
    else if (action === "open-event") { state.drawer = { kind: "events", id }; state.drawerTab = "details"; if (!state.route.startsWith("/events") && !state.route.startsWith("/d/")) navigate("/events", true); else render(); }
    else if (action === "open-event-filters") { state.drawer = { kind: "events", id: state.drawer?.kind === "events" ? state.drawer.id : null }; state.drawerTab = "filters"; render(); }
    else if (action === "drawer-tab") { state.drawerTab = target.dataset.value; render(); }
    else if (action === "close-drawer") { if (/^\/maintenance\/work-orders\/[^/]+$/.test(state.route)) navigate("/maintenance/work-orders"); else { state.drawer = null; render(); } }
    else if (action === "event-mode") { state.eventMode = target.dataset.value; state.selectedEvents.clear(); render(); }
    else if (action === "apply-pending-events") { if (!requireRealtimeEventAction()) return; if (!events.some((item) => item.id === pendingEvent.id)) events.unshift(pendingEvent); state.pendingEvents = 0; render(); toast("Список обновлён", "Новая critical-запись вставлена по активной сортировке."); }
    else if (action === "toggle-event-selection") { event.stopPropagation(); if (!requireRealtimeEventAction()) return; state.selectedEvents.has(id) ? state.selectedEvents.delete(id) : state.selectedEvents.add(id); render(); }
    else if (action === "clear-event-selection") { state.selectedEvents.clear(); render(); }
    else if (action === "bulk-preview") showBulkPreview();
    else if (action === "apply-bulk-ack") { if (!requireRealtimeEventAction()) return; events.filter((item) => state.selectedEvents.has(item.id) && item.ack === false).forEach((item) => { item.ack = true; item.timeline.unshift(["14:31", "Подтверждено массовым действием"]); }); state.selectedEvents.clear(); closeModal(); render(); toast("Частичный результат", "Допустимые записи подтверждены, остальные пропущены."); }
    else if (action === "ack-event") { if (!requireRealtimeEventAction()) return; showAckModal(id); }
    else if (action === "confirm-ack") { if (!requireRealtimeEventAction()) return; const item = events.find((entry) => entry.id === id); const note = document.getElementById("ack-note")?.value.trim(); if (item.ackNoteRequired && !note) { document.getElementById("ack-error").textContent = "Комментарий обязателен по политике."; return; } item.ack = true; item.timeline.unshift(["14:31", `Подтвердил А. Смирнов${note ? `: ${note}` : ""}`]); closeModal(); render(); toast("Состояние подтверждено", "Assignment и состояние условия не изменены."); }
    else if (action === "assign-event") { if (!requireRealtimeEventAction()) return; const item = events.find((entry) => entry.id === id); item.assignment = "Алексей Смирнов"; item.timeline.unshift(["14:31", "Назначено А. Смирнову"]); render(); }
    else if (action === "shelve-event") { if (!requireRealtimeEventAction()) return; showShelveModal(id); }
    else if (action === "confirm-shelve") { if (!requireRealtimeEventAction()) return; const reason = document.getElementById("shelve-reason")?.value.trim(); if (!reason) { toast("Нужна причина", "Shelving без причины запрещён."); return; } const item = events.find((entry) => entry.id === id); item.shelvedUntil = document.getElementById("shelve-until").value; item.shelveReason = reason; item.timeline.unshift(["14:31", `Shelved до ${item.shelvedUntil}: ${reason}`]); closeModal(); render(); }
    else if (action === "unshelve-event") { if (!requireRealtimeEventAction()) return; const item = events.find((entry) => entry.id === id); delete item.shelvedUntil; delete item.shelveReason; item.timeline.unshift(["14:31", "Возвращено в рабочую очередь"]); render(); }
    else if (action === "create-incident") showCreateIncidentModal(id);
    else if (action === "confirm-create-incident") createIncidentFromEvent(id);
    else if (action === "open-incident") showIncidentSummary(id);
    else if (action === "create-work-request") createWorkRequestFromEvent(id);
    else if (action === "apply-event-filters") { state.drawer = null; render(); }
    else if (action === "reset-event-filters") { state.eventSeverity = "all"; state.eventCondition = "all"; state.eventLocation = "all"; state.eventAssignment = "all"; state.eventSearch = ""; render(); }
    else if (action === "equipment-view") { state.equipmentView = target.dataset.value; render(); }
    else if (action === "maintenance-filter") { state.maintenanceFilter = target.dataset.value; render(); }
    else if (action === "maintenance-calendar-mode") { state.maintenanceCalendarMode = target.dataset.value === "list" ? "list" : "calendar"; render(); }
    else if (action === "open-maintenance-asset") { if (!maintenanceAssets.some((item) => item.id === id)) return; state.drawer = { kind: "maintenance-asset", id }; if (!state.route.startsWith("/maintenance/assets")) navigate("/maintenance/assets", true); else render(); }
    else if (action === "open-maintenance-plan") { if (!maintenancePlans.some((item) => item.id === id)) return; state.drawer = { kind: "maintenance-plan", id }; if (!state.route.startsWith("/maintenance/plans")) navigate("/maintenance/plans", true); else render(); }
    else if (action === "open-maintenance-forecast") { if (!maintenanceForecasts.some((item) => item.id === id)) return; state.drawer = { kind: "maintenance-forecast", id }; if (!state.route.startsWith("/maintenance/calendar")) navigate("/maintenance/calendar", true); else render(); }
    else if (action === "open-maintenance-request") { if (!maintenanceRequests.some((item) => item.id === id)) return; state.drawer = { kind: "maintenance-request", id }; if (!state.route.startsWith("/maintenance/requests")) navigate("/maintenance/requests", true); else render(); }
    else if (action === "open-maintenance-work-order") { if (!maintenanceWorkOrders.some((item) => item.id === id)) return; state.drawer = { kind: "maintenance-work-order", id }; if (state.route.startsWith("/maintenance/calendar")) render(); else navigate(`/maintenance/work-orders/${id}`, true); }
    else if (action === "toggle-work-order-check") { const item = maintenanceWorkOrders.find((entry) => entry.id === id); const index = Number(target.dataset.index); if (!item || item.status !== "В работе" || !item.checklist[index]) return; item.checklist[index][1] = !item.checklist[index][1]; render(); }
    else if (action === "advance-work-order") advanceMaintenanceWorkOrder(id);
    else if (action === "equipment-draft-add") {
      const draft = createEquipmentDraft();
      state.equipmentDrafts.push(draft);
      state.selectedEquipmentDraftKey = draft.draftKey;
      state.equipmentCommunityVisible = false;
      render();
    }
    else if (action === "equipment-draft-select") {
      if (!state.equipmentDrafts.some((item) => item.draftKey === id)) return;
      state.selectedEquipmentDraftKey = id;
      state.equipmentCommunityVisible = false;
      render();
    }
    else if (action === "equipment-draft-delete") {
      const draft = selectedEquipmentDraft();
      if (!draft) return;
      state.equipmentDrafts = state.equipmentDrafts.filter((item) => item.draftKey !== draft.draftKey);
      delete state.equipmentDiagnostics[draft.draftKey];
      state.selectedEquipmentDraftKey = state.equipmentDrafts[0]?.draftKey || "";
      render();
      toast("Черновик удалён", "Оборудование в реестре не изменялось.");
    }
    else if (action === "equipment-draft-toggle-update") {
      if (!currentUser.isAdmin) { toast("Недостаточно прав", "Разрешить update может только администратор."); return; }
      const draft = state.equipmentDrafts.find((item) => item.draftKey === id);
      if (!draft || !equipment.some((item) => item.id === draft.id)) return;
      draft.allowUpdate = !draft.allowUpdate;
      render();
    }
    else if (action === "equipment-toggle-community") { state.equipmentCommunityVisible = !state.equipmentCommunityVisible; render(); }
    else if (action === "equipment-draft-connection" || action === "equipment-draft-poll") {
      const draft = selectedEquipmentDraft();
      if (!draft) return;
      const field = action === "equipment-draft-connection" ? "connection" : "poll";
      const signatureField = `${field}Signature`;
      const detailField = `${field}Detail`;
      const diagnosticErrors = equipmentDiagnosticValidation(draft);
      if (Object.keys(diagnosticErrors).length) {
        const detail = `Конфигурация: ${Object.values(diagnosticErrors).join("; ")}`;
        const nextDiagnostic = { ...(state.equipmentDiagnostics[draft.draftKey] || {}), [field]: "failure", [detailField]: detail };
        delete nextDiagnostic[signatureField];
        state.equipmentDiagnostics[draft.draftKey] = nextDiagnostic;
        render();
        toast("Диагностика завершена с ошибкой", `${detail}. Это не блокирует Apply других валидных строк.`);
        return;
      }
      const signature = equipmentDraftSignature(draft);
      state.equipmentDiagnostics[draft.draftKey] = { ...(state.equipmentDiagnostics[draft.draftKey] || {}), [field]: "testing", [signatureField]: signature, [detailField]: "" };
      render();
      setTimeout(() => {
        const currentDraft = state.equipmentDrafts.find((item) => item.draftKey === draft.draftKey);
        const currentDiagnostic = state.equipmentDiagnostics[draft.draftKey];
        if (!currentDraft || !currentDiagnostic || currentDiagnostic[signatureField] !== signature || equipmentDraftSignature(currentDraft) !== signature) return;
        const detail = field === "connection" ? "Reachability → session → authorization · 120 ms" : "Read sample · 3 points · 180 ms";
        state.equipmentDiagnostics[draft.draftKey] = { ...(state.equipmentDiagnostics[draft.draftKey] || {}), [field]: "success", [detailField]: detail };
        render();
        toast(field === "connection" ? "Связь проверена" : "Пробный опрос завершён", "Демонстрационный результат информационный и не влияет на Apply.");
      }, field === "connection" ? 400 : 500);
    }
    else if (action === "equipment-drafts-apply") {
      const reviews = state.equipmentDrafts.map(equipmentDraftReview);
      const actionable = reviews.filter((item) => item.valid && (item.operation === "create" || item.operation === "update"));
      if (!actionable.length) { toast("Нет строк для Apply", "Исправьте структурные ошибки или явно разрешите update существующего ID."); return; }
      const appliedKeys = new Set();
      let created = 0; let updated = 0;
      actionable.forEach(({ draft, operation }) => {
        const existing = equipment.find((item) => item.id === draft.id);
        const protocolConfig = {
          protocol: draft.protocol, host: draft.host, port: draft.port,
          unitId: draft.protocol === "Modbus TCP" ? draft.unitId : undefined,
          timeout: draft.protocol === "Modbus TCP" ? draft.timeout : undefined,
          snmpVersion: draft.protocol === "SNMP" ? draft.snmpVersion : undefined,
          snmpUsername: draft.protocol === "SNMP" && draft.snmpVersion === "v3" ? draft.snmpUsername : undefined,
          secretConfigured: draft.protocol === "SNMP" && Boolean(draft.snmpVersion === "v2c" ? draft.community : draft.authSecret && draft.privacySecret),
        };
        if (operation === "update" && existing) {
          Object.assign(existing, { name: draft.name, type: draft.type, location: draft.location, connection: "На связи", quality: "good", protocolConfig });
          updated += 1; appliedKeys.add(draft.draftKey);
        } else if (operation === "create" && !existing) {
          equipment.push({ id: draft.id, name: draft.name, type: draft.type, location: draft.location, connection: "На связи", quality: "good", status: "Норма", alarms: 0, ppr: "Не назначен", owner: "Не назначен", protocolConfig, params: [["Temperature", "21,4 °C", "good"], ["Status", "Работа", "good"], ["Runtime", "1 248 ч", "good"]] });
          created += 1; appliedKeys.add(draft.draftKey);
        }
      });
      state.equipmentDrafts = state.equipmentDrafts.filter((item) => !appliedKeys.has(item.draftKey));
      appliedKeys.forEach((key) => { delete state.equipmentDiagnostics[key]; });
      state.selectedEquipmentDraftKey = state.equipmentDrafts[0]?.draftKey || "";
      render();
      toast("Валидные строки применены", `create: ${created} · update: ${updated} · осталось в staging: ${state.equipmentDrafts.length}. Удалений оборудования: 0.`);
    }
    else if (action === "equipment-copy-open") showEquipmentCopyModal();
    else if (action === "equipment-copy-confirm") {
      const source = selectedEquipmentDraft();
      const quantity = Number(document.getElementById("equipment-copy-quantity")?.value);
      if (!source) { closeModal(); return; }
      if (!Number.isInteger(quantity) || quantity < 1 || quantity > 50) { toast("Копии не созданы", "Количество должно быть целым числом от 1 до 50."); return; }
      const unitStartInput = source.protocol === "Modbus TCP" ? document.getElementById("equipment-copy-unit-start") : null;
      const unitStart = unitStartInput?.value === "" || !unitStartInput ? null : Number(unitStartInput.value);
      if (unitStart !== null && (!Number.isInteger(unitStart) || unitStart < 1 || unitStart + quantity - 1 > 247)) {
        toast("Копии не созданы", "Диапазон последовательных Unit ID должен находиться в пределах 1–247."); return;
      }
      let firstKey = "";
      for (let index = 0; index < quantity; index += 1) {
        const copy = {
          ...source,
          draftKey: nextEquipmentDraftKey(),
          id: uniqueEquipmentDraftValue(source.id || "NEW", "id"),
          name: uniqueEquipmentDraftValue(source.name || "Новое устройство", "name"),
          host: source.host,
          unitId: source.protocol === "Modbus TCP" && unitStart !== null ? String(unitStart + index) : source.unitId,
          source: "copy", allowUpdate: false,
        };
        state.equipmentDrafts.push(copy);
        if (!firstKey) firstKey = copy.draftKey;
      }
      state.selectedEquipmentDraftKey = firstKey;
      state.equipmentCommunityVisible = false;
      closeModal(); render();
      toast("Копии добавлены в staging", `Количество: ${quantity}. IP/host сохранён без автоматического изменения.`);
    }
    else if (action === "equipment-template-save") showEquipmentTemplateSaveModal();
    else if (action === "equipment-template-save-confirm") {
      const draft = selectedEquipmentDraft();
      const name = document.getElementById("equipment-template-name")?.value.trim();
      if (!draft || !name) { toast("Шаблон не сохранён", "Укажите название шаблона."); return; }
      const template = { id: `template-${Date.now()}`, name, config: equipmentTemplateConfig(draft) };
      state.equipmentTemplates.push(template);
      state.equipmentSelectedTemplateId = template.id;
      closeModal(); render(); toast("Шаблон сохранён", "ID, имя, IP/host, Unit ID и секреты исключены.");
    }
    else if (action === "equipment-template-apply") {
      const template = state.equipmentTemplates.find((item) => item.id === state.equipmentSelectedTemplateId) || state.equipmentTemplates[0];
      if (!template) return;
      let draft = selectedEquipmentDraft();
      if (draft) Object.assign(draft, template.config, { source: "template" });
      else { draft = createEquipmentDraft({ ...template.config, source: "template" }); state.equipmentDrafts.push(draft); state.selectedEquipmentDraftKey = draft.draftKey; }
      if (draft.protocol === "SNMP") { draft.port ||= "161"; draft.snmpVersion ||= "v2c"; if (draft.snmpVersion === "v2c") draft.community ||= "public"; }
      else { draft.port ||= "502"; draft.unitId ||= "1"; draft.timeout ||= "1500"; }
      delete state.equipmentDiagnostics[draft.draftKey];
      state.equipmentCommunityVisible = false;
      render(); toast("Шаблон применён", "Создан или обновлён редактируемый снимок черновика.");
    }
    else if (action === "equipment-template-delete") {
      const template = state.equipmentTemplates.find((item) => item.id === state.equipmentSelectedTemplateId) || state.equipmentTemplates[0];
      if (!template) return;
      openModal("Удалить шаблон?", `<p>Шаблон <strong>${escapeHtml(template.name)}</strong> будет удалён. Черновики и оборудование не изменятся.</p>`, `<button class="button" data-action="close-modal">Отмена</button><button class="button danger" data-action="equipment-template-delete-confirm" data-id="${escapeHtml(template.id)}">Удалить</button>`);
    }
    else if (action === "equipment-template-delete-confirm") {
      if (!state.equipmentTemplates.some((item) => item.id === id)) { closeModal(); return; }
      state.equipmentTemplates = state.equipmentTemplates.filter((item) => item.id !== id);
      state.equipmentSelectedTemplateId = state.equipmentTemplates[0]?.id || "";
      closeModal(); render(); toast("Шаблон удалён", "Черновики и оборудование не изменялись.");
    }
    else if (action === "open-equipment" || action === "select-equipment") { closeModal(); state.drawer = { kind: "equipment", id }; if (!state.route.startsWith("/equipment") && !state.route.startsWith("/d/")) navigate("/equipment", true); else render(); }
    else if (action === "notification-scenario") { state.notificationScenario = target.dataset.value; render(); }
    else if (action === "notification-schedule-preset") { state.notificationSchedulePreset = target.dataset.value === "always" ? "always" : "work"; render(); }
    else if (action === "toggle-subscription") { state.subscriptions[id] = !state.subscriptions[id]; render(); }
    else if (action === "new-subscription") showNewSubscriptionModal();
    else if (action === "confirm-new-subscription") createSubscriptionFromModal();
    else if (action === "toggle-channel") { if (!(id in state.notificationChannels)) return; state.notificationChannels[id] = !state.notificationChannels[id]; render(); toast("Канал изменён", "Изменение действует только на дополнительные доставки."); }
    else if (action === "test-channel") { state.channelTests[id] = "тестирование…"; render(); setTimeout(() => { state.channelTests[id] = id === "messenger" ? "ошибка теста" : "тест успешен"; render(); }, 800); }
    else if (action === "request-vacation") { state.vacationState = "pending"; render(); }
    else if (action === "accept-vacation") { state.vacationState = "active"; state.notificationScenario = "vacation"; render(); toast("Замещение принято", "Обязательное покрытие подтверждено Сергеем Петровым."); }
    else if (action === "work-filter") { state.workFilter = target.dataset.value; render(); }
    else if (action === "open-task") { state.drawer = { kind: "task", id }; if (!state.route.startsWith("/my-work")) navigate("/my-work", true); else render(); }
    else if (action === "open-task-event-source") { const eventId = target.dataset.eventId; if (!events.some((item) => item.id === eventId)) { toast("Источник недоступен", "Событие не найдено или нет права просмотра."); return; } state.drawer = { kind: "events", id: eventId }; state.drawerTab = "details"; navigate("/events", true); }
    else if (action === "accept-task") { const item = tasks.find((entry) => entry.id === id); if (!isTaskActionable(item) || item.status === "Принято") return; item.status = "Принято"; item.timeline.unshift(["14:31", "Принял А. Смирнов"]); render(); }
    else if (action === "transfer-task") showTaskModal("transfer", id);
    else if (action === "cannot-task") showTaskModal("cannot", id);
    else if (action === "confirm-transfer") { const assignee = document.getElementById("task-assignee")?.value; if (!assignee) { document.getElementById("task-error").textContent = "Выберите нового исполнителя."; return; } const item = tasks.find((entry) => entry.id === id); if (!isTaskActionable(item)) { closeModal(); return; } item.status = `Передано: ${assignee}`; item.assignedToMe = false; item.timeline.unshift(["14:31", `Передано: ${assignee}`]); closeModal(); render(); }
    else if (action === "confirm-cannot") { const reason = document.getElementById("task-reason")?.value.trim(); if (!reason) { document.getElementById("task-error").textContent = "Причина обязательна."; return; } const item = tasks.find((entry) => entry.id === id); if (!isTaskActionable(item)) { closeModal(); return; } item.status = "Возвращено"; item.assignedToMe = false; item.timeline.unshift(["14:31", `Не могу выполнить: ${reason}`]); closeModal(); render(); }
    else if (action === "editor-canvas-only") { state.editorCanvasOnly = !state.editorCanvasOnly; render(); }
    else if (action === "editor-profile") { state.editorProfile = target.dataset.value; render(); }
    else if (action === "editor-preview") showEditorPreview();
    else if (action === "editor-versions") showEditorVersions();
    else if (action === "editor-save-draft") saveEditorDraft();
    else if (action === "editor-validate") validateEditorDraft();
    else if (action === "editor-publish") showEditorPublishPreview();
    else if (action === "editor-publish-confirm") publishEditorRevision();
    else if (action === "editor-rollback-preview") showEditorRollbackPreview(Number(target.dataset.version));
    else if (action === "editor-rollback-confirm") publishEditorRollback(Number(target.dataset.version));
    else if (action === "editor-leave-stay") { state.pendingEditorNavigation = null; closeModal(); }
    else if (action === "editor-leave-save") continueEditorNavigation("save");
    else if (action === "editor-leave-discard") continueEditorNavigation("discard");
    else if (action === "editor-focus-name") { closeModal(); requestAnimationFrame(() => document.querySelector('[data-input="editor-property"][data-field="elementName"]')?.focus()); }
    else if (action === "close-modal") closeModal();
    else if (action === "close-toast") document.getElementById(id)?.remove();
  });

  document.addEventListener("input", (event) => {
    const target = event.target.closest("[data-input]");
    if (!target || target.dataset.input !== "editor-property") return;
    markEditorDirty(target.dataset.field, target.value);
    refreshEditorStateIndicators();
  });

  document.addEventListener("change", (event) => {
    const target = event.target.closest("[data-change]");
    if (!target) return;
    if (target.dataset.change === "event-view") { state.eventView = target.value; state.selectedEvents.clear(); render(); }
    else if (target.dataset.change === "event-severity") { state.eventSeverity = target.value; render(); }
    else if (target.dataset.change === "event-condition") { state.eventCondition = target.value; render(); }
    else if (target.dataset.change === "event-location") { state.eventLocation = target.value; render(); }
    else if (target.dataset.change === "event-assignment") { state.eventAssignment = target.value; render(); }
    else if (target.dataset.change === "event-search") { state.eventSearch = target.value.trim(); render(); }
    else if (target.dataset.change === "work-type") { state.workType = target.value; render(); }
    else if (target.dataset.change === "admin-audit-filter") { if (!currentUser.isAdmin) return; state.adminAuditFilter = target.value; state.drawer = null; render(); }
    else if (target.dataset.change === "maintenance-section") navigate(target.value);
    else if (target.dataset.change === "dashboard-search") { state.dashboardSearch = target.value.trim(); state.dashboardCatalogScroll = 0; render(); }
    else if (target.dataset.change === "equipment-csv") {
      const file = target.files?.[0];
      if (file) readEquipmentCsv(file);
      target.value = "";
    }
    else if (target.dataset.change === "equipment-template-select") {
      state.equipmentSelectedTemplateId = target.value;
      render();
    }
    else if (target.dataset.change === "equipment-draft-field") {
      const draft = selectedEquipmentDraft();
      if (!draft || !target.dataset.field) return;
      const field = target.dataset.field;
      const value = target.value.trim();
      if (field === "id" && value !== draft.id) draft.allowUpdate = false;
      draft[field] = value;
      delete state.equipmentDiagnostics[draft.draftKey];
      render();
    }
    else if (target.dataset.change === "equipment-draft-protocol") {
      const draft = selectedEquipmentDraft();
      if (!draft) return;
      draft.protocol = target.value === "SNMP" ? "SNMP" : "Modbus TCP";
      draft.port = draft.protocol === "SNMP" ? "161" : "502";
      if (draft.protocol === "SNMP") { draft.snmpVersion ||= "v2c"; draft.community ||= "public"; }
      else { draft.unitId ||= "1"; draft.timeout ||= "1500"; }
      delete state.equipmentDiagnostics[draft.draftKey];
      state.equipmentCommunityVisible = false;
      render();
    }
    else if (target.dataset.change === "equipment-draft-snmp-version") {
      const draft = selectedEquipmentDraft();
      if (!draft) return;
      draft.snmpVersion = target.value === "v3" ? "v3" : "v2c";
      if (draft.snmpVersion === "v2c") draft.community ||= "public";
      delete state.equipmentDiagnostics[draft.draftKey];
      state.equipmentCommunityVisible = false;
      render();
    }
    else if (target.dataset.change === "settings-route") navigate(target.value);
    else if (target.dataset.change === "person-status") { state.profileStatus = target.value; safeSet("dispatcher.prototype.v3.profileStatus", target.value); render(); toast("Статус обновлён", `${target.value} · ${state.profileStatusUntil}`); }
    else if (target.dataset.change === "person-status-until") { state.profileStatusUntil = target.value; safeSet("dispatcher.prototype.v3.profileStatusUntil", target.value); render(); }
  });

  document.addEventListener("keydown", (event) => {
    if ((event.ctrlKey || event.metaKey) && event.key.toLowerCase() === "k" && !isKioskRuntimeRoute() && state.route !== "/terminal/enroll") { event.preventDefault(); showGlobalSearch(); return; }
    if (event.key === "Escape") {
      if (modalRoot.innerHTML) closeModal();
      else if (state.headerMenu) { state.headerMenu = null; render(); }
      else if (state.workFullscreen) { state.workFullscreen = false; render(); }
      else if (state.editorCanvasOnly) { state.editorCanvasOnly = false; render(); }
      else if (state.drawer) { if (/^\/maintenance\/work-orders\/[^/]+$/.test(state.route)) navigate("/maintenance/work-orders"); else { state.drawer = null; render(); } }
      else if (state.navOpen) { state.navOpen = false; render(); }
    }
    if (event.key === "Enter" && event.target.matches("[data-action][tabindex='0']")) event.target.click();
    if (event.key === "Enter" && event.target.dataset.change === "event-search") { state.eventSearch = event.target.value.trim(); render(); }
    if (event.key === "Enter" && event.target.dataset.change === "dashboard-search") { state.dashboardSearch = event.target.value.trim(); state.dashboardCatalogScroll = 0; render(); }
  });

  window.addEventListener("hashchange", () => {
    const nextRoute = routeFromHash();
    const currentDocument = editorDocumentFromRoute(state.route, false);
    if (nextRoute !== state.route && currentDocument?.dirty) {
      history.replaceState(null, "", `#${state.route}`);
      showEditorLeaveWarning(nextRoute, false);
      return;
    }
    state.route = nextRoute;
    state.headerMenu = null;
    state.equipmentCommunityVisible = false;
    state.workFullscreen = false;
    if (!state.preserveDrawerOnRouteChange) state.drawer = null;
    state.preserveDrawerOnRouteChange = false;
    render();
  });

  window.addEventListener("beforeunload", (event) => {
    if (!editorDocumentFromRoute(state.route, false)?.dirty) return;
    event.preventDefault();
    event.returnValue = "";
  });

  if (!location.hash) location.hash = "/home";
  render();
})();
