using Dispatcher.Domain.Common;
using Dispatcher.Domain.Telemetry;

namespace Dispatcher.UnitTests.Telemetry;

public sealed class TelemetrySourceTests
{
    [Fact]
    public void Create_requires_secret_reference_instead_of_plaintext_secret()
    {
        var source = TelemetrySource.Create(
            EntityId.New(),
            "modbus-main",
            "Main Modbus source",
            TelemetryProtocol.ModbusTcp,
            "tcp://10.0.0.10:502",
            configurationSchemaVersion: 1,
            "{\"unitId\":1}",
            "secret://telemetry/modbus-main",
            null,
            DateTimeOffset.UtcNow);

        Assert.Equal("MODBUS-MAIN", source.Code);
        Assert.Equal(TelemetryProtocol.ModbusTcp, source.Protocol);
        Assert.Equal("secret://telemetry/modbus-main", source.SecretReference);
        Assert.False(source.IsEnabled);
    }

    [Fact]
    public void Create_rejects_plaintext_secret_reference()
    {
        Assert.Throws<ArgumentException>(() => TelemetrySource.Create(
            EntityId.New(),
            "snmp-ups",
            "UPS SNMP",
            TelemetryProtocol.Snmp,
            "udp://10.0.0.20:161",
            configurationSchemaVersion: 1,
            "{\"version\":\"v3\"}",
            "plain-password",
            null,
            DateTimeOffset.UtcNow));
    }

    [Fact]
    public void Archive_disables_source()
    {
        var source = TelemetrySource.Create(EntityId.New(), "sim", "Simulator", TelemetryProtocol.Simulator, "sim://local", 1, "{}", null, null, DateTimeOffset.UtcNow);
        source.Enable(DateTimeOffset.UtcNow.AddSeconds(1));

        source.Archive(DateTimeOffset.UtcNow.AddSeconds(2));

        Assert.False(source.IsEnabled);
        Assert.True(source.IsArchived);
    }
}
