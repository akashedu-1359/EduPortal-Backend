using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EduPortal.Domain.Enums;
using DomainEntities = EduPortal.Domain.Entities;
using EduPortal.Infrastructure.Persistence;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;

namespace EduPortal.IntegrationTests.User;

[Trait("Category", "Integration")]
public class UserExamsTests : IntegrationTestBase
{
    public UserExamsTests(CustomWebApplicationFactory factory) : base(factory) { }

    private async Task<Guid> SeedPublishedExamAsync(Guid adminId)
    {
        using var scope = CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        var exam = new DomainEntities.Exam("Test Exam", "A test exam", 30, 60m, adminId)
        {
            MaxAttempts = 3,
            Status = ExamStatus.Active
        };
        exam.Questions.Add(new DomainEntities.Question(exam.Id, "What is 2+2?", "3", "4", "5", "6", 1, 1));
        exam.Questions.Add(new DomainEntities.Question(exam.Id, "What is 3+3?", "5", "6", "7", "8", 1, 2));

        db.Exams.Add(exam);
        await db.SaveChangesAsync();
        return exam.Id;
    }

    [Fact]
    public async Task GetExams_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.GetAsync("/api/user/exams");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task GetExams_WithAuth_Returns200()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.GetAsync("/api/user/exams");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task StartExam_WithAuth_Returns201()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        var examId = await SeedPublishedExamAsync(adminId);

        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.PostAsync($"/api/user/exams/{examId}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.Created);
    }

    [Fact]
    public async Task StartExam_WithoutAuth_Returns401()
    {
        ClearAuthentication();

        var response = await Client.PostAsync($"/api/user/exams/{Guid.NewGuid()}/start", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task StartExam_NonexistentExam_ReturnsError()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var response = await Client.PostAsync($"/api/user/exams/{Guid.NewGuid()}/start", null);

        response.StatusCode.Should().BeOneOf(HttpStatusCode.NotFound, HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task SubmitExam_AfterStart_Returns200()
    {
        var (_, adminId) = await CreateTestAdminAsync();
        var examId = await SeedPublishedExamAsync(adminId);

        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var startResponse = await Client.PostAsync($"/api/user/exams/{examId}/start", null);
        startResponse.StatusCode.Should().Be(HttpStatusCode.Created);

        var startJson = await startResponse.Content.ReadFromJsonAsync<JsonElement>();
        var attemptId = startJson.GetProperty("attemptId").GetGuid();
        var questionId = startJson.GetProperty("questions")[0].GetProperty("id").GetGuid();

        var submitResponse = await Client.PostAsJsonAsync($"/api/user/exams/attempts/{attemptId}/submit", new
        {
            AttemptId = attemptId,
            Answers = new[] { new { QuestionId = questionId, SelectedOptionIndex = 1 } }
        });

        submitResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task SubmitExam_WithMismatchedAttemptId_Returns400()
    {
        var (_, userId) = await CreateTestUserAsync();
        AuthenticateAsUser(userId);

        var attemptId = Guid.NewGuid();
        var differentAttemptId = Guid.NewGuid();

        var response = await Client.PostAsJsonAsync($"/api/user/exams/attempts/{attemptId}/submit", new
        {
            AttemptId = differentAttemptId,
            Answers = new[] { new { QuestionId = Guid.NewGuid(), SelectedOptionIndex = 1 } }
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }
}
