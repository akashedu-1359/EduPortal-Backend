using Amazon.S3;
using EduPortal.Application.Interfaces;
using EduPortal.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Moq;
using StackExchange.Redis;

namespace EduPortal.IntegrationTests.Fixtures;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private SqliteConnection? _connection;

    static CustomWebApplicationFactory()
    {
        Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        builder.UseSetting("JwtSettings:Secret", "ThisIsATestSecretKeyThatMustBe32CharsLong!!");
        builder.UseSetting("JwtSettings:Issuer", "EduPortal.Tests");
        builder.UseSetting("JwtSettings:Audience", "EduPortal.Tests");
        builder.UseSetting("JwtSettings:AccessTokenMinutes", "60");
        builder.UseSetting("ConnectionStrings:DefaultConnection", "Data Source=:memory:");
        builder.UseSetting("Redis:ConnectionString", "localhost:6379");
        builder.UseSetting("Storage:AccountId", "test");
        builder.UseSetting("Storage:AccessKeyId", "test");
        builder.UseSetting("Storage:SecretAccessKey", "test");
        builder.UseSetting("Email:ResendApiKey", "test_key");

        builder.ConfigureLogging(logging =>
        {
            logging.ClearProviders();
            logging.AddConsole();
        });

        builder.ConfigureServices(services =>
        {
            // Remove PostgreSQL DbContext and replace with SQLite in-memory
            services.RemoveAll<DbContextOptions<AppDbContext>>();
            services.RemoveAll<AppDbContext>();

            _connection = new SqliteConnection("Data Source=:memory:");
            _connection.Open();

            services.AddDbContext<AppDbContext>(options =>
            {
                options.UseSqlite(_connection);
            });

            // Remove Redis
            services.RemoveAll<IConnectionMultiplexer>();
            var redisMock = new Mock<IConnectionMultiplexer>();
            redisMock.Setup(r => r.IsConnected).Returns(true);
            redisMock.Setup(r => r.GetDatabase(It.IsAny<int>(), It.IsAny<object>()))
                .Returns(new Mock<IDatabase>().Object);
            services.AddSingleton(redisMock.Object);

            // Replace ICacheService with no-op
            services.RemoveAll<ICacheService>();
            services.AddScoped<ICacheService, NoOpCacheService>();

            // Replace S3/Storage with no-op
            services.RemoveAll<IAmazonS3>();
            services.AddSingleton(new Mock<IAmazonS3>().Object);
            services.RemoveAll<IStorageService>();
            services.AddScoped<IStorageService, NoOpStorageService>();

            // Replace Email with no-op
            services.RemoveAll<IEmailService>();
            services.AddScoped<IEmailService, NoOpEmailService>();

            // Replace PDF generator with no-op
            services.RemoveAll<IPdfGeneratorService>();
            services.AddScoped<IPdfGeneratorService, NoOpPdfGeneratorService>();

            // Replace Revalidation with no-op
            services.RemoveAll<IRevalidationService>();
            services.AddScoped<IRevalidationService, NoOpRevalidationService>();

            // Replace payment gateways with test doubles
            services.RemoveAll<IPaymentGateway>();
            services.AddScoped<IPaymentGateway, TestPaymentGateway>();

            // Remove background services that could interfere
            services.RemoveAll<IHostedService>();

            // Remove health checks that depend on real Npgsql/Redis and re-add empty
            services.RemoveAll<Microsoft.Extensions.Diagnostics.HealthChecks.IHealthCheck>();
            services.AddHealthChecks();
        });
    }

    protected override IHost CreateHost(IHostBuilder builder)
    {
        var host = base.CreateHost(builder);

        using var scope = host.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        DataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>()).GetAwaiter().GetResult();

        return host;
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        _connection?.Close();
        _connection?.Dispose();
    }
}

#region No-Op Service Implementations

internal class NoOpCacheService : ICacheService
{
    public Task<T?> GetAsync<T>(string key, CancellationToken ct = default) => Task.FromResult<T?>(default);
    public Task SetAsync<T>(string key, T value, TimeSpan ttl, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteAsync(string key, CancellationToken ct = default) => Task.CompletedTask;
    public Task DeleteByPatternAsync(string pattern, CancellationToken ct = default) => Task.CompletedTask;
}

internal class NoOpStorageService : IStorageService
{
    public Task<string> GetUploadUrlAsync(string objectKey, string contentType, int expirySeconds = 3600, CancellationToken ct = default)
        => Task.FromResult($"https://test-storage.local/{objectKey}");
    public Task<string> GetReadUrlAsync(string objectKey, int expirySeconds = 3600, CancellationToken ct = default)
        => Task.FromResult($"https://test-storage.local/{objectKey}");
    public Task UploadAsync(string objectKey, Stream content, string contentType, CancellationToken ct = default)
        => Task.CompletedTask;
    public Task DeleteAsync(string objectKey, CancellationToken ct = default) => Task.CompletedTask;
}

internal class NoOpEmailService : IEmailService
{
    public Task SendWelcomeAsync(string toEmail, string fullName, CancellationToken ct = default) => Task.CompletedTask;
    public Task SendCertificateAsync(string toEmail, string fullName, string examTitle, string certificateUrl, CancellationToken ct = default) => Task.CompletedTask;
    public Task SendCertificateEmailAsync(string toEmail, string fullName, string certificateUrl, CancellationToken ct = default) => Task.CompletedTask;
    public Task SendPasswordResetAsync(string toEmail, string resetLink, CancellationToken ct = default) => Task.CompletedTask;
}

internal class NoOpPdfGeneratorService : IPdfGeneratorService
{
    public Task<byte[]> GenerateCertificateAsync(string userName, string examTitle, decimal score, DateTime issuedAt, CancellationToken ct = default)
        => Task.FromResult(new byte[] { 0x25, 0x50, 0x44, 0x46 }); // %PDF header bytes
}

internal class NoOpRevalidationService : IRevalidationService
{
    public Task TriggerRevalidationAsync(string tag, CancellationToken ct = default) => Task.CompletedTask;
}

internal class TestPaymentGateway : IPaymentGateway
{
    public string GatewayName => "Stripe";
    public Task<CreateOrderResponse> CreateOrderAsync(CreateOrderRequest request, CancellationToken ct = default)
        => Task.FromResult(new CreateOrderResponse($"gw_order_{Guid.NewGuid():N}", "cs_test_secret"));
    public Task<bool> VerifyPaymentAsync(VerifyPaymentRequest request, CancellationToken ct = default)
        => Task.FromResult(true);
}

#endregion
