# Step 8B — Equipment API

This step adds the application and REST API layer for canonical Equipment.

## Scope

- Equipment public contracts.
- Equipment application service and repository boundary.
- EF repository for `assets.equipment`.
- REST endpoints under `/api/equipment`.
- Permission names `equipment.view` and `equipment.manage`.

## Explicitly out of scope

- Blazor Equipment UI.
- TelemetrySource, DataPoint, Modbus, SNMP, protocol fields and current values.
- MaintenanceObject and CMMS links.
- Deleting equipment.

## Smoke checks

1. `GET /api/equipment` returns `200`.
2. `POST /api/equipment` creates an equipment record linked to an existing Location.
3. `GET /api/equipment/{id}` returns the created equipment.
4. `PUT /api/equipment/{id}` renames the equipment.
5. `POST /api/equipment/{id}/archive` archives without deleting.
6. A request with only `identity.me.view` returns `403`.
