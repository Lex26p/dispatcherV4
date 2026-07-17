using Dispatcher.Application.Abstractions.Persistence;
using Dispatcher.Application.Tags.Commands;
using Dispatcher.Domain.Devices;
using Dispatcher.Domain.Tags;

namespace Dispatcher.Application.Tags;

internal sealed class TagService : ITagService
{
    private readonly IDeviceRepository _deviceRepository;
    private readonly ITagRepository _tagRepository;
    private readonly IUnitOfWork _unitOfWork;

    public TagService(
        IDeviceRepository deviceRepository,
        ITagRepository tagRepository,
        IUnitOfWork unitOfWork)
    {
        _deviceRepository = deviceRepository;
        _tagRepository = tagRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<TagDto> CreateModbusTagAsync(
        CreateModbusTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var device = await GetRequiredDeviceAsync(command.DeviceId, cancellationToken);

        if (device.Protocol != DeviceProtocol.ModbusTcp)
        {
            throw new InvalidOperationException("Modbus tag can only be created for a Modbus TCP device.");
        }

        await EnsureTagCodeIsUniqueAsync(command.Code, cancellationToken);

        var address = new ModbusTagAddress(
            command.RegisterType,
            command.Address,
            command.UnitId,
            command.ByteOrder,
            command.WordOrder);

        var tag = Tag.CreateModbusTag(
            command.DeviceId,
            command.Name,
            command.Code,
            address,
            command.DataType,
            command.Unit,
            command.Scale,
            command.Offset,
            command.PollIntervalMs,
            command.HistoryEnabled,
            command.Description);

        await _tagRepository.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToDto();
    }

    public async Task<TagDto> CreateSnmpTagAsync(
        CreateSnmpTagCommand command,
        CancellationToken cancellationToken = default)
    {
        var device = await GetRequiredDeviceAsync(command.DeviceId, cancellationToken);

        if (device.Protocol != DeviceProtocol.Snmp)
        {
            throw new InvalidOperationException("SNMP tag can only be created for an SNMP device.");
        }

        await EnsureTagCodeIsUniqueAsync(command.Code, cancellationToken);

        var address = new SnmpTagAddress(command.Oid);

        var tag = Tag.CreateSnmpTag(
            command.DeviceId,
            command.Name,
            command.Code,
            address,
            command.DataType,
            command.Unit,
            command.Scale,
            command.Offset,
            command.PollIntervalMs,
            command.HistoryEnabled,
            command.Description);

        await _tagRepository.AddAsync(tag, cancellationToken);
        await _unitOfWork.SaveChangesAsync(cancellationToken);

        return tag.ToDto();
    }

    public async Task<TagDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);
        return tag?.ToDto();
    }

    public async Task<IReadOnlyList<TagDto>> GetByDeviceIdAsync(
        Guid deviceId,
        CancellationToken cancellationToken = default)
    {
        var tags = await _tagRepository.GetByDeviceIdAsync(deviceId, cancellationToken);
        return tags.Select(tag => tag.ToDto()).ToArray();
    }

    public async Task EnableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await GetRequiredTagAsync(id, cancellationToken);

        tag.Enable();
        _tagRepository.Update(tag);

        await _unitOfWork.SaveChangesAsync(cancellationToken);
    }

    public async Task DisableAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var tag = await GetRequiredTagAsync(id, cancellationToken);

        tag.Disable();
        _tagRepository.Update(tag);

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

    private async Task<Tag> GetRequiredTagAsync(Guid id, CancellationToken cancellationToken)
    {
        var tag = await _tagRepository.GetByIdAsync(id, cancellationToken);

        if (tag is null)
        {
            throw new InvalidOperationException($"Tag with id '{id}' was not found.");
        }

        return tag;
    }

    private async Task EnsureTagCodeIsUniqueAsync(string code, CancellationToken cancellationToken)
    {
        var existingTag = await _tagRepository.GetByCodeAsync(code, cancellationToken);

        if (existingTag is not null)
        {
            throw new InvalidOperationException($"Tag code '{code}' is already used.");
        }
    }
}
