# PROJECT_STATE

## Current phase
- Phase: Industrial MVP foundation
- Status: In progress

## Current step
- Step: 10C
- Name: Current Values Web UI
- Status: Ready to apply

## Recently completed steps
- Step 7A: Locations domain and persistence — 86aad3e8386c783646064624f82519b1b5e43611
- Step 7B: Locations API — 2856caead2cda37b723c018e2b88242622063377
- Step 7C: Locations Web UI — b4a4c6db086bb664024bde33238184f27ee59603
- Step 8A: Equipment domain and persistence — 3bc727ce2325cecaefd7d582f2b8d956a06204b8
- Step 8B: Equipment API — ab70ec72a6e81eea218447189411eaf204ce03ab
- Step 8C: Equipment Web UI — 0469e0bf5d369bb2f8dc42a7c8748affe654410c
- Step 9A: Telemetry domain and persistence — 452138affb73a8bae043eae360bb3de36b7ec791
- Step 9B: Telemetry configuration API — 1d36e46856818e36d9a256d1256a3e9085fd3602
- Step 9C: Telemetry configuration Web UI — 9ae0fbfb40e82fb624995ad86a13485f9ecdfc31
- Step 10A: Current values domain and persistence — 50d977f2a8447faf509bb1a72cbfed1f7bfc003e
- Step 10B: Current Values API — c77e20fe4c9ca60d3d6d2458b61db594b2505c2f

## Added in Step 10C
- Web route: `/telemetry/current`.
- Web API client methods for current values and history.
- Manual current value upsert form for local development smoke checks.
- Current value list with freshness/quality/sequence/source metadata.
- Recent history table for a selected DataPoint.

## Invariants
- SignalR is not a source of truth.
- Current values still come from REST API backed by PostgreSQL.
- Manual Web upsert is development/smoke tooling, not a real telemetry runtime.
- Value records must keep value kind, raw value, unit, quality, source timestamp, received timestamp, freshness and source id.

## Known limitations
- No SignalR realtime delivery yet.
- No polling worker writes values yet.
- No freshness worker yet.
- No charts/dashboards yet.

## Next steps
1. Step 10D — SignalR hub for current value notifications.
2. Step 10E — Simulator polling worker writing through the values service/API boundary.
