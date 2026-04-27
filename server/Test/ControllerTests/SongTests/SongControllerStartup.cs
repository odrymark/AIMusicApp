using Api.Controllers;
using Api.Services.Song;
using Api.Services.R2;
using FHHelper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using System.Security.Claims;

namespace Test.ControllerTests.SongTests;

public class SongControllerStartup
{
    public void ConfigureServices(IServiceCollection services)
    {
        services.AddSingleton(Substitute.For<ISongService>());
        services.AddSingleton(Substitute.For<IR2Service>());
        services.AddSingleton(Substitute.For<IFeatureStateProvider>());
        services.AddTransient<SongController>();
        services.AddSingleton(this);
    }

    public static SongController GetController(IServiceProvider provider)
    {
        var controller = provider.GetRequiredService<SongController>();
        controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext()
        };
        return controller;
    }

    public static void SetupUserClaims(SongController controller, Guid userId)
    {
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        controller.ControllerContext.HttpContext.User = new ClaimsPrincipal(identity);
    }
}