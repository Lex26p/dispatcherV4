using Dispatcher.Domain.Telemetry;

namespace Dispatcher.Application.Telemetry.Values;

public sealed record CurrentValueWithDataPoint(CurrentValue Value, DataPoint DataPoint);
