using EduPortal.Application.Common;
using EduPortal.Application.Features.Resources.Commands;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduPortal.Tests.Resources;

public class CreateResourceCommandTests
{
    private readonly Mock<IResourceRepository> _resourceRepo = new();
    private readonly Mock<ICategoryRepository> _categoryRepo = new();
    private readonly Mock<ICurrentUserService> _currentUser = new();

    private CreateResourceCommandHandler CreateHandler() =>
        new(_resourceRepo.Object, _categoryRepo.Object, _currentUser.Object);

    [Fact]
    public async Task Handle_ValidRequest_CreatesResourceAndReturns201()
    {
        var category = new Category("Dev", "dev");
        var adminId = Guid.NewGuid();
        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _currentUser.Setup(u => u.UserId).Returns(adminId);

        var command = new CreateResourceCommand("New Course", "Desc", ResourceType.Video, null, null, null, null, 499m, category.Id);
        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Value!.Title.Should().Be("New Course");
        _resourceRepo.Verify(r => r.AddAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>()), Times.Once);
        _resourceRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_CategoryNotFound_Returns404()
    {
        _categoryRepo.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid());

        var command = new CreateResourceCommand("Title", "Desc", ResourceType.Video, null, null, null, null, 100m, Guid.NewGuid());
        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(404);
        _resourceRepo.Verify(r => r.AddAsync(It.IsAny<Resource>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_FreeResource_CreatesWithZeroPrice()
    {
        var category = new Category("Free", "free");
        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid());

        var command = new CreateResourceCommand("Free Course", "Desc", ResourceType.Blog, null, null, "<p>content</p>", null, 0m, category.Id);
        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Price.Should().Be(0m);
        result.Value.PricingType.Should().Be("Free");
    }

    [Fact]
    public async Task Handle_PaidResource_SetsPricingTypeToPaid()
    {
        var category = new Category("Paid", "paid");
        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid());

        var command = new CreateResourceCommand("Premium Course", "Desc", ResourceType.Video, "file.mp4", null, null, null, 999m, category.Id);
        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.PricingType.Should().Be("Paid");
    }

    [Fact]
    public async Task Handle_WithOptionalFields_SetsFileKeyAndUrl()
    {
        var category = new Category("Dev", "dev");
        _categoryRepo.Setup(r => r.GetByIdAsync(category.Id, It.IsAny<CancellationToken>())).ReturnsAsync(category);
        _currentUser.Setup(u => u.UserId).Returns(Guid.NewGuid());

        var command = new CreateResourceCommand("Course", "Desc", ResourceType.Video, "uploads/video.mp4", "https://ext.com", null, "uploads/thumb.jpg", 100m, category.Id);
        var result = await CreateHandler().Handle(command, default);

        result.IsSuccess.Should().BeTrue();
        _resourceRepo.Verify(r => r.AddAsync(It.Is<Resource>(res =>
            res.FileKey == "uploads/video.mp4" &&
            res.ExternalUrl == "https://ext.com" &&
            res.ThumbnailKey == "uploads/thumb.jpg"), It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public void GenerateSlug_ConvertsToLowercaseWithDashes()
    {
        var slug = CreateResourceCommandHandler.GenerateSlug("Hello World Course!");

        slug.Should().Be("hello-world-course");
    }

    [Fact]
    public void GenerateSlug_HandlesSpecialCharacters()
    {
        var slug = CreateResourceCommandHandler.GenerateSlug("C# & .NET Basics 101");

        slug.Should().Be("c-net-basics-101");
    }

    [Fact]
    public void GenerateSlug_TrimsLeadingAndTrailingDashes()
    {
        var slug = CreateResourceCommandHandler.GenerateSlug("  --Hello World--  ");

        slug.Should().Be("hello-world");
    }
}
