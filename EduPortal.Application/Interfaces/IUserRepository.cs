using EduPortal.Domain.Entities;

namespace EduPortal.Application.Interfaces;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<bool> ExistsByEmailAsync(string email, CancellationToken ct = default);
    Task AddAsync(User user, CancellationToken ct = default);
    Task<List<string>> GetPermissionsAsync(Guid userId, CancellationToken ct = default);
    Task<string?> GetRoleNameAsync(Guid userId, CancellationToken ct = default);
    Task AssignRoleAsync(Guid userId, int roleId, CancellationToken ct = default);
    Task SaveChangesAsync(CancellationToken ct = default);
}
