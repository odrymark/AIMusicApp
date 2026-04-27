using Api.Services;
using Api.Services.Token;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Test.ServiceTests.TokenTests;

public class TokenStartup
{
    private static DbContainer? _container;

    public static void ConfigureServices(IServiceCollection services)
    {
        if (_container == null)
        {
            _container = new DbContainer();
            _container.InitializeAsync().AsTask().GetAwaiter().GetResult();
        }

        services.AddSingleton(_container);

        services.AddDbContext<MusicDbContext>(options =>
            options.UseNpgsql(_container.Container.GetConnectionString()));

        var key = new byte[64];
        new Random().NextBytes(key);
        var jwtKey = Convert.ToBase64String(key);
        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Key"] = jwtKey,
                ["Jwt:Issuer"] = "test_issuer",
                ["Jwt:Audience"] = "test_audience",
                ["Jwt:ExpireMinutes"] = "15"
            })
            .Build();

        services.AddSingleton<IConfiguration>(config);

        services.AddSingleton<ITokenService, TokenService>();
    }
}