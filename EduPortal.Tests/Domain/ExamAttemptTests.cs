using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using EduPortal.Domain.Events;
using FluentAssertions;

namespace EduPortal.Tests.Domain;

public class ExamAttemptTests
{
    [Fact]
    public void Complete_PassingScore_SetsIsPassedTrueAndFiresEvent()
    {
        var attempt = ExamAttempt.Start(Guid.NewGuid(), Guid.NewGuid());

        attempt.Complete(85m, 70m);

        attempt.IsPassed.Should().BeTrue();
        attempt.Score.Should().Be(85m);
        attempt.Status.Should().Be(AttemptStatus.Completed);
        attempt.DomainEvents.Should().ContainSingle(e => e is ExamPassedDomainEvent);
    }

    [Fact]
    public void Complete_FailingScore_SetsIsPassedFalseAndNoEvents()
    {
        var attempt = ExamAttempt.Start(Guid.NewGuid(), Guid.NewGuid());

        attempt.Complete(50m, 70m);

        attempt.IsPassed.Should().BeFalse();
        attempt.DomainEvents.Should().BeEmpty();
    }

    [Fact]
    public void Complete_ExactPassingScore_SetsIsPassed()
    {
        var attempt = ExamAttempt.Start(Guid.NewGuid(), Guid.NewGuid());

        attempt.Complete(70m, 70m);

        attempt.IsPassed.Should().BeTrue();
        attempt.DomainEvents.Should().ContainSingle(e => e is ExamPassedDomainEvent);
    }

    [Fact]
    public void TimeOut_SetsStatusToTimedOut()
    {
        var attempt = ExamAttempt.Start(Guid.NewGuid(), Guid.NewGuid());

        attempt.TimeOut();

        attempt.Status.Should().Be(AttemptStatus.TimedOut);
        attempt.CompletedAt.Should().NotBeNull();
    }

    [Fact]
    public void ClearDomainEvents_EmptiesEventList()
    {
        var attempt = ExamAttempt.Start(Guid.NewGuid(), Guid.NewGuid());
        attempt.Complete(90m, 70m);
        attempt.DomainEvents.Should().NotBeEmpty();

        attempt.ClearDomainEvents();

        attempt.DomainEvents.Should().BeEmpty();
    }
}
