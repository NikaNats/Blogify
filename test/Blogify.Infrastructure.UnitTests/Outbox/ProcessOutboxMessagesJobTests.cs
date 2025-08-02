using Blogify.Application.Abstractions.Clock;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts.Events;
using Blogify.Infrastructure.Outbox;
using Blogify.Infrastructure.UnitTests.Outbox;
using MediatR;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using Moq;

namespace Blogify.Infrastructure.UnitTests.Outbox;

public class ProcessOutboxMessagesJobTests
{
    private static string Serialize(IDomainEvent ev) => System.Text.Json.JsonSerializer.Serialize(ev, typeof(IDomainEvent));

    private static (ProcessOutboxMessagesJob Job, InMemoryOutboxDataAccess Store, Mock<IPublisher> Publisher, FakeClock Clock) CreateSut()
    {
        var store = new InMemoryOutboxDataAccess();
        var publisher = new Mock<IPublisher>();
        var clock = new FakeClock(DateTime.UtcNow);
        var options = Options.Create(new OutboxOptions { BatchSize = 50, IntervalInSeconds = 10 });
    var logger = NullLogger<ProcessOutboxMessagesJob>.Instance;
        var job = new ProcessOutboxMessagesJob(store, publisher.Object, clock, options, logger);
        return (job, store, publisher, clock);
    }

    [Fact]
    public async Task Processes_success_message_and_marks_processed()
    {
        var (job, store, publisher, clock) = CreateSut();
        var id = Guid.NewGuid();
        var ev = new PostCreatedDomainEvent(Guid.NewGuid(), "Title", Guid.NewGuid());
        store.Seed((id, nameof(PostCreatedDomainEvent), Serialize(ev), 0, null));

        await job.Execute(Mock.Of<Quartz.IJobExecutionContext>());

        publisher.Verify(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>()), Times.Once);
        var entry = store.Get(id);
        Assert.NotNull(entry.ProcessedOnUtc);
        Assert.Null(entry.Error);
    }

    [Fact]
    public async Task Retries_failures_and_sets_next_retry()
    {
        var (job, store, publisher, clock) = CreateSut();
        var id = Guid.NewGuid();
        var ev = new PostCreatedDomainEvent(Guid.NewGuid(), "Title", Guid.NewGuid());
        store.Seed((id, nameof(PostCreatedDomainEvent), Serialize(ev), 0, null));
        publisher.Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(new InvalidOperationException("boom"));

        await job.Execute(Mock.Of<Quartz.IJobExecutionContext>());

        var afterFirst = store.Get(id);
        Assert.Equal(1, afterFirst.Record.Attempts);
        Assert.Null(afterFirst.ProcessedOnUtc); // not processed yet
        Assert.NotNull(afterFirst.NextRetryUtc);
        Assert.NotNull(afterFirst.Error);
    }

    [Fact]
    public async Task Marks_poison_after_max_attempts()
    {
        var (job, store, publisher, clock) = CreateSut();
        var id = Guid.NewGuid();
        var ev = new PostCreatedDomainEvent(Guid.NewGuid(), "Title", Guid.NewGuid());
        // Seed with 4 attempts already (Max is 5 in job)
        store.Seed((id, nameof(PostCreatedDomainEvent), Serialize(ev), 4, null));
        publisher.Setup(p => p.Publish(It.IsAny<IDomainEvent>(), It.IsAny<CancellationToken>())).ThrowsAsync(new Exception("still failing"));

        await job.Execute(Mock.Of<Quartz.IJobExecutionContext>());

        var entry = store.Get(id);
        Assert.Equal(5, entry.Record.Attempts);
        Assert.NotNull(entry.ProcessedOnUtc); // poison marked processed
        Assert.Contains("Poison", entry.Error);
    }

    private sealed class FakeClock : IDateTimeProvider
    {
        public FakeClock(DateTime utcNow) => UtcNow = utcNow;
        public DateTime UtcNow { get; private set; }
        public void Advance(TimeSpan by) => UtcNow = UtcNow.Add(by);
    }
}
