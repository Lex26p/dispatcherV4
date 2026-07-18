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
- Sprint: 2
- Goal: Identity, roles, scopes и shell

## Current step
- Step: 5
- Name: Identity/RBAC baseline
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
| 4 | 2026-07-18 | 714cece5c877f3636e5c28af7dc3c6cc991cc01a | PostgreSQL infrastructure completed | EF Core PostgreSQL baseline, empty migration and ready health check added |
| 4B | 2026-07-18 | local verification only | Local PostgreSQL migration verified | PostgreSQL 17 installed locally; `dispatcher` DB created; baseline migration applied; `/api/health/ready` returned 200; no repository changes |

## Architecture decisions
| ADR | Decision | Status | Consequences |
|---|---|---|---|
| ADR-0001 | Use C#/.NET, ASP.NET Core, Blazor, PostgreSQL, SignalR and .NET Worker Services as first industrial baseline | Accepted | C++ is deferred to future measured extraction candidates |
| Step 2 | Use explicit public contracts for correlation, paging and problem details | Accepted | Public API shape is not coupled to EF entities or exception types |
| Step 3 | Keep domain primitives framework-free and protocol-neutral | Accepted | Domain can be tested without EF Core, ASP.NET Core, SignalR, UI or protocol dependencies |
| Step 4 | Use PostgreSQL through EF Core baseline migration | Accepted | Later vertical slices add schemas/tables incrementally; no business table is created in Step 4 |
| Step 5 | Use development header current user until production auth provider is selected | Accepted | Backend permission checks exist now, but production authentication remains a known limitation |

## Created projects
| Project | Purpose | Created in step | Build status |
|---|---|---|---|
| Dispatcher.Api | ASP.NET Core API composition root, health endpoints, correlation, exception middleware and identity endpoints | 1, 2, 4, 5 | To verify locally |
| Dispatcher.Web | Blazor WebAssembly shell skeleton and baseline identity/admin routes | 1, 5 | To verify locally |
| Dispatcher.Domain | Domain primitives and identity access entities | 1, 3, 5 | To verify locally |
| Dispatcher.Application | Application abstractions, current user placeholder, correlation context, permission constants and DI registration | 1, 2, 5 | To verify locally |
| Dispatcher.Infrastructure | Infrastructure adapters baseline, system clock, EF Core PostgreSQL DbContext and identity mappings | 1, 4, 5 | To verify locally |
| Dispatcher.Contracts | Public REST/SignalR contracts, problem details, paging, correlation, readiness and identity contracts | 1, 2, 4, 5 | To verify locally |
| Dispatcher.Telemetry.Worker | Future telemetry runtime host skeleton | 1 | To verify locally |
| Dispatcher.Workers | Future background jobs host skeleton | 1 | To verify locally |
| Dispatcher.UnitTests | Unit tests for contracts, smoke checks, domain primitives and identity domain rules | 1, 2, 3, 5 | To verify locally |
| Dispatcher.IntegrationTests | Integration test project with optional PostgreSQL and identity model smoke tests | 1, 4, 5 | To verify locally |

## Database status
- Provider: PostgreSQL 17 locally; EF Core via `Npgsql.EntityFrameworkCore.PostgreSQL`
- Connection method: `ConnectionStrings:DispatcherDatabase` or `DISPATCHER_CONNECTION_STRING`
- Schemas created: `identity_access`
- Current migration: `20260718001000_AddIdentityAccessBaseline`
- Clean install verified: baseline migration applied locally on 2026-07-18
- Upgrade verified: Step 5 migration pending local verification

## Migrations
| Migration | Step | Applied locally | Roll-forward tested | Notes |
|---|---|---|---|---|
| 20260718000000_BaselineDatabase | 4 | yes | yes | Empty EF Core baseline; creates EF migrations history when applied |
| 20260718001000_AddIdentityAccessBaseline | 5 | pending | pending | Creates identity schema, users, roles, scopes, role assignments and development admin seed |

