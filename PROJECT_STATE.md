# PROJECT_STATE

## Current phase
- Phase: Industrial MVP foundation
- Status: In progress

## Current step
- Step: 10A
- Name: Current values domain and persistence
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

## Added in Step 10A
- Domain: `CurrentValue`, `HistoricalValue`.
- Persistence: `telemetry.current_values`, `telemetry.historical_values`.
- Migration: `20260718005000_AddCurrentValuesBaseline`.
- Tests: current sequence guard, historical sample shape, EF model smoke.

## Invariants
- SignalR is not a source of truth.
- One current row per DataPoint.
- Current sample update requires a strictly newer sequence.
- Historical samples are append-oriented and deduplicated by `(data_point_id, sequence)`.
- Value records include value kind, raw value, unit, quality, source timestamp, receive timestamp and source id.

## Known limitations
- No current values REST API yet.
- No history query API yet.
- No SignalR realtime delivery yet.
- No polling worker writes values yet.
- No freshness worker yet.

## Next steps
1. Step 10B — Current values write/read application service and API.
2. Step 10C — Current values Web read-only view.
