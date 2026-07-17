namespace Dispatcher.Web.Models;

public sealed record ApiHealthResponse(
    string Status,
    string Service,
    string Product,
    DateTimeOffset TimestampUtc);
