using Blogify.Infrastructure.Outbox;
using System.Collections.Concurrent;

namespace Blogify.Infrastructure.UnitTests.Outbox;

internal sealed class InMemoryOutboxDataAccess : IOutboxDataAccess
{
    private readonly ConcurrentDictionary<Guid, (OutboxMessageRecord Record, DateTime? NextRetryUtc, DateTime? ProcessedOnUtc, string? Error, DateTime? LockedUntilUtc, string? LockedBy)> _store = new();

    public void Seed(params (Guid Id, string Type, string Content, int Attempts, DateTime? NextRetryUtc)[] items)
    {
        foreach (var i in items)
        {
            _store[i.Id] = (new OutboxMessageRecord(i.Id, i.Type, i.Content, i.Attempts), i.NextRetryUtc, null, null, null, null);
        }
    }

    public Task<IReadOnlyList<OutboxMessageRecord>> ClaimPendingAsync(int batchSize, DateTime nowUtc, int maxAttempts, TimeSpan lockDuration, string workerId, CancellationToken ct)
    {
        var claimed = new List<OutboxMessageRecord>();
        foreach (var kvp in _store.OrderBy(k => k.Key))
        {
            if (claimed.Count >= batchSize) break;
            var (rec, nextRetry, processed, err, lockedUntil, _) = kvp.Value;
            if (processed is not null) continue;
            if (rec.Attempts >= maxAttempts) continue;
            if (nextRetry is not null && nextRetry > nowUtc) continue;
            if (lockedUntil is not null && lockedUntil > nowUtc) continue;
            var updated = (rec, nextRetry, processed, err, nowUtc.Add(lockDuration), workerId);
            if (_store.TryUpdate(kvp.Key, updated, kvp.Value))
            {
                claimed.Add(rec);
            }
        }
        return Task.FromResult<IReadOnlyList<OutboxMessageRecord>>(claimed);
    }

    public Task MarkSuccessAsync(Guid id, DateTime processedOnUtc, string workerId, CancellationToken ct)
    {
        _store.AddOrUpdate(id, _ => throw new InvalidOperationException(), (_, old) => (old.Record, old.NextRetryUtc, processedOnUtc, null, null, null));
        return Task.CompletedTask;
    }

    public Task MarkRetryAsync(Guid id, int attempts, DateTime nextRetryUtc, string error, string workerId, CancellationToken ct)
    {
        _store.AddOrUpdate(id, _ => throw new InvalidOperationException(), (_, old) => (old.Record with { Attempts = attempts }, nextRetryUtc, null, error, null, null));
        return Task.CompletedTask;
    }

    public Task MarkPoisonAsync(Guid id, int attempts, DateTime processedOnUtc, string error, string workerId, CancellationToken ct)
    {
        _store.AddOrUpdate(id, _ => throw new InvalidOperationException(), (_, old) => (old.Record with { Attempts = attempts }, old.NextRetryUtc, processedOnUtc, error, null, null));
        return Task.CompletedTask;
    }

    // Test helpers
    public (OutboxMessageRecord Record, DateTime? NextRetryUtc, DateTime? ProcessedOnUtc, string? Error, DateTime? LockedUntilUtc, string? LockedBy) Get(Guid id) => _store[id];
}
