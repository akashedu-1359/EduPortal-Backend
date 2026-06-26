using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using FluentAssertions;

namespace EduPortal.Tests.Domain;

public class ResourceTests
{
    [Fact]
    public void Constructor_SetsCorrectProperties()
    {
        var categoryId = Guid.NewGuid();
        var adminId = Guid.NewGuid();

        var resource = new Resource("Course Title", "Description", ResourceType.Video, 999m, categoryId, adminId);

        resource.Title.Should().Be("Course Title");
        resource.Description.Should().Be("Description");
        resource.ResourceType.Should().Be(ResourceType.Video);
        resource.Price.Should().Be(999m);
        resource.CategoryId.Should().Be(categoryId);
        resource.CreatedByAdminId.Should().Be(adminId);
        resource.Status.Should().Be(ResourceStatus.Draft);
        resource.IsFeatured.Should().BeFalse();
        resource.IsDeleted.Should().BeFalse();
    }

    [Fact]
    public void Constructor_DefaultsToDraftStatus()
    {
        var resource = new Resource("Title", "Desc", ResourceType.Blog, 0m, Guid.NewGuid(), Guid.NewGuid());

        resource.Status.Should().Be(ResourceStatus.Draft);
    }

    [Fact]
    public void Status_CanTransitionFromDraftToPublished()
    {
        var resource = new Resource("Title", "Desc", ResourceType.Video, 100m, Guid.NewGuid(), Guid.NewGuid());

        resource.Status = ResourceStatus.Published;

        resource.Status.Should().Be(ResourceStatus.Published);
    }

    [Fact]
    public void Status_CanTransitionFromPublishedToArchived()
    {
        var resource = new Resource("Title", "Desc", ResourceType.Video, 100m, Guid.NewGuid(), Guid.NewGuid());
        resource.Status = ResourceStatus.Published;

        resource.Status = ResourceStatus.Archived;

        resource.Status.Should().Be(ResourceStatus.Archived);
    }

    [Fact]
    public void Status_CanTransitionFromArchivedBackToDraft()
    {
        var resource = new Resource("Title", "Desc", ResourceType.Video, 100m, Guid.NewGuid(), Guid.NewGuid());
        resource.Status = ResourceStatus.Archived;

        resource.Status = ResourceStatus.Draft;

        resource.Status.Should().Be(ResourceStatus.Draft);
    }

    [Fact]
    public void FreeResource_HasZeroPrice()
    {
        var resource = new Resource("Free Course", "Desc", ResourceType.Blog, 0m, Guid.NewGuid(), Guid.NewGuid());

        resource.Price.Should().Be(0m);
    }

    [Fact]
    public void Constructor_LeavesOptionalFieldsNull()
    {
        var resource = new Resource("Title", "Desc", ResourceType.Video, 100m, Guid.NewGuid(), Guid.NewGuid());

        resource.FileKey.Should().BeNull();
        resource.ExternalUrl.Should().BeNull();
        resource.BlogContent.Should().BeNull();
        resource.ThumbnailKey.Should().BeNull();
    }

    [Fact]
    public void OptionalFields_CanBeSet()
    {
        var resource = new Resource("Title", "Desc", ResourceType.Video, 100m, Guid.NewGuid(), Guid.NewGuid())
        {
            FileKey = "uploads/video.mp4",
            ExternalUrl = "https://example.com",
            BlogContent = "<p>Blog content</p>",
            ThumbnailKey = "uploads/thumb.jpg"
        };

        resource.FileKey.Should().Be("uploads/video.mp4");
        resource.ExternalUrl.Should().Be("https://example.com");
        resource.BlogContent.Should().Be("<p>Blog content</p>");
        resource.ThumbnailKey.Should().Be("uploads/thumb.jpg");
    }

    [Fact]
    public void IsFeatured_CanBeToggled()
    {
        var resource = new Resource("Title", "Desc", ResourceType.Video, 100m, Guid.NewGuid(), Guid.NewGuid());

        resource.IsFeatured = true;

        resource.IsFeatured.Should().BeTrue();
    }

    [Fact]
    public void Constructor_InitializesEmptyEnrollments()
    {
        var resource = new Resource("Title", "Desc", ResourceType.Video, 100m, Guid.NewGuid(), Guid.NewGuid());

        resource.Enrollments.Should().BeEmpty();
    }

    [Theory]
    [InlineData(ResourceType.Video)]
    [InlineData(ResourceType.Blog)]
    [InlineData(ResourceType.PDF)]
    public void Constructor_AcceptsAllResourceTypes(ResourceType type)
    {
        var resource = new Resource("Title", "Desc", type, 100m, Guid.NewGuid(), Guid.NewGuid());

        resource.ResourceType.Should().Be(type);
    }
}
