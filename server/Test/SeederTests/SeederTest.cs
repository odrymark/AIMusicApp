using Api;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Test.ServiceTests;
using Xunit.DependencyInjection;

namespace Test.SeederTests;

[Startup(typeof(SeederStartup))]
public class SeederTests(MusicDbContext db) : TestBase(db)
{
    [Fact]
    public async Task Seed_Creates_Admin_User()
    {
        var seeder = new Seeder(Db);
        await seeder.Seed();
 
        var admin = await Db.Users.FirstOrDefaultAsync(u => u.username == "admin", cancellationToken: TestContext.Current.CancellationToken);
        Assert.NotNull(admin);
        Assert.Equal("admin", admin.username);
        Assert.True(admin.isAdmin);
    }
 
    [Fact]
    public async Task Seed_Is_Idempotent()
    {
        var seeder = new Seeder(Db);
        await seeder.Seed();
        var firstCount = await Db.Users.CountAsync(cancellationToken: TestContext.Current.CancellationToken);
 
        await seeder.Seed();
        var secondCount = await Db.Users.CountAsync(cancellationToken: TestContext.Current.CancellationToken);
 
        Assert.Equal(firstCount, secondCount);
    }
 
#if DEBUG
    [Fact]
    public async Task Seed_Creates_Test_Users_And_Songs()
    {
        var seeder = new Seeder(Db);
        await seeder.Seed();
 
        var testUsers = await Db.Users.Where(u => u.username.StartsWith("testuser")).CountAsync(cancellationToken: TestContext.Current.CancellationToken);
        var songs = await Db.Songs.CountAsync(cancellationToken: TestContext.Current.CancellationToken);
 
        Assert.Equal(20, testUsers);
        Assert.Equal(60, songs);
    }
#endif
}