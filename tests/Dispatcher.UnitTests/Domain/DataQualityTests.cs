using Dispatcher.Domain.Telemetry;

namespace Dispatcher.UnitTests.Domain;

public sealed class DataQualityTests
{
    [Fact]
    public void DataQuality_contains_required_operator_states()
    {
        var names = Enum.GetNames<DataQuality>();

        Assert.Contains(nameof(DataQuality.Good), names);
        Assert.Contains(nameof(DataQuality.Uncertain), names);
        Assert.Contains(nameof(DataQuality.Bad), names);
        Assert.Contains(nameof(DataQuality.Stale), names);
        Assert.Contains(nameof(DataQuality.Offline), names);
        Assert.Contains(nameof(DataQuality.Initializing), names);
    }

    [Fact]
    public void Quality_values_are_distinct_from_freshness_kind_values_by_type()
    {
        Assert.True(typeof(DataQuality) != typeof(FreshnessKind));
    }
}