## API endpoints
| Method | Route | Authorization | Implemented in step | Tests |
|---|---|---|---|---|
| GET | `/api/health/live` | Anonymous | 1 | Manual smoke; response has correlation header after Step 2 |
| GET | `/api/health/ready` | Anonymous | 1, PostgreSQL check in 4 | Manual smoke; returned 200 after local PostgreSQL verification |
| GET | `/` | Anonymous | 1 | Redirects to `/api/health/live`; manual smoke required |
| GET | `/api/diagnostics/exception` | Development only | 2 | Manual smoke for exception middleware/problem details |
| GET | `/api/me` | Development current user | 5 | Manual smoke pending |
| GET | `/api/users` | `identity.users.view` | 5 | Manual smoke pending |
| GET | `/api/users/{id}` | `identity.users.view` | 5 | Manual smoke pending |
| GET | `/api/roles` | `identity.roles.view` | 5 | Manual smoke pending |
| GET | `/api/permission-scopes` | `identity.scopes.view` | 5 | Manual smoke pending |
| GET | `/api/role-assignments` | `identity.assignments.view` | 5 | Manual smoke pending |
| POST | `/api/role-assignments` | `identity.assignments.manage` | 5 | Manual smoke pending |
| POST | `/api/role-assignments/{id}/revoke` | `identity.assignments.manage` | 5 | Manual smoke pending; last-admin guard included |

## Frontend routes
| Route | Status | Authorization UX | Smoke test |
|---|---|---|---|
| `/` | Redirects to `/home` | None yet | Manual browser smoke required |
| `/home` | Implemented skeleton | None yet | Manual browser smoke required |
| `/me` | Implemented skeleton | Permission-aware UX pending Step 6 | Manual browser smoke required |
| `/admin/users` | Implemented skeleton | Permission-aware UX pending Step 6 | Manual browser smoke required |
| `/forbidden` | Implemented skeleton | None | Manual browser smoke required |
| `/not-found` | Implemented skeleton | None | Manual browser smoke required |

## Workers
| Worker/job | Host | Schedule/trigger | Health/metrics | Status |
|---|---|---|---|---|
| Telemetry Worker skeleton | Dispatcher.Telemetry.Worker | BackgroundService placeholder | Logs startup | To verify locally |
| General Worker skeleton | Dispatcher.Workers | BackgroundService placeholder | Logs startup | To verify locally |

## Domain primitives and entities
| Type | Namespace | Purpose | Added in step |
|---|---|---|---|
| EntityId | Dispatcher.Domain.Common | Non-empty stable domain identifier | 3 |
| DomainError | Dispatcher.Domain.Common | Machine-readable domain validation/business error | 3 |
| UtcTimestamp | Dispatcher.Domain.Common | UTC-normalized timestamp | 3 |
| ConcurrencyToken | Dispatcher.Domain.Common | Opaque optimistic-concurrency token | 3 |
| DataQuality | Dispatcher.Domain.Telemetry | Protocol-neutral quality state | 3 |
| FreshnessState | Dispatcher.Domain.Telemetry | Fresh/stale/offline/initializing trust state | 3 |
| TypedValue | Dispatcher.Domain.Telemetry | Protocol-neutral scalar value with optional unit | 3 |
| UserAccount | Dispatcher.Domain.IdentityAccess | User identity/account record | 5 |
| Role | Dispatcher.Domain.IdentityAccess | Role with normalized machine permissions | 5 |
| PermissionScope | Dispatcher.Domain.IdentityAccess | Scope boundary for assignments | 5 |
| RoleAssignment | Dispatcher.Domain.IdentityAccess | Grant/revoke lifecycle with reason and last-admin guard at API layer | 5 |

## Known limitations
- Authentication is development-header based only; production provider is not selected yet.
- Permission-aware navigation is not implemented yet; Step 6 will improve shell behavior.
- Audit hooks are documented but not persisted until the audit step.
- No business tables beyond identity baseline yet.
- No real telemetry, SignalR, dashboards, alarms or maintenance yet.
- Step 2 diagnostics exception endpoint is development-only and must not become a business API.

## Next steps
1. Step 6 — Web shell baseline.

## Commit hash history
| Date UTC | Step | Commit hash | Message |
|---|---|---|---|
| 2026-07-18 | 0A | 4c889a93c62a2612cac4651029d39d5f4113742d | Step 0A: Reset repository for industrial Dispatcher baseline |
| 2026-07-18 | 0 | 4b9ef12bac7a5acff540b11daba07c9c65d9504f | Step 0: Complete repository preparation |
| 2026-07-18 | 1 | 371743d192c45ab7c3c5501dee11b81c7df08ba3 | Fix Step 1 test project global usings |
| 2026-07-18 | 2 | 19d3f062ffd0e12b43e5213bfa96ea33c0e84a69 | Step 2: Add shared contracts and API correlation |
| 2026-07-18 | 3 | 6a9fd8d69a363677d7fabcd7dd54d418f7b2acb7 | Step 3: Add domain primitives |
| 2026-07-18 | 4 | 714cece5c877f3636e5c28af7dc3c6cc991cc01a | Fix Step 4 baseline migration build |
| 2026-07-18 | 5 | pending | Step 5: Add Identity/RBAC baseline |
