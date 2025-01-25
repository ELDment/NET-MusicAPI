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
        var result = await api.Search("初音未来");
        TestContext.Out.WriteLine(result[0].ToDebugString());
        Assert.IsNotNull(result);
    }
}