using Api.Controllers;
using Api.DTOs.Request;
using Api.DTOs.Response;
using Api.Services.Auth;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using System.Security.Claims;
using NSubstitute.ExceptionExtensions;
using Xunit.DependencyInjection;

namespace Test.ControllerTests.AuthTests;

[Startup(typeof(AuthControllerStartup))]
public class AuthControllerTests(IAuthService mockAuthService, IServiceProvider provider)
{
    private AuthController GetFreshController() => AuthControllerStartup.GetController(provider);

    [Fact]
    public async Task Login_Returns_Ok_With_Valid_Credentials()
    {
        var controller = GetFreshController();
        var loginRequest = new UserLoginReqDto { username = "testuser", password = "password123" };
        var userId = Guid.NewGuid();
        
        var loginResponse = new UserLoginResDto
        {
            id = userId,
            username = "testuser",
            token = "jwt-token",
            refreshToken = "refresh-token",
            isAdmin = false
        };
        
        mockAuthService.ClearReceivedCalls();
        mockAuthService.Login(loginRequest).Returns(Task.FromResult(loginResponse));

        var result = await controller.Login(loginRequest);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetMeResDto>(okResult.Value);
        
        Assert.Equal(userId, response.id);
        Assert.Equal("testuser", response.username);
        Assert.False(response.isAdmin);
        Assert.True(controller.Response.Headers.SetCookie.Count > 0);
    }

    [Fact]
    public async Task Login_Returns_Unauthorized_With_Invalid_Credentials()
    {
        var controller = GetFreshController();
        var loginRequest = new UserLoginReqDto { username = "testuser", password = "wrong" };
        
        mockAuthService.Login(loginRequest).ThrowsAsync(new UnauthorizedAccessException());

        var result = await controller.Login(loginRequest);

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var message = GetErrorMessage(unauthorizedResult);
        Assert.Equal("Invalid login credentials", message);
    }

    [Fact]
    public async Task Login_Sets_Jwt_Cookie()
    {
        var controller = GetFreshController();
        var loginRequest = new UserLoginReqDto { username = "testuser", password = "password123" };
        var userId = Guid.NewGuid();
        
        var loginResponse = new UserLoginResDto
        {
            id = userId,
            username = "testuser",
            token = "jwt-token",
            refreshToken = "refresh-token",
            isAdmin = false
        };
        
        mockAuthService.Login(loginRequest).Returns(Task.FromResult(loginResponse));

        await controller.Login(loginRequest);

        var setCookieHeader = controller.Response.Headers.SetCookie;
        var jwtCookie = setCookieHeader.FirstOrDefault(c => c!.StartsWith("jwt="));
        Assert.NotNull(jwtCookie);
    }

    [Fact]
    public async Task Login_Sets_RefreshToken_Cookie()
    {
        var controller = GetFreshController();
        var loginRequest = new UserLoginReqDto { username = "testuser", password = "password123" };
        var userId = Guid.NewGuid();
        
        var loginResponse = new UserLoginResDto
        {
            id = userId,
            username = "testuser",
            token = "jwt-token",
            refreshToken = "refresh-token",
            isAdmin = false
        };
        
        mockAuthService.Login(loginRequest).Returns(Task.FromResult(loginResponse));

        await controller.Login(loginRequest);

        var setCookieHeader = controller.Response.Headers.SetCookie;
        var refreshCookie = setCookieHeader.FirstOrDefault(c => c!.StartsWith("refreshToken="));
        Assert.NotNull(refreshCookie);
    }

    [Fact]
    public async Task Login_Sets_Admin_Flag_When_User_Is_Admin()
    {
        var controller = GetFreshController();
        var loginRequest = new UserLoginReqDto { username = "adminuser", password = "password123" };
        var userId = Guid.NewGuid();
        
        var loginResponse = new UserLoginResDto
        {
            id = userId,
            username = "adminuser",
            token = "jwt-token",
            refreshToken = "refresh-token",
            isAdmin = true
        };
        
        mockAuthService.Login(loginRequest).Returns(Task.FromResult(loginResponse));

        var result = await controller.Login(loginRequest);

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetMeResDto>(okResult.Value);
        Assert.True(response.isAdmin);
    }

