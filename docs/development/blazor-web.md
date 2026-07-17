# Blazor WebAssembly development

## Purpose

`Dispatcher.Web` is the Blazor WebAssembly client for the product UI.

Current step adds:

- Russian navigation shell.
- API client configuration.
- Dashboard page.
- System status page.
- Placeholder pages for devices and tags.

## Local URLs

API:

```text
http://localhost:5076
```

Blazor WebAssembly:

```text
http://localhost:5048
```

The Blazor API base URL is configured in:

```text
src/Dispatcher.Web/wwwroot/appsettings.json
```

## Development run order

Start API first:

```powershell
dotnet run --project .\src\Dispatcher.Api\Dispatcher.Api.csproj
```

Then start Blazor WebAssembly:

```powershell
dotnet run --project .\src\Dispatcher.Web\Dispatcher.Web.csproj
```

Open:

```text
http://localhost:5048
```

## CORS

API allows the Blazor development origins:

```text
http://localhost:5048
https://localhost:7201
```

This is development-only behavior.
