namespace MusicAPI.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task NeteaseTest()
    {
        var api = new NeteaseAPI();
        var result = await api.Search("初音未来", page: 1);
        var song = result[2]!;
        TestContext.Out.WriteLine(song.ToString());
        var lyric = await api.GetLyric(song.Id);
        TestContext.Out.WriteLine(lyric.ToString());
        Assert.IsNotNull(result);
    }
}