using Amazon.Runtime;
using Amazon.S3;
using EduPortal.Application.Common;
using EduPortal.Application.Interfaces;
using EduPortal.Infrastructure.Persistence;
using EduPortal.Infrastructure.Persistence.Repositories;
using EduPortal.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Resend;
using StackExchange.Redis;

namespace EduPortal.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        // EF Core / PostgreSQL
        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(configuration.GetConnectionString("DefaultConnection"),
                b => b.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // Redis
        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var connStr = configuration["Redis:ConnectionString"] ?? "localhost:6379";
            return ConnectionMultiplexer.Connect(connStr);
        });
        services.AddScoped<ICacheService, RedisCacheService>();

        // Cloudflare R2 (S3-compatible)
        services.AddSingleton<IAmazonS3>(_ =>
        {
            var accountId = configuration["Storage:AccountId"] ?? "";
            var accessKey = configuration["Storage:AccessKeyId"] ?? "";
            var secretKey = configuration["Storage:SecretAccessKey"] ?? "";

            var config = new AmazonS3Config
            {
                ServiceURL = $"https://{accountId}.r2.cloudflarestorage.com",
                ForcePathStyle = true
            };
            return new AmazonS3Client(new BasicAWSCredentials(accessKey, secretKey), config);
        });
        services.AddScoped<IStorageService, CloudflareR2StorageService>();

        // Resend Email
        services.AddOptions<ResendClientOptions>().Configure(opts =>
        {
            opts.ApiToken = configuration["Email:ResendApiKey"] ?? "";
        });
        services.AddHttpClient<ResendClient>();
        services.AddTransient<IResend, ResendClient>();
        services.AddScoped<IEmailService, ResendEmailService>();

        // PDF Generator
        services.AddScoped<IPdfGeneratorService, QuestPdfCertificateService>();

        // JWT Token Service
        services.AddScoped<ITokenService, JwtTokenService>();

        // Password Hasher
        services.AddScoped<IPasswordHasher, BcryptPasswordHasher>();

        // HTTP Context
        services.AddHttpContextAccessor();
        services.AddScoped<ICurrentUserService, CurrentUserService>();

        // Repositories
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<ICategoryRepository, CategoryRepository>();
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IOrderRepository, OrderRepository>();
        services.AddScoped<IEnrollmentRepository, EnrollmentRepository>();
        services.AddScoped<ICmsRepository, CmsRepository>();

        return services;
    }
}
