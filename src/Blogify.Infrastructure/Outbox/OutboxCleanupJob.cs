using Blogify.Application.Abstractions.Clock;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Blogify.Infrastructure.Outbox;

/// <summary>Removes processed outbox rows once they are older than the configured retention window.</summary>
[DisallowConcurrentExecution]
internal sealed class OutboxCleanupJob(
    IOutboxDataAccess dataAccess,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    ILogger<OutboxCleanupJob> logger) : IJob
{
    public async ValueTask Execute(IJobExecutionContext context, CancellationToken cancellationToken)
    {
        var cutoff = dateTimeProvider.UtcNow.AddDays(-outboxOptions.Value.RetentionDays);
        var deleted = await dataAccess.DeleteProcessedAsync(cutoff, cancellationToken);

        if (deleted > 0) logger.LogInformation("Outbox cleanup removed {Count} processed messages older than {Cutoff:u}", deleted, cutoff);
    }
}
