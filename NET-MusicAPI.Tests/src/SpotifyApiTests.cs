using Microsoft.Extensions.DependencyInjection;
using MusicAPI.Extensions;
using MusicAPI.Abstractions;

namespace MusicAPI.Tests;

[TestFixture]
public sealed class SpotifyApiTests
{
    private ServiceProvider? serviceProvider;
    private IMusicApi? musicApi;

    private const string TestKeyword = "星茶会";
    private const int TestSearchLimit = 5;

    [SetUp]
    public void Setup()
    {
        serviceProvider = new ServiceCollection().AddSpotifyApi().BuildServiceProvider();
        musicApi = serviceProvider.GetRequiredService<IMusicApi>();
    }

    [TearDown]
    public void TearDown()
    {
        serviceProvider?.Dispose();
    }

    [Test]
    public async Task SearchAsyncWithValidKeywordShouldReturnResults()
    {
        TestContext.Out.WriteLine($"[Test] {nameof(SearchAsyncWithValidKeywordShouldReturnResults)}");

        var results = await musicApi!.SearchAsync(TestKeyword, limit: TestSearchLimit);
        Assert.That(results, Is.Not.Null);
        Assert.That(results, Is.Not.Empty);
        Assert.That(results!.Count, Is.LessThanOrEqualTo(TestSearchLimit));

        var firstSong = results.First();
        Assert.That(firstSong.Id, Is.Not.Empty);
        Assert.That(firstSong.Name, Is.Not.Empty);
        Assert.That(firstSong.Artists, Is.Not.Empty);

        TestContext.Out.WriteLine($"Found song: {firstSong.Name}");
        TestContext.Out.WriteLine($"Artists: {string.Join(", ", firstSong.Artists)}");
        TestContext.Out.WriteLine($"Album: {firstSong.AlbumName}");
        TestContext.Out.WriteLine($"Spotify ID: {firstSong.Id}");
    }

    [Test]
    public async Task GetSongAsyncWithValidIdShouldReturnDetails()
    {
        TestContext.Out.WriteLine($"[Test] {nameof(GetSongAsyncWithValidIdShouldReturnDetails)}");

        var searchResults = await musicApi!.SearchAsync(TestKeyword, limit: TestSearchLimit);
        Assert.That(searchResults, Is.Not.Null.And.Not.Empty);

        var songId = searchResults!.First().Id;
        var song = await musicApi.GetSongAsync(songId);

        Assert.That(song, Is.Not.Null);
        Assert.That(song!.Id, Is.EqualTo(songId));
        Assert.That(song.Name, Is.Not.Empty);
        Assert.That(song.Artists, Is.Not.Empty);

        TestContext.Out.WriteLine($"Song details: {song.Name}");
        TestContext.Out.WriteLine($"Artists: {string.Join(", ", song.Artists)}");
        TestContext.Out.WriteLine($"Album: {song.AlbumName}");
        TestContext.Out.WriteLine($"Cover: {song.Picture}");
    }

    [Test]
    public async Task GetSongResourceAsyncShouldReturnYouTubeSearchUrl()
    {
        TestContext.Out.WriteLine($"[Test] {nameof(GetSongResourceAsyncShouldReturnYouTubeSearchUrl)}");

        var searchResults = await musicApi!.SearchAsync(TestKeyword, limit: TestSearchLimit);
        Assert.That(searchResults, Is.Not.Null.And.Not.Empty);

        var songId = searchResults!.First().Id;
        var song = searchResults!.First();
        var resource = await musicApi.GetSongResourceAsync(songId);

        Assert.That(song, Is.Not.Null);
        Assert.That(resource, Is.Not.Null);
        Assert.That(resource!.Url, Is.Not.Empty);
        Assert.That(resource.Url, Does.Contain("youtube.com/results?search_query="));
        Assert.That(resource.Type, Is.EqualTo("youtube-search"));

        TestContext.Out.WriteLine($"Song: {song.Name} - {string.Join(", ", song.Artists)}");
        TestContext.Out.WriteLine($"YouTube search URL: {resource.Url}");
        TestContext.Out.WriteLine($"Type: {resource.Type}");
        TestContext.Out.WriteLine($"Duration: {resource.Duration}s");
    }

    [Test]
    public async Task GetPictureAsyncWithValidIdShouldReturnUrl()
    {
        TestContext.Out.WriteLine($"[Test] {nameof(GetPictureAsyncWithValidIdShouldReturnUrl)}");

        var searchResults = await musicApi!.SearchAsync(TestKeyword, limit: TestSearchLimit);
        Assert.That(searchResults, Is.Not.Null.And.Not.Empty);

        var songId = searchResults!.First().Id;
        var pictureUrl = await musicApi.GetPictureAsync(songId);

        Assert.That(pictureUrl, Is.Not.Null.And.Not.Empty);
        TestContext.Out.WriteLine($"Cover URL: {pictureUrl}");
    }

    [Test]
    public async Task GetLyricAsyncShouldReturnNull()
    {
        TestContext.Out.WriteLine($"[Test] {nameof(GetLyricAsyncShouldReturnNull)}");

        var searchResults = await musicApi!.SearchAsync(TestKeyword, limit: TestSearchLimit);
        Assert.That(searchResults, Is.Not.Null.And.Not.Empty);

        var songId = searchResults!.First().Id;
        var lyric = await musicApi.GetLyricAsync(songId);

        Assert.That(lyric, Is.Null);
        TestContext.Out.WriteLine("Lyrics feature not implemented (expected)");
    }

    [Test]
    public void SearchAsyncWithNullKeywordShouldThrowArgumentNullException()
    {
        TestContext.Out.WriteLine($"[Test] {nameof(SearchAsyncWithNullKeywordShouldThrowArgumentNullException)}");

        Assert.ThrowsAsync<ArgumentNullException>(async () => await musicApi!.SearchAsync(null!));
    }

    [Test]
    public void GetSongAsyncWithNullIdShouldThrowArgumentNullException()
    {
        TestContext.Out.WriteLine($"[Test] {nameof(GetSongAsyncWithNullIdShouldThrowArgumentNullException)}");

        Assert.ThrowsAsync<ArgumentNullException>(async () => await musicApi!.GetSongAsync(null!));
    }
}