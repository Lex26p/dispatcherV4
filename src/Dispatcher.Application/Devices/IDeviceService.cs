using Dispatcher.Application.Devices.Commands;

namespace Dispatcher.Application.Devices;

public interface IDeviceService
{
    Task<DeviceDto> CreateModbusTcpAsync(CreateModbusDeviceCommand command, CancellationToken cancellationToken = default);

    Task<DeviceDto> CreateSnmpAsync(CreateSnmpDeviceCommand command, CancellationToken cancellationToken = default);

    Task<DeviceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<DeviceDto>> GetAllAsync(CancellationToken cancellationToken = default);

    Task EnableAsync(Guid id, CancellationToken cancellationToken = default);

    Task DisableAsync(Guid id, CancellationToken cancellationToken = default);
}
