<p align="center">

<img alt="Author" src="https://img.shields.io/badge/Author-ELDment-blue.svg?style=flat-square" height="20"/>
<img alt="Star" src="https://img.shields.io/github/stars/ELDment/CSharp-Music-API?style=for-the-badge&logo=github" height="20">

</p>

 > 🍰 .NET平台强大的音乐API框架，支持网易云音乐、QQ音乐
 >
 > ✨ Wow, such a powerful .NET music API framework, Support Netease Music, Tencent(QQ) Music.

## Introduction

一个强大的音乐平台API框架，他会助力您的开发❤️
 + **🍰优雅** - 使用简单, 为全平台构造了统一数据结构
 + **🙀强大** - 支持主流音乐平台, 包含：网易云音乐、QQ音乐
 + **🤩免费** - 使用GPLv3协议

## Requirement
[Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)
> **📍Note:** 测试开发时使用NetCore8.0

## Building
**克隆项目库**
```powershell
git clone https://github.com/ELDment/CSharp-Music-API.git
```
**通过以下指令构建库:**
```powershell
cd CSharp-Music-API

dotnet clean

dotnet build -c Release
```
**或者**直接运行测试模块:
```powershell
cd CSharp-Music-API

dotnet test
```

> **📍Note:** 编译时库要求 [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json)


## Quick Start
```csharp
//using MusicAPI;

public class Program {
	static async Task Main(string[] args) {
		//声明平台API实例
		var api = new NeteaseAPI();
		//var api = new TencentAPI();

		//设置Headers
		api.Headers = new Dictionary<string, string> { { "Addition", "12345" } /*, { "Cookie", "Yours" }*/ };

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
```

## More usage
 - [网易云的Headers设置](https://github.com/metowolf/Meting/wiki/special-for-netease)

## Related Projects
 - [metowolf/Meting](https://github.com/metowolf/Meting)
 - [ELDment/Meting-MusicApi-Fixed](https://github.com/ELDment/Meting-MusicApi-Fixed)

## Contribution
- **samyycX** 项目发起者之一 😋 重构并制定了代码标准
- （欢迎大家提交高质量PR🤓👍）