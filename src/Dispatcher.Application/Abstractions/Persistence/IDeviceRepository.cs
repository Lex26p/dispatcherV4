using Dispatcher.Domain.Devices;

namespace Dispatcher.Application.Abstractions.Persistence;

public interface IDeviceRepository
{
    Task<Device?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<Device>> GetAllAsync(CancellationToken cancellationToken = default);

    Task AddAsync(Device device, CancellationToken cancellationToken = default);

    void Update(Device device);

    void Remove(Device device);
}
