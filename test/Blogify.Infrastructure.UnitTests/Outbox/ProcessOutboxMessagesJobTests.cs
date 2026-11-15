using System.Text.Json;
using Blogify.Application.Abstractions.Clock;
using Blogify.Domain.Abstractions;
using Blogify.Domain.Posts.Events;
using Blogify.Infrastructure.Outbox;
using Blogify.Infrastructure.Serialization;
using MediatR;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Quartz;
using Shouldly;

namespace Blogify.Infrastructure.UnitTests.Outbox;

public class ProcessOutboxMessagesJobTests
{
    private readonly InMemoryOutboxDataAccess _store = new();
    private readonly IPublisher _publisher = Substitute.For<IPublisher>();
    private readonly IJobExecutionContext _context = Substitute.For<IJobExecutionContext>();
    private readonly ProcessOutboxMessagesJob _job;

    public ProcessOutboxMessagesJobTests()
    {
        var options = Options.Create(new OutboxOptions { BatchSize = 50, IntervalInSeconds = 10 });
        _job = new ProcessOutboxMessagesJob(
            _store,
            _publisher,
            new FakeClock(DateTime.UtcNow),
            options,
            NullLogger<ProcessOutboxMessagesJob>.Instance);
    }

    [Fact]
    public async Task Processes_success_message_and_marks_processed()
    {
        var id = SeedPostCreated(attempts: 0);

        await _job.Execute(_context, TestContext.Current.CancellationToken);

        await _publisher.Received(1).Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>());
        var entry = _store.Get(id);
        entry.ProcessedOnUtc.ShouldNotBeNull();
        entry.Error.ShouldBeNull();
    }

    [Fact]
    public async Task Retries_failures_and_sets_next_retry()
    {
        var id = SeedPostCreated(attempts: 0);
        _publisher.Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new InvalidOperationException("boom"));

        await _job.Execute(_context, TestContext.Current.CancellationToken);

        var entry = _store.Get(id);
        entry.Record.Attempts.ShouldBe(1);
        entry.ProcessedOnUtc.ShouldBeNull();
        entry.NextRetryUtc.ShouldNotBeNull();
        entry.Error.ShouldNotBeNull();
    }

    [Fact]
    public async Task Marks_poison_after_max_attempts()
    {
        // Max attempts in the job is 5; one more failure turns the message into poison.
        var id = SeedPostCreated(attempts: 4);
        _publisher.Publish(Arg.Any<IDomainEvent>(), Arg.Any<CancellationToken>())
            .ThrowsAsync(new Exception("still failing"));

        await _job.Execute(_context, TestContext.Current.CancellationToken);

        var entry = _store.Get(id);
        entry.Record.Attempts.ShouldBe(5);
        entry.ProcessedOnUtc.ShouldNotBeNull();
        entry.Error.ShouldNotBeNull().ShouldContain("Poison");
    }

    private Guid SeedPostCreated(int attempts)
    {
        var id = Guid.NewGuid();
        IDomainEvent domainEvent = new PostCreatedDomainEvent(Guid.NewGuid(), "Title", Guid.NewGuid());
        var content = JsonSerializer.Serialize(domainEvent, typeof(IDomainEvent), DomainEventSerializer.Options);
        _store.Seed((id, nameof(PostCreatedDomainEvent), content, attempts, null));
        return id;
    }

    private sealed class FakeClock(DateTime utcNow) : IDateTimeProvider
    {
        public DateTime UtcNow { get; } = utcNow;
    }
}
