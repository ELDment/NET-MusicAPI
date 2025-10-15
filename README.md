# NET-MusicAPI

## 环境要求
- [Newtonsoft.Json](https://github.com/JamesNK/Newtonsoft.Json) (≥13.0.3)
- .NET 8.0+ (测试环境)

## 快速开始
```powershell
# 克隆本项目
git clone https://github.com/ELDment/NET-MusicAPI.git
cd NET-MusicAPI

# 构建类库
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
 - [ELDment/Meting-Fixed](https://github.com/ELDment/Meting-Fixed)

## 贡献
- **[samyycX](https://github.com/samyycX)**

## 关键词
```
网易云音乐[搜索|直链|歌词|封面|详情|专辑|歌单|歌手]API
QQ音乐[搜索|直链|歌词|封面|详情|专辑|歌单|歌手]API
Netease [Search|URL|Stream|Lyrics|Cover|Details|Album|Playlist|Artist] API
QQ Music [Search|URL|Stream|Lyrics|Cover|Details|Album|Playlist|Artist] API
```
