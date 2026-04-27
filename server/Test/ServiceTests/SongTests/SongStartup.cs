using Api.Services.Song;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Test.ServiceTests.SongTests;

public class SongStartup
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

        services.AddScoped<ISongService, SongService>();
    }
}