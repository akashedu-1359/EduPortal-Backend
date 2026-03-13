using EduPortal.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace EduPortal.Infrastructure.Persistence.Configurations;

public class AttemptAnswerConfiguration : IEntityTypeConfiguration<AttemptAnswer>
{
    public void Configure(EntityTypeBuilder<AttemptAnswer> builder)
    {
        builder.HasKey(a => a.Id);
        builder.HasOne(a => a.ExamAttempt).WithMany(e => e.Answers).HasForeignKey(a => a.ExamAttemptId);
        builder.HasOne(a => a.Question).WithMany(q => q.AttemptAnswers).HasForeignKey(a => a.QuestionId);
    }
}
