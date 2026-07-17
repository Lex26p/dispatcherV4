using Dispatcher.Contracts.Health;

namespace Dispatcher.IntegrationTests;

public sealed class AssemblySmokeTests
{
    [Fact]
    public void Api_and_contracts_are_available()
    {
        var apiAssembly = typeof(Program).Assembly.GetName().Name;
        var response = new HealthResponse("Live", "Dispatcher.Api", DateTimeOffset.UtcNow);

        Assert.Equal("Dispatcher.Api", apiAssembly);
        Assert.Equal("Live", response.Status);
    }
}
