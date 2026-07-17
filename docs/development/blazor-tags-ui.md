# Blazor Tags UI

Step 16 adds the first working Blazor page for tag management.

## Page

```text
/tags
```

## Features

- Loads devices from `GET /api/devices`.
- Lets the operator select a device.
- Loads tags from `GET /api/devices/{deviceId}/tags`.
- Creates Modbus tags through `POST /api/tags/modbus`.
- Creates SNMP tags through `POST /api/tags/snmp`.
- Enables and disables tags through tag API endpoints.

## Current limitation

PostgreSQL is still optional at this stage. The UI and solution can build without PostgreSQL, but data operations need PostgreSQL plus the initial EF Core migration applied.

## Next step

Add Blazor realtime/current-value view for tag values and SignalR subscription.
