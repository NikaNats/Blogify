using System.Text.Json;
using Blogify.Application.Abstractions.Clock;
using Blogify.Domain.Abstractions;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Quartz;

namespace Blogify.Infrastructure.Outbox;

[DisallowConcurrentExecution]
internal sealed class ProcessOutboxMessagesJob(
    IOutboxDataAccess dataAccess,
    IPublisher publisher,
    IDateTimeProvider dateTimeProvider,
    IOptions<OutboxOptions> outboxOptions,
    ILogger<ProcessOutboxMessagesJob> logger) : IJob
{
    private static readonly JsonSerializerOptions DomainEventSerializerOptions = new();
    private readonly OutboxOptions _outboxOptions = outboxOptions.Value;
    private const int MaxRetryAttempts = 5;

    public async Task Execute(IJobExecutionContext context)
    {
        logger.LogInformation("Beginning to process outbox messages");

    var workerId = Environment.MachineName + "-" + Guid.NewGuid().ToString("N").Substring(0, 6);
    var lockDuration = TimeSpan.FromMinutes(2); // configurable future option
    var outboxMessages = await dataAccess.ClaimPendingAsync(_outboxOptions.BatchSize, dateTimeProvider.UtcNow, MaxRetryAttempts, lockDuration, workerId, context.CancellationToken);

    foreach (var message in outboxMessages)
    {
            Exception? exception = null;
            try
            {
        var domainEvent = JsonSerializer.Deserialize<IDomainEvent>(message.Content, DomainEventSerializerOptions);
                if (domainEvent is null)
                {
                    throw new InvalidOperationException($"Failed to deserialize domain event {message.Id}");
                }
                await publisher.Publish(domainEvent, context.CancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
        logger.LogError(ex, "Exception while processing outbox message {MessageId} (Attempt {Attempt})", message.Id, message.Attempts + 1);
            }
            await UpdateOutboxMessageAsync(message, exception, workerId, context.CancellationToken);
        }

        logger.LogInformation("Completed processing outbox messages");
    }

    private async Task UpdateOutboxMessageAsync(OutboxMessageRecord message, Exception? exception, string workerId, CancellationToken ct)
    {
        if (exception is null)
        {
            await dataAccess.MarkSuccessAsync(message.Id, dateTimeProvider.UtcNow, workerId, ct);
            return;
        }

        var newAttempts = message.Attempts + 1;
        if (newAttempts >= MaxRetryAttempts)
        {
            await dataAccess.MarkPoisonAsync(message.Id, newAttempts, dateTimeProvider.UtcNow, $"Poison message after {newAttempts} attempts: {exception}", workerId, ct);
            return;
        }

        var nextRetryDelay = TimeSpan.FromSeconds(Math.Pow(2, newAttempts) * 10); // 10s,20s,40s,...
    await dataAccess.MarkRetryAsync(message.Id, newAttempts, dateTimeProvider.UtcNow.Add(nextRetryDelay), exception.ToString(), workerId, ct);
    }
}