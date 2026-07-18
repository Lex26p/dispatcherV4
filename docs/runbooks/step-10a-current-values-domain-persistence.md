# Step 10A — Current values domain and persistence

This step adds the committed telemetry value storage baseline.

Added domain objects:

- `CurrentValue` — one committed current row per `DataPoint`.
- `HistoricalValue` — append-only telemetry sample history.

Added persistence objects:

- `telemetry.current_values`
- `telemetry.historical_values`
- migration `20260718005000_AddCurrentValuesBaseline`

Important rules:

- SignalR is not a source of truth.
- `CurrentValue` uses `DataPointId` as primary key, so there can be only one current row per DataPoint.
- New current samples must have a strictly newer monotonic `sequence`.
- Historical values are append-oriented and deduplicated by `(data_point_id, sequence)`.
- Each value stores value kind, raw value, unit, quality, source timestamp, receive timestamp and source id.

This step does not add REST endpoints, SignalR, worker ingestion, history query API, or UI.
