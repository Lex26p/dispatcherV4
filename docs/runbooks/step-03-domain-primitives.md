# Step 3 — domain primitives

## Goal

Add minimal domain primitives required by the first industrial vertical slices without introducing EF Core, ASP.NET Core, SignalR, UI, or protocol dependencies into `Dispatcher.Domain`.

## Added primitives

- `EntityId`
- `DomainError`
- `UtcTimestamp`
- `ConcurrencyToken`
- `DataQuality`
- `FreshnessKind`
- `FreshnessState`
- `TypedValue`
- `TypedValueKind`

## Verification

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

## Rules

- `Dispatcher.Domain` must not reference EF Core, ASP.NET Core, SignalR, Web, API, or Infrastructure assemblies.
- `DataQuality` and `FreshnessState` are related but separate concepts.
- `TypedValue` is protocol-neutral and must not expose Modbus/SNMP register/OID details.
- `UtcTimestamp` normalizes values to UTC.

## Known limitations

- No product entities are created in this step.
- No persistence mapping is created in this step.
- No API endpoint is added in this step.
