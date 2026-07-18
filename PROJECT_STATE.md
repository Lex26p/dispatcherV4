# PROJECT_STATE

## Current phase
- Phase: Industrial MVP foundation
- Status: In progress

## Current step
- Step: 9B
- Name: Telemetry configuration API
- Status: Completed after local build/test/API smoke and commit

## Recently completed steps
- Step 7A: Locations domain and persistence — 86aad3e8386c783646064624f82519b1b5e43611
- Step 7B: Locations API — 2856caead2cda37b723c018e2b88242622063377
- Step 7C: Locations Web UI — b4a4c6db086bb664024bde33238184f27ee59603
- Step 8A: Equipment domain and persistence — 3bc727ce2325cecaefd7d582f2b8d956a06204b8
- Step 8B: Equipment API — ab70ec72a6e81eea218447189411eaf204ce03ab
- Step 8C: Equipment Web UI — 0469e0bf5d369bb2f8dc42a7c8748affe654410c
- Step 9A: Telemetry domain and persistence — 452138affb73a8bae043eae360bb3de36b7ec791

## API endpoints added in Step 9B
- GET /api/telemetry-sources
- GET /api/telemetry-sources/{id}
- POST /api/telemetry-sources
- PUT /api/telemetry-sources/{id}
- POST /api/telemetry-sources/{id}/enable
- POST /api/telemetry-sources/{id}/disable
- POST /api/telemetry-sources/{id}/archive
- POST /api/telemetry-sources/{id}/restore
- GET /api/data-points
- GET /api/data-points/{id}
- POST /api/data-points
- PUT /api/data-points/{id}
- POST /api/data-points/{id}/archive
- POST /api/data-points/{id}/restore
- GET /api/protocol-mappings
- GET /api/protocol-mappings/{id}
- POST /api/protocol-mappings
- PUT /api/protocol-mappings/{id}
- POST /api/protocol-mappings/{id}/archive
- POST /api/protocol-mappings/{id}/restore

## Known limitations
- No Web UI for telemetry configuration yet.
- No current values/history ingestion yet.
- No polling worker uses these configuration records yet.
- No real Modbus/SNMP transport calls in this step.

## Next steps
1. Step 9C — Telemetry configuration Web UI.
2. Step 10A — Current values domain and persistence.
