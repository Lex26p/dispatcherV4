# Dispatcher Project State

## Product

Product name: Диспетчер  
Repository: dispatcherV4  
Solution: Dispatcher.slnx  
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
- The solution uses the Visual Studio solution file format `Dispatcher.slnx`.
- Application layer defines repository and service contracts, but concrete persistence is implemented later in Infrastructure.

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
- Defined solution name: Dispatcher.slnx.
- Defined namespace: Dispatcher.
- Defined MVP stack.
- Defined workflow with zip archives and PowerShell commands.
- Decided to work only on master branch.
- Decided to maintain this PROJECT_STATE.md file after every step.

Commit:
- Not committed yet.

### Step 01 — Create solution structure

Done:
- Created base solution structure.
- Created projects: Api, Web, Worker, Domain, Application, Infrastructure, Shared.
- Added project references.
- Added project documentation and repository settings.
- Added explicit Microsoft.OpenApi 2.7.5 package reference to avoid vulnerable transitive 2.0.0 package.
- Built solution successfully.

Commit:
- ffda784a074bb43a6930f16708950157923940dd

### Step 02 — Add Domain base classes and enums

Done:
- Added common Domain base classes: Entity, AggregateRoot, DomainException.
- Added base enums for devices.
- Added base enums for tags.
- Added base enums for alarms.
- Added base enums for notifications and users.
- Updated project state file for Dispatcher.slnx.
- Built solution successfully.

Commit:
- 71df25e4c2552af05a25f5619d53e23ff1ed0fc9

### Step 03 — Add Device and Tag domain entities

Done:
- Added Device aggregate.
- Added DeviceConnectionSettings value object.
- Added ModbusTagAddress value object.
- Added SnmpTagAddress value object.
- Added Tag aggregate.
- Added TagValue entity for current values.
- Built solution successfully.

Commit:
- 788ffa51f9fdad613ef75a82c4db9d71d1a4063c

## Current Step

Step 04 — Add Application contracts for devices, tags and current values.

## Next Steps

1. Add repository interfaces for devices, tags and current values.
2. Add UnitOfWork and Clock abstractions.
3. Add Application DTOs for devices, tags and current values.
4. Add Application service contracts for devices, tags and current values.
5. Build solution.
6. Commit changes.

## Backlog

- Alarm entities.
- Notification entities.
- User entity.
- Application service implementations.
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
