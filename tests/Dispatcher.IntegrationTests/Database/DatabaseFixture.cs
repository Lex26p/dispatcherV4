namespace Dispatcher.IntegrationTests.Database;

public static class DatabaseFixture
{
    public static string? ConnectionString =>
        Environment.GetEnvironmentVariable("DISPATCHER_TEST_CONNECTION_STRING")
        ?? Environment.GetEnvironmentVariable("DISPATCHER_CONNECTION_STRING");

    public static bool ShouldRunDatabaseTests =>
        string.Equals(Environment.GetEnvironmentVariable("DISPATCHER_RUN_DB_TESTS"), "1", StringComparison.Ordinal)
        && !string.IsNullOrWhiteSpace(ConnectionString);
}
