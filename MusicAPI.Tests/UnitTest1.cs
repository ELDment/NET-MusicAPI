using System.Reflection;
using System;

namespace MusicAPI.Tests;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public async Task Test()
    {
        
        var api = new NeteaseAPI();
        //var api = new TencentAPI();

        //设置Headers
        //api.Headers = new Dictionary<string, string> { { "Addition", "12345" } /*, { "Cookie", "Yours" }*/ };

        //搜索歌曲
        var search = await api.Search("Avid", limit: 5);
        var song = search[0]!;
        Console.WriteLine(song);

        //获取歌曲信息
        var songInfo = await api.GetSong(song!.Id);
        Console.WriteLine(songInfo);

        //获取歌曲资源
        var songResource = await api.GetSongResource(song!.Id);
        Console.WriteLine(songResource);

        //获取歌词
        var songLyric = await api.GetLyric(song!.Id);
        Console.WriteLine(songLyric);

        //获取歌曲头图
        var songPicture = await api.GetPicture(song!.Id, 520);
        Console.WriteLine(songPicture);

        return;
    }
}