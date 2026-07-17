# PROJECT_STATE

## Product
- Name: Диспетчер
- Repository: dispatcherV4
- Master specification: DISPATCHER_TECHNICAL_SPECIFICATION_AND_ROADMAP.md
- AI guide: DISPATCHER_AI_IMPLEMENTATION_SPEC.md
- Stack: .NET / ASP.NET Core / Blazor / PostgreSQL / SignalR / Worker Services
- Target branch: master

## Current phase
- Phase: Preparation
- Status: In progress

## Current sprint
- Sprint: 1
- Goal: Архитектурный baseline и рабочий репозиторий

## Current step
- Step: 0A
- Name: Repository cleanup and baseline reset
- Status: Pending commit
- Started: 2026-07-18T00:00:00Z
- Completed: null

## Completed steps
| Step | Date UTC | Commit | Result | Notes |
|---|---|---|---|---|
| 0A | pending | pending | Repository cleanup baseline prepared | Старый тренировочный код удаляется обычным commit без переписывания Git history. |

## Architecture decisions
| ADR | Decision | Status | Consequences |
|---|---|---|---|
| ADR-0001 | Industrial Dispatcher baseline starts from clean repository contents while preserving Git history | Accepted | Старый учебный код не переносится автоматически; product architecture follows master-ТЗ and AI guide. |

## Created projects
| Project | Purpose | Created in step | Build status |
|---|---|---|---|
| none | Solution skeleton is not created yet | n/a | n/a |

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
| none | n/a | No | No | Database infrastructure starts in Step 4. |

## API endpoints
| Method | Route | Authorization | Implemented in step | Tests |
|---|---|---|---|---|
| none | n/a | n/a | n/a | API solution starts in Step 1. |

## Frontend routes
| Route | Status | Authorization UX | Smoke test |
|---|---|---|---|
| none | Not implemented | n/a | No |

## Workers
| Worker/job | Host | Schedule/trigger | Health/metrics | Status |
|---|---|---|---|---|
| none | n/a | n/a | n/a | Worker hosts start in Step 1. |

## Known limitations
- Repository contains documentation baseline only after Step 0A.
- No .NET solution exists until Step 1.
- No API, UI, database, workers or tests exist yet.
- Existing Git history still contains earlier training prototype commits; this is intentional and not a production code dependency.

## Next steps
1. Step 0 — подготовка репозитория по `DISPATCHER_AI_IMPLEMENTATION_SPEC.md`.
2. Step 1 — solution skeleton.

## Commit hash history
| Date UTC | Step | Commit hash | Message |
|---|---|---|---|
| pending | 0A | pending | Reset repository for industrial Dispatcher baseline |
