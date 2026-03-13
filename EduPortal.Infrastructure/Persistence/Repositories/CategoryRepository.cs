using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Persistence.Repositories;

public class CategoryRepository : ICategoryRepository
{
    private readonly AppDbContext _db;

    public CategoryRepository(AppDbContext db) => _db = db;

    public Task<Category?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Categories.FindAsync(new object[] { id }, ct).AsTask();

    public Task<Category?> GetBySlugAsync(string slug, CancellationToken ct) =>
        _db.Categories.FirstOrDefaultAsync(c => c.Slug == slug, ct);

    public Task<List<Category>> GetAllAsync(CancellationToken ct) =>
        _db.Categories.OrderBy(c => c.SortOrder).ToListAsync(ct);

    public async Task AddAsync(Category category, CancellationToken ct) =>
        await _db.Categories.AddAsync(category, ct);

    public void Remove(Category category) => _db.Categories.Remove(category);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
