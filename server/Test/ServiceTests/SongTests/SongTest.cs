using Api.Services.Song;
using DataAccess;
using Xunit.DependencyInjection;

namespace Test.ServiceTests.SongTests;

[Startup(typeof(SongStartup))]
public class SongServiceTests(MusicDbContext db, ISongService songService) : TestBase(db)
{
    // -------------------------
    // CreateSong Tests
    // -------------------------

    [Fact]
    public async Task CreateSong_Saves_Song_To_Db()
    {
        var user = await CreateUserAsync("song_create_" + Guid.NewGuid().ToString("N"));

        await songService.CreateSong(user.id, "My Song", "song-key", "Artist", true, "mood");

        var song = Db.Songs.FirstOrDefault(s => s.userId == user.id);
        Assert.NotNull(song);
    }

    [Fact]
    public async Task CreateSong_Saves_Correct_Fields()
    {
        var user = await CreateUserAsync("song_fields_" + Guid.NewGuid().ToString("N"));

        await songService.CreateSong(user.id, "My Song", "song-key", "Artist", true, "some-mood", "image.jpg");

        var song = Db.Songs.First(s => s.userId == user.id);
        Assert.Equal("My Song", song.title);
        Assert.Equal("song-key", song.songKey);
        Assert.Equal("Artist", song.artist);
        Assert.True(song.isPublic);
        Assert.Equal("image.jpg", song.image);
    }

    [Fact]
    public async Task CreateSong_Image_Is_Null_When_Not_Provided()
    {
        var user = await CreateUserAsync("song_noimage_" + Guid.NewGuid().ToString("N"));

        await songService.CreateSong(user.id, "My Song", "song-key", "Artist", false, "mood");

        var song = Db.Songs.First(s => s.userId == user.id);
        Assert.Null(song.image);
    }

