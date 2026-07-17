using Dispatcher.Application.Abstractions.Persistence;
using Dispatcher.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Infrastructure.Persistence.Repositories;

public sealed class TagValueRepository : ITagValueRepository
{
    private readonly DispatcherDbContext dbContext;

    public TagValueRepository(DispatcherDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<TagValue?> GetCurrentValueAsync(Guid tagId, CancellationToken cancellationToken = default)
    {
        return dbContext.CurrentTagValues
            .AsNoTracking()
            .SingleOrDefaultAsync(value => value.TagId == tagId, cancellationToken);
    }

    public async Task<IReadOnlyList<TagValue>> GetCurrentValuesAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.CurrentTagValues
            .AsNoTracking()
            .OrderBy(value => value.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<TagValue>> GetCurrentValuesAsync(IEnumerable<Guid> tagIds, CancellationToken cancellationToken = default)
    {
        var ids = tagIds.ToArray();

        return await dbContext.CurrentTagValues
            .AsNoTracking()
            .Where(value => ids.Contains(value.TagId))
            .OrderBy(value => value.Timestamp)
            .ToListAsync(cancellationToken);
    }

    public async Task UpsertCurrentValueAsync(TagValue value, CancellationToken cancellationToken = default)
    {
        var existing = await dbContext.CurrentTagValues
            .SingleOrDefaultAsync(currentValue => currentValue.TagId == value.TagId, cancellationToken);

        if (existing is null)
        {
            await dbContext.CurrentTagValues.AddAsync(value, cancellationToken);
            return;
        }

        existing.Update(value.Value, value.Quality, value.Timestamp, value.ErrorMessage);
    }
}
