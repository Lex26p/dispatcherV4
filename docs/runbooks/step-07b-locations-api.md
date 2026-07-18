# Step 7B — Locations API and application layer

## Goal

Expose the persisted `Location` model through a small application service and REST API without adding Web UI.

## Added responsibilities

- `Dispatcher.Contracts.Assets` request/response contracts.
- `Dispatcher.Application.Assets.Locations` service and repository boundary.
- `Dispatcher.Infrastructure.Assets.EfLocationRepository` EF implementation.
- `Dispatcher.Api.Endpoints.Locations.LocationEndpoints` REST endpoints.
- Permission names: `locations.view`, `locations.manage`.

## API endpoints

- `GET /api/locations`
- `GET /api/locations/{id}`
- `POST /api/locations`
- `PUT /api/locations/{id}`
- `POST /api/locations/{id}/move`
- `POST /api/locations/{id}/archive`

## Notes

- This step does not add UI.
- This step does not implement ETag/concurrency yet.
- Archive blocks active child locations instead of silently deleting or cascading them.
- Move validates parent existence and rejects hierarchy cycles.
