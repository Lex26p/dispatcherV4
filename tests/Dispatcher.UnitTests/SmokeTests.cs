using Dispatcher.Contracts.Common;
using Dispatcher.Domain.Common;

namespace Dispatcher.UnitTests;

public sealed class SmokeTests
{
    [Fact]
    public void Domain_and_contracts_are_available()
    {
        Assert.Equal("Диспетчер", AssemblyMarker.ProductName);
        Assert.Equal("Dispatcher.Domain", DomainAssemblyMarker.Name);
    }
}
