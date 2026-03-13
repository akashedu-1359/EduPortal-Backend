using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduPortal.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Amount).HasColumnType("decimal(10,2)");
        builder.Property(o => o.Status).HasConversion<string>();
        builder.HasIndex(o => o.GatewayEventId).IsUnique();
        builder.HasIndex(o => o.UserId);
        builder.Property(o => o.CreatedAt).HasColumnType("timestamptz");
        builder.Property(o => o.UpdatedAt).HasColumnType("timestamptz");
        builder.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId);
        builder.HasOne(o => o.Resource).WithMany().HasForeignKey(o => o.ResourceId);
    }
}
