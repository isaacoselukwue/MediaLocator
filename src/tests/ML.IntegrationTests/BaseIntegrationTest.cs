using MediatR;
using Microsoft.Extensions.DependencyInjection;
using ML.Infrastructure.Data;

namespace ML.IntegrationTests;

public abstract class BaseIntegrationTest : IDisposable
{
    protected IServiceScope _scope;
    protected ISender Sender;
    protected MLDbContext DbContext;
    protected static IntegrationTestWebAppFactory Factory;

    static BaseIntegrationTest()
    {
        Factory = new IntegrationTestWebAppFactory();
        Factory.InitializeAsync().GetAwaiter().GetResult();
    }

    [SetUp]
    public virtual void Setup()
    {
        _scope = Factory.Services.CreateScope();
        Sender = _scope.ServiceProvider.GetRequiredService<ISender>();
        DbContext = _scope.ServiceProvider.GetRequiredService<MLDbContext>();
    }

    [TearDown]
    public virtual void TearDown()
    {
        _scope?.Dispose();
    }

    [OneTimeTearDown]
    public static async Task DisposeFactory()
    {
        if (Factory != null)
        {
            await Factory.DisposeAsync();
            Factory.Dispose();
        }
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
    }
}