using Dispatcher.Application.Abstractions.Persistence;
using Dispatcher.Application.Devices.Commands;
using Dispatcher.Domain.Devices;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Devices;

internal sealed class DeviceService : IDeviceService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly IUnitOfWork _unitOfWork;

    public DeviceService(
        IDeviceRepository deviceRepository,
        IUnitOfWork unitOfWork)
    {
        _deviceRepository = deviceRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<DeviceDto> CreateModbusTcpAsync(
        CreateModbusDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        var device = Device.CreateModbusTcp(
            command.Name,
            command.Host,
            command.Port,
            command.PollIntervalMs,
            command.TimeoutMs,
            command.RetryCount,
            command.Description);

        await _deviceRepository.AddAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return device.ToDto();
    }

    public async Task<DeviceDto> CreateSnmpAsync(
        CreateSnmpDeviceCommand command,
        CancellationToken cancellationToken = default)
    {
        var device = Device.CreateSnmp(
            command.Name,
            command.Host,
            command.Community,
            command.Version,
            command.Port,
            command.PollIntervalMs,
            command.TimeoutMs,
            command.RetryCount,
            command.Description);

        await _deviceRepository.AddAsync(device, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return device.ToDto();
    }

    public async Task<DeviceDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var device = await _deviceRepository.GetByIdAsync(id, cancellationToken);
        return device?.ToDto();
    }

    public async Task<IReadOnlyList<DeviceDto>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var devices = await _deviceRepository.GetAllAsync(cancellationToken);
        return devices.Select(device => device.ToDto()).ToArray();
    }

    public async Task EnableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var device = await GetRequiredDeviceAsync(id, cancellationToken);

        device.Enable();
        _deviceRepository.Update(device);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DisableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var device = await GetRequiredDeviceAsync(id, cancellationToken);

        device.Disable();
        _deviceRepository.Update(device);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    private async Task<Device> GetRequiredDeviceAsync(Guid id, CancellationToken cancellationToken)
    {
        var device = await _deviceRepository.GetByIdAsync(id, cancellationToken);

        if (device is null)
        {
            throw new InvalidOperationException($"Device with id '{id}' was not found.");
        }

        return device;
    }
}