    [Fact]
    public async Task CreateSong_Throws_When_Title_Empty()
    {
        var user = await CreateUserAsync("song_notitle_" + Guid.NewGuid().ToString("N"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            songService.CreateSong(user.id, "", "song-key", "Artist", true, "mood"));
    }

    [Fact]
    public async Task CreateSong_Throws_When_Title_Whitespace()
    {
        var user = await CreateUserAsync("song_wstitle_" + Guid.NewGuid().ToString("N"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            songService.CreateSong(user.id, "   ", "song-key", "Artist", true, "mood"));
    }

    [Fact]
    public async Task CreateSong_Throws_When_SongKey_Empty()
    {
        var user = await CreateUserAsync("song_nokey_" + Guid.NewGuid().ToString("N"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            songService.CreateSong(user.id, "My Song", "", "Artist", true, "mood"));
    }

    [Fact]
    public async Task CreateSong_Throws_When_SongKey_Whitespace()
    {
        var user = await CreateUserAsync("song_wskey_" + Guid.NewGuid().ToString("N"));

        await Assert.ThrowsAsync<ArgumentException>(() =>
            songService.CreateSong(user.id, "My Song", "   ", "Artist", true, "mood"));
    }

    // -------------------------
    // GetUserSongs Tests
    // -------------------------

    [Fact]
    public async Task GetUserSongsAsync_Returns_Songs_For_User()
    {
        var user = await CreateUserAsync("song_getuser_" + Guid.NewGuid().ToString("N"));
        await CreateSongAsync(user.id, "Song 1");
        await CreateSongAsync(user.id, "Song 2");

        var result = await songService.GetUserSongs(user.id);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetUserSongsAsync_Returns_Only_Users_Own_Songs()
    {
        var user1 = await CreateUserAsync("song_own1_" + Guid.NewGuid().ToString("N"));
        var user2 = await CreateUserAsync("song_own2_" + Guid.NewGuid().ToString("N"));
        await CreateSongAsync(user1.id, "User1 Song");
        await CreateSongAsync(user2.id, "User2 Song");

        var result = await songService.GetUserSongs(user1.id);

        Assert.All(result, s => Assert.Equal("User1 Song", s.title));
    }

    [Fact]
    public async Task GetUserSongsAsync_Returns_Empty_When_No_Songs()
    {
        var user = await CreateUserAsync("song_empty_" + Guid.NewGuid().ToString("N"));

        var result = await songService.GetUserSongs(user.id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserSongsAsync_Returns_Correct_Dto_Fields()
    {
        var user = await CreateUserAsync("song_dto_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user.id, "DTO Song", "dto-key", "DTO Artist", image: "img.jpg");

        var result = await songService.GetUserSongs(user.id);
        var dto = result.First();

        Assert.Equal(song.id, dto.id);
        Assert.Equal(song.title, dto.title);
        Assert.Equal(song.songKey, dto.songKey);
        Assert.Equal(song.artist, dto.artist);
        Assert.Equal(song.image, dto.image);
    }

    [Fact]
    public async Task GetUserSongsAsync_Throws_When_UserId_Empty()
    {
        await Assert.ThrowsAsync<ArgumentException>(() =>
            songService.GetUserSongs(Guid.Empty));
    }

    // -------------------------
    // GetSongs Tests
    // -------------------------

    [Fact]
    public async Task GetSongs_Returns_Only_Public_Songs()
    {
        var user = await CreateUserAsync("song_public_" + Guid.NewGuid().ToString("N"));
        await CreateSongAsync(user.id, "Public Song", isPublic: true);
        await CreateSongAsync(user.id, "Private Song", isPublic: false);

        var result = await songService.GetSongs();

        Assert.DoesNotContain(result, s => s.title == "Private Song");
        Assert.Contains(result, s => s.title == "Public Song");
    }

    [Fact]
    public async Task GetSongs_Returns_Empty_When_No_Public_Songs()
    {
        var user = await CreateUserAsync("song_nopublic_" + Guid.NewGuid().ToString("N"));
        await CreateSongAsync(user.id, "Private Song", isPublic: false);

        var result = await songService.GetSongs();

        Assert.DoesNotContain(result, s => s.title == "Private Song");
    }

    [Fact]
    public async Task GetSongs_Returns_Correct_Dto_Fields()
    {
        var user = await CreateUserAsync("song_pubdto_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user.id, "Public DTO Song", "pub-key", "Pub Artist", isPublic: true, image: "pub.jpg");

        var result = await songService.GetSongs();
        var dto = result.First(s => s.id == song.id);

        Assert.Equal(song.title, dto.title);
        Assert.Equal(song.songKey, dto.songKey);
        Assert.Equal(song.artist, dto.artist);
        Assert.Equal(song.image, dto.image);
    }

    // -------------------------
    // GetSongsById Tests
    // -------------------------

    [Fact]
    public async Task GetSongsById_Returns_Correct_Songs()
    {
        var user = await CreateUserAsync("song_byid_" + Guid.NewGuid().ToString("N"));
        var song1 = await CreateSongAsync(user.id, "Song 1");
        var song2 = await CreateSongAsync(user.id, "Song 2");
        await CreateSongAsync(user.id, "Song 3");

        var result = await songService.GetSongsById(new[] { song1.id, song2.id });

        Assert.Equal(2, result.Count());
        Assert.Contains(result, s => s.id == song1.id);
        Assert.Contains(result, s => s.id == song2.id);
    }

    [Fact]
    public async Task GetSongsById_Returns_Empty_When_No_Ids_Match()
    {
        var result = await songService.GetSongsById(new[] { Guid.NewGuid() });

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetSongsById_Returns_Empty_For_Empty_List()
    {
        var result = await songService.GetSongsById(Array.Empty<Guid>());

        Assert.Empty(result);
    }

    // -------------------------
    // GetRecentSongs Tests
    // -------------------------

    [Fact]
    public async Task GetRecentSongs_Returns_Songs_From_History()
    {
        var user = await CreateUserAsync("song_recent_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user.id, "Recent Song");
        await songService.AddHistory(user.id, song.id);

        var result = await songService.GetRecentSongs(user.id);

        Assert.Contains(result, s => s.title == "Recent Song");
    }

    [Fact]
    public async Task GetRecentSongs_Returns_Empty_When_No_History()
    {
        var user = await CreateUserAsync("song_nohistory_" + Guid.NewGuid().ToString("N"));

        var result = await songService.GetRecentSongs(user.id);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetRecentSongs_Returns_Only_Users_History()
    {
        var user1 = await CreateUserAsync("song_hist1_" + Guid.NewGuid().ToString("N"));
        var user2 = await CreateUserAsync("song_hist2_" + Guid.NewGuid().ToString("N"));
        var song1 = await CreateSongAsync(user1.id, "User1 Song");
        var song2 = await CreateSongAsync(user2.id, "User2 Song");
        await songService.AddHistory(user1.id, song1.id);
        await songService.AddHistory(user2.id, song2.id);

        var result = await songService.GetRecentSongs(user1.id);

        Assert.DoesNotContain(result, s => s.title == "User2 Song");
    }

    // -------------------------
    // GetRandomSongs Tests
    // -------------------------

    [Fact]
    public async Task GetRandomSongs_Returns_Songs()
    {
        var user = await CreateUserAsync("song_random_" + Guid.NewGuid().ToString("N"));
        await CreateSongAsync(user.id, "Random Song 1");
        await CreateSongAsync(user.id, "Random Song 2");

        var result = await songService.GetRandomSongs();

        Assert.NotEmpty(result);
    }

    [Fact]
    public async Task GetRandomSongs_Returns_At_Most_Count()
    {
        var user = await CreateUserAsync("song_randomcount_" + Guid.NewGuid().ToString("N"));
        for (var i = 0; i < 15; i++)
            await CreateSongAsync(user.id, $"Song {i}");

        var result = await songService.GetRandomSongs(10);

        Assert.True(result.Count() <= 10);
    }

    // -------------------------
    // AddHistory Tests
    // -------------------------

    [Fact]
    public async Task AddHistory_Saves_To_Db()
    {
        var user = await CreateUserAsync("song_addhist_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user.id, "History Song");

        await songService.AddHistory(user.id, song.id);

        var history = Db.History.FirstOrDefault(h => h.userId == user.id && h.songId == song.id);
        Assert.NotNull(history);
    }

    [Fact]
    public async Task AddHistory_Sets_PlayedAt()
    {
        var user = await CreateUserAsync("song_histtime_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user.id, "Timed Song");
        var before = DateTime.UtcNow;

        await songService.AddHistory(user.id, song.id);

        var history = Db.History.First(h => h.userId == user.id && h.songId == song.id);
        Assert.True(history.playedAt >= before);
    }

    // -------------------------
    // EditSong Tests
    // -------------------------

    [Fact]
    public async Task EditSong_Updates_Fields()
    {
        var user = await CreateUserAsync("song_edit_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user.id, "Old Title", artist: "Old Artist");

        await songService.EditSong(user.id, song.id, "New Title", "New Artist", false);

        var updated = Db.Songs.First(s => s.id == song.id);
        Assert.Equal("New Title", updated.title);
        Assert.Equal("New Artist", updated.artist);
        Assert.False(updated.isPublic);
    }

    [Fact]
    public async Task EditSong_Updates_Image_When_Provided()
    {
        var user = await CreateUserAsync("song_editimg_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user.id, "Song");

        await songService.EditSong(user.id, song.id, "Song", "Artist", true, "new-image.jpg");

        var updated = Db.Songs.First(s => s.id == song.id);
        Assert.Equal("new-image.jpg", updated.image);
    }

    [Fact]
    public async Task EditSong_Does_Not_Update_Image_When_Null()
    {
        var user = await CreateUserAsync("song_editnoimg_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user.id, "Song", image: "original.jpg");

        await songService.EditSong(user.id, song.id, "Song", "Artist", true);

        var updated = Db.Songs.First(s => s.id == song.id);
        Assert.Equal("original.jpg", updated.image);
    }

    [Fact]
    public async Task EditSong_Throws_When_Song_Not_Found()
    {
        var user = await CreateUserAsync("song_editnotfound_" + Guid.NewGuid().ToString("N"));

        await Assert.ThrowsAsync<KeyNotFoundException>(() =>
            songService.EditSong(user.id, Guid.NewGuid(), "Title", "Artist", true));
    }

    [Fact]
    public async Task EditSong_Throws_When_User_Does_Not_Own_Song()
    {
        var user1 = await CreateUserAsync("song_editown1_" + Guid.NewGuid().ToString("N"));
        var user2 = await CreateUserAsync("song_editown2_" + Guid.NewGuid().ToString("N"));
        var song = await CreateSongAsync(user1.id, "Song");

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() =>
            songService.EditSong(user2.id, song.id, "New Title", "Artist", true));
    }
}