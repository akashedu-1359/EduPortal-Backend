using EduPortal.Domain.Enums;
using EduPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace EduPortal.Infrastructure.Services;

public class ExamStatusSchedulerService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ILogger<ExamStatusSchedulerService> _logger;

    public ExamStatusSchedulerService(IServiceProvider services, ILogger<ExamStatusSchedulerService> logger)
    { _services = services; _logger = logger; }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await UpdateExamStatusesAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating exam statuses");
            }
            await Task.Delay(TimeSpan.FromMinutes(1), stoppingToken);
        }
    }

    private async Task UpdateExamStatusesAsync(CancellationToken ct)
    {
        using var scope = _services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var now = DateTime.UtcNow;

        // Auto-activate scheduled exams whose start time has arrived and end time has not yet passed
        var toActivate = await db.Exams
            .Where(e => !e.IsDeleted && e.Status == ExamStatus.Draft && e.ScheduledStartAt != null && e.ScheduledStartAt <= now && (e.ScheduledEndAt == null || e.ScheduledEndAt > now))
            .ToListAsync(ct);

        foreach (var exam in toActivate)
        {
            exam.Status = ExamStatus.Active;
            _logger.LogInformation("Auto-activated exam {ExamId} ({Title})", exam.Id, exam.Title);
        }

        // Auto-complete active exams whose scheduled end time has passed
        var toComplete = await db.Exams
            .Where(e => !e.IsDeleted && e.Status == ExamStatus.Active && e.ScheduledEndAt != null && e.ScheduledEndAt <= now)
            .ToListAsync(ct);

        foreach (var exam in toComplete)
        {
            exam.Status = ExamStatus.Completed;
            _logger.LogInformation("Auto-completed exam {ExamId} ({Title})", exam.Id, exam.Title);
        }

        if (toActivate.Any() || toComplete.Any())
            await db.SaveChangesAsync(ct);
    }
}
