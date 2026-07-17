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
- EF Core migrations are generated into `Dispatcher.Infrastructure`.
- PostgreSQL is not ready locally yet; database update can be postponed.
- Mock polling worker is disabled by default until PostgreSQL and test tags are ready.
- Blazor WebAssembly calls the API through configurable `ApiBaseUrl`.
- API allows Blazor local development origins with a development CORS policy.
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

Commit:
- ffda784a074bb43a6930f16708950157923940dd

### Step 02 — Add Domain base classes and enums

Commit:
- 71df25e4c2552af05a25f5619d53e23ff1ed0fc9

### Step 03 — Add Device and Tag domain entities

Commit:
- 788ffa51f9fdad613ef75a82c4db9d71d1a4063c

### Step 04 — Add Application contracts

Commit:
- 4c3876b75a0b7ab8dc83083df2bd457774db4dc9

### Step 05 — Add Infrastructure EF Core persistence

Commit:
- dc9d8f050238494f06a240afa7db6dbf7743f8f2

### Step 06 — Add Application service implementations

Commit:
- c81f217a0189b9ba5d0f07f0b4efe99475b24bce

### Step 07 — Wire API and add device endpoints

Commit:
- 4af0e4d875c796c7569fb1f55f5ca55cc3dbb38f

### Step 08 — Add EF Core migration tooling and create initial database migration

Done:
- Added migration tooling.
- PostgreSQL database update was postponed because PostgreSQL is not ready locally yet.

Commit:
- 3d42910f282777bdaeadeef5467ec12490b9c328

### Step 09 — Align EF Core package versions

Commit:
- ec88c0d17206d4b49a5b5241bd2b88a0c38e42f8

### Step 10 — Add Tag API endpoints

Commit:
- 5e837f325984f1cfc7b5f5afae22e5c45ede1e41

### Step 11 — Add current tag value API endpoints

Commit:
- ffdca97b4d069ffa1dd10ba0def432f934e4043d

### Step 12 — Add mock polling worker

Commit:
- 8a700ff9dc9eca7cc5aba793bfe95b521e2c7e47

### Step 13 — Add SignalR realtime API foundation

Commit:
- ff061137964ae183c870294ba88bf43538dbd4f5

### Step 14 — Add Blazor WebAssembly UI foundation

Commit:
- 8f241dce0f24782a85ba570aafcf8b1c74b06403

### Step 15 — Add Blazor devices UI

Done:
- Added Blazor device models.
- Added device API client methods.
- Added device list page.
- Added Modbus TCP and SNMP device creation forms.
- Added enable and disable actions for devices.
- Built solution successfully.

Commit:
- 9ae61aaa75bd0ecdea6ed4e3374ab376a8b16ec5

## Current Step

Step 16 — Add Blazor tags UI.

## Next Steps

1. Add Blazor tag models.
2. Add tag API methods to DispatcherApiClient.
3. Replace the tags placeholder page with a working tag management page.
4. Add documentation for the tags page.
5. Build solution.
6. Commit changes.

## Backlog

- PostgreSQL local setup and database update.
- Blazor realtime client.
- Blazor current tag value page.
- Modbus TCP polling.
- SNMP polling.
- Tag history.
- Alarm entities.
- Alarm rules.
- Alarm journal.
- Notification entities.
- Email notifications.
- User entity.
- User roles.
- Mnemoscheme page.
- C++ polling engine evaluation.
- Docker.
- Backup.
- Audit log.
