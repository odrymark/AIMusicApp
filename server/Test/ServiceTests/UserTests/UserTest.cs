using Api.DTOs.Request;
using Api.Services.User;
using DataAccess;
using Xunit.DependencyInjection;

namespace Test.ServiceTests.UserTests;

[Startup(typeof(UserStartup))]
public class UserServiceTests(MusicDbContext db, IUserService userService) : TestBase(db)
{
    private static UserCreateReqDto BuildDto(
        string? username = null,
        string? email = null,
        string password = "password123",
        string? passwordConfirm = null)
    {
        username ??= "user_" + Guid.NewGuid().ToString("N");
        email ??= username + "@example.com";
        return new UserCreateReqDto
        {
            username = username,
            email = email,
            password = password,
            passwordConfirm = passwordConfirm ?? password
        };
    }

    // -------------------------
    // CreateUser Tests
    // -------------------------

    [Fact]
    public async Task CreateUser_Saves_User_To_Db()
    {
        var dto = BuildDto();

        await userService.CreateUser(dto);

        var user = Db.Users.FirstOrDefault(u => u.username == dto.username);
        Assert.NotNull(user);
    }

    [Fact]
    public async Task CreateUser_Stores_Hashed_Password()
    {
        var dto = BuildDto();

        await userService.CreateUser(dto);

        var user = Db.Users.First(u => u.username == dto.username);
        Assert.Equal("HASH_" + dto.password, user.password);
    }

    [Fact]
    public async Task CreateUser_Sets_IsAdmin_False()
    {
        var dto = BuildDto();

        await userService.CreateUser(dto);

        var user = Db.Users.First(u => u.username == dto.username);
        Assert.False(user.isAdmin);
    }

    [Fact]
    public async Task CreateUser_Throws_ArgumentException_When_Passwords_Do_Not_Match()
    {
        var dto = BuildDto(password: "password123", passwordConfirm: "different");

        await Assert.ThrowsAsync<ArgumentException>(() => userService.CreateUser(dto));
    }

    [Fact]
    public async Task CreateUser_Throws_InvalidOperationException_When_Username_Already_Exists()
    {
        var username = "duplicate_" + Guid.NewGuid().ToString("N");
        await CreateUserAsync(username);

        var dto = BuildDto(username: username);

        await Assert.ThrowsAsync<InvalidOperationException>(() => userService.CreateUser(dto));
    }

    [Fact]
    public async Task CreateUser_Throws_InvalidOperationException_When_Email_Already_Exists()
    {
        var username = "emailtest_" + Guid.NewGuid().ToString("N");
        var email = username + "@example.com";
        await CreateUserAsync(username, email: email);

        var dto = BuildDto(email: email);

        await Assert.ThrowsAsync<InvalidOperationException>(() => userService.CreateUser(dto));
    }

    [Fact]
    public async Task CreateUser_Throws_Correct_Message_When_Passwords_Do_Not_Match()
    {
        var dto = BuildDto(password: "password123", passwordConfirm: "different");

        var ex = await Assert.ThrowsAsync<ArgumentException>(() => userService.CreateUser(dto));
        Assert.Equal("Passwords do not match.", ex.Message);
    }

    [Fact]
    public async Task CreateUser_Throws_Correct_Message_When_Username_Taken()
    {
        var username = "taken_" + Guid.NewGuid().ToString("N");
        await CreateUserAsync(username);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.CreateUser(BuildDto(username: username)));
        Assert.Equal("Username already exists.", ex.Message);
    }

    [Fact]
    public async Task CreateUser_Throws_Correct_Message_When_Email_Taken()
    {
        var username = "emailtaken_" + Guid.NewGuid().ToString("N");
        var email = username + "@example.com";
        await CreateUserAsync(username, email: email);

        var ex = await Assert.ThrowsAsync<InvalidOperationException>(() => userService.CreateUser(BuildDto(email: email)));
        Assert.Equal("Email already exists.", ex.Message);
    }
}