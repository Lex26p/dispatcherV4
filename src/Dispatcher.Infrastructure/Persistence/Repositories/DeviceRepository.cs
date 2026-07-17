using Dispatcher.Application.Abstractions.Persistence;
using Dispatcher.Domain.Devices;
using Microsoft.EntityFrameworkCore;

namespace Dispatcher.Infrastructure.Persistence.Repositories;

public sealed class DeviceRepository : IDeviceRepository
{
    private readonly DispatcherDbContext dbContext;

    public DeviceRepository(DispatcherDbContext dbContext)
    {
        this.dbContext = dbContext;
    }

    public Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return dbContext.Devices
            .SingleOrDefaultAsync(device => device.Id == id, cancellationToken);
    }

    public async Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return await dbContext.Devices
            .AsNoTracking()
            .OrderBy(device => device.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task AddAsync(Device device, CancellationToken cancellationToken = default)
    {
        await dbContext.Devices.AddAsync(device, cancellationToken);
    }

    public void Update(Device device)
    {
        dbContext.Devices.Update(device);
    }

    public void Remove(Device device)
    {
        dbContext.Devices.Remove(device);
    }
}
