using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Persistence.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _db;

    public UserRepository(AppDbContext db) => _db = db;

    public Task<User?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Id == id, ct);

    public Task<User?> GetByEmailAsync(string email, CancellationToken ct) =>
        _db.Users.Include(u => u.UserRoles).ThenInclude(ur => ur.Role).FirstOrDefaultAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public Task<bool> ExistsByEmailAsync(string email, CancellationToken ct) =>
        _db.Users.AnyAsync(u => u.Email == email.ToLowerInvariant(), ct);

    public async Task AddAsync(User user, CancellationToken ct) =>
        await _db.Users.AddAsync(user, ct);

    public async Task<List<string>> GetPermissionsAsync(Guid userId, CancellationToken ct)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .SelectMany(ur => ur.Role.RolePermissions)
            .Select(rp => rp.Permission.Key)
            .Distinct()
            .ToListAsync(ct);
    }

    public async Task<string?> GetRoleNameAsync(Guid userId, CancellationToken ct)
    {
        return await _db.UserRoles
            .Where(ur => ur.UserId == userId)
            .Select(ur => ur.Role.Name)
            .FirstOrDefaultAsync(ct);
    }

    public async Task AssignRoleAsync(Guid userId, int roleId, CancellationToken ct)
    {
        var existing = await _db.UserRoles.AnyAsync(ur => ur.UserId == userId && ur.RoleId == roleId, ct);
        if (!existing)
            await _db.UserRoles.AddAsync(new UserRole(userId, roleId), ct);
    }

    public async Task<(List<User> Items, int Total)> GetPagedAsync(int page, int pageSize, string? search, CancellationToken ct)
    {
        var query = _db.Users.AsQueryable();
        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Email.Contains(search) || u.FullName.Contains(search));
        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(u => u.CreatedAt).Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
