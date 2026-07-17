# Step 2 — shared contracts and API correlation

## Goal

Add stable shared API contracts, correlation ID middleware, exception handling middleware, and baseline tests.

## Verification

Run:

```powershell
cd C:\Projects\dispatcherV4
dotnet restore .\Dispatcher.sln
dotnet build .\Dispatcher.sln --no-restore
dotnet test .\Dispatcher.sln --no-build
```

Run API and verify:

```powershell
Invoke-WebRequest http://localhost:5076/api/health/live | Select-Object StatusCode,Headers
Invoke-WebRequest -Headers @{ 'X-Correlation-ID' = 'manual-step-02' } http://localhost:5076/api/health/ready | Select-Object StatusCode,Headers
Invoke-WebRequest http://localhost:5076/api/diagnostics/exception -SkipHttpErrorCheck | Select-Object StatusCode,Content,Headers
```

## Known limitations

- Authentication is not implemented yet.
- Validation-specific error mapping is not implemented yet.
- Diagnostics endpoint is development-only.
