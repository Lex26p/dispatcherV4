namespace Dispatcher.Contracts.Common;

public sealed record PagedResponse<T>(
    IReadOnlyList<T> Items,
    int Page,
    int PageSize,
    long TotalCount,
    string? NextCursor = null)
{
    public static PagedResponse<T> Empty(int page = 1, int pageSize = 50) =>
        new(Array.Empty<T>(), page, pageSize, 0);
}
