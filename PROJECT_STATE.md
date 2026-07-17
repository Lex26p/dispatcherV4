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
- ORM: Entity Framework Core
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
- Application layer defines repository and service contracts.
- Application service implementations coordinate domain objects through repository interfaces.
- Infrastructure layer implements persistence contracts with EF Core and PostgreSQL provider.
- The default development connection string is temporary and will be moved to configuration during API wiring.
- Each assistant step starts with an estimate of remaining steps until the first MVP.

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

### Step 04 — Add Application contracts

Done:
- Added repository interfaces for devices, tags and current values.
- Added UnitOfWork and Clock abstractions.
- Added Application DTOs for devices, tags and current values.
- Added Application service contracts for devices, tags and current values.
- Added command records for creating Modbus and SNMP devices/tags.
- Built solution successfully.

Commit:
- 4c3876b75a0b7ab8dc83083df2bd457774db4dc9

### Step 05 — Add Infrastructure EF Core persistence

Done:
- Added EF Core package references to Dispatcher.Infrastructure.
- Added DispatcherDbContext.
- Added EF Core configurations for Device, Tag and TagValue.
- Added repository implementations for devices, tags and current tag values.
- Added SystemClock implementation.
- Added Infrastructure dependency injection extension.
- Built solution successfully.

Commit:
- dc9d8f050238494f06a240afa7db6dbf7743f8f2

## Current Step

Step 06 — Add Application service implementations.

## Next Steps

1. Add Application dependency injection extension.
2. Add DeviceService implementation.
3. Add TagService implementation.
4. Add TagValueService implementation.
5. Build solution.
6. Commit changes.

## Backlog

- Alarm entities.
- Notification entities.
- User entity.
- API wiring for Application and Infrastructure.
- PostgreSQL connection configuration.
- EF Core migrations.
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
