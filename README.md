# Диспетчер — Step 17

Step 17 adds the Blazor current tag values page and SignalR realtime client.

## Added

- Blazor SignalR client package reference.
- `TagValueDto` and `UpsertTagValueRequest` models.
- Current tag value API methods in `DispatcherApiClient`.
- `TagValueRealtimeClient` service.
- `/tag-values` page.
- Navigation link for current values.
- Documentation for the realtime values page.

PostgreSQL is not required for building the solution. Data requests require PostgreSQL and applied migrations.
