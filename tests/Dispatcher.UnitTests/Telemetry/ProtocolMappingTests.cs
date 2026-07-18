using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.UnitTests.Telemetry;

public sealed class ProtocolMappingTests
{
    [Fact]
    public void Create_keeps_protocol_mapping_separate_from_datapoint()
    {
        var dataPointId = EntityId.New();
        var sourceId = EntityId.New();

        var mapping = ProtocolMapping.Create(
            EntityId.New(),
            dataPointId,
            sourceId,
            TelemetryProtocol.Snmp,
            mappingSchemaVersion: 1,
            "{\"oid\":\"1.3.6.1.2.1.1.3.0\"}",
            DateTimeOffset.UtcNow);

        Assert.Equal(dataPointId, mapping.DataPointId);
        Assert.Equal(sourceId, mapping.TelemetrySourceId);
        Assert.Equal(TelemetryProtocol.Snmp, mapping.Protocol);
        Assert.Contains("oid", mapping.MappingJson);
    }

    [Fact]
    public void Create_rejects_non_json_mapping()
    {
        Assert.Throws<ArgumentException>(() => ProtocolMapping.Create(
            EntityId.New(),
            EntityId.New(),
            EntityId.New(),
            TelemetryProtocol.ModbusTcp,
            mappingSchemaVersion: 1,
            "holding-register-1",
            DateTimeOffset.UtcNow));
    }
}
