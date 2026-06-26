using EduPortal.Application.Common;
using EduPortal.Application.Features.Exams.Commands;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using FluentAssertions;
using MediatR;
using Moq;

namespace EduPortal.Tests.Exams;

public class SubmitExamAttemptCommandTests
{
    private readonly Mock<IExamRepository> _examRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IPublisher> _publisher = new();

    private SubmitExamCommandHandler CreateHandler() =>
        new(_examRepo.Object, _currentUser.Object, _publisher.Object);

    private (Exam Exam, ExamAttempt Attempt, Guid UserId) SetupExamWithQuestions(int questionCount = 3, decimal passingPercentage = 70m)
    {
        var userId = Guid.NewGuid();
        var adminId = Guid.NewGuid();
        var exam = new Exam("Test Exam", "Desc", 60, passingPercentage, adminId);
        for (int i = 0; i < questionCount; i++)
        {
            exam.Questions.Add(new Question(exam.Id, $"Q{i + 1}?", "A", "B", "C", "D", 1, i));
        }

        var attempt = ExamAttempt.Start(userId, exam.Id);
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _examRepo.Setup(r => r.GetAttemptAsync(attempt.Id, It.IsAny<CancellationToken>())).ReturnsAsync(attempt);
        _examRepo.Setup(r => r.GetByIdAsync(exam.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(exam);

        return (exam, attempt, userId);
    }

    [Fact]
    public async Task Handle_AllCorrectAnswers_ReturnsPassWithFullScore()
    {
        var (exam, attempt, _) = SetupExamWithQuestions(3, 70m);
        var answers = exam.Questions.Select(q => new AnswerSubmission(q.Id, 1)).ToList();

        var result = await CreateHandler().Handle(new SubmitExamCommand(attempt.Id, answers), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Score.Should().Be(100m);
        result.Value.IsPassed.Should().BeTrue();
        result.Value.CorrectAnswers.Should().Be(3);
        result.Value.TotalQuestions.Should().Be(3);
    }

    [Fact]
    public async Task Handle_AllWrongAnswers_ReturnsFailWithZeroScore()
    {
        var (exam, attempt, _) = SetupExamWithQuestions(3, 70m);
        var answers = exam.Questions.Select(q => new AnswerSubmission(q.Id, 3)).ToList();

        var result = await CreateHandler().Handle(new SubmitExamCommand(attempt.Id, answers), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Score.Should().Be(0m);
        result.Value.IsPassed.Should().BeFalse();
        result.Value.CorrectAnswers.Should().Be(0);
    }

    [Fact]
    public async Task Handle_PartialCorrect_CalculatesScoreCorrectly()
    {
        var (exam, attempt, _) = SetupExamWithQuestions(4, 50m);
        var questions = exam.Questions.ToList();
        var answers = new List<AnswerSubmission>
        {
            new(questions[0].Id, 1),
            new(questions[1].Id, 1),
            new(questions[2].Id, 3),
            new(questions[3].Id, 3)
        };

        var result = await CreateHandler().Handle(new SubmitExamCommand(attempt.Id, answers), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Score.Should().Be(50m);
        result.Value.IsPassed.Should().BeTrue();
        result.Value.CorrectAnswers.Should().Be(2);
    }

    [Fact]
    public async Task Handle_AttemptNotFound_Returns404()
    {
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _examRepo.Setup(r => r.GetAttemptAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((ExamAttempt?)null);

        var result = await CreateHandler().Handle(new SubmitExamCommand(Guid.NewGuid(), new List<AnswerSubmission>()), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_WrongUser_Returns401()
    {
        var (_, attempt, _) = SetupExamWithQuestions();
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid());

        var result = await CreateHandler().Handle(new SubmitExamCommand(attempt.Id, new List<AnswerSubmission>()), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Handle_AlreadyCompleted_Returns400()
    {
        var (exam, attempt, _) = SetupExamWithQuestions();
        attempt.Complete(80m, 70m);
        attempt.ClearDomainEvents();

        var result = await CreateHandler().Handle(new SubmitExamCommand(attempt.Id, new List<AnswerSubmission>()), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_PassingScore_PublishesDomainEvent()
    {
        var (exam, attempt, _) = SetupExamWithQuestions(2, 50m);
        var answers = exam.Questions.Select(q => new AnswerSubmission(q.Id, 1)).ToList();

        await CreateHandler().Handle(new SubmitExamCommand(attempt.Id, answers), default);

        _publisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_FailingScore_DoesNotPublishDomainEvent()
    {
        var (exam, attempt, _) = SetupExamWithQuestions(3, 70m);
        var answers = exam.Questions.Select(q => new AnswerSubmission(q.Id, 3)).ToList();

        await CreateHandler().Handle(new SubmitExamCommand(attempt.Id, answers), default);

        _publisher.Verify(p => p.Publish(It.IsAny<INotification>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NullSelectedOption_TreatedAsIncorrect()
    {
        var (exam, attempt, _) = SetupExamWithQuestions(2, 50m);
        var questions = exam.Questions.ToList();
        var answers = new List<AnswerSubmission>
        {
            new(questions[0].Id, null),
            new(questions[1].Id, 1)
        };

        var result = await CreateHandler().Handle(new SubmitExamCommand(attempt.Id, answers), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.CorrectAnswers.Should().Be(1);
        result.Value.Score.Should().Be(50m);
    }

    [Fact]
    public async Task Handle_SavesChangesAfterSubmission()
    {
        var (exam, attempt, _) = SetupExamWithQuestions(1, 50m);
        var answers = exam.Questions.Select(q => new AnswerSubmission(q.Id, 1)).ToList();

        await CreateHandler().Handle(new SubmitExamCommand(attempt.Id, answers), default);

        _examRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}
