using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using MediatR;

namespace EduPortal.Application.Features.Users.Queries;

public record GetUsersQuery(int Page = 1, int PageSize = 20, string? Search = null) : IRequest<Result<PagedResult<UserAdminDto>>>;
public record UserAdminDto(Guid Id, string Email, string FullName, string? Role, bool IsActive, DateTime CreatedAt);

public class GetUsersQueryHandler : IRequestHandler<GetUsersQuery, Result<PagedResult<UserAdminDto>>>
{
    private readonly IUserRepository _users;

    public GetUsersQueryHandler(IUserRepository users) => _users = users;

    public async Task<Result<PagedResult<UserAdminDto>>> Handle(GetUsersQuery request, CancellationToken cancellationToken)
    {
        var (items, total) = await _users.GetPagedAsync(request.Page, request.PageSize, request.Search, cancellationToken);
        var dtos = new List<UserAdminDto>();
        foreach (var u in items)
        {
            var role = await _users.GetRoleNameAsync(u.Id, cancellationToken);
            dtos.Add(new UserAdminDto(u.Id, u.Email, u.FullName, role, u.IsActive, u.CreatedAt));
        }
        return Result<PagedResult<UserAdminDto>>.Success(PagedResult<UserAdminDto>.Create(dtos, request.Page, request.PageSize, total));
    }
}
