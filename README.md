# Step 9A — Telemetry domain and persistence

This archive adds the first telemetry configuration baseline for the industrial Dispatcher repository.

## Adds

- `DataPoint`
- `TelemetrySource`
- `ProtocolMapping`
- EF Core configurations
- migration `20260718004000_AddTelemetryConfigurationBaseline`
- telemetry model tests

## Does not add

- telemetry REST API
- Web UI
- current values/history
- polling workers/adapters
- real Modbus/SNMP calls
