using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;

namespace Dispatcher.UnitTests.Assets;

public sealed class LocationTests
{
    [Fact]
    public void Create_normalizes_code_and_trims_text()
    {
        var location = Location.Create(
            EntityId.New(),
            null,
            "  building-a  ",
            "  Building A  ",
            "  Main building  ",
            DateTimeOffset.UtcNow);

        Assert.Equal("BUILDING-A", location.Code);
        Assert.Equal("Building A", location.Name);
        Assert.Equal("Main building", location.Description);
        Assert.False(location.IsArchived);
    }

    [Fact]
    public void Create_rejects_self_parent()
    {
        var id = EntityId.New();

        Assert.Throws<ArgumentException>(() => Location.Create(
            id,
            id,
            "ROOT",
            "Root",
            null,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void MoveTo_rejects_self_parent()
    {
        var id = EntityId.New();
        var location = Location.Create(id, null, "ROOT", "Root", null, DateTimeOffset.UtcNow);

        Assert.Throws<ArgumentException>(() => location.MoveTo(id, DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Archive_marks_location_archived_without_deleting_identity()
    {
        var id = EntityId.New();
        var location = Location.Create(id, null, "ROOT", "Root", null, DateTimeOffset.UtcNow);

        location.Archive(DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Equal(id, location.Id);
        Assert.True(location.IsArchived);
    }
}
