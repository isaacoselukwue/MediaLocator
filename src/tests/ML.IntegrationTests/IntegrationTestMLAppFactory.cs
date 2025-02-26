using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestPlatform.TestHost;
using ML.Infrastructure.Data;
using Testcontainers.PostgreSql;

namespace ML.IntegrationTests;

public class IntegrationTestWebAppFactory : WebApplicationFactory<Program>
{
    private readonly PostgreSqlContainer _dbContainer;

    public IntegrationTestWebAppFactory()
    {
        _dbContainer = new PostgreSqlBuilder()
            .WithImage("postgres:15")
            .WithPassword("Strong_password_123!")
            .WithUsername("postgres")
            .WithDatabase("ml_test_db")
            .Build();
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureTestServices(services =>
        {
            var descriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextOptions<MLDbContext>));

            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            var pooledDescriptor = services.SingleOrDefault(s => s.ServiceType == typeof(DbContextPool<MLDbContext>));

            if (pooledDescriptor is not null)
            {
                services.Remove(pooledDescriptor);
            }

            services.AddDbContext<MLDbContext>(options => options.UseNpgsql(_dbContainer.GetConnectionString()));

            var interceptorDescriptor = services
                .SingleOrDefault(s => s.ServiceType == typeof(Microsoft.EntityFrameworkCore.Diagnostics.ISaveChangesInterceptor));

            if (interceptorDescriptor is not null && interceptorDescriptor.Lifetime == ServiceLifetime.Scoped)
            {
                services.Remove(interceptorDescriptor);
                services.AddSingleton(interceptorDescriptor.ImplementationType);
            }
        });
    }

    public async Task InitializeAsync()
    {
        await _dbContainer.StartAsync();
    }

    public async Task DisposeAsync()
    {
        await _dbContainer.StopAsync();
    }

}