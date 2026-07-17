# Realtime

The API exposes a SignalR hub for current tag value updates.

## Hub

```text
/hubs/tag-values
```

## Hub methods

Clients can call these methods after connecting:

```text
SubscribeToAll
UnsubscribeFromAll
SubscribeToTag(Guid tagId)
UnsubscribeFromTag(Guid tagId)
```

## Client events

Clients should listen for these events:

```text
TagValueUpdated
CurrentValuesSnapshot
```

## HTTP metadata endpoint

```text
GET /api/realtime
```

This endpoint returns the hub path, hub method names and client event names. It does not require PostgreSQL.

## Current limitation

At this step the API can broadcast values when current values are updated through API endpoints.
The separate Worker service still writes through the Application layer and does not yet push directly to the API SignalR hub. Worker-to-realtime integration will be handled later.
