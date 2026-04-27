using Api.Controllers;
using Api.DTOs.Request;
using Api.Services.User;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using NSubstitute.ExceptionExtensions;
using Xunit.DependencyInjection;

namespace Test.ControllerTests.UserTests;

[Startup(typeof(UserControllerStartup))]
public class UserControllerTests(IUserService mockUserService, UserController controller)
{
    [Fact]
    public async Task CreateUser_Returns_Created_On_Success()
    {
        var dto = new UserCreateReqDto 
        { 
            username = "newuser",
            password = "securePassword123",
            email = "test123456789@email.com",
            passwordConfirm = "securePassword123"
        };

        var result = await controller.CreateUser(dto);

        Assert.IsType<CreatedResult>(result);
        await mockUserService.Received(1).CreateUser(dto);
    }

    [Fact]
    public async Task CreateUser_Returns_BadRequest_On_ArgumentException()
    {
        var dto = new UserCreateReqDto 
        { 
            username = "testuser",
            password = "pass123", 
            passwordConfirm = "differentPass", 
            email = "test@email.com"
        };
        var errorMessage = "Passwords do not match.";
        
        mockUserService.CreateUser(dto).Throws(new ArgumentException(errorMessage));

        var result = await controller.CreateUser(dto);

        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.NotNull(badRequestResult.Value);
        
        var resultMessage = badRequestResult.Value?.GetType().GetProperty("message")?.GetValue(badRequestResult.Value);
        Assert.Equal(errorMessage, resultMessage);
    }

    [Fact]
    public async Task CreateUser_Returns_Conflict_On_InvalidOperationException()
    {
        var dto = new UserCreateReqDto 
        { 
            username = "existinguser",
            password = "securePassword123",
            passwordConfirm = "securePassword123",
            email = "existinguser@email.com"
        };
        var errorMessage = "Username already exists.";
        
        mockUserService.CreateUser(dto).Throws(new InvalidOperationException(errorMessage));

        var result = await controller.CreateUser(dto);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.NotNull(conflictResult.Value);
        
        var resultMessage = conflictResult.Value?.GetType().GetProperty("message")?.GetValue(conflictResult.Value);
        Assert.Equal(errorMessage, resultMessage);
    }

    [Fact]
    public async Task CreateUser_Returns_Conflict_On_Email_Exists()
    {
        var dto = new UserCreateReqDto 
        { 
            username = "newuser",
            password = "securePassword123",
            passwordConfirm = "securePassword123",
            email = "existing@email.com"
        };
        var errorMessage = "Email already exists.";
        
        mockUserService.CreateUser(dto).Throws(new InvalidOperationException(errorMessage));

        var result = await controller.CreateUser(dto);

        var conflictResult = Assert.IsType<ConflictObjectResult>(result);
        Assert.NotNull(conflictResult.Value);
    }
}