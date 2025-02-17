using Microsoft.Extensions.Options;
using ML.Api.Filters;
using ML.Api.Services;
using ML.Application;
using ML.Application.Common.Interfaces;
using ML.Infrastructure;
using Scalar.AspNetCore;
using Serilog;

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

builder.Services.AddHealthChecks();

builder.Services.AddTransient<ICurrentUser, CurrentUser>();
builder.Services.AddApiVersioning(x =>
{
    x.DefaultApiVersion = new ApiVersion(1, 0);
    x.AssumeDefaultVersionWhenUnspecified = true;
    x.ReportApiVersions = true;
});

builder.AddApplicationServices();
builder.AddInfrastructureServices();


var app = builder.Build();
//app.UseMiddleware<GlobalExceptionHandlingMiddleware>();
// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
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



app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.UseHealthChecks("/health");


app.Run();
