using Api.Services;
using Api.Services.Password;
using DataAccess;

namespace Api;

public class Seeder(MusicDbContext ctx) : ISeeder
{
    public async Task Seed()
    {
        SeedAdminUser();
        
#if DEBUG
        SeedTestUsers();
#endif

        await ctx.SaveChangesAsync();
    }

    private void SeedAdminUser()
    {
        var passwd = new PasswordService().HashPassword("passwd123");
        
        if (!ctx.Users.Any(u => u.username == "admin"))
        {
            var admin = new User
            {
                username = "admin",
                password = passwd,
                email = "admin@gmail.com",
                isAdmin = true
            };

            ctx.Users.Add(admin);
        }
    }

#if DEBUG
    private void SeedTestUsers()
    {
        var passwd = new PasswordService().HashPassword("testpassword");

        for (int i = 1; i <= 20; i++)
        {
            var username = $"testuser{i}";

            var user = ctx.Users.FirstOrDefault(u => u.username == username);

            if (user == null)
            {
                user = new User
                {
                    id = Guid.NewGuid(),
                    username = username,
                    password = passwd,
                    email = $"testuser{i}@test.com",
                    isAdmin = false
                };

                ctx.Users.Add(user);
            }

            if (!ctx.Songs.Any(s => s.userId == user.id))
            {
                SeedTestSongsForUser(user);
            }
        }
    }

    private void SeedTestSongsForUser(User user)
    {
        var songTitles = new[]
        {
            "Test Song Alpha",
            "Test Song Beta",
            "Test Song Gamma",
        };

        var artists = new[]
        {
            "Artist One",
            "Artist Two",
            "Artist Three",
        };

        for (int i = 0; i < songTitles.Length; i++)
        {
            ctx.Songs.Add(new Song
            {
                id = Guid.NewGuid(),
                userId = user.id,
                title = songTitles[i],
                artist = artists[i],
                songKey = $"songs/test-{user.username}-{i + 1}.mp3",
                isPublic = i == 0,
                image = null
            });
        }
    }
#endif
}

public interface ISeeder
{
    Task Seed();
}