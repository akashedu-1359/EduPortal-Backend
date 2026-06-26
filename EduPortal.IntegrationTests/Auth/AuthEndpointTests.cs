using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using EduPortal.IntegrationTests.Fixtures;
using FluentAssertions;

namespace EduPortal.IntegrationTests.Auth;

[Trait("Category", "Integration")]
public class AuthEndpointTests : IntegrationTestBase
{
    public AuthEndpointTests(CustomWebApplicationFactory factory) : base(factory) { }

    [Fact]
    public async Task Register_WithValidData_Returns201WithTokens()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "newuser@test.com",
            Password = "StrongP@ssw0rd!",
            FullName = "New User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Created);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();

        var data = json.RootElement.GetProperty("data");
        data.GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
        data.GetProperty("refreshToken").GetString().Should().NotBeNullOrEmpty();
        data.GetProperty("user").GetProperty("email").GetString().Should().Be("newuser@test.com");
    }

    [Fact]
    public async Task Register_WithDuplicateEmail_Returns409()
    {
        // First registration
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "duplicate@test.com",
            Password = "StrongP@ssw0rd!",
            FullName = "User One"
        });

        // Second registration with same email
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "duplicate@test.com",
            Password = "StrongP@ssw0rd!",
            FullName = "User Two"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Conflict);
    }

    [Fact]
    public async Task Register_WithInvalidEmail_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "not-an-email",
            Password = "StrongP@ssw0rd!",
            FullName = "Bad Email User"
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Register_WithMissingFields_Returns400()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "",
            Password = "",
            FullName = ""
        });

        response.StatusCode.Should().Be(HttpStatusCode.BadRequest);
    }

    [Fact]
    public async Task Login_WithValidCredentials_Returns200WithTokens()
    {
        // Register first
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "logintest@test.com",
            Password = "StrongP@ssw0rd!",
            FullName = "Login Test User"
        });

        // Login
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "logintest@test.com",
            Password = "StrongP@ssw0rd!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").GetProperty("accessToken").GetString().Should().NotBeNullOrEmpty();
    }

    [Fact]
    public async Task Login_WithInvalidPassword_Returns401()
    {
        await Client.PostAsJsonAsync("/api/auth/register", new
        {
            Email = "wrongpw@test.com",
            Password = "StrongP@ssw0rd!",
            FullName = "Wrong PW User"
        });

        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "wrongpw@test.com",
            Password = "WrongPassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Login_WithNonexistentEmail_Returns401()
    {
        var response = await Client.PostAsJsonAsync("/api/auth/login", new
        {
            Email = "nobody@test.com",
            Password = "SomePassword!"
        });

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Refresh_WithoutCookie_Returns401()
    {
        var response = await Client.PostAsync("/api/auth/refresh", null);

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ReturnsSuccess()
    {
        var response = await Client.PostAsync("/api/auth/logout", null);

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        var json = JsonDocument.Parse(content);
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
    }
}
