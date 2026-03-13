using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Persistence.Repositories;

public class ResourceRepository : IResourceRepository
{
    private readonly AppDbContext _db;

    public ResourceRepository(AppDbContext db) => _db = db;

    public Task<Resource?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Resources.Include(r => r.Category).FirstOrDefaultAsync(r => r.Id == id && !r.IsDeleted, ct);

    public async Task<(List<Resource> Items, int Total)> GetPagedAsync(int page, int pageSize, ResourceStatus? status = null, Guid? categoryId = null, CancellationToken ct = default)
    {
        var query = _db.Resources.Include(r => r.Category).Where(r => !r.IsDeleted);
        if (status.HasValue) query = query.Where(r => r.Status == status.Value);
        if (categoryId.HasValue) query = query.Where(r => r.CategoryId == categoryId.Value);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(r => r.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task AddAsync(Resource resource, CancellationToken ct) =>
        await _db.Resources.AddAsync(resource, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
