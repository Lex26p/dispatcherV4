namespace Dispatcher.Api.Configuration;

public static class DatabaseOptions
{
    public const string ConnectionStringName = "DispatcherDatabase";
    public const string EnvironmentVariableName = "DISPATCHER_CONNECTION_STRING";

    public static string GetDispatcherDatabaseConnectionString(this IConfiguration configuration)
    {
        var fromConfiguration = configuration.GetConnectionString(ConnectionStringName);

        if (!string.IsNullOrWhiteSpace(fromConfiguration))
        {
            return fromConfiguration;
        }

        var fromEnvironment = Environment.GetEnvironmentVariable(EnvironmentVariableName);

        if (!string.IsNullOrWhiteSpace(fromEnvironment))
        {
            return fromEnvironment;
        }

        // Development fallback intentionally contains no password.
        // Set ConnectionStrings:DispatcherDatabase or DISPATCHER_CONNECTION_STRING for real local runs.
        return "Host=localhost;Port=5432;Database=dispatcher;Username=postgres;Include Error Detail=false";
    }
}
