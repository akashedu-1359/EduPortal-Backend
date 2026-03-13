using EduPortal.Application.Interfaces;
using EduPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EduPortal.Infrastructure.Persistence.Repositories;

public class OrderRepository : IOrderRepository
{
    private readonly AppDbContext _db;

    public OrderRepository(AppDbContext db) => _db = db;

    public Task<Order?> GetByIdAsync(Guid id, CancellationToken ct) =>
        _db.Orders.FindAsync(new object[] { id }, ct).AsTask();

    public Task<Order?> GetByGatewayEventIdAsync(string eventId, CancellationToken ct) =>
        _db.Orders.FirstOrDefaultAsync(o => o.GatewayEventId == eventId, ct);

    public Task<List<Order>> GetByUserIdAsync(Guid userId, CancellationToken ct) =>
        _db.Orders.Where(o => o.UserId == userId).OrderByDescending(o => o.CreatedAt).ToListAsync(ct);

    public async Task AddAsync(Order order, CancellationToken ct) =>
        await _db.Orders.AddAsync(order, ct);

    public Task SaveChangesAsync(CancellationToken ct) => _db.SaveChangesAsync(ct);
}
