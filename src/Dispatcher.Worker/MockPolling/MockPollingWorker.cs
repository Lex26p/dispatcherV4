using System.Globalization;
using Dispatcher.Application.Abstractions.Persistence;
using Dispatcher.Domain.Tags;
using Microsoft.Extensions.Options;

namespace Dispatcher.Worker.MockPolling;

internal sealed class MockPollingWorker : BackgroundService
{
    private readonly IServiceScopeFactory scopeFactory;
    private readonly IOptionsMonitor<MockPollingOptions> optionsMonitor;
    private readonly ILogger<MockPollingWorker> logger;

    public MockPollingWorker(
        IServiceScopeFactory scopeFactory,
        IOptionsMonitor<MockPollingOptions> optionsMonitor,
        ILogger<MockPollingWorker> logger)
    {
        this.scopeFactory = scopeFactory;
        this.optionsMonitor = optionsMonitor;
        this.logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        logger.LogInformation("Mock polling worker started.");

        while (!stoppingToken.IsCancellationRequested)
        {
            var options = optionsMonitor.CurrentValue;

            if (!options.Enabled)
            {
                logger.LogInformation(
                    "Mock polling worker is disabled. Set MockPolling:Enabled=true to generate simulated tag values.");

                await DelayAsync(options.DisabledDelaySeconds, stoppingToken);
                continue;
            }

            await PollOnceAsync(options, stoppingToken);
            await DelayAsync(options.IntervalSeconds, stoppingToken);
        }
    }

    private async Task PollOnceAsync(MockPollingOptions options, CancellationToken cancellationToken)
    {
        try
        {
            using var scope = scopeFactory.CreateScope();

            var tagRepository = scope.ServiceProvider.GetRequiredService<ITagRepository>();
            var tagValueRepository = scope.ServiceProvider.GetRequiredService<ITagValueRepository>();
            var unitOfWork = scope.ServiceProvider.GetRequiredService<IUnitOfWork>();

            var enabledTags = await tagRepository.GetEnabledAsync(cancellationToken);
            var tagsToPoll = enabledTags.Take(Math.Max(1, options.MaxTagsPerCycle)).ToArray();

            var timestamp = DateTimeOffset.UtcNow;

            foreach (var tag in tagsToPoll)
            {
                var value = GenerateValue(tag, options, timestamp);
                var currentValue = TagValue.Good(tag.Id, tag.DeviceId, value, timestamp);

                await tagValueRepository.UpsertCurrentValueAsync(currentValue, cancellationToken);
            }

            await unitOfWork.SaveChangesAsync(cancellationToken);

            logger.LogInformation(
                "Mock polling cycle completed. Updated {UpdatedTagCount} tag values.",
                tagsToPoll.Length);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            throw;
        }
        catch (Exception exception)
        {
            logger.LogWarning(
                exception,
                "Mock polling cycle failed. If PostgreSQL is not ready yet, keep MockPolling:Enabled=false.");
        }
    }

    private static string GenerateValue(Tag tag, MockPollingOptions options, DateTimeOffset timestamp)
    {
        return tag.DataType switch
        {
            TagDataType.Bool => (timestamp.Second % 2 == 0).ToString().ToLowerInvariant(),
            TagDataType.Int16 or TagDataType.Int32 => GenerateInteger(options, allowNegative: true),
            TagDataType.UInt16 or TagDataType.UInt32 => GenerateInteger(options, allowNegative: false),
            TagDataType.Float32 or TagDataType.Float64 => GenerateDouble(tag, options),
            TagDataType.String => $"mock-{timestamp:HHmmss}",
            _ => "0"
        };
    }

    private static string GenerateInteger(MockPollingOptions options, bool allowNegative)
    {
        var minValue = allowNegative ? (int)Math.Round(options.MinValue) : Math.Max(0, (int)Math.Round(options.MinValue));
        var maxValue = Math.Max(minValue + 1, (int)Math.Round(options.MaxValue));

        return Random.Shared.Next(minValue, maxValue).ToString(CultureInfo.InvariantCulture);
    }

    private static string GenerateDouble(Tag tag, MockPollingOptions options)
    {
        var minValue = Math.Min(options.MinValue, options.MaxValue);
        var maxValue = Math.Max(options.MinValue, options.MaxValue);
        var rawValue = minValue + Random.Shared.NextDouble() * (maxValue - minValue);
        var scaledValue = tag.ApplyScale(rawValue);

        return scaledValue.ToString("0.###", CultureInfo.InvariantCulture);
    }

    private static Task DelayAsync(int seconds, CancellationToken cancellationToken)
    {
        return Task.Delay(TimeSpan.FromSeconds(Math.Max(1, seconds)), cancellationToken);
    }
}
