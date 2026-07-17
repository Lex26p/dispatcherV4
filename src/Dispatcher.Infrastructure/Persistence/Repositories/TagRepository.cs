using Dispatcher.Application.Abstractions.Persistence;
using Dispatcher.Domain.Tags;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Infrastructure.Persistence.Repositories;

public sealed class TagRepository : ITagRepository
{
    private readonly DispatcherDbContext dbContext;

    public TagRepository(DispatcherDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Tag?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Tags
            .SingleOrDefaultAsync(tag => tag.Id == id, cancellationToken);
    }

    public Task<Tag?> GetByCodeAsync(string code, CancellationToken cancellationToken = default)
    {
        return dbContext.Tags
            .SingleOrDefaultAsync(tag => tag.Code == code, cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default)
    {
        return await dbContext.Tags
            .AsNoTracking()
            .Where(tag => tag.DeviceId == deviceId)
            .OrderBy(tag => tag.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<Tag>> GetEnabledAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Tags
            .AsNoTracking()
            .Where(tag => tag.IsEnabled)
            .OrderBy(tag => tag.Code)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Tag tag, CancellationToken cancellationToken = default)
    {
        await dbContext.Tags.AddAsync(tag, cancellationToken);
    }

    public void Update(Tag tag)
    {
        dbContext.Tags.Update(tag);
    }

    public void Remove(Tag tag)
    {
        dbContext.Tags.Remove(tag);
    }
}
