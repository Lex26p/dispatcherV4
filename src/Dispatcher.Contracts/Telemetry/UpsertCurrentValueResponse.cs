namespace Dispatcher.Contracts.Telemetry;

public sealed record UpsertCurrentValueResponse(
    bool Applied,
    string Result,
    CurrentValueDto CurrentValue);
