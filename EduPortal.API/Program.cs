using System.Text;
using EduPortal.Application;
using EduPortal.Application.Common;
using EduPortal.Infrastructure;
using EduPortal.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;
using System.Threading.RateLimiting;
using EduPortal.API.Middleware;

// Bootstrap logger for startup errors
Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateBootstrapLogger();

try
{
    var builder = WebApplication.CreateBuilder(args);

    // Serilog
    builder.Host.UseSerilog((ctx, services, cfg) =>
    {
        cfg.ReadFrom.Configuration(ctx.Configuration)
           .ReadFrom.Services(services)
           .Enrich.FromLogContext()
           .Enrich.WithMachineName()
           .Enrich.WithThreadId();

        if (ctx.HostingEnvironment.IsDevelopment())
            cfg.WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}");
        else
            cfg.WriteTo.Console().WriteTo.File("logs/log-.txt", rollingInterval: RollingInterval.Day);
    });

    // Application & Infrastructure layers
    builder.Services.AddApplication();
    builder.Services.AddInfrastructure(builder.Configuration);

    // Controllers
    builder.Services.AddControllers();
    builder.Services.AddEndpointsApiExplorer();

    // Swagger
    builder.Services.AddSwaggerGen(c =>
    {
        c.SwaggerDoc("v1", new OpenApiInfo { Title = "EduPortal API", Version = "v1" });
        c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
        {
            Type = SecuritySchemeType.Http,
            Scheme = "bearer",
            BearerFormat = "JWT",
            Description = "Enter JWT Bearer token"
        });
        c.AddSecurityRequirement(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecurityScheme { Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" } },
                Array.Empty<string>()
            }
        });
    });

    // JWT Auth
    var jwtSecret = builder.Configuration["JwtSettings:Secret"]
        ?? throw new InvalidOperationException("JwtSettings:Secret is required");

    builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
        .AddJwtBearer(opts =>
        {
            opts.TokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSecret)),
                ValidateIssuer = true,
                ValidIssuer = builder.Configuration["JwtSettings:Issuer"],
                ValidateAudience = true,
                ValidAudience = builder.Configuration["JwtSettings:Audience"],
                ClockSkew = TimeSpan.Zero
            };
        });

    // Authorization
    builder.Services.AddAuthorization(opts =>
    {
        opts.AddPolicy("ContentWrite", policy => policy.RequireClaim("permissions", PermissionKeys.ContentWrite));
        opts.AddPolicy("CmsManage", policy => policy.RequireClaim("permissions", PermissionKeys.CmsManage));
        opts.AddPolicy("ExamManage", policy => policy.RequireClaim("permissions", PermissionKeys.ExamManage));
        opts.AddPolicy("AnalyticsView", policy => policy.RequireClaim("permissions", PermissionKeys.AnalyticsView));
        opts.AddPolicy("UserManage", policy => policy.RequireClaim("permissions", PermissionKeys.ManageUsers));
        opts.AddPolicy("AdminOnly", policy => policy.RequireRole("SuperAdmin", "Admin"));
    });

    // Rate Limiting
    builder.Services.AddRateLimiter(opts =>
    {
        // Login: 5 per 15 min per IP
        opts.AddSlidingWindowLimiter("login", limiterOpts =>
        {
            limiterOpts.PermitLimit = 5;
            limiterOpts.Window = TimeSpan.FromMinutes(15);
            limiterOpts.SegmentsPerWindow = 3;
            limiterOpts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOpts.QueueLimit = 0;
        });

        // Global: 100 per min per IP
        opts.AddFixedWindowLimiter("global", limiterOpts =>
        {
            limiterOpts.PermitLimit = 100;
            limiterOpts.Window = TimeSpan.FromMinutes(1);
            limiterOpts.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
            limiterOpts.QueueLimit = 0;
        });

        opts.RejectionStatusCode = 429;
    });

    // CORS
    var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
        ?? ["http://localhost:3000"];
    builder.Services.AddCors(opts =>
    {
        opts.AddPolicy("default", policy =>
            policy.WithOrigins(allowedOrigins).AllowAnyMethod().AllowAnyHeader().AllowCredentials());
    });

    // Health Checks
    builder.Services.AddHealthChecks()
        .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection") ?? "", name: "postgres")
        .AddRedis(builder.Configuration["Redis:ConnectionString"] ?? "localhost:6379", name: "redis");

    var app = builder.Build();

    // Migrate & Seed on startup
    using (var scope = app.Services.CreateScope())
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync();
        await DataSeeder.SeedAsync(db, scope.ServiceProvider.GetRequiredService<ILogger<AppDbContext>>());
    }

    // Middleware pipeline
    app.UseMiddleware<ExceptionHandlingMiddleware>();
    app.UseMiddleware<RequestLoggingMiddleware>();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "EduPortal API v1"));
    }

    // Security headers
    app.Use(async (ctx, next) =>
    {
        ctx.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        ctx.Response.Headers.Append("X-Frame-Options", "DENY");
        ctx.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
        await next();
    });

    app.UseHsts();
    app.UseCors("default");
    app.UseRateLimiter();
    app.UseAuthentication();
    app.UseAuthorization();

    // Health check endpoint
    app.MapGet("/api/health", async (AppDbContext db, StackExchange.Redis.IConnectionMultiplexer redis) =>
    {
        var dbOk = await db.Database.CanConnectAsync();
        var redisOk = redis.IsConnected;
        var healthy = dbOk && redisOk;

        var result = new
        {
            status = healthy ? "healthy" : "degraded",
            timestamp = DateTime.UtcNow,
            version = "1.0.0",
            checks = new { postgres = dbOk ? "healthy" : "unhealthy", redis = redisOk ? "healthy" : "unhealthy" }
        };

        return healthy ? Results.Ok(result) : Results.Json(result, statusCode: 503);
    });

    app.MapControllers();

    Log.Information("EduPortal API starting up...");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application startup failed.");
    throw;
}
finally
{
    Log.CloseAndFlush();
}