    [Fact]
    public async Task Refresh_Returns_Ok_With_Valid_RefreshToken()
    {
        var controller = GetFreshController();
        var refreshToken = "valid-rt";
        AuthControllerStartup.SetupRequestCookies(controller, new() { ["refreshToken"] = refreshToken });
        mockAuthService.ClearReceivedCalls();
        mockAuthService.RefreshToken(refreshToken, 7).Returns(("new-jwt", "new-rt"));

        var result = await controller.Refresh();

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Refresh_Returns_Unauthorized_When_No_RefreshToken()
    {
        var controller = GetFreshController();
        AuthControllerStartup.SetupRequestCookies(controller, new Dictionary<string, string>());
    
        var result = await controller.Refresh();

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var message = GetErrorMessage(unauthorizedResult);
        Assert.Equal("Missing refresh token", message);
    }

    [Fact]
    public async Task Refresh_Returns_Unauthorized_When_RefreshToken_Is_Invalid()
    {
        var controller = GetFreshController();
        var refreshToken = "invalid-rt";
        AuthControllerStartup.SetupRequestCookies(controller, new() { ["refreshToken"] = refreshToken });
        
        mockAuthService.RefreshToken(refreshToken, 7)
            .ThrowsAsync(new UnauthorizedAccessException("Token expired"));

        var result = await controller.Refresh();

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var message = GetErrorMessage(unauthorizedResult);
        Assert.Equal("Token expired", message);
    }

    [Fact]
    public async Task Refresh_Sets_New_Jwt_Cookie()
    {
        var controller = GetFreshController();
        var refreshToken = "valid-rt";
        AuthControllerStartup.SetupRequestCookies(controller, new() { ["refreshToken"] = refreshToken });
        
        mockAuthService.RefreshToken(refreshToken, 7).Returns(("new-jwt-token", "new-refresh-token"));

        await controller.Refresh();

        var setCookieHeader = controller.Response.Headers.SetCookie;
        var jwtCookie = setCookieHeader.FirstOrDefault(c => c!.StartsWith("jwt="));
        Assert.NotNull(jwtCookie);
        Assert.Contains("new-jwt-token", jwtCookie);
    }

    [Fact]
    public async Task Refresh_Sets_New_RefreshToken_Cookie()
    {
        var controller = GetFreshController();
        var refreshToken = "valid-rt";
        AuthControllerStartup.SetupRequestCookies(controller, new() { ["refreshToken"] = refreshToken });
        
        mockAuthService.RefreshToken(refreshToken, 7).Returns(("new-jwt-token", "new-refresh-token"));

        await controller.Refresh();

        var setCookieHeader = controller.Response.Headers.SetCookie;
        var refreshCookie = setCookieHeader.FirstOrDefault(c => c!.StartsWith("refreshToken="));
        Assert.NotNull(refreshCookie);
        Assert.Contains("new-refresh-token", refreshCookie);
    }

    [Fact]
    public async Task Refresh_Returns_Unauthorized_When_RefreshToken_Is_Empty()
    {
        var controller = GetFreshController();
        AuthControllerStartup.SetupRequestCookies(controller, new() { ["refreshToken"] = "" });
    
        var result = await controller.Refresh();

        var unauthorizedResult = Assert.IsType<UnauthorizedObjectResult>(result);
        var message = GetErrorMessage(unauthorizedResult);
        Assert.Equal("Missing refresh token", message);
    }

    [Fact]
    public async Task Logout_Clears_Jwt_Cookie()
    {
        var controller = GetFreshController();
        AuthControllerStartup.SetupRequestCookies(controller, new() { ["refreshToken"] = "active-token" });
        mockAuthService.Logout("active-token").Returns(Task.CompletedTask);

        await controller.Logout();

        var setCookieHeader = controller.Response.Headers.SetCookie;
        var jwtCookie = setCookieHeader.FirstOrDefault(c => c!.StartsWith("jwt="));
        Assert.NotNull(jwtCookie);
    }

    [Fact]
    public async Task Logout_Clears_RefreshToken_Cookie()
    {
        var controller = GetFreshController();
        AuthControllerStartup.SetupRequestCookies(controller, new() { ["refreshToken"] = "active-token" });
        mockAuthService.Logout("active-token").Returns(Task.CompletedTask);

        await controller.Logout();

        var setCookieHeader = controller.Response.Headers.SetCookie;
        var refreshCookie = setCookieHeader.FirstOrDefault(c => c!.StartsWith("refreshToken="));
        Assert.NotNull(refreshCookie);
    }

    [Fact]
    public async Task Logout_Returns_Ok()
    {
        var controller = GetFreshController();
        AuthControllerStartup.SetupRequestCookies(controller, new() { ["refreshToken"] = "active-token" });
        mockAuthService.Logout("active-token").Returns(Task.CompletedTask);

        var result = await controller.Logout();

        Assert.IsType<OkResult>(result);
    }

    [Fact]
    public async Task Logout_Calls_Service_With_RefreshToken()
    {
        var controller = GetFreshController();
        var refreshToken = "active-token";
        AuthControllerStartup.SetupRequestCookies(controller, new() { ["refreshToken"] = refreshToken });
        mockAuthService.ClearReceivedCalls();
        mockAuthService.Logout(refreshToken).Returns(Task.CompletedTask);

        await controller.Logout();

        await mockAuthService.Received(1).Logout(refreshToken);
    }

    [Fact]
    public async Task Logout_Handles_Missing_RefreshToken_Gracefully()
    {
        var controller = GetFreshController();
        AuthControllerStartup.SetupRequestCookies(controller, new Dictionary<string, string>());

        var result = await controller.Logout();

        Assert.IsType<OkResult>(result);
        var setCookieHeader = controller.Response.Headers.SetCookie;
        Assert.True(setCookieHeader.Count > 0);
    }

    [Fact]
    public async Task Logout_Handles_Service_Error_Gracefully()
    {
        var controller = GetFreshController();
        var refreshToken = "invalid-token";
        AuthControllerStartup.SetupRequestCookies(controller, new() { ["refreshToken"] = refreshToken });
        mockAuthService.Logout(refreshToken).Throws(new KeyNotFoundException());

        var result = await controller.Logout();

        Assert.IsType<OkResult>(result);
        var setCookieHeader = controller.Response.Headers.SetCookie;
        Assert.True(setCookieHeader.Count > 0);
    }

    [Fact]
    public void GetMe_Returns_User_Info_With_Valid_Claims()
    {
        var controller = GetFreshController();
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "testuser"),
            new(ClaimTypes.Role, "User")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext.HttpContext.User = principal;

        var result = controller.GetMe();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetMeResDto>(okResult.Value);
        
        Assert.Equal(userId, response.id);
        Assert.Equal("testuser", response.username);
        Assert.False(response.isAdmin);
    }

