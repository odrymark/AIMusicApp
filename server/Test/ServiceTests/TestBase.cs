using DataAccess;
using Microsoft.EntityFrameworkCore.Storage;

namespace Test.ServiceTests;

public abstract class TestBase : IAsyncLifetime
{
    protected readonly MusicDbContext Db;
    private IDbContextTransaction _transaction = null!;

    protected TestBase(MusicDbContext db)
    {
        Db = db;
        Db.Database.EnsureCreated();
    }

    public async ValueTask InitializeAsync()
    {
        _transaction = await Db.Database.BeginTransactionAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _transaction.RollbackAsync();
            await _transaction.DisposeAsync();
        }
        finally
        {
            GC.SuppressFinalize(this);
        }
    }

    protected async Task<User> CreateUserAsync(
        string username = "testuser",
        string password = "hashed_password",
        string? email = null,
        bool isAdmin = false,
        string? refreshToken = null,
        DateTime? refreshTokenExpiry = null)
    {
        var user = new User
        {
            id = Guid.NewGuid(),
            username = username,
            password = password,
            email = email ?? $"t_{Guid.NewGuid().ToString("N").AsSpan(0, 12)}@test.com",
            isAdmin = isAdmin,
            refreshToken = refreshToken,
            refreshTokenExpiry = refreshTokenExpiry
        };

        await Db.Users.AddAsync(user);
        await Db.SaveChangesAsync();
        return user;
    }

    protected async Task<Song> CreateSongAsync(
        Guid userId,
        string title = "Test Song",
        string songKey = "test-key",
        string artist = "Test Artist",
        bool isPublic = true,
        string? image = null)
    {
        var song = new Song
        {
            id = Guid.NewGuid(),
            userId = userId,
            title = title,
            songKey = songKey,
            artist = artist,
            isPublic = isPublic,
            image = image
        };

        await Db.Songs.AddAsync(song);
        await Db.SaveChangesAsync();
        return song;
    }

    protected async Task<Playlist> CreatePlaylistAsync(
        Guid userId, 
        string title, 
        List<Guid> songIds,
        bool isPublic = true,
        string? image = null)
    {
        var playlist = new Playlist
        {
            userId = userId,
            title = title,
            isPublic = isPublic,
            image = image,
            songs = Db.Songs.Where(s => songIds.Contains(s.id)).ToList()
        };
        Db.Playlists.Add(playlist);
        await Db.SaveChangesAsync();
        return playlist;
    }
}