namespace Dispatcher.Contracts.Common;

public sealed record ApiProblemDetails(
    string Type,
    string Title,
    int Status,
    string Code,
    string Detail,
    string CorrelationId,
    IReadOnlyDictionary<string, string[]>? Errors = null);
