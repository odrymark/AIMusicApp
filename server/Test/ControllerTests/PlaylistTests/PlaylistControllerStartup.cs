using Api.Controllers;
using Api.Services.Playlist;
using Api.Services.R2;
using FHHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Security.Claims;

namespace Test.ControllerTests.PlaylistTests;

public class PlaylistControllerStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(Substitute.For<IPlaylistService>());
        services.AddSingleton(Substitute.For<IR2Service>());
        services.AddSingleton(Substitute.For<IFeatureStateProvider>());  // ← Add this line
        
        services.AddTransient<PlaylistController>();
        services.AddSingleton(this);
    }

    public static PlaylistController GetController(IServiceProvider provider)
    {
        var controller = provider.GetRequiredService<PlaylistController>();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }
    
    public static void SetupUserClaims(PlaylistController controller, Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString()),
            new Claim(ClaimTypes.Name, "testuser")
        };
        
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);

        controller.ControllerContext.HttpContext.User = principal;
    }
}