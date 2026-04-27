using Api.Services.Playlist;
using DataAccess;
using Microsoft.EntityFrameworkCore;
using Xunit.DependencyInjection;

namespace Test.ServiceTests.PlaylistTests;

[Startup(typeof(PlaylistStartup))]
public class PlaylistTests(MusicDbContext db, IPlaylistService playlistService) : TestBase(db)
{
    private static string GetUniqueUsername()
    {
        return $"u_{Guid.NewGuid().ToString("N").AsSpan(0, 30)}";
    }

    // -------------------------
    // CreatePlaylist Tests
    // -------------------------

    [Fact]
    public async Task CreatePlaylist_Saves_Playlist_To_Db()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");

        await playlistService.CreatePlaylist(user.id, "My Playlist", new List<Guid> { song.id }, true);

        var playlist = Db.Playlists.FirstOrDefault(p => p.userId == user.id);
        Assert.NotNull(playlist);
    }

    [Fact]
    public async Task CreatePlaylist_Saves_Correct_Fields()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");

        await playlistService.CreatePlaylist(user.id, "My Playlist", new List<Guid> { song.id }, true, "playlist.jpg");

        var playlist = Db.Playlists.First(p => p.userId == user.id);
        Assert.Equal("My Playlist", playlist.title);
        Assert.True(playlist.isPublic);
        Assert.Equal("playlist.jpg", playlist.image);
    }

    [Fact]
    public async Task CreatePlaylist_Image_Is_Null_When_Not_Provided()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");

        await playlistService.CreatePlaylist(user.id, "My Playlist", new List<Guid> { song.id }, false);

        var playlist = Db.Playlists.First(p => p.userId == user.id);
        Assert.Null(playlist.image);
    }

    [Fact]
    public async Task CreatePlaylist_Associates_Songs()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song1 = await CreateSongAsync(user.id, "Song 1");
        var song2 = await CreateSongAsync(user.id, "Song 2");

        await playlistService.CreatePlaylist(user.id, "My Playlist", new List<Guid> { song1.id, song2.id }, true);

        var playlist = Db.Playlists.Include(p => p.songs).First(p => p.userId == user.id);
        Assert.Equal(2, playlist.songs.Count);
        Assert.Contains(playlist.songs, s => s.id == song1.id);
        Assert.Contains(playlist.songs, s => s.id == song2.id);
    }

    [Fact]
    public async Task CreatePlaylist_Throws_When_UserId_Empty()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");

        await Assert.ThrowsAsync<ArgumentException>(() =>
            playlistService.CreatePlaylist(Guid.Empty, "Playlist", new List<Guid> { song.id }, true));
    }

    [Fact]
    public async Task CreatePlaylist_Throws_When_User_Not_Found()
    {
        var nonexistentUserId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            playlistService.CreatePlaylist(nonexistentUserId, "Playlist", new List<Guid>(), true));
    }

    [Fact]
    public async Task CreatePlaylist_Throws_When_Song_Not_Found()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Real Song");
        var fakeId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            playlistService.CreatePlaylist(user.id, "Playlist", new List<Guid> { song.id, fakeId }, true));
    }

    [Fact]
    public async Task CreatePlaylist_Throws_When_Multiple_Songs_Not_Found()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var fakeId1 = Guid.NewGuid();
        var fakeId2 = Guid.NewGuid();

        var exception = await Assert.ThrowsAsync<ArgumentException>(() =>
            playlistService.CreatePlaylist(user.id, "Playlist", new List<Guid> { fakeId1, fakeId2 }, true));

        Assert.Contains("Songs not found", exception.Message);
    }

    [Fact]
    public async Task CreatePlaylist_Allows_Empty_SongList()
    {
        var user = await CreateUserAsync(GetUniqueUsername());

        await playlistService.CreatePlaylist(user.id, "Empty Playlist", new List<Guid>(), true);

        var playlist = Db.Playlists.Include(p => p.songs).First(p => p.userId == user.id);
        Assert.Empty(playlist.songs);
    }

    // -------------------------
    // EditPlaylist Tests
    // -------------------------

    [Fact]
    public async Task EditPlaylist_Updates_Title()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");
        var playlist = await CreatePlaylistAsync(user.id, "Old Title", new List<Guid> { song.id });

        await playlistService.EditPlaylist(user.id, playlist.id, "New Title", new List<Guid> { song.id }, true);

        var updated = Db.Playlists.First(p => p.id == playlist.id);
        Assert.Equal("New Title", updated.title);
    }

    [Fact]
    public async Task EditPlaylist_Updates_IsPublic()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");
        var playlist = await CreatePlaylistAsync(user.id, "Playlist", new List<Guid> { song.id }, isPublic: false);

        await playlistService.EditPlaylist(user.id, playlist.id, "Playlist", new List<Guid> { song.id }, true);

        var updated = Db.Playlists.First(p => p.id == playlist.id);
        Assert.True(updated.isPublic);
    }

    [Fact]
    public async Task EditPlaylist_Updates_Image()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");
        var playlist = await CreatePlaylistAsync(user.id, "Playlist", new List<Guid> { song.id }, image: "old.jpg");

        await playlistService.EditPlaylist(user.id, playlist.id, "Playlist", new List<Guid> { song.id }, true, "new.jpg");

        var updated = Db.Playlists.First(p => p.id == playlist.id);
        Assert.Equal("new.jpg", updated.image);
    }

    [Fact]
    public async Task EditPlaylist_Updates_Songs()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song1 = await CreateSongAsync(user.id, "Song 1");
        var song2 = await CreateSongAsync(user.id, "Song 2");
        var song3 = await CreateSongAsync(user.id, "Song 3");
        var playlist = await CreatePlaylistAsync(user.id, "Playlist", new List<Guid> { song1.id, song2.id });

        await playlistService.EditPlaylist(user.id, playlist.id, "Playlist", new List<Guid> { song2.id, song3.id }, true);

        var updated = Db.Playlists.Include(p => p.songs).First(p => p.id == playlist.id);
        Assert.Equal(2, updated.songs.Count);
        Assert.DoesNotContain(updated.songs, s => s.id == song1.id);
        Assert.Contains(updated.songs, s => s.id == song2.id);
        Assert.Contains(updated.songs, s => s.id == song3.id);
    }

    [Fact]
    public async Task EditPlaylist_Throws_When_Playlist_Not_Found()
    {
        var user = await CreateUserAsync(GetUniqueUsername());

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            playlistService.EditPlaylist(user.id, Guid.NewGuid(), "Playlist", new List<Guid>(), true));
    }

    [Fact]
    public async Task EditPlaylist_Throws_When_Not_Owner()
    {
        var user1 = await CreateUserAsync(GetUniqueUsername());
        var user2 = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user1.id, "Song 1");
        var playlist = await CreatePlaylistAsync(user1.id, "Playlist", new List<Guid> { song.id });

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            playlistService.EditPlaylist(user2.id, playlist.id, "Hacked", new List<Guid> { song.id }, true));
    }

    [Fact]
    public async Task EditPlaylist_Throws_When_Song_Not_Found()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");
        var playlist = await CreatePlaylistAsync(user.id, "Playlist", new List<Guid> { song.id });
        var fakeId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            playlistService.EditPlaylist(user.id, playlist.id, "Playlist", new List<Guid> { song.id, fakeId }, true));
    }

    [Fact]
    public async Task EditPlaylist_Clears_Image_When_Not_Provided()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");
        var playlist = await CreatePlaylistAsync(user.id, "Playlist", new List<Guid> { song.id }, image: "old.jpg");

        await playlistService.EditPlaylist(user.id, playlist.id, "Playlist", new List<Guid> { song.id }, true, null);

        var updated = Db.Playlists.First(p => p.id == playlist.id);
        Assert.Equal("old.jpg", updated.image);
    }

    // -------------------------
    // GetUserPlaylists Tests
    // -------------------------

    [Fact]
    public async Task GetUserPlaylists_Returns_Playlists_For_User()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");
        await CreatePlaylistAsync(user.id, "Playlist 1", new List<Guid> { song.id });
        await CreatePlaylistAsync(user.id, "Playlist 2", new List<Guid> { song.id });

        var result = await playlistService.GetUserPlaylists(user.id);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUserPlaylists_Returns_Only_Users_Own_Playlists()
    {
        var user1 = await CreateUserAsync(GetUniqueUsername());
        var user2 = await CreateUserAsync(GetUniqueUsername());
        var song1 = await CreateSongAsync(user1.id, "Song 1");
        var song2 = await CreateSongAsync(user2.id, "Song 2");
        await CreatePlaylistAsync(user1.id, "User1 Playlist", new List<Guid> { song1.id });
        await CreatePlaylistAsync(user2.id, "User2 Playlist", new List<Guid> { song2.id });

        var result = await playlistService.GetUserPlaylists(user1.id);

        Assert.All(result, p => Assert.Equal("User1 Playlist", p.title));
    }

    [Fact]
    public async Task GetUserPlaylists_Returns_Empty_When_No_Playlists()
    {
        var user = await CreateUserAsync(GetUniqueUsername());

        var result = await playlistService.GetUserPlaylists(user.id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserPlaylists_Returns_Correct_Dto_Fields()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "DTO Song", "dto-key", "DTO Artist", image: "song.jpg");
        var playlist = await CreatePlaylistAsync(user.id, "DTO Playlist", new List<Guid> { song.id }, image: "playlist.jpg");

        var result = await playlistService.GetUserPlaylists(user.id);
        var dto = result.First();

        Assert.Equal(playlist.id, dto.id);
        Assert.Equal(playlist.title, dto.title);
        Assert.Equal(user.username, dto.creatorUser);
        Assert.Equal(playlist.image, dto.image);
        Assert.Equal(playlist.isPublic, dto.isPublic);
        Assert.Single(dto.songs);
        Assert.Equal(song.id, dto.songs.First().id);
        Assert.Equal(song.title, dto.songs.First().title);
    }

    [Fact]
    public async Task GetUserPlaylists_Throws_When_UserId_Empty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            playlistService.GetUserPlaylists(Guid.Empty));
    }

    [Fact]
    public async Task GetUserPlaylists_Throws_When_User_Not_Found()
    {
        var nonexistentUserId = Guid.NewGuid();

        await Assert.ThrowsAsync<ArgumentException>(() =>
            playlistService.GetUserPlaylists(nonexistentUserId));
    }

    [Fact]
    public async Task GetUserPlaylists_Returns_Songs_In_Playlist()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song1 = await CreateSongAsync(user.id, "Song 1");
        var song2 = await CreateSongAsync(user.id, "Song 2");
        await CreatePlaylistAsync(user.id, "Multi Song Playlist", new List<Guid> { song1.id, song2.id });

        var result = await playlistService.GetUserPlaylists(user.id);
        var dto = result.First();

        Assert.Equal(2, dto.songs.Count);
    }

    // -------------------------
    // GetPlaylists Tests
    // -------------------------

    [Fact]
    public async Task GetPlaylists_Returns_Only_Public_Playlists()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");
        await CreatePlaylistAsync(user.id, "Public Playlist", new List<Guid> { song.id }, isPublic: true);
        await CreatePlaylistAsync(user.id, "Private Playlist", new List<Guid> { song.id }, isPublic: false);

        var result = await playlistService.GetPlaylists();

        Assert.DoesNotContain(result, p => p.title == "Private Playlist");
        Assert.Contains(result, p => p.title == "Public Playlist");
    }

    [Fact]
    public async Task GetPlaylists_Returns_Empty_When_No_Public_Playlists()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Song 1");
        await CreatePlaylistAsync(user.id, "Private Playlist", new List<Guid> { song.id }, isPublic: false);

        var result = await playlistService.GetPlaylists();

        Assert.DoesNotContain(result, p => p.title == "Private Playlist");
    }

    [Fact]
    public async Task GetPlaylists_Returns_Correct_Dto_Fields()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song = await CreateSongAsync(user.id, "Public DTO Song", "pub-key", "Pub Artist", image: "song.jpg");
        var playlist = await CreatePlaylistAsync(user.id, "Public DTO Playlist", new List<Guid> { song.id }, isPublic: true, image: "pub.jpg");

        var result = await playlistService.GetPlaylists();
        var dto = result.First(p => p.id == playlist.id);

        Assert.Equal(playlist.title, dto.title);
        Assert.Equal(user.username, dto.creatorUser);
        Assert.Equal(playlist.image, dto.image);
        Assert.True(dto.isPublic);
        Assert.Single(dto.songs);
    }

    [Fact]
    public async Task GetPlaylists_Includes_Songs_In_Response()
    {
        var user = await CreateUserAsync(GetUniqueUsername());
        var song1 = await CreateSongAsync(user.id, "Song 1", isPublic: true);
        var song2 = await CreateSongAsync(user.id, "Song 2", isPublic: true);
        await CreatePlaylistAsync(user.id, "Public Multi Playlist", new List<Guid> { song1.id, song2.id }, isPublic: true);

        var result = await playlistService.GetPlaylists();
        var dto = result.First(p => p.title == "Public Multi Playlist");

        Assert.Equal(2, dto.songs.Count);
    }
}