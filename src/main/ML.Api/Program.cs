using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using ML.Api.Filters;
using ML.Api.Services;
using ML.Application;
using ML.Application.Common.Interfaces;
using ML.Infrastructure;
using ML.Infrastructure.Data;
using Scalar.AspNetCore;
using Serilog;
using System.IO.Compression;
using System.Security.Cryptography;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Host.UseSerilog((context, config) =>
{
    config.Enrich.FromLogContext()
        .WriteTo.Console()
        .ReadFrom.Configuration(context.Configuration);

});

builder.Services.AddScoped<ApiKeyAuthorizationFilter>();
builder.Services.AddControllers(x =>
{
    //x.Filters.Add<ApiKeyAuthorizationFilter>();
});
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer<OpenApiFilter>();
});

builder.Services.AddHealthChecks()
    .AddNpgSql(connectionString: builder.Configuration.GetConnectionString("DefaultConnection")!, name: "postgres",
        failureStatus: HealthStatus.Degraded, tags: ["db", "postgres", "data"])
    .AddUrlGroup(new Uri(builder.Configuration["OpenTelemetry:HealthUrl"]!), name: "jaeger-ui", failureStatus: HealthStatus.Degraded,
        tags: ["tracing", "jaeger"])
    .AddUrlGroup(new Uri($"{builder.Configuration["OpenVerseSettings:BaseUrl"]}/v1/audio/?q=test"), name: "open-api-provider",
        failureStatus: HealthStatus.Degraded,tags: ["media", "image", "audio", "open"]);

builder.Services.AddTransient<ICurrentUser, CurrentUser>();
builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1, 0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
});

builder.AddApplicationServices();
builder.AddInfrastructureServices();

builder.Services.AddAuthentication(authentication =>
{
    authentication.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    authentication.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    authentication.DefaultSignInScheme = JwtBearerDefaults.AuthenticationScheme;
    authentication.DefaultSignOutScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = true;
    options.SaveToken = true;
    options.IncludeErrorDetails = true;
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = context => {
            if (context.Exception.GetType() == typeof(SecurityTokenExpiredException))
            {
                context.Response.Headers.Append("X-Token-Expired", "true");
            }
            return Task.CompletedTask;
        }
    };
    string securityKey = builder.Configuration["JwtSettings:Secret"]!;
    options.TokenValidationParameters = new()
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["JwtSettings:Issuer"]!,
        ValidAudience = builder.Configuration["JwtSettings:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(securityKey)),
        ClockSkew = TimeSpan.Zero
    };
})
.AddCookie(IdentityConstants.ApplicationScheme)
.AddCookie(IdentityConstants.ExternalScheme)
.AddCookie(IdentityConstants.TwoFactorUserIdScheme);

builder.Services.AddAuthorizationBuilder()
    .AddPolicy("UserPolicy", pb =>
    {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .AddRequirements()
            .RequireRole("User");
    })
    .AddPolicy("AdminPolicy", pb => {
        pb.RequireAuthenticatedUser()
            .AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
            .AddRequirements()
            .RequireRole("Admin");
    });

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes;
});
builder.Services.Configure<BrotliCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.Optimal;
});
builder.Services.Configure<GzipCompressionProviderOptions>(options =>
{
    options.Level = CompressionLevel.SmallestSize;
});


bool seedDatabase = builder.Configuration.GetValue<bool>("SeedDatabase");
string[] baseUrls = builder.Configuration.GetSection("FEBaseUrl").Get<string[]>()!;
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowBlazorApp", policy =>
    {
        policy.WithOrigins(baseUrls)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .AllowCredentials();
    });
});

var app = builder.Build();
app.UseCors("AllowBlazorApp");
app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
//if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(x =>
    {
        x.WithTitle("Media Locator Api");
        x.WithTheme(ScalarTheme.Moon);
    });
    app.UseSwaggerUi(settings =>
    {
        settings.DocumentTitle = "Media Locator Api";
        settings.DocumentPath = "openapi/v1.json";
    });
}

if(seedDatabase)
{
    await app.InitialiseDatabaseAsync();
}

app.UseHttpsRedirection();

app.UseResponseCompression();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = _ => true,
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/db", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("db"),
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});


app.Run();
