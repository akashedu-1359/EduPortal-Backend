using EduPortal.Application.Common;
using EduPortal.Application.Features.Resources.Queries;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using FluentAssertions;
using Moq;

namespace EduPortal.Tests.Resources;

public class GetResourcesQueryTests
{
    private readonly Mock<IResourceRepository> _resourceRepo = new();
    private readonly Mock<IStorageService> _storage = new();
    private readonly Mock<ICacheService> _cache = new();

    private GetPublicResourcesQueryHandler CreateHandler() =>
        new(_resourceRepo.Object, _storage.Object, _cache.Object);

    private Resource CreatePublishedResource(string title, ResourceType type = ResourceType.Video, decimal price = 100m, bool featured = false)
    {
        var category = new Category("Cat", "cat");
        var resource = new Resource(title, "Description", type, price, category.Id, Guid.NewGuid())
        {
            Status = ResourceStatus.Published,
            IsFeatured = featured
        };
        return resource;
    }

    [Fact]
    public async Task Handle_ReturnsPagedResults()
    {
        var resources = new List<Resource>
        {
            CreatePublishedResource("Course 1"),
            CreatePublishedResource("Course 2")
        };
        _resourceRepo.Setup(r => r.GetPagedAsync(1, 20, ResourceStatus.Published, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((resources, 2));

        var result = await CreateHandler().Handle(new GetPublicResourcesQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.TotalCount.Should().Be(2);
    }

    [Fact]
    public async Task Handle_WithTypeFilter_ReturnsFilteredResults()
    {
        var resources = new List<Resource>
        {
            CreatePublishedResource("Video Course", ResourceType.Video),
            CreatePublishedResource("Blog Post", ResourceType.Blog)
        };
        _resourceRepo.Setup(r => r.GetPagedAsync(1, 20, ResourceStatus.Published, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((resources, 2));

        var result = await CreateHandler().Handle(new GetPublicResourcesQuery(Type: ResourceType.Video), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithFeaturedFilter_ReturnsOnlyFeatured()
    {
        var resources = new List<Resource>
        {
            CreatePublishedResource("Normal", featured: false),
            CreatePublishedResource("Featured", featured: true)
        };
        _resourceRepo.Setup(r => r.GetPagedAsync(1, 20, ResourceStatus.Published, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((resources, 2));

        var result = await CreateHandler().Handle(new GetPublicResourcesQuery(Featured: true), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_WithSearchTerm_FiltersbyTitleOrDescription()
    {
        var resources = new List<Resource>
        {
            CreatePublishedResource("Python Basics"),
            CreatePublishedResource("Java Advanced")
        };
        _resourceRepo.Setup(r => r.GetPagedAsync(1, 20, ResourceStatus.Published, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((resources, 2));

        var result = await CreateHandler().Handle(new GetPublicResourcesQuery(Search: "python"), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().ContainSingle();
    }

    [Fact]
    public async Task Handle_EmptyResults_ReturnsEmptyPage()
    {
        _resourceRepo.Setup(r => r.GetPagedAsync(1, 20, ResourceStatus.Published, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Resource>(), 0));

        var result = await CreateHandler().Handle(new GetPublicResourcesQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.Should().BeEmpty();
        result.Value.TotalCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_WithCategoryFilter_PassesCategoryIdToRepo()
    {
        var categoryId = Guid.NewGuid();
        _resourceRepo.Setup(r => r.GetPagedAsync(1, 20, ResourceStatus.Published, categoryId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Resource>(), 0));

        await CreateHandler().Handle(new GetPublicResourcesQuery(CategoryId: categoryId), default);

        _resourceRepo.Verify(r => r.GetPagedAsync(1, 20, ResourceStatus.Published, categoryId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_WithThumbnail_GeneratesPresignedUrl()
    {
        var resource = CreatePublishedResource("Course");
        resource.ThumbnailKey = "thumbnails/thumb.jpg";
        _resourceRepo.Setup(r => r.GetPagedAsync(1, 20, ResourceStatus.Published, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Resource> { resource }, 1));
        _storage.Setup(s => s.GetReadUrlAsync("thumbnails/thumb.jpg", 3600, It.IsAny<CancellationToken>()))
            .ReturnsAsync("https://cdn.example.com/thumb.jpg");

        var result = await CreateHandler().Handle(new GetPublicResourcesQuery(), default);

        result.IsSuccess.Should().BeTrue();
        result.Value!.Items.First().ThumbnailUrl.Should().Be("https://cdn.example.com/thumb.jpg");
    }

    [Fact]
    public async Task Handle_CustomPagination_PassesPageParams()
    {
        _resourceRepo.Setup(r => r.GetPagedAsync(3, 10, ResourceStatus.Published, null, It.IsAny<CancellationToken>()))
            .ReturnsAsync((new List<Resource>(), 0));

        await CreateHandler().Handle(new GetPublicResourcesQuery(PageNumber: 3, PageSize: 10), default);

        _resourceRepo.Verify(r => r.GetPagedAsync(3, 10, ResourceStatus.Published, null, It.IsAny<CancellationToken>()), Times.Once);
    }
}
