using Api.Services.Playlist;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Test.ServiceTests.PlaylistTests;

public class PlaylistStartup
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

        services.AddScoped<IPlaylistService, PlaylistService>();
    }
}