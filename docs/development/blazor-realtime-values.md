# Blazor realtime current tag values

This step adds the first Blazor realtime client for current tag values.

## Routes

- `/tag-values` — current tag value table and SignalR connection controls.

## API endpoints used by the page

- `GET /api/tag-values/current`
- `GET /api/tag-values/current/{tagId}`
- SignalR hub: `/hubs/tag-values`

## SignalR events

- `TagValueUpdated` — one current tag value was updated.
- `CurrentValuesSnapshot` — initial snapshot sent after subscribing to all tag values.

## Local development notes

PostgreSQL is still optional for building and running the UI shell.

If PostgreSQL is not running or migrations have not been applied, the page can show a friendly data API error. This is expected until local database setup is completed.