    [Fact]
    public void GetMe_Returns_Admin_Flag_When_User_In_Admin_Role()
    {
        var controller = GetFreshController();
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "adminuser"),
            new(ClaimTypes.Role, "Admin")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext.HttpContext.User = principal;

        var result = controller.GetMe();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetMeResDto>(okResult.Value);
        
        Assert.True(response.isAdmin);
    }

    [Fact]
    public void GetMe_Returns_BadRequest_When_UserId_Is_Missing()
    {
        var controller = GetFreshController();
        var claims = new List<Claim>
        {
            new(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext.HttpContext.User = principal;

        var result = controller.GetMe();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var message = GetErrorMessage(badRequestResult);
        Assert.Equal("Invalid user claims", message);
    }

    [Fact]
    public void GetMe_Returns_BadRequest_When_UserId_Is_Invalid_Guid()
    {
        var controller = GetFreshController();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, "not-a-guid"),
            new(ClaimTypes.Name, "testuser")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext.HttpContext.User = principal;

        var result = controller.GetMe();

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        var message = GetErrorMessage(badRequestResult);
        Assert.Equal("Invalid user claims", message);
    }

    [Fact]
    public void GetMe_Returns_Unknown_When_Username_Is_Missing()
    {
        var controller = GetFreshController();
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString())
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext.HttpContext.User = principal;

        var result = controller.GetMe();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetMeResDto>(okResult.Value);
        
        Assert.Equal("Unknown", response.username);
    }

    [Fact]
    public void GetMe_Handles_Multiple_Roles()
    {
        var controller = GetFreshController();
        var userId = Guid.NewGuid();
        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId.ToString()),
            new(ClaimTypes.Name, "superuser"),
            new(ClaimTypes.Role, "User"),
            new(ClaimTypes.Role, "Admin"),
            new(ClaimTypes.Role, "Moderator")
        };
        var identity = new ClaimsIdentity(claims, "TestAuth");
        var principal = new ClaimsPrincipal(identity);
        
        controller.ControllerContext.HttpContext.User = principal;

        var result = controller.GetMe();

        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<GetMeResDto>(okResult.Value);
        
        Assert.True(response.isAdmin);
    }

    private static string GetErrorMessage(ObjectResult result)
    {
        var errorObject = result.Value;
        var messageProperty = errorObject?.GetType().GetProperty("message");
        return messageProperty?.GetValue(errorObject)?.ToString() ?? string.Empty;
    }
}