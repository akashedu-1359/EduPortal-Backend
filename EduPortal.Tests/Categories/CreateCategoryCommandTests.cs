using EduPortal.Application.Features.Categories.Commands;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using FluentAssertions;
using Moq;

namespace EduPortal.Tests.Categories;

public class CreateCategoryCommandTests
{
    private readonly Mock<ICategoryRepository> _categoryRepo = new();

    private CreateCategoryCommandHandler CreateHandler() =>
        new(_categoryRepo.Object);

    [Fact]
    public async Task Handle_ValidRequest_CreatesCategoryAndReturns201()
    {
        _categoryRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var result = await CreateHandler().Handle(new CreateCategoryCommand("Web Development", "Courses about web dev"), default);

        result.IsSuccess.Should().BeTrue();
        result.StatusCode.Should().Be(201);
        result.Value!.Name.Should().Be("Web Development");
        result.Value.Slug.Should().Be("web-development");
        _categoryRepo.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Once);
        _categoryRepo.Verify(r => r.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DuplicateSlug_Returns409()
    {
        var existing = new Category("Web Development", "web-development");
        _categoryRepo.Setup(r => r.GetBySlugAsync("web-development", It.IsAny<CancellationToken>())).ReturnsAsync(existing);

        var result = await CreateHandler().Handle(new CreateCategoryCommand("Web Development", null), default);

        result.IsSuccess.Should().BeFalse();
        result.StatusCode.Should().Be(409);
        _categoryRepo.Verify(r => r.AddAsync(It.IsAny<Category>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_WithVisibilityAndSortOrder_SetsProperties()
    {
        _categoryRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var result = await CreateHandler().Handle(new CreateCategoryCommand("Hidden Category", "desc", IsVisible: false, SortOrder: 5), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsVisible.Should().BeFalse();
        result.Value.SortOrder.Should().Be(5);
    }

    [Fact]
    public async Task Handle_DefaultVisibility_IsTrue()
    {
        _categoryRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var result = await CreateHandler().Handle(new CreateCategoryCommand("Visible Category", null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.IsVisible.Should().BeTrue();
    }

    [Fact]
    public void GenerateSlug_ConvertsToLowercaseWithDashes()
    {
        var slug = CreateCategoryCommandHandler.GenerateSlug("Machine Learning & AI");

        slug.Should().Be("machine-learning-ai");
    }

    [Fact]
    public void GenerateSlug_HandlesMultipleSpacesAndSpecialChars()
    {
        var slug = CreateCategoryCommandHandler.GenerateSlug("  C++  / C#  Courses  ");

        slug.Should().Be("c-c-courses");
    }

    [Fact]
    public async Task Handle_NullDescription_CreatesWithNullDescription()
    {
        _categoryRepo.Setup(r => r.GetBySlugAsync(It.IsAny<string>(), It.IsAny<CancellationToken>())).ReturnsAsync((Category?)null);

        var result = await CreateHandler().Handle(new CreateCategoryCommand("No Description", null), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Description.Should().BeNull();
    }
}
