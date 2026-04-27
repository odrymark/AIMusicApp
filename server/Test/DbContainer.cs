using Testcontainers.PostgreSql;
using Xunit;

namespace Test;

public class DbContainer : IAsyncLifetime
{
    public PostgreSqlContainer Container { get; }

    public DbContainer()
    {
        Container = new PostgreSqlBuilder("postgres:17")
            .WithDatabase("pigeons_test")
            .WithUsername("test")
            .WithPassword("test")
            .Build();
    }
    
    public async ValueTask InitializeAsync() => await Container.StartAsync();
    public async ValueTask DisposeAsync()
    {
        await Container.DisposeAsync().AsTask();
        GC.SuppressFinalize(this);
    }
}