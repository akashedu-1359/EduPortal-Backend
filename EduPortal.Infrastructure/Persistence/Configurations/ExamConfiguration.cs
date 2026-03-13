using EduPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduPortal.Infrastructure.Persistence.Configurations;

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.HasKey(e => e.Id);
        builder.Property(e => e.Title).HasMaxLength(255).IsRequired();
        builder.Property(e => e.PassingPercentage).HasColumnType("decimal(5,2)");
        builder.Property(e => e.Status).HasConversion<string>();
        builder.Property(e => e.ScheduledStartAt).HasColumnType("timestamptz");
        builder.Property(e => e.ScheduledEndAt).HasColumnType("timestamptz");
        builder.Property(e => e.CreatedAt).HasColumnType("timestamptz");
        builder.Property(e => e.UpdatedAt).HasColumnType("timestamptz");
        builder.HasOne(e => e.CreatedByAdmin).WithMany().HasForeignKey(e => e.CreatedByAdminId).OnDelete(DeleteBehavior.Restrict);
    }
}
