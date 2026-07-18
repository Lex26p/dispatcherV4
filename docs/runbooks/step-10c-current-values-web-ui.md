# Step 10C — Current Values Web UI

This step adds a Blazor WebAssembly page for reading current values, writing manual smoke/debug samples and inspecting recent history.

## Scope

- Route: `/telemetry/current`
- Navigation entry: `Текущие значения`
- API client methods for:
  - `GET /api/values/current`
  - `GET /api/values/current/{dataPointId}`
  - `POST /api/values/current`
  - `GET /api/values/history`
- Manual upsert form for local development and smoke testing.
- History panel for the selected DataPoint.

## Out of scope

- No SignalR realtime updates.
- No polling worker.
- No freshness worker.
- No alarm evaluation.
- No charting/dashboard widgets.

## Smoke check

1. Run `Dispatcher.Api` on `http://localhost:5076`.
2. Run `Dispatcher.Web` on `http://localhost:5048`.
3. Open `/telemetry/current`.
4. Pick an active DataPoint.
5. Submit a value with a larger sequence.
6. Verify current values list updates.
7. Open history for the same DataPoint.
8. Stop API and verify the page shows a user-facing error.
