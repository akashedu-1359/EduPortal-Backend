using EduPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduPortal.Infrastructure.Persistence.Configurations;

public class CertificateConfiguration : IEntityTypeConfiguration<Certificate>
{
    public void Configure(EntityTypeBuilder<Certificate> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.IssuedAt).HasColumnType("timestamptz");
        builder.HasIndex(c => c.UserId);
        builder.HasIndex(c => c.ExamAttemptId);
        builder.HasOne(c => c.User).WithMany().HasForeignKey(c => c.UserId);
        builder.HasOne(c => c.ExamAttempt).WithMany().HasForeignKey(c => c.ExamAttemptId);
    }
}
