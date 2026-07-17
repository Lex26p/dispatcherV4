# Mock polling worker

`Dispatcher.Worker` contains a mock polling worker that can generate simulated current values for enabled tags.

The worker is disabled by default:

```json
"MockPolling": {
  "Enabled": false
}
```

Keep it disabled while PostgreSQL is not ready.

After PostgreSQL is installed, the database is updated, and test devices/tags exist, enable it in `src/Dispatcher.Worker/appsettings.Development.json`:

```json
"MockPolling": {
  "Enabled": true,
  "IntervalSeconds": 5,
  "DisabledDelaySeconds": 30,
  "MinValue": 0,
  "MaxValue": 100,
  "MaxTagsPerCycle": 1000
}
```

Run the worker:

```powershell
dotnet run --project .\src\Dispatcher.Worker\Dispatcher.Worker.csproj
```

The worker reads enabled tags through `ITagRepository` and upserts values through `ITagValueRepository`.
