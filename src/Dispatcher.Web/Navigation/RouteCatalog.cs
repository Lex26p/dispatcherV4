namespace Dispatcher.Web.Navigation;

public sealed record AppRoute(
    string Href,
    string Label,
    string ShortLabel,
    string Area,
    string RequiredPermission,
    bool IsPrimary,
    int SortOrder);

public static class RouteCatalog
{
    public const string PermissionNotRequired = "none";

    private static readonly IReadOnlyList<AppRoute> Routes = new List<AppRoute>
    {
        new("/home", "Рабочий стол", "Дом", "Оператор", PermissionNotRequired, true, 10),
        new("/me", "Мой профиль", "Я", "Пользователь", "identity.me.view", true, 20),
        new("/settings", "Настройки", "Настр", "Пользователь", PermissionNotRequired, true, 30),
        new("/locations", "Локации", "Лок", "Объекты", "locations.view", true, 40),
        new("/equipment", "Оборудование", "Обор", "Объекты", "equipment.view", true, 50),
        new("/telemetry/configuration", "Телеметрия", "Телем", "Телеметрия", "telemetry.configuration.view", true, 60),
        new("/admin", "Администрирование", "Адм", "Админ", "identity.users.view", true, 80),
        new("/admin/users", "Пользователи", "Польз", "Админ", "identity.users.view", false, 90),
        new("/forbidden", "Нет доступа", "403", "Система", PermissionNotRequired, false, 900),
        new("/not-found", "Не найдено", "404", "Система", PermissionNotRequired, false, 910)
    };

    public static IReadOnlyList<AppRoute> PrimaryRoutes => Routes
        .Where(route => route.IsPrimary)
        .OrderBy(route => route.SortOrder)
        .ToArray();

    public static IReadOnlyList<AppRoute> AdminRoutes => Routes
        .Where(route => string.Equals(route.Area, "Админ", StringComparison.OrdinalIgnoreCase))
        .OrderBy(route => route.SortOrder)
        .ToArray();

    public static IReadOnlyList<AppRoute> AllRoutes => Routes
        .OrderBy(route => route.SortOrder)
        .ToArray();
}
