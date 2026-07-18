# Step 10B — Current Values API

Adds REST access to the committed telemetry current-value read model and bounded history query.

## Scope

- `GET /api/values/current`
- `GET /api/values/current/{dataPointId}`
- `POST /api/values/current`
- `GET /api/values/history`
- Application service and EF repository for current/history values
- Unit tests for upsert, sequence guard and bounded history validation

## Non-scope

- Web UI
- SignalR
- Worker polling
- Protocol adapters
- Physical devices

## Notes

`POST /api/values/current` is a development/admin ingestion endpoint for Step 10B. It validates DataPoint, optional TelemetrySource, value kind, data quality and monotonic sequence. SignalR remains out of scope; REST current snapshot is the source of truth.
