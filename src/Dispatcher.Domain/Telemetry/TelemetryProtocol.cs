namespace Dispatcher.Domain.Telemetry;

/// <summary>
/// Technical protocol family used by a telemetry source.
/// This enum is intentionally outside Assets/Equipment to keep Equipment protocol-neutral.
/// </summary>
public enum TelemetryProtocol
{
    Simulator = 0,
    ModbusTcp = 1,
    Snmp = 2,
}
