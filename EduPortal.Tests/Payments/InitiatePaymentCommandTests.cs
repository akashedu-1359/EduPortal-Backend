using EduPortal.Application.Common;
using EduPortal.Application.Features.Payments.Commands;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduPortal.Tests.Payments;

public class InitiatePaymentCommandTests
{
    private readonly Mock<IResourceRepository> _resourceRepo = new();
    private readonly Mock<IEnrollmentRepository> _enrollmentRepo = new();
    private readonly Mock<IOrderRepository> _orderRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();
    private readonly Mock<IPaymentGateway> _stripeGateway = new();

    private InitiatePaymentCommandHandler CreateHandler() =>
        new(_resourceRepo.Object, _orderRepo.Object, _enrollmentRepo.Object, _currentUser.Object, new[] { _stripeGateway.Object });

    [Fact]
    public async Task Handle_FreeResource_Returns400()
    {
        var userId = Guid.NewGuid();
        var resource = new Resource("Free Course", "Desc", ResourceType.Video, 0m, Guid.NewGuid(), Guid.NewGuid())
        {
            Status = ResourceStatus.Published
        };
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _resourceRepo.Setup(r => r.GetByIdAsync(resource.Id, It.IsAny<CancellationToken>())).ReturnsAsync(resource);

        var result = await CreateHandler().Handle(new InitiatePaymentCommand(resource.Id, "stripe"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_AlreadyEnrolled_Returns409()
    {
        var userId = Guid.NewGuid();
        var resource = new Resource("Paid Course", "Desc", ResourceType.Video, 999m, Guid.NewGuid(), Guid.NewGuid())
        {
            Status = ResourceStatus.Published
        };
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _resourceRepo.Setup(r => r.GetByIdAsync(resource.Id, It.IsAny<CancellationToken>())).ReturnsAsync(resource);
        _enrollmentRepo.Setup(e => e.IsEnrolledAsync(userId, resource.Id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        var result = await CreateHandler().Handle(new InitiatePaymentCommand(resource.Id, "stripe"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
    }

    [Fact]
    public async Task Handle_UnsupportedGateway_Returns400()
    {
        var userId = Guid.NewGuid();
        var resource = new Resource("Paid Course", "Desc", ResourceType.Video, 999m, Guid.NewGuid(), Guid.NewGuid())
        {
            Status = ResourceStatus.Published
        };
        _currentUser.Setup(u => u.UserId).Returns(userId);
        _resourceRepo.Setup(r => r.GetByIdAsync(resource.Id, It.IsAny<CancellationToken>())).ReturnsAsync(resource);
        _enrollmentRepo.Setup(e => e.IsEnrolledAsync(userId, resource.Id, It.IsAny<CancellationToken>())).ReturnsAsync(false);
        _stripeGateway.Setup(g => g.GatewayName).Returns("stripe");

        var result = await CreateHandler().Handle(new InitiatePaymentCommand(resource.Id, "paypal"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(400);
    }

    [Fact]
    public async Task Handle_ResourceNotFound_Returns404()
    {
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid());
        _resourceRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Resource?)null);

        var result = await CreateHandler().Handle(new InitiatePaymentCommand(Guid.NewGuid(), "stripe"), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
    }
}
