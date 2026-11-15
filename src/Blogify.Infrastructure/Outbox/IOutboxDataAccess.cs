using Dapper;
using Blogify.Application.Abstractions.Data;

namespace Blogify.Infrastructure.Outbox;

/// <summary>
/// Abstraction over the outbox persistence used by the processing job. Enables
/// unit testing of retry/backoff logic without a physical database.
/// </summary>
internal interface IOutboxDataAccess
{
    Task<IReadOnlyList<OutboxMessageRecord>> ClaimPendingAsync(int batchSize, DateTime nowUtc, int maxAttempts, TimeSpan lockDuration, string workerId, CancellationToken ct);
    Task MarkSuccessAsync(Guid id, DateTime processedOnUtc, string workerId, CancellationToken ct);
    Task MarkRetryAsync(Guid id, int attempts, DateTime nextRetryUtc, string error, string workerId, CancellationToken ct);
    Task MarkPoisonAsync(Guid id, int attempts, DateTime processedOnUtc, string error, string workerId, CancellationToken ct);
}

internal sealed record OutboxMessageRecord(Guid Id, string Type, string Content, int Attempts);

/// <summary>
/// Dapper based implementation used in production.
/// </summary>
/// <remarks>
/// All timestamps are sent as UTC <see cref="DateTimeOffset"/>. Dapper maps <see cref="DateTime"/> to
/// <c>DbType.DateTime</c>, which Npgsql binds as <c>timestamp without time zone</c>; against the
/// <c>timestamptz</c> outbox columns that either throws (UTC kind) or is silently shifted by the
/// session time zone. <see cref="DateTimeOffset"/> binds as <c>timestamptz</c>.
/// </remarks>
internal sealed class DapperOutboxDataAccess(ISqlConnectionFactory sqlConnectionFactory) : IOutboxDataAccess
{
    public async Task<IReadOnlyList<OutboxMessageRecord>> ClaimPendingAsync(int batchSize, DateTime nowUtc, int maxAttempts, TimeSpan lockDuration, string workerId, CancellationToken ct)
    {
        using var connection = sqlConnectionFactory.CreateConnection();
        // Atomic claim: pick eligible rows and set lock (attempts unchanged)
        var sql = $"""
            WITH cte AS (
                SELECT id
                FROM outbox_messages
                WHERE processed_on_utc IS NULL
                  AND (next_retry_utc IS NULL OR next_retry_utc <= @Now)
                  AND (locked_until_utc IS NULL OR locked_until_utc < @Now)
                  AND attempts < @MaxAttempts
                ORDER BY occurred_on_utc
                LIMIT {batchSize}
                FOR UPDATE SKIP LOCKED
            )
            UPDATE outbox_messages o
            SET locked_until_utc = @LockUntil,
                locked_by = @WorkerId
            FROM cte
            WHERE o.id = cte.id
            RETURNING o.id, o.type, o.content, o.attempts;
            """;
        var now = ToUtcOffset(nowUtc);
        var rows = await connection.QueryAsync<OutboxMessageRecord>(new CommandDefinition(sql, new
        {
            Now = now,
            MaxAttempts = maxAttempts,
            LockUntil = now.Add(lockDuration),
            WorkerId = workerId
        }, cancellationToken: ct));
        return rows.ToList();
    }

    public async Task MarkSuccessAsync(Guid id, DateTime processedOnUtc, string workerId, CancellationToken ct)
    {
        using var connection = sqlConnectionFactory.CreateConnection();
        const string sql = "UPDATE outbox_messages SET processed_on_utc = @ProcessedOnUtc, error = NULL, locked_until_utc=NULL, locked_by=NULL WHERE id = @Id AND locked_by=@WorkerId";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, ProcessedOnUtc = ToUtcOffset(processedOnUtc), WorkerId = workerId }, cancellationToken: ct));
    }

    public async Task MarkRetryAsync(Guid id, int attempts, DateTime nextRetryUtc, string error, string workerId, CancellationToken ct)
    {
        using var connection = sqlConnectionFactory.CreateConnection();
        const string sql = "UPDATE outbox_messages SET error = @Error, attempts = @Attempts, next_retry_utc = @NextRetryUtc, locked_until_utc=NULL, locked_by=NULL WHERE id = @Id AND locked_by=@WorkerId";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, Error = error, Attempts = attempts, NextRetryUtc = ToUtcOffset(nextRetryUtc), WorkerId = workerId }, cancellationToken: ct));
    }

    public async Task MarkPoisonAsync(Guid id, int attempts, DateTime processedOnUtc, string error, string workerId, CancellationToken ct)
    {
        using var connection = sqlConnectionFactory.CreateConnection();
        const string sql = "UPDATE outbox_messages SET error = @Error, processed_on_utc = @ProcessedOnUtc, attempts = @Attempts, locked_until_utc=NULL, locked_by=NULL WHERE id = @Id AND locked_by=@WorkerId";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { Id = id, Error = error, ProcessedOnUtc = ToUtcOffset(processedOnUtc), Attempts = attempts, WorkerId = workerId }, cancellationToken: ct));
    }

    private static DateTimeOffset ToUtcOffset(DateTime value) =>
        new(value.Kind == DateTimeKind.Utc ? value : DateTime.SpecifyKind(value.ToUniversalTime(), DateTimeKind.Utc));
}
