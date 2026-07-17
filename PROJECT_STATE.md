# Dispatcher Project State

## Product

Product name: Диспетчер  
Repository: dispatcherV4  
Solution: Dispatcher.sln  
Root namespace: Dispatcher  
Main branch: master  

## Technology Stack

- Backend: ASP.NET Core / C#
- Frontend: Blazor WebAssembly
- Realtime: SignalR
- Database: PostgreSQL, later TimescaleDB
- Workers: .NET Worker Service
- High-performance modules: possible future C++ service after performance measurements
- Development OS: Windows
- IDE: Visual Studio 2026
- Target framework: .NET 10

## Architecture Decisions

- MVP is implemented in C#.
- C++ polling engine is postponed until performance measurements.
- Frontend is Blazor WebAssembly.
- Realtime updates will use SignalR.
- Devices are read-only in MVP.
- Future SCADA control must be considered architecturally.
- Email is the only notification channel in the first version.
- Docker, backup, mobile version and audit log are postponed.
- Work is done in the master branch.
- Terminal commands are written as one command per line.

## Current Solution Structure

```text
/src
 ├─ Dispatcher.Api
 ├─ Dispatcher.Web
 ├─ Dispatcher.Worker
 ├─ Dispatcher.Application
 ├─ Dispatcher.Domain
 ├─ Dispatcher.Infrastructure
 └─ Dispatcher.Shared
```

## Completed Steps

### Step 00 — Planning

Done:
- Defined product name: Диспетчер.
- Defined repository name: dispatcherV4.
- Defined solution name: Dispatcher.sln.
- Defined namespace: Dispatcher.
- Defined MVP stack.
- Defined workflow with zip archives and PowerShell commands.
- Decided to work only on master branch.
- Decided to maintain this PROJECT_STATE.md file after every step.

Commit:
- Not committed yet.

## Current Step

Step 01 — Create solution structure.

## Next Steps

1. Create Dispatcher.sln.
2. Create base projects.
3. Add projects to solution.
4. Add project references.
5. Add project documentation files.
6. Build solution.
7. Commit changes.

## Backlog

- Domain entities.
- Application interfaces.
- EF Core infrastructure.
- PostgreSQL connection.
- Device CRUD API.
- Tag CRUD API.
- Mock polling worker.
- SignalR realtime updates.
- Blazor device and tag pages.
- Modbus TCP polling.
- SNMP polling.
- Tag history.
- Alarm rules.
- Alarm journal.
- Email notifications.
- Mnemoscheme page.
- User roles.
- C++ polling engine evaluation.
- Docker.
- Backup.
- Audit log.
