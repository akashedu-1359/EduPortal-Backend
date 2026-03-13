using EduPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduPortal.Infrastructure.Persistence.Configurations;

public class ExamAttemptConfiguration : IEntityTypeConfiguration<ExamAttempt>
{
    public void Configure(EntityTypeBuilder<ExamAttempt> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Score).HasColumnType("decimal(5,2)");
        builder.Property(a => a.Status).HasConversion<string>();
        builder.Property(a => a.StartedAt).HasColumnType("timestamptz");
        builder.Property(a => a.CompletedAt).HasColumnType("timestamptz");
        builder.HasIndex(a => new { a.UserId, a.ExamId });
        builder.HasIndex(a => a.Status);
        builder.HasOne(a => a.User).WithMany(u => u.ExamAttempts).HasForeignKey(a => a.UserId);
        builder.HasOne(a => a.Exam).WithMany(e => e.Attempts).HasForeignKey(a => a.ExamId);
        builder.Ignore(a => a.DomainEvents);
    }
}
