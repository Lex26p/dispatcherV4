# Dispatcher Step 10A — Current Values Domain and Persistence

This archive adds the current-value and historical-value persistence baseline.

It assumes Steps 0A through 9C are already applied.

Added:

- `CurrentValue`
- `HistoricalValue`
- `telemetry.current_values`
- `telemetry.historical_values`
- migration `20260718005000_AddCurrentValuesBaseline`
- unit tests for sequence guard and append-only sample shape
- integration model smoke for current/history value tables

This step does not add current values API, SignalR, polling workers, history UI, or realtime delivery.


## Step 10B
- Added Current Values REST API baseline.
- Commit: pending.
