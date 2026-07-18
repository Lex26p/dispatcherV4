# PROJECT_STATE

## Product
- Name: Диспетчер
- Repository: dispatcherV4
- Master specification: DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md
- AI guide: DISPATCHER_AI_IMPLEMENTATION_SPEC.md
- Stack: .NET / ASP.NET Core / Blazor / PostgreSQL / SignalR / Worker Services

## Current phase
- Phase: Foundation
- Status: In progress

## Current sprint
- Sprint: 1
- Goal: Архитектурный baseline и рабочий репозиторий

## Current step
- Step: 4
- Name: PostgreSQL infrastructure
- Status: Ready for local verification
- Started: 2026-07-18T00:00:00Z
- Completed: pending local verification and commit

## Completed steps
| Step | Date UTC | Commit | Result | Notes |
|---|---|---|---|---|
| 0A | 2026-07-18 | 4c889a93c62a2612cac4651029d39d5f4113742d | Repository cleanup and industrial baseline reset | Old training code removed without rewriting Git history |
| 0 | 2026-07-18 | 4b9ef12bac7a5acff540b11daba07c9c65d9504f | Repository preparation completed | Root documentation and AI guide baseline committed |
| 1 | 2026-07-18 | 371743d192c45ab7c3c5501dee11b81c7df08ba3 | Solution skeleton completed | API, Web, Domain, Application, Infrastructure, Contracts, workers and tests created; Step 1 build/test fixes included |
| 2 | 2026-07-18 | 19d3f062ffd0e12b43e5213bfa96ea33c0e84a69 | Shared contracts and API correlation completed | Correlation ID, problem details, paging contracts and development diagnostics endpoint added |
| 3 | 2026-07-18 | 6a9fd8d69a363677d7fabcd7dd54d418f7b2acb7 | Domain primitives completed | Framework-free primitives for identifiers, timestamps, quality, freshness and typed values |

## Architecture decisions
| ADR | Decision | Status | Consequences |
|---|---|---|---|
| ADR-0001 | Use C#/.NET, ASP.NET Core, Blazor, PostgreSQL, SignalR and .NET Worker Services as first industrial baseline | Accepted | C++ is deferred to future measured extraction candidates |
| Step 2 | Use explicit public contracts for correlation, paging and problem details | Accepted | Public API shape is not coupled to EF entities or exception types |
| Step 3 | Keep domain primitives framework-free and protocol-neutral | Accepted | Domain can be tested without EF Core, ASP.NET Core, SignalR, UI or protocol dependencies |
| Step 4 | Use PostgreSQL through EF Core baseline migration | Accepted | Later vertical slices add schemas/tables incrementally; no business table is created in Step 4 |

## Created projects
| Project | Purpose | Created in step | Build status |
|---|---|---|---|
| Dispatcher.Api | ASP.NET Core API composition root, health endpoints, correlation and exception middleware | 1, 2, 4 | To verify locally |
| Dispatcher.Web | Blazor WebAssembly shell skeleton and `/home` route | 1 | To verify locally |
| Dispatcher.Domain | Domain primitives and future bounded-context folders | 1, 3 | To verify locally |
| Dispatcher.Application | Application abstractions, current user placeholder, correlation context and DI registration | 1, 2 | To verify locally |
| Dispatcher.Infrastructure | Infrastructure adapters baseline, system clock and EF Core PostgreSQL DbContext | 1, 4 | To verify locally |
| Dispatcher.Contracts | Public REST/SignalR contracts, problem details, paging, correlation and readiness contracts | 1, 2, 4 | To verify locally |
| Dispatcher.Telemetry.Worker | Future telemetry runtime host skeleton | 1 | To verify locally |
| Dispatcher.Workers | Future background jobs host skeleton | 1 | To verify locally |
| Dispatcher.UnitTests | Unit tests for contracts, smoke checks and domain primitives | 1, 2, 3 | To verify locally |
| Dispatcher.IntegrationTests | Integration test project with optional PostgreSQL smoke test | 1, 4 | To verify locally |

