using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EduPortal.Domain.Enums;
using DomainEntities = EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Persistence;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EduPortal.IntegrationTests.Admin;

[Trait("Category", "Integration")]
public class AdminExamsTests : IntegrationTestBase
{
    public AdminExamsTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<Guid> SeedExamAsync(Guid adminId, ExamStatus status = ExamStatus.Draft)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var exam = new DomainEntities.Exam("Seeded Exam", "Exam desc", 30, 60m, adminId)
        {
            MaxAttempts = 3,
            Status = status
        };
        exam.Questions.Add(new DomainEntities.Question(exam.Id, "Q1?", "A", "B", "C", "D", 0, 1));
        db.Exams.Add(exam);
        await db.SaveChangesAsync();
        return exam.Id;
    }

    [Fact]
    public async Task GetExams_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/admin/exams");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExams_WithUserRole_Returns403()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync("/api/admin/exams");

        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task GetExams_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/exams");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task CreateExam_WithAdmin_Returns201()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.PostAsJsonAsync("/api/admin/exams", new
        {
            Title = "New Exam",
            Description = "A new exam",
            DurationMinutes = 45,
            PassingPercentage = 70m,
            MaxAttempts = 2,
            Questions = new[]
            {
                new { QuestionText = "Q1?", Option1 = "A", Option2 = "B", Option3 = "C", Option4 = "D", CorrectOptionIndex = 0, SortOrder = 1 },
                new { QuestionText = "Q2?", Option1 = "A", Option2 = "B", Option3 = "C", Option4 = "D", CorrectOptionIndex = 1, SortOrder = 2 }
            }
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task GetExamById_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var examId = await SeedExamAsync(adminId);

        var response = await Client.GetAsync($"/api/admin/exams/{examId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetExamById_NotFound_Returns404()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync($"/api/admin/exams/{Guid.NewGuid()}");

        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task UpdateExam_WithIdMismatch_Returns400()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var examId = await SeedExamAsync(adminId);

        var response = await Client.PutAsJsonAsync($"/api/admin/exams/{examId}", new
        {
            Id = Guid.NewGuid(),
            Title = "Updated",
            Description = "Updated",
            DurationMinutes = 30,
            PassingPercentage = 50m,
            MaxAttempts = 1,
            Questions = Array.Empty<object>()
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task AddQuestion_ToDraftExam_Returns201()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        using (var scope = CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var exam = new DomainEntities.Exam("Draft Exam", "Desc", 30, 60m, adminId)
            {
                MaxAttempts = 3,
                Status = ExamStatus.Draft
            };
            db.Exams.Add(exam);
            await db.SaveChangesAsync();

            var response = await Client.PostAsJsonAsync("/api/admin/questions", new
            {
                ExamId = exam.Id,
                QuestionText = "What is 2+2?",
                Option1 = "3",
                Option2 = "4",
                Option3 = "5",
                Option4 = "6",
                CorrectOptionIndex = 1,
                SortOrder = 0
            });

            response.StatusCode.Should().Be(HttpStatusCode.Created);
        }
    }

    [Fact]
    public async Task GetExamAttempts_WithAdmin_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);

        var response = await Client.GetAsync("/api/admin/exam-attempts?page=1&pageSize=20");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteExam_WithAdmin_ReturnsSuccess()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var examId = await SeedExamAsync(adminId);

        var response = await Client.DeleteAsync($"/api/admin/exams/{examId}");

        response.StatusCode.Should().Be(HttpStatusCode.NoContent);
    }

    [Fact]
    public async Task PublishExam_WithAdmin_ReturnsSuccess()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var examId = await SeedExamAsync(adminId, ExamStatus.Draft);

        var response = await Client.PostAsync($"/api/admin/exams/{examId}/publish", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task UnpublishExam_WithAdmin_ReturnsSuccess()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        AuthenticateAsAdmin(adminId);
        var examId = await SeedExamAsync(adminId, ExamStatus.Active);

        var response = await Client.PostAsync($"/api/admin/exams/{examId}/unpublish", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
