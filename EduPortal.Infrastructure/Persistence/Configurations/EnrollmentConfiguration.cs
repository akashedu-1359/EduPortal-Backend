using EduPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduPortal.Infrastructure.Persistence.Configurations;

public class EnrollmentConfiguration : IEntityTypeConfiguration<Enrollment>
{
    public void Configure(EntityTypeBuilder<Enrollment> builder)
    {
        builder.HasKey(e => e.Id);
        builder.HasIndex(e => new { e.UserId, e.ResourceId }).IsUnique();
        builder.Property(e => e.EnrolledAt).HasColumnType("timestamptz");
        builder.HasOne(e => e.User).WithMany(u => u.Enrollments).HasForeignKey(e => e.UserId);
        builder.HasOne(e => e.Resource).WithMany(r => r.Enrollments).HasForeignKey(e => e.ResourceId);
    }
}
