using Dispatcher.Domain.Common;

namespace Dispatcher.UnitTests.Domain;

public sealed class DomainDependencyTests
{
    [Fact]
    public void Domain_assembly_does_not_reference_infrastructure_frameworks()
    {
        var referencedAssemblies = typeof(EntityId).Assembly.GetReferencedAssemblies().Select(static assembly => assembly.Name).ToArray();

        Assert.DoesNotContain("Microsoft.EntityFrameworkCore", referencedAssemblies);
        Assert.DoesNotContain("Microsoft.AspNetCore", referencedAssemblies);
        Assert.DoesNotContain("Microsoft.AspNetCore.SignalR", referencedAssemblies);
        Assert.DoesNotContain("Dispatcher.Infrastructure", referencedAssemblies);
        Assert.DoesNotContain("Dispatcher.Api", referencedAssemblies);
        Assert.DoesNotContain("Dispatcher.Web", referencedAssemblies);
    }
}
