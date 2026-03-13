using EduPortal.Application.Common;
using EduPortal.Application.Features.Exams.Commands;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduPortal.Tests.Exams;

public class StartExamAttemptCommandTests
{
    private readonly Mock<IExamRepository> _examRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private StartExamAttemptCommandHandler CreateHandler() =>
        new(_examRepo.Object, _currentUser.Object);

    [Fact]
    public async Task Handle_ValidRequest_CreatesAttemptAndReturns201()
    {
        var userId = Guid.NewGuid();
        var exam = new Exam("Test Exam", "Desc", 60, 70m, Guid.NewGuid()) { MaxAttempts = 3 };
        exam.Status = ExamStatus.Active;
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _examRepo.Setup(r => r.GetByIdAsync(exam.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(exam);
        _examRepo.Setup(r => r.GetAttemptCountAsync(userId, exam.Id, It.IsAny<CancellationToken>())).ReturnsAsync(0);

        var result = await CreateHandler().Handle(new StartExamAttemptCommand(exam.Id), default);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        _examRepo.Verify(r => r.AddAttemptAsync(It.IsAny<ExamAttempt>(), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_MaxAttemptsReached_Returns400()
    {
        var userId = Guid.NewGuid();
        var exam = new Exam("Test Exam", "Desc", 60, 70m, Guid.NewGuid()) { MaxAttempts = 2 };
        exam.Status = ExamStatus.Active;
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _examRepo.Setup(r => r.GetByIdAsync(exam.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(exam);
        _examRepo.Setup(r => r.GetAttemptCountAsync(userId, exam.Id, It.IsAny<CancellationToken>())).ReturnsAsync(2);

        var result = await CreateHandler().Handle(new StartExamAttemptCommand(exam.Id), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
        _examRepo.Verify(r => r.AddAttemptAsync(It.IsAny<ExamAttempt>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExamNotFound_Returns404()
    {
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _examRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), false, It.IsAny<CancellationToken>())).ReturnsAsync((Exam?)null);

        var result = await CreateHandler().Handle(new StartExamAttemptCommand(Guid.NewGuid()), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Handle_ExamNotActive_Returns400()
    {
        var userId = Guid.NewGuid();
        var exam = new Exam("Test Exam", "Desc", 60, 70m, Guid.NewGuid());
        // Status is Draft by default
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _examRepo.Setup(r => r.GetByIdAsync(exam.Id, true, It.IsAny<CancellationToken>())).ReturnsAsync(exam);

        var result = await CreateHandler().Handle(new StartExamAttemptCommand(exam.Id), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }
}
