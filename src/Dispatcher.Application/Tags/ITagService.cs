using Dispatcher.Application.Tags.Commands;

namespace Dispatcher.Application.Tags;

public interface ITagService
{
    Task<TagDto> CreateModbusTagAsync(CreateModbusTagCommand command, CancellationToken cancellationToken = default);

    Task<TagDto> CreateSnmpTagAsync(CreateSnmpTagCommand command, CancellationToken cancellationToken = default);

    Task<TagDto?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<TagDto>> GetByDeviceIdAsync(Guid deviceId, CancellationToken cancellationToken = default);

    Task EnableAsync(Guid id, CancellationToken cancellationToken = default);

    Task DisableAsync(Guid id, CancellationToken cancellationToken = default);
}