## Database status
- Provider: PostgreSQL via `Npgsql.EntityFrameworkCore.PostgreSQL`
- Connection method: `ConnectionStrings:DispatcherDatabase` or `DISPATCHER_CONNECTION_STRING`
- Schemas created: none yet; schema constants defined for future contexts
- Current migration: `20260718000000_BaselineDatabase`
- Clean install verified: pending local PostgreSQL verification
- Upgrade verified: pending local PostgreSQL verification

## Migrations
| Migration | Step | Applied locally | Roll-forward tested | Notes |
|---|---|---|---|---|
| 20260718000000_BaselineDatabase | 4 | pending | pending | Empty EF Core baseline; creates only EF migrations history when applied |

## API endpoints
| Method | Route | Authorization | Implemented in step | Tests |
|---|---|---|---|---|
| GET | `/api/health/live` | Anonymous | 1 | Manual smoke; response has correlation header after Step 2 |
| GET | `/api/health/ready` | Anonymous | 1, PostgreSQL check in 4 | Manual smoke; returns 200 when DB reachable and 503 when unavailable |
| GET | `/` | Anonymous | 1 | Redirects to `/api/health/live`; manual smoke required |
| GET | `/api/diagnostics/exception` | Development only | 2 | Manual smoke for exception middleware/problem details |

## Frontend routes
| Route | Status | Authorization UX | Smoke test |
|---|---|---|---|
| `/` | Redirects to `/home` | None yet | Manual browser smoke required |
| `/home` | Implemented skeleton | None yet | Manual browser smoke required |
| `/not-found` | Implemented skeleton | None yet | Manual browser smoke required |

## Workers
| Worker/job | Host | Schedule/trigger | Health/metrics | Status |
|---|---|---|---|---|
| Telemetry Worker skeleton | Dispatcher.Telemetry.Worker | BackgroundService placeholder | Logs startup | To verify locally |
| General Worker skeleton | Dispatcher.Workers | BackgroundService placeholder | Logs startup | To verify locally |

## Domain primitives
| Primitive | Namespace | Purpose | Added in step |
|---|---|---|---|
| EntityId | Dispatcher.Domain.Common | Non-empty stable domain identifier | 3 |
| DomainError | Dispatcher.Domain.Common | Machine-readable domain validation/business error | 3 |
| UtcTimestamp | Dispatcher.Domain.Common | UTC-normalized timestamp | 3 |
| ConcurrencyToken | Dispatcher.Domain.Common | Opaque optimistic-concurrency token | 3 |
| DataQuality | Dispatcher.Domain.Telemetry | Protocol-neutral quality state | 3 |
| FreshnessState | Dispatcher.Domain.Telemetry | Fresh/stale/offline/initializing trust state | 3 |
| TypedValue | Dispatcher.Domain.Telemetry | Protocol-neutral scalar value with optional unit | 3 |

## Known limitations
- No business tables yet; Step 4 intentionally creates only an empty EF Core baseline migration.
- Clean install and DB-ready health require local PostgreSQL and a local non-committed connection string.
- No authentication/RBAC yet; planned for Step 5.
- `ICurrentUser` is currently an anonymous placeholder until Step 5.
- No real telemetry, SignalR, dashboards, alarms or maintenance yet.
- Step 2 diagnostics exception endpoint is development-only and must not become a business API.

## Next steps
1. Step 5 — Identity/RBAC baseline.

## Commit hash history
| Date UTC | Step | Commit hash | Message |
|---|---|---|---|
| 2026-07-18 | 0A | 4c889a93c62a2612cac4651029d39d5f4113742d | Step 0A: Reset repository for industrial Dispatcher baseline |
| 2026-07-18 | 0 | 4b9ef12bac7a5acff540b11daba07c9c65d9504f | Step 0: Complete repository preparation |
| 2026-07-18 | 1 | 371743d192c45ab7c3c5501dee11b81c7df08ba3 | Fix Step 1 test project global usings |
| 2026-07-18 | 2 | 19d3f062ffd0e12b43e5213bfa96ea33c0e84a69 | Step 2: Add shared contracts and API correlation |
| 2026-07-18 | 3 | 6a9fd8d69a363677d7fabcd7dd54d418f7b2acb7 | Step 3: Add domain primitives |
| 2026-07-18 | 4 | pending | Step 4: Add PostgreSQL infrastructure |
