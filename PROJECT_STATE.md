# PROJECT_STATE

## Current phase
- Phase: Industrial MVP foundation
- Status: In progress

## Current step
- Step: 9C
- Name: Telemetry configuration Web UI
- Status: Completed after local build/test/Web smoke and commit

## Recently completed steps
- Step 7A: Locations domain and persistence — 86aad3e8386c783646064624f82519b1b5e43611
- Step 7B: Locations API — 2856caead2cda37b723c018e2b88242622063377
- Step 7C: Locations Web UI — b4a4c6db086bb664024bde33238184f27ee59603
- Step 8A: Equipment domain and persistence — 3bc727ce2325cecaefd7d582f2b8d956a06204b8
- Step 8B: Equipment API — ab70ec72a6e81eea218447189411eaf204ce03ab
- Step 8C: Equipment Web UI — 0469e0bf5d369bb2f8dc42a7c8748affe654410c
- Step 9A: Telemetry domain and persistence — 452138affb73a8bae043eae360bb3de36b7ec791
- Step 9B: Telemetry configuration API — 1d36e46856818e36d9a256d1256a3e9085fd3602

## Frontend routes added in Step 9C
- /telemetry/configuration

## Web client methods added in Step 9C
- GetTelemetrySourcesAsync
- CreateTelemetrySourceAsync
- EnableTelemetrySourceAsync
- DisableTelemetrySourceAsync
- ArchiveTelemetrySourceAsync
- RestoreTelemetrySourceAsync
- GetDataPointsAsync
- CreateDataPointAsync
- ArchiveDataPointAsync
- RestoreDataPointAsync
- GetProtocolMappingsAsync
- CreateProtocolMappingAsync
- ArchiveProtocolMappingAsync
- RestoreProtocolMappingAsync

## Known limitations
- UI is still baseline/operator-admin utility, not final design.
- No current values or history display yet.
- No polling worker uses telemetry configuration yet.
- Protocol-specific authoring forms are deferred; JSON fields are temporary admin-oriented input.
- Use ASCII unit values such as C/bar/%/V/A until encoding validation is hardened.

## Next steps
1. Step 10A — Current values domain and persistence.
2. Step 10B — Current values API.
