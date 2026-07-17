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
- Step: 2
- Name: shared contracts and project state
- Status: Ready for local verification
- Started: 2026-07-18T00:00:00Z
- Completed: pending local verification and commit

## Completed steps
| Step | Date UTC | Commit | Result | Notes |
|---|---|---|---|---|
| 0A | 2026-07-18 | 4c889a93c62a2612cac4651029d39d5f4113742d | Repository cleanup and industrial baseline reset | Old training code removed without rewriting Git history |
| 0 | 2026-07-18 | 4b9ef12bac7a5acff540b11daba07c9c65d9504f | Repository preparation completed | Root documentation and AI guide baseline committed |
| 1 | 2026-07-18 | 371743d192c45ab7c3c5501dee11b81c7df08ba3 | Solution skeleton completed | API, Web, Domain, Application, Infrastructure, Contracts, workers and tests created; Step 1 build/test fixes included |

## Architecture decisions
| ADR | Decision | Status | Consequences |
|---|---|---|---|
| ADR-0001 | Use C#/.NET, ASP.NET Core, Blazor, PostgreSQL, SignalR and .NET Worker Services as first industrial baseline | Accepted | C++ is deferred to future measured extraction candidates |
| Step 2 | Use explicit public contracts for correlation, paging and problem details | Accepted | Public API shape is not coupled to EF entities or exception types |

## Created projects
| Project | Purpose | Created in step | Build status |
|---|---|---|---|
| Dispatcher.Api | ASP.NET Core API composition root, health endpoints, correlation and exception middleware | 1, 2 | To verify locally |
| Dispatcher.Web | Blazor WebAssembly shell skeleton and `/home` route | 1 | To verify locally |
| Dispatcher.Domain | Domain primitives and future bounded-context folders | 1 | To verify locally |
| Dispatcher.Application | Application abstractions, current user placeholder, correlation context and DI registration | 1, 2 | To verify locally |
| Dispatcher.Infrastructure | Infrastructure adapters baseline and system clock | 1 | To verify locally |
| Dispatcher.Contracts | Public REST/SignalR contracts, problem details, paging and correlation constants | 1, 2 | To verify locally |
| Dispatcher.Telemetry.Worker | Future telemetry runtime host skeleton | 1 | To verify locally |
| Dispatcher.Workers | Future background jobs host skeleton | 1 | To verify locally |
| Dispatcher.UnitTests | Unit tests for contracts and smoke checks | 1, 2 | To verify locally |
| Dispatcher.IntegrationTests | Integration test project | 1 | To verify locally |

## Database status
- Provider: PostgreSQL
- Connection method: not configured yet
- Schemas created: none
- Current migration: none
- Clean install verified: No
- Upgrade verified: No

## Migrations
| Migration | Step | Applied locally | Roll-forward tested | Notes |
|---|---|---|---|---|
| None | 2 | No | No | Database infrastructure starts at Step 4 |

## API endpoints
| Method | Route | Authorization | Implemented in step | Tests |
|---|---|---|---|---|
| GET | `/api/health/live` | Anonymous | 1 | Manual smoke; response has correlation header after Step 2 |
| GET | `/api/health/ready` | Anonymous | 1 | Manual smoke; response has correlation header after Step 2 |
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

## Known limitations
- No PostgreSQL integration yet; planned for Step 4.
- No authentication/RBAC yet; planned for Step 5.
- `ICurrentUser` is currently an anonymous placeholder until Step 5.
- No real telemetry, SignalR, dashboards, alarms or maintenance yet.
- Step 2 diagnostics exception endpoint is development-only and must not become a business API.

## Next steps
1. Step 3 — domain primitives.

## Commit hash history
| Date UTC | Step | Commit hash | Message |
|---|---|---|---|
| 2026-07-18 | 0A | 4c889a93c62a2612cac4651029d39d5f4113742d | Step 0A: Reset repository for industrial Dispatcher baseline |
| 2026-07-18 | 0 | 4b9ef12bac7a5acff540b11daba07c9c65d9504f | Step 0: Complete repository preparation |
| 2026-07-18 | 1 | 371743d192c45ab7c3c5501dee11b81c7df08ba3 | Fix Step 1 test project global usings |
| 2026-07-18 | 2 | pending | Step 2: Add shared contracts and API correlation |
