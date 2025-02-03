using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using ML.Application.Common.Interfaces;
using ML.Domain.Entities;
using ML.Infrastructure.Data;
using ML.Infrastructure.Data.Interceptors;
using ML.Infrastructure.Email;
using ML.Infrastructure.Identity;
using Polly;
using Polly.Retry;

namespace ML.Infrastructure;

public static class DependencyInjection
{
    public static void AddInfrastructureServices(this IHostApplicationBuilder builder)
    {
        string? connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
        if (string.IsNullOrEmpty(connectionString))
        {
            throw new InvalidOperationException("Connection string not found");
        }

        builder.Services.AddScoped<ISaveChangesInterceptor, AuditableEntityInterceptor>();

        builder.Services.AddDbContextPool<MLDbContext>((sp, options) =>
        {
            options.AddInterceptors(sp.GetServices<ISaveChangesInterceptor>());
            options.UseNpgsql(connectionString,
                npgsqlOptions =>
                {
                    npgsqlOptions.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: TimeSpan.FromSeconds(10),
                        errorCodesToAdd: null
                        );
                });
        });

        builder.Services.AddIdentityCore<Users>(
            options =>
            {
                options.User.RequireUniqueEmail = true;
                options.Password.RequireNonAlphanumeric = true;
                options.Password.RequiredLength = 12;
                options.Password.RequireDigit = true;
                options.Password.RequireLowercase = true;
                options.Password.RequireUppercase = true;
                options.SignIn.RequireConfirmedEmail = true;
                options.SignIn.RequireConfirmedAccount = true;
                options.Lockout.AllowedForNewUsers = true;
                options.Lockout.MaxFailedAccessAttempts = 5;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromHours(24);
            }
            )
            .AddRoles<UserRoles>()
            .AddSignInManager()
            .AddTokenProvider<DataProtectorTokenProvider<Users>>("MediaLocatorApp")
            .AddEntityFrameworkStores<MLDbContext>()
            .AddDefaultTokenProviders();

        builder.Services.AddDataProtection()
                        .PersistKeysToDbContext<MLDbContext>()
                        .SetApplicationName("MediaLocatorApplicationService");

        builder.Services.AddHttpClient("OpenApi")
            .ConfigureHttpClient((sp, client) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["OpenApi:BaseAddress"]);
            })
            .AddResilienceHandler("retry", pipeline =>
            {
                pipeline.AddRetry(new HttpRetryStrategyOptions
                {
                    Delay = TimeSpan.FromSeconds(1),
                    MaxRetryAttempts = 3,
                    BackoffType = DelayBackoffType.Exponential,
                    UseJitter = true
                });
                pipeline.AddCircuitBreaker(new HttpCircuitBreakerStrategyOptions
                {
                    FailureRatio = 0.5,
                    SamplingDuration = TimeSpan.FromSeconds(30),
                    MinimumThroughput = 10,
                    BreakDuration = TimeSpan.FromSeconds(30)
                });
                pipeline.AddTimeout(new HttpTimeoutStrategyOptions
                {
                    Timeout = TimeSpan.FromSeconds(10)
                });
            });

        builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
        builder.Services.AddTransient<IEmailService, EmailService>();

        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddTransient<IJwtService, JwtService>();

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
    }
}
