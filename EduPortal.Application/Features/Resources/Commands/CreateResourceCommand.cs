using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using FluentValidation;
using MediatR;

namespace EduPortal.Application.Features.Resources.Commands;

public record CreateResourceCommand(
    string Title, string Description, ResourceType ResourceType,
    string? FileKey, string? ExternalUrl, string? BlogContent,
    string? ThumbnailKey, decimal Price, Guid CategoryId) : IRequest<Result<ResourceDto>>;

public record ResourceDto(
    Guid Id, string Title, string Slug, string Description, string? ThumbnailUrl,
    ResourceType Type, ResourceStatus Status, string PricingType, decimal Price, string? Currency,
    Guid CategoryId, ResourceCategoryDto? Category,
    string[] Tags, int? DurationMinutes,
    Guid AuthorId, string AuthorName,
    int ViewCount, int EnrollmentCount,
    bool IsPublished, DateTime? PublishedAt,
    DateTime CreatedAt, DateTime UpdatedAt);

public record ResourceCategoryDto(Guid Id, string Name, string Slug, string? Description, bool IsActive);

public class CreateResourceCommandValidator : AbstractValidator<CreateResourceCommand>
{
    public CreateResourceCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
    }
}

public class CreateResourceCommandHandler : IRequestHandler<CreateResourceCommand, Result<ResourceDto>>
{
    private readonly IResourceRepository _resources;
    private readonly ICategoryRepository _categories;
    private readonly ICurrentUserService _currentUser;

    public CreateResourceCommandHandler(IResourceRepository resources, ICategoryRepository categories, ICurrentUserService currentUser)
    {
        _resources = resources;
        _categories = categories;
        _currentUser = currentUser;
    }

    public async Task<Result<ResourceDto>> Handle(CreateResourceCommand request, CancellationToken cancellationToken)
    {
        var category = await _categories.GetByIdAsync(request.CategoryId, cancellationToken);
        if (category == null) return Result<ResourceDto>.NotFound("Category not found.");

        var adminId = _currentUser.UserId ?? Guid.Empty;
        var resource = new Resource(request.Title, request.Description, request.ResourceType, request.Price, request.CategoryId, adminId)
        {
            FileKey = request.FileKey,
            ExternalUrl = request.ExternalUrl,
            BlogContent = request.BlogContent,
            ThumbnailKey = request.ThumbnailKey,
        };
        await _resources.AddAsync(resource, cancellationToken);
        await _resources.SaveChangesAsync(cancellationToken);
        return Result<ResourceDto>.Created(ToDto(resource, category));
    }

    public static ResourceDto ToDto(Resource r, Domain.Entities.Category? category = null, string? thumbnailUrl = null)
    {
        var cat = category ?? r.Category;
        return new ResourceDto(
            r.Id,
            r.Title,
            GenerateSlug(r.Title),
            r.Description,
            thumbnailUrl,
            r.ResourceType,
            r.Status,
            r.Price > 0 ? "Paid" : "Free",
            r.Price,
            null,
            r.CategoryId,
            cat != null ? new ResourceCategoryDto(cat.Id, cat.Name, cat.Slug, cat.Description, cat.IsVisible) : null,
            Array.Empty<string>(),
            null,
            r.CreatedByAdminId,
            r.CreatedByAdmin?.FullName ?? string.Empty,
            0,
            r.Enrollments.Count,
            r.Status == ResourceStatus.Published,
            null,
            r.CreatedAt,
            r.UpdatedAt);
    }

    private static string GenerateSlug(string title) =>
        System.Text.RegularExpressions.Regex.Replace(title.ToLowerInvariant().Trim(), @"[^a-z0-9]+", "-").Trim('-');
}
