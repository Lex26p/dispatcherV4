# API error model

Step 2 introduces the first public API error contract.

## Rules

- Every failed API response must include a machine-readable `code`.
- Every response must carry `X-Correlation-ID`.
- Server exceptions must not expose stack traces or internal type names.
- Public contracts must stay in `Dispatcher.Contracts` and must not reference EF entities.
- Validation and authorization errors will receive specialized handling in later steps.

## Current contract

`Dispatcher.Contracts.Common.ApiProblemDetails` contains:

- `type`
- `title`
- `status`
- `code`
- `detail`
- `correlationId`
- optional field errors

## Current middleware

- `CorrelationMiddleware` reads or creates `X-Correlation-ID`.
- `ExceptionHandlingMiddleware` converts unhandled exceptions into `application/problem+json`.

## Development smoke endpoint

`GET /api/diagnostics/exception` intentionally throws an exception in development. It exists only to smoke-test the error middleware.
