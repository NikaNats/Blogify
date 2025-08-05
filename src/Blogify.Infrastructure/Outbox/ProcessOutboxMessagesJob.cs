using System.Text.Json;
using Blogify.Application.Abstractions.Clock;
using Blogify.Domain.Abstractions;
using Blogify.Infrastructure.Serialization;
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
    private readonly OutboxOptions _outboxOptions = outboxOptions.Value;
    private const int MaxRetryAttempts = 5;
    private readonly string _workerId = $"{Environment.MachineName}-{Guid.NewGuid():N}";

    public async Task Execute(IJobExecutionContext context)
    {
    logger.LogDebug("Beginning to process outbox messages for worker {WorkerId}", _workerId);

        var lockDuration = TimeSpan.FromMinutes(2);
        var outboxMessages = await dataAccess.ClaimPendingAsync(_outboxOptions.BatchSize, dateTimeProvider.UtcNow, MaxRetryAttempts, lockDuration, _workerId, context.CancellationToken);

        if (!outboxMessages.Any())
        {
            logger.LogInformation("No pending outbox messages found for worker {WorkerId}", _workerId);
            return;
        }

        logger.LogInformation("Worker {WorkerId} claimed {MessageCount} messages to process.", _workerId, outboxMessages.Count);

        foreach (var message in outboxMessages)
        {
            Exception? exception = null;
            try
            {
                var domainEvent = JsonSerializer.Deserialize<IDomainEvent>(message.Content, DomainEventSerializer.Options);
                if (domainEvent is null)
                {
                    throw new JsonException($"Failed to deserialize domain event of type '{message.Type}' with ID '{message.Id}'.");
                }
                await publisher.Publish(domainEvent, context.CancellationToken);
            }
            catch (Exception ex)
            {
                exception = ex;
                logger.LogError(ex, "Exception while processing outbox message {MessageId} (Attempt {Attempt}) by worker {WorkerId}", message.Id, message.Attempts + 1, _workerId);
            }
            await UpdateOutboxMessageAsync(message, exception, context.CancellationToken);
        }

        logger.LogInformation("Worker {WorkerId} completed processing a batch of outbox messages.", _workerId);
    }

    private async Task UpdateOutboxMessageAsync(OutboxMessageRecord message, Exception? exception, CancellationToken ct)
    {
        if (exception is null)
        {
            await dataAccess.MarkSuccessAsync(message.Id, dateTimeProvider.UtcNow, _workerId, ct);
            return;
        }

        var newAttempts = message.Attempts + 1;
        if (newAttempts >= MaxRetryAttempts)
        {
            await dataAccess.MarkPoisonAsync(message.Id, newAttempts, dateTimeProvider.UtcNow, $"Poison message after {newAttempts} attempts: {exception}", _workerId, ct);
            return;
        }

        var nextRetryDelay = TimeSpan.FromSeconds(Math.Pow(2, newAttempts)); // 2s,4s,8s,16s,...
        await dataAccess.MarkRetryAsync(message.Id, newAttempts, dateTimeProvider.UtcNow.Add(nextRetryDelay), exception.ToString(), _workerId, ct);
    }
}