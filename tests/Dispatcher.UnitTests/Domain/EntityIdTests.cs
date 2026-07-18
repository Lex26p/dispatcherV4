using Dispatcher.Domain.Common;

namespace Dispatcher.UnitTests.Domain;

public sealed class EntityIdTests
{
    [Fact]
    public void New_returns_non_empty_id()
    {
        var id = EntityId.New();

        Assert.NotEqual(Guid.Empty, id.Value);
    }

    [Fact]
    public void From_rejects_empty_guid()
    {
        Assert.Throws<ArgumentException>(() => EntityId.From(Guid.Empty));
    }

    [Fact]
    public void FromString_parses_guid_and_keeps_stable_string_format()
    {
        var guid = Guid.NewGuid();

        var id = EntityId.FromString(guid.ToString());

        Assert.Equal(guid, id.Value);
        Assert.Equal(guid.ToString("D"), id.ToString());
    }
}
