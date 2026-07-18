# Step 9B — Telemetry configuration API

Adds REST API and application services for protocol-neutral telemetry configuration.

## Scope

- Telemetry sources
- Data points
- Protocol mappings
- Backend authorization for telemetry configuration
- Secret reference masking in API DTOs

## Out of scope

- Current values
- Historical values
- Polling worker
- Modbus/SNMP transport calls
- Web UI

## Smoke checks

- `GET /api/telemetry-sources`
- `POST /api/telemetry-sources`
- `POST /api/data-points`
- `POST /api/protocol-mappings`
- restricted request with only `identity.me.view` returns `403`

Telemetry source DTOs must never return secret plaintext. The current baseline returns only `HasSecretReference` and `MaskedSecretReference`.
