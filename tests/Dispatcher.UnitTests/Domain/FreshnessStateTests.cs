using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.UnitTests.Domain;

public sealed class FreshnessStateTests
{
    [Fact]
    public void Fresh_allows_operator_trust()
    {
        var state = FreshnessState.Fresh(UtcTimestamp.From(DateTimeOffset.UtcNow));

        Assert.Equal(FreshnessKind.Fresh, state.Kind);
        Assert.True(state.AllowsOperatorTrust);
    }

    [Fact]
    public void Stale_requires_reason_and_blocks_operator_trust()
    {
        var changedAt = UtcTimestamp.From(DateTimeOffset.UtcNow);

        var state = FreshnessState.Stale(changedAt, "polling timeout");

        Assert.Equal(FreshnessKind.Stale, state.Kind);
        Assert.False(state.AllowsOperatorTrust);
        Assert.Equal("polling timeout", state.Reason);
    }

    [Fact]
    public void Offline_requires_reason()
    {
        Assert.Throws<ArgumentException>(() => FreshnessState.Offline(UtcTimestamp.From(DateTimeOffset.UtcNow), " "));
    }
}
