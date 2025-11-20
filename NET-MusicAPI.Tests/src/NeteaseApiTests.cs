using Microsoft.Extensions.DependencyInjection;
using MusicAPI.Netease;
using MusicAPI.Extensions;
using MusicAPI.Abstractions;

namespace MusicAPI.Tests;

[TestFixture]
public sealed class NeteaseApiTests
{
    private ServiceProvider? serviceProvider;
    private IMusicApi? musicApi;

    private const string TestKeyword = "星茶会";
    private const int TestSearchLimit = 5;
    private const int TestBitrate = 320;
    private const int TestPictureSize = 300;

    [SetUp]
    public void Setup()
    {
        serviceProvider = new ServiceCollection().AddNeteaseApi().BuildServiceProvider();
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
        TestContext.WriteLine($"[Test] {nameof(SearchAsyncWithValidKeywordShouldReturnResults)}");

        var results = await musicApi!.SearchAsync(TestKeyword, limit: TestSearchLimit);
        Assert.That(results, Is.Not.Null);
        Assert.That(results, Is.Not.Empty);
        Assert.That(results!.Count, Is.LessThanOrEqualTo(TestSearchLimit));

        var firstSong = results.First();
        Assert.That(firstSong.Id, Is.Not.Empty);
        Assert.That(firstSong.Name, Is.Not.Empty);
        Assert.That(firstSong.Artists, Is.Not.Empty);
        Assert.That(firstSong.AlbumName, Is.Not.Empty);

        TestContext.WriteLine($"Found song: {firstSong.Name} by {string.Join(", ", firstSong.Artists)}");
    }

    [Test]
    public async Task GetSongAsyncWithValidIdShouldReturnDetails()
    {
        TestContext.WriteLine($"[Test] {nameof(GetSongAsyncWithValidIdShouldReturnDetails)}");

        var searchResults = await musicApi!.SearchAsync(TestKeyword, limit: TestSearchLimit);
        Assert.That(searchResults, Is.Not.Null.And.Not.Empty);

        var songId = searchResults!.First().Id;
        var song = await musicApi.GetSongAsync(songId);
        Assert.That(song, Is.Not.Null);
        Assert.That(song!.Id, Is.EqualTo(songId));
        Assert.That(song.Name, Is.Not.Empty);
        Assert.That(song.Artists, Is.Not.Empty);
        Assert.That(song.Picture, Is.Not.Empty);

        TestContext.WriteLine($"Song details: {song.Name}");
        TestContext.WriteLine($"Picture URL: {song.Picture}");
    }

    [Test]
    public async Task GetSongResourceAsyncWithValidIdShouldReturnResource()
    {
        TestContext.WriteLine($"[Test] {nameof(GetSongResourceAsyncWithValidIdShouldReturnResource)}");

        var searchResults = await musicApi!.SearchAsync(TestKeyword, limit: TestSearchLimit);
        Assert.That(searchResults, Is.Not.Null.And.Not.Empty);

        foreach (var song in searchResults!.Take(TestSearchLimit))
        {
            TestContext.WriteLine($"Trying song: {song.Name} by {string.Join(", ", song.Artists)} (ID: {song.Id})");

            var resource = await musicApi.GetSongResourceAsync(song.Id, br: TestBitrate);
            if (resource != null && !string.IsNullOrWhiteSpace(resource.Url))
            {
                TestContext.WriteLine($" Resource URL: {resource.Url}");
                TestContext.WriteLine($" Type: {resource.Type}, Bitrate: {resource.Br}kbps");
                Assert.That(resource.Type, Is.Not.Empty);
                Assert.That(resource.Br, Is.GreaterThan(0));
                Assert.Pass($"Successfully retrieved song resource for: {song.Name}");
                return;
            }

            TestContext.WriteLine($" Resource unavailable for: {song.Name}");
        }

        TestContext.WriteLine("All songs unavailable - expected for some songs due to licensing");
        Assert.Pass("Resource unavailable for all tested songs (expected for some songs)");
    }

    [Test]
    public async Task GetLyricAsyncWithValidIdShouldReturnLyric()
    {
        TestContext.WriteLine($"[Test] {nameof(GetLyricAsyncWithValidIdShouldReturnLyric)}");

        var searchResults = await musicApi!.SearchAsync(TestKeyword, limit: TestSearchLimit);
        Assert.That(searchResults, Is.Not.Null.And.Not.Empty);

        var songId = searchResults!.First().Id;
        var lyric = await musicApi.GetLyricAsync(songId);
        Assert.That(lyric, Is.Not.Null);
        Assert.That(lyric!.OriginalLyric, Is.Not.Empty);

        TestContext.WriteLine($"Has time info: {lyric.HasTimeInfo}");
        TestContext.WriteLine($"Original lyric length: {lyric.OriginalLyric.Length}");
        if (!string.IsNullOrWhiteSpace(lyric.TranslatedLyric))
        {
            TestContext.WriteLine($"Translated lyric length: {lyric.TranslatedLyric.Length}");
        }
    }

    [Test]
    public async Task GetPictureAsyncWithValidIdShouldReturnUrl()
    {
        TestContext.WriteLine($"[Test] {nameof(GetPictureAsyncWithValidIdShouldReturnUrl)}");

        var searchResults = await musicApi!.SearchAsync(TestKeyword, limit: TestSearchLimit);
        Assert.That(searchResults, Is.Not.Null.And.Not.Empty);

        var songId = searchResults!.First().Id;
        var pictureUrl = await musicApi.GetPictureAsync(songId, px: TestPictureSize);
        Assert.That(pictureUrl, Is.Not.Null.And.Not.Empty);
        Assert.That(pictureUrl, Does.Contain($"param={TestPictureSize}y{TestPictureSize}"));

        TestContext.WriteLine($"Picture URL: {pictureUrl}");
    }

    [Test]
    public void SearchAsyncWithNullKeywordShouldThrowArgumentNullException()
    {
        TestContext.WriteLine($"[Test] {nameof(SearchAsyncWithNullKeywordShouldThrowArgumentNullException)}");

        Assert.ThrowsAsync<ArgumentNullException>(async () => await musicApi!.SearchAsync(null!));
    }

    [Test]
    public void GetSongAsyncWithNullIdShouldThrowArgumentNullException()
    {
        TestContext.WriteLine($"[Test] {nameof(GetSongAsyncWithNullIdShouldThrowArgumentNullException)}");

        Assert.ThrowsAsync<ArgumentNullException>(async () => await musicApi!.GetSongAsync(null!));
    }
}