namespace Dispatcher.Worker.MockPolling;

public sealed class MockPollingOptions
{
    public const string SectionName = "MockPolling";

    public bool Enabled { get; set; }

    public int IntervalSeconds { get; set; } = 5;

    public int DisabledDelaySeconds { get; set; } = 30;

    public double MinValue { get; set; } = 0;

    public double MaxValue { get; set; } = 100;

    public int MaxTagsPerCycle { get; set; } = 1000;
}
