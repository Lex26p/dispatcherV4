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
- Realtime updates use SignalR.
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
- The default development connection string is temporary and will be moved to secure configuration later.
- EF Core migrations are generated into `Dispatcher.Infrastructure`.
- PostgreSQL is not ready locally yet; database update can be postponed.
- Mock polling worker is disabled by default until PostgreSQL and test tags are ready.
- API exposes a SignalR hub for current tag value updates.
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

/docs
 └─ development
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

### Step 06 — Add Application service implementations

Done:
- Added Application dependency injection extension.
- Added DeviceService implementation.
- Added TagService implementation.
- Added TagValueService implementation.
- Built solution successfully.

Commit:
- c81f217a0189b9ba5d0f07f0b4efe99475b24bce

### Step 07 — Wire API and add device endpoints

Done:
- Connected API to Application layer.
- Connected API to Infrastructure layer.
- Added health endpoint.
- Added device endpoints.
- Removed default WeatherForecast endpoint.
- Verified API starts on localhost.
- Built solution successfully.

Commit:
- 4af0e4d875c796c7569fb1f55f5ca55cc3dbb38f

### Step 08 — Add EF Core migration tooling and create initial database migration

Done:
- Added local dotnet-ef tool manifest.
- Added EF Core design package references.
- Added design-time DispatcherDbContext factory.
- Kept HTTPS redirection disabled in Development to avoid local HTTP warning noise.
- Added migration setup.
- PostgreSQL database update was postponed because PostgreSQL is not ready locally yet.
- Built solution successfully.

Commit:
- 3d42910f282777bdaeadeef5467ec12490b9c328

### Step 09 — Align EF Core package versions

Done:
- Aligned EF Core related package versions.
- Aligned Worker hosting package version with the rest of the solution.
- Removed MSB3277 assembly version conflict warning.
- Built solution successfully.

Commit:
- ec88c0d17206d4b49a5b5241bd2b88a0c38e42f8

### Step 10 — Add Tag API endpoints

Done:
- Added API contracts for creating Modbus tags.
- Added API contracts for creating SNMP tags.
- Added tag endpoints.
- Registered tag endpoints in API startup.
- Built solution successfully.

Commit:
- 5e837f325984f1cfc7b5f5afae22e5c45ede1e41

### Step 11 — Add current tag value API endpoints

Done:
- Added current tag value API contract.
- Added current tag value endpoints.
- Registered current tag value endpoints in API startup.
- Built solution successfully.

Commit:
- ffdca97b4d069ffa1dd10ba0def432f934e4043d

### Step 12 — Add mock polling worker

Done:
- Registered Application and Infrastructure services in Worker.
- Added mock polling options.
- Added mock polling background service.
- Kept mock polling disabled by default until PostgreSQL is ready.
- Added worker configuration and documentation.
- Built solution successfully.

Commit:
- 8a700ff9dc9eca7cc5aba793bfe95b521e2c7e47

## Current Step

Step 13 — Add SignalR realtime API foundation.

## Next Steps

1. Add SignalR hub for current tag values.
2. Add realtime constants.
3. Add current tag value broadcaster abstraction and SignalR implementation.
4. Add realtime metadata endpoint.
5. Register SignalR and hub route in API startup.
6. Broadcast current tag value changes made through API endpoints.
7. Build solution.
8. Commit changes.

## Backlog

- Alarm entities.
- Notification entities.
- User entity.
- PostgreSQL local setup and database update.
- Worker-to-API realtime integration.
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
