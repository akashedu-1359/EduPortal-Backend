using EduPortal.Domain.Entities;
using EduPortal.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduPortal.Infrastructure.Persistence.Configurations;

public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
{
    public void Configure(EntityTypeBuilder<Resource> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).HasMaxLength(255).IsRequired();
        builder.Property(r => r.ResourceType).HasConversion<string>();
        builder.Property(r => r.Status).HasConversion<string>();
        builder.Property(r => r.Price).HasColumnType("decimal(10,2)");
        builder.HasIndex(r => new { r.Status, r.IsDeleted });
        builder.HasIndex(r => r.CategoryId);
        builder.HasIndex(r => r.Price);
        builder.Property(r => r.CreatedAt).HasColumnType("timestamptz");
        builder.Property(r => r.UpdatedAt).HasColumnType("timestamptz");
        builder.HasOne(r => r.Category).WithMany(c => c.Resources).HasForeignKey(r => r.CategoryId);
        builder.HasOne(r => r.CreatedByAdmin).WithMany().HasForeignKey(r => r.CreatedByAdminId).OnDelete(DeleteBehavior.Restrict);
    }
}
