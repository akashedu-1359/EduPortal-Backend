using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities.Cms;
using MediatR;

namespace EduPortal.Application.Features.Cms.Commands;

public record CreateFaqCommand(string Question, string Answer, int SortOrder, bool IsVisible) : IRequest<Result<FaqDto>>;
public record UpdateFaqCommand(Guid Id, string Question, string Answer, int SortOrder, bool IsVisible) : IRequest<Result>;
public record DeleteFaqCommand(Guid Id) : IRequest<Result>;
public record ReorderFaqsCommand(List<FaqOrderItem> Items) : IRequest<Result>;

public record FaqDto(Guid Id, string Question, string Answer, int SortOrder, bool IsVisible);
public record FaqOrderItem(Guid Id, int SortOrder);

public class CreateFaqCommandHandler : IRequestHandler<CreateFaqCommand, Result<FaqDto>>
{
    private readonly ICmsRepository _cms;
    private readonly ICacheService _cache;
    public CreateFaqCommandHandler(ICmsRepository cms, ICacheService cache) { _cms = cms; _cache = cache; }

    public async Task<Result<FaqDto>> Handle(CreateFaqCommand request, CancellationToken cancellationToken)
    {
        var faq = new CmsFaq { Question = request.Question, Answer = request.Answer, SortOrder = request.SortOrder, IsVisible = request.IsVisible };
        await _cms.AddFaqAsync(faq, cancellationToken);
        await _cms.SaveChangesAsync(cancellationToken);
        await _cache.DeleteAsync("cms:faqs", cancellationToken);
        return Result<FaqDto>.Created(new FaqDto(faq.Id, faq.Question, faq.Answer, faq.SortOrder, faq.IsVisible));
    }
}

public class UpdateFaqCommandHandler : IRequestHandler<UpdateFaqCommand, Result>
{
    private readonly ICmsRepository _cms;
    private readonly ICacheService _cache;
    public UpdateFaqCommandHandler(ICmsRepository cms, ICacheService cache) { _cms = cms; _cache = cache; }

    public async Task<Result> Handle(UpdateFaqCommand request, CancellationToken cancellationToken)
    {
        var faqs = await _cms.GetFaqsAsync(cancellationToken);
        var faq = faqs.FirstOrDefault(f => f.Id == request.Id);
        if (faq == null) return Result.NotFound("FAQ not found.");
        faq.Question = request.Question; faq.Answer = request.Answer;
        faq.SortOrder = request.SortOrder; faq.IsVisible = request.IsVisible; faq.UpdatedAt = DateTime.UtcNow;
        await _cms.SaveChangesAsync(cancellationToken);
        await _cache.DeleteAsync("cms:faqs", cancellationToken);
        return Result.Success();
    }
}

public class DeleteFaqCommandHandler : IRequestHandler<DeleteFaqCommand, Result>
{
    private readonly ICmsRepository _cms;
    private readonly ICacheService _cache;
    public DeleteFaqCommandHandler(ICmsRepository cms, ICacheService cache) { _cms = cms; _cache = cache; }

    public async Task<Result> Handle(DeleteFaqCommand request, CancellationToken cancellationToken)
    {
        var faqs = await _cms.GetFaqsAsync(cancellationToken);
        var faq = faqs.FirstOrDefault(f => f.Id == request.Id);
        if (faq == null) return Result.NotFound("FAQ not found.");
        _cms.RemoveFaq(faq);
        await _cms.SaveChangesAsync(cancellationToken);
        await _cache.DeleteAsync("cms:faqs", cancellationToken);
        return Result.Success();
    }
}

public class ReorderFaqsCommandHandler : IRequestHandler<ReorderFaqsCommand, Result>
{
    private readonly ICmsRepository _cms;
    private readonly ICacheService _cache;
    public ReorderFaqsCommandHandler(ICmsRepository cms, ICacheService cache) { _cms = cms; _cache = cache; }

    public async Task<Result> Handle(ReorderFaqsCommand request, CancellationToken cancellationToken)
    {
        var faqs = await _cms.GetFaqsAsync(cancellationToken);
        foreach (var item in request.Items)
        {
            var faq = faqs.FirstOrDefault(f => f.Id == item.Id);
            if (faq != null) { faq.SortOrder = item.SortOrder; faq.UpdatedAt = DateTime.UtcNow; }
        }
        await _cms.SaveChangesAsync(cancellationToken);
        await _cache.DeleteAsync("cms:faqs", cancellationToken);
        return Result.Success();
    }
}
