# CSharp-Music-API

> 🔥 .NET平台强大的音乐API框架，支持网易云音乐、QQ音乐
>
> 🎵 A powerful music API framework for .NET, supporting NetEase Cloud Music and QQ Music

- **🍰 优雅** - 使用简单, 为全平台构造了统一数据结构
- **🙀 强大** - 支持主流音乐平台：网易云音乐、QQ音乐
- **🤩 开放** - 使用GPLv3协议

## 环境要求
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) (≥13.0.3)
- .NET 8.0+ (测试环境)

## 快速开始
```powershell
# 克隆本项目
git clone https://github.com/ELDment/CSharp-Music-API
cd CSharp-Music-API

# 构建类库
dotnet clean
dotnet build -c Release

# 运行测试模块
dotnet test
```

## 使用实例
```csharp
// using MusicAPI;

public class Program {
    static async Task Main(string[] args) {
        // 声明平台API实例
        var api = new NeteaseAPI();
        //var api = new TencentAPI();

        // 设置Headers
        api.Headers = new Dictionary<string, string> { { "special", "xxx" } /*, { "Cookie", "xxx" }*/ };

        // 搜索歌曲
        var search = await api.Search("Avid", limit: 5);
        var song = search[0]!;
        Console.WriteLine(song);

        // 获取歌曲信息
        var songInfo = await api.GetSong(song!.Id);
        Console.WriteLine(songInfo);

        // 获取歌曲资源
        var songResource = await api.GetSongResource(song!.Id);
        Console.WriteLine(songResource);

        // 获取歌词
        var songLyric = await api.GetLyric(song!.Id);
        Console.WriteLine(songLyric);

        // 获取歌曲头图
        var songPicture = await api.GetPicture(song!.Id, 520);
        Console.WriteLine(songPicture);

        return;
    }
}
```

## 更多用法
 - [网易云的Headers设置](https://github.com/metowolf/Meting/wiki/special-for-netease)

## 相关项目
 - [metowolf/Meting](https://github.com/metowolf/Meting)
 - [ELDment/Meting-MusicApi-Fixed](https://github.com/ELDment/Meting-MusicApi-Fixed)

## 贡献
- samyycX 项目发起者之一（重构并制定了代码标准）
- **...**

## 关键词
```
音乐API 聚合音乐API 多平台音乐API 网易云音乐API QQ音乐API 酷狗音乐API 酷我音乐API 音乐搜索API 获取音乐直链API 获取音乐歌词API 获取音乐封面API 获取歌曲详情API 获取专辑信息API 获取歌单信息API 获取歌手信息API 网易云音乐搜索API 网易云音乐获取直链API 网易云音乐获取歌词API 网易云音乐获取封面API 网易云音乐获取歌曲详情API 网易云音乐获取专辑信息API 网易云音乐获取歌单信息API 网易云音乐获取歌手信息API QQ音乐搜索API QQ音乐获取直链API QQ音乐获取歌词API QQ音乐获取封面API QQ音乐获取歌曲详情API QQ音乐获取专辑信息API QQ音乐获取歌单信息API QQ音乐获取歌手信息API 酷狗音乐搜索API 酷狗音乐获取直链API 酷狗音乐获取歌词API 酷狗音乐获取封面API 酷狗音乐获取歌曲详情API 酷狗音乐获取专辑信息API 酷狗音乐获取歌单信息API 酷狗音乐获取歌手信息API 酷我音乐搜索API 酷我音乐获取直链API 酷我音乐获取歌词API 酷我音乐获取封面API 酷我音乐获取歌曲详情API 酷我音乐获取专辑信息API 酷我音乐获取歌单信息API 酷我音乐获取歌手信息API Music API Aggregated Music API Multi-platform Music API Netease Cloud Music API QQ Music API KuGou Music API Kuwo Music API Music Search API Get Music MP3 URL API Get Music Stream URL API Get Music Lyrics API Get Music Cover Art API Get Song Details API Get Album Info API Get Playlist Info API Get Artist Info API Netease Cloud Music Search API Netease Cloud Music MP3 URL API Netease Cloud Music Stream URL API Netease Cloud Music Lyrics API Netease Cloud Music Cover Art API Netease Cloud Music Song Details API Netease Cloud Music Album Info API Netease Cloud Music Playlist Info API Netease Cloud Music Artist Info API QQ Music Search API QQ Music MP3 URL API QQ Music Stream URL API QQ Music Lyrics API QQ Music Cover Art API QQ Music Song Details API QQ Music Album Info API QQ Music Playlist Info API QQ Music Artist Info API KuGou Music Search API KuGou Music MP3 URL API KuGou Music Stream URL API KuGou Music Lyrics API KuGou Music Cover Art API KuGou Music Song Details API KuGou Music Album Info API KuGou Music Playlist Info API KuGou Music Artist Info API Kuwo Music Search API Kuwo Music MP3 URL API Kuwo Music Stream URL API Kuwo Music Lyrics API Kuwo Music Cover Art API Kuwo Music Song Details API Kuwo Music Album Info API Kuwo Music Playlist Info API Kuwo Music Artist Info API
```
