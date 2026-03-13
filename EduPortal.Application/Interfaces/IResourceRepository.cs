using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;

namespace EduPortal.Application.Interfaces;

public interface IResourceRepository
{
    Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<(List<Resource> Items, int Total)> GetPagedAsync(int page, int pageSize, ResourceStatus? status = null, Guid? categoryId = null, CancellationToken ct = default);
    Task AddAsync(Resource resource, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
