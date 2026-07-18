using Dispatcher.Domain.Assets;
using Dispatcher.Domain.Common;

namespace Dispatcher.UnitTests.Assets;

public sealed class EquipmentTests
{
    [Fact]
    public void Create_normalizes_code_and_trims_text()
    {
        var equipment = Equipment.Create(
            EntityId.New(),
            EntityId.New(),
            "  pump-001  ",
            "  Pump 001  ",
            "  Main circulation pump  ",
            DateTimeOffset.UtcNow);

        Assert.Equal("PUMP-001", equipment.Code);
        Assert.Equal("Pump 001", equipment.Name);
        Assert.Equal("Main circulation pump", equipment.Description);
        Assert.False(equipment.IsArchived);
    }

    [Fact]
    public void Create_rejects_empty_name()
    {
        Assert.Throws<ArgumentException>(() => Equipment.Create(
            EntityId.New(),
            EntityId.New(),
            "PUMP-001",
            "   ",
            null,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Create_rejects_protocol_like_code_characters()
    {
        Assert.Throws<ArgumentException>(() => Equipment.Create(
            EntityId.New(),
            EntityId.New(),
            "modbus://1/40001",
            "Pump",
            null,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void MoveToLocation_changes_only_location_reference()
    {
        var firstLocationId = EntityId.New();
        var secondLocationId = EntityId.New();
        var equipment = Equipment.Create(EntityId.New(), firstLocationId, "PUMP-001", "Pump", null, DateTimeOffset.UtcNow);

        equipment.MoveToLocation(secondLocationId, DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Equal(secondLocationId, equipment.LocationId);
        Assert.Equal("PUMP-001", equipment.Code);
    }

    [Fact]
    public void Archive_marks_equipment_archived_without_deleting_identity()
    {
        var id = EntityId.New();
        var equipment = Equipment.Create(id, EntityId.New(), "PUMP-001", "Pump", null, DateTimeOffset.UtcNow);

        equipment.Archive(DateTimeOffset.UtcNow.AddMinutes(1));

        Assert.Equal(id, equipment.Id);
        Assert.True(equipment.IsArchived);
    }
}
