# Dispatcher Step 10C — Current Values Web UI

This archive adds a Web UI slice for current telemetry values.

It assumes Steps 0A through 10B are already applied.

Added:

- Web route `/telemetry/current`
- navigation entry for current values
- `DispatcherApiClient` methods for current/history values
- manual current value upsert form
- current values list
- recent history table for a selected DataPoint

This step does not add SignalR, polling workers, freshness workers, alarm evaluation, charting, or dashboard widgets.
