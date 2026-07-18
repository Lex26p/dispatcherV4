# Step 9C — Telemetry Configuration Web UI

## Scope

Adds a small Blazor UI for telemetry configuration that uses the Step 9B API:

- `/telemetry/configuration`
- list/create/archive/restore telemetry sources
- list/create/archive/restore DataPoints
- list/create/archive/restore protocol mappings
- no current values
- no history
- no polling worker
- no protocol transport calls

## Important constraints

- Equipment remains protocol-neutral.
- Secret values are not displayed; only masked secret references are visible.
- Units in smoke tests should use ASCII values such as `C`, `bar`, `%`, `V`, and `A` until a later encoding/validation pass.
- Protocol mapping protocol is derived from the selected source.

## Smoke test

1. Run API with `DISPATCHER_CONNECTION_STRING` configured.
2. Run Web.
3. Open `/telemetry/configuration`.
4. Create a simulator source.
5. Create a DataPoint for existing equipment.
6. Create a protocol mapping.
7. Archive/restore source, DataPoint, and mapping.
8. Stop API and verify the page shows an understandable error.
