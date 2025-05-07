global using Microsoft.AspNetCore.DataProtection;
global using Microsoft.AspNetCore.Identity;
global using Microsoft.Extensions.Configuration;
global using Microsoft.Extensions.DependencyInjection;
global using Microsoft.Extensions.Hosting;
global using Microsoft.Extensions.Http.Resilience;
global using ML.Application.Common.Interfaces;
global using ML.Infrastructure.Data;
global using ML.Infrastructure.Data.Interceptors;
global using ML.Infrastructure.Email;
global using ML.Infrastructure.Identity;
global using Polly;
using MassTransit;
using MassTransit.Logging;
using MediatR;
using ML.Infrastructure.OpenVerse;
using ML.Infrastructure.Queue;
using ML.Infrastructure.Search;
using Npgsql;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

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
            using (var scope = sp.CreateScope())
            {
                var scopedProvider = scope.ServiceProvider;
                options.AddInterceptors(scopedProvider.GetServices<ISaveChangesInterceptor>());
            }
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
        builder.Services.AddScoped<MLDbContextInitialiser>();
        builder.Services.AddScoped<IMLDbContext>(provider => provider.GetRequiredService<MLDbContext>());
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

        builder.Services.AddHttpClient("OpenVerseClient")
            .ConfigureHttpClient((sp, client) =>
            {
                var configuration = sp.GetRequiredService<IConfiguration>();
                client.BaseAddress = new Uri(configuration["OpenVerseSettings:BaseUrl"]!);
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
        string serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "MediaLocator";
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithMetrics(metrics =>
            {
                metrics.AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddNpgsqlInstrumentation()
                    .AddMeter("Microsoft.AspNetCore.Hosting")
                    .AddMeter("Microsoft.AspNetCore.Server.Kestrel")
                    .AddMeter("System.Net.Http")
                    .AddMeter("System.Net.NameResolution");
            })
            .WithTracing(tracing =>
            {
                tracing
                .AddSource(DiagnosticHeaders.DefaultListenerName)
                .AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddNpgsql();

                tracing.AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317");
                });
            });

        //builder.Services.AddMemoryCache();
#pragma warning disable EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.
        //builder.Services.AddStackExchangeRedisCache(options =>
        //{
        //    options.Configuration = builder.Configuration.GetConnectionString("RedisConnection");
        //});
        builder.Services.AddHybridCache(options =>
        {
            options.MaximumPayloadBytes = 1024 * 1024 * 20;
            options.MaximumKeyLength = 512;

            options.DefaultEntryOptions = new Microsoft.Extensions.Caching.Hybrid.HybridCacheEntryOptions
            {
                Expiration = TimeSpan.FromHours(24),
                LocalCacheExpiration = TimeSpan.FromHours(24)
            };
        });
#pragma warning restore EXTEXP0018 // Type is for evaluation purposes only and is subject to change or removal in future updates. Suppress this diagnostic to proceed.

        builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
        builder.Services.AddTransient<IEmailService, EmailService>();

        builder.Services.AddTransient<IIdentityService, IdentityService>();

        builder.Services.AddSingleton<TimeProvider>(TimeProvider.System);

        builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));
        builder.Services.AddTransient<IJwtService, JwtService>();

        builder.Services.Configure<OpenVerseSettings>(builder.Configuration.GetSection("OpenVerseSettings"));
        builder.Services.AddTransient<IOpenVerseService, OpenVerseService>();

        builder.Services.AddTransient<ISearchService, SearchService>();

        builder.Services.AddMassTransit(config =>
        {
            config.UsingRabbitMq((context, cfg) =>
            {
                RabbitMqSettings queueSettings = builder.Configuration.GetSection("RabbitMqSettings").Get<RabbitMqSettings>()!;

                cfg.Host(queueSettings.Host, queueSettings.VHost, h =>
                {
                    h.Username(queueSettings.Username!);
                    h.Password(queueSettings.Password!);
                });

                cfg.ConfigureEndpoints(context);
                cfg.UseInstrumentation();
            });
            config.AddHealthChecks();
        });
        builder.Services.AddScoped<IPublisher, MassTransitEventPublisher>();
    }

    public static void AddInfrastructureWorkerServices(this IHostApplicationBuilder builder)
    {
        string serviceName = builder.Configuration["OpenTelemetry:ServiceName"] ?? "MediaLocatorWorker";
        builder.Services
            .AddOpenTelemetry()
            .ConfigureResource(resource => resource.AddService(serviceName))
            .WithTracing(tracing =>
            {
                tracing
                .AddSource(DiagnosticHeaders.DefaultListenerName)
                .AddSource("MassTransit")
                    .AddAspNetCoreInstrumentation()
                    .AddHttpClientInstrumentation()
                    .AddEntityFrameworkCoreInstrumentation()
                    .AddRedisInstrumentation()
                    .AddNpgsql();

                tracing.AddOtlpExporter(exporter =>
                {
                    exporter.Endpoint = new Uri(builder.Configuration["OpenTelemetry:OtlpEndpoint"] ?? "http://localhost:4317");
                });
            });



        builder.Services.Configure<MailSettings>(builder.Configuration.GetSection("MailSettings"));
        builder.Services.AddTransient<IEmailService, EmailService>();

        builder.Services.AddSingleton(TimeProvider.System);
    }
}
