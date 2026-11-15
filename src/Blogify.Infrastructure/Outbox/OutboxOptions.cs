namespace Blogify.Infrastructure.Outbox;

public sealed class OutboxOptions
{
    public int IntervalInSeconds { get; init; }

    public int BatchSize { get; init; }

    /// <summary>How long processed messages are kept before the cleanup job deletes them.</summary>
    public int RetentionDays { get; init; } = 7;
}