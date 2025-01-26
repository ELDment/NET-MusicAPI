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
		//声明API实例
		//var api = new NeteaseAPI();
		var api = new TencentAPI();

		//需要增加（修改）的Headers
		//api.Headers = new Dictionary<string, string> { { "Addition", "12345" } /*, { "Cookie", "Yours" }*/ };

		//搜索音乐
		var search = await api.Search("secret base～君がくれたもの～", limit: 30);
		var song = search[0]!;
		Console.WriteLine(song);

		//获取音乐信息
		var songInfo = await api.GetSong(song!.Id);
		Console.WriteLine(songInfo);

		//获取音乐资源
		//var songResource = await api.GetSongResource(song!.Id);
		//Console.WriteLine(songResource);

		//获取音乐歌词
		var songLyric = await api.GetLyric(song!.Id);
		Console.WriteLine(songLyric);

		//获取音乐头图
		var songPicture = await api.GetPicture(song!.Id, 300);
		Console.WriteLine(songPicture);

		return;
	}
}