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

public record ResourceDto(Guid Id, string Title, string Description, ResourceType ResourceType,
    string? FileKey, string? ExternalUrl, string? BlogContent, string? ThumbnailKey,
    decimal Price, ResourceStatus Status, bool IsFeatured, Guid CategoryId, string? CategoryName,
    string? ThumbnailUrl, DateTime CreatedAt);

public class CreateResourceCommandValidator : AbstractValidator<CreateResourceCommand>
{
    public CreateResourceCommandValidator()
    {
        RuleFor(x => x.Title).NotEmpty().MaximumLength(255);
        RuleFor(x => x.Description).NotEmpty();
        RuleFor(x => x.Price).GreaterThanOrEqualTo(0);
        RuleFor(x => x.CategoryId).NotEmpty();
        RuleFor(x => x).Must(x => x.ResourceType != ResourceType.Blog || !string.IsNullOrEmpty(x.BlogContent))
            .WithMessage("Blog content is required for blog resources.");
        RuleFor(x => x).Must(x => x.ResourceType == ResourceType.Blog || !string.IsNullOrEmpty(x.FileKey) || !string.IsNullOrEmpty(x.ExternalUrl))
            .WithMessage("FileKey or ExternalUrl required for Video/PDF resources.");
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
        return Result<ResourceDto>.Created(ToDto(resource));
    }

    public static ResourceDto ToDto(Resource r, string? thumbnailUrl = null) =>
        new(r.Id, r.Title, r.Description, r.ResourceType, r.FileKey, r.ExternalUrl, r.BlogContent,
            r.ThumbnailKey, r.Price, r.Status, r.IsFeatured, r.CategoryId,
            r.Category?.Name, thumbnailUrl, r.CreatedAt);
}
