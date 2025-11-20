# NET-MusicAPI

[![.NET](https://img.shields.io/badge/.NET-10.0-blue.svg)](https://dotnet.microsoft.com/)
[![C#](https://img.shields.io/badge/C%23-12-green.svg)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![License](https://img.shields.io/badge/license-MIT-orange.svg)](LICENSE)

现代化的 .NET 音乐 API 库，支持网易云音乐。使用 .NET 10 和 C# 12 最新特性构建，提供简洁优雅的 API 接口。

## 📋 环境要求

- .NET 10.0
- C# 12

### NuGet 包依赖

- `Microsoft.Extensions.Http` (≥10.0.0)
- `Microsoft.Extensions.DependencyInjection.Abstractions` (≥10.0.0)
- `System.Text.Json` (≥10.0.0)

## 🚀 快速开始

### 安装

```powershell
# 克隆项目
git clone https://github.com/ELDment/NET-MusicAPI.git
cd NET-MusicAPI

# 构建项目
dotnet build -c Release

# 运行测试
dotnet test -c Release
```

### 基本使用

#### 独立使用

```csharp
using MusicAPI.Netease;

using var api = new NeteaseApi();

// 搜索歌曲
var songs = await api.SearchAsync("坏女孩", limit: 10);
foreach (var song in songs!)
{
    Console.WriteLine($"{song.Name} - {string.Join(", ", song.Artists)}");
}

// 获取歌曲详情
var songDetail = await api.GetSongAsync(songs.First().Id);
Console.WriteLine($"歌曲：{songDetail!.Name}");
Console.WriteLine($"封面：{songDetail.Picture}");

// 获取歌曲资源（播放链接）
var resource = await api.GetSongResourceAsync(songs.First().Id, br: 320);
if (!string.IsNullOrEmpty(resource?.Url))
{
    Console.WriteLine($"播放链接：{resource.Url}");
    Console.WriteLine($"比特率：{resource.Br}kbps");
}

// 获取歌词
var lyric = await api.GetLyricAsync(songs.First().Id);
Console.WriteLine($"原文歌词：{lyric!.OriginalLyric}");
if (!string.IsNullOrEmpty(lyric.TranslatedLyric))
{
    Console.WriteLine($"翻译歌词：{lyric.TranslatedLyric}");
}

// 获取封面图片
var pictureUrl = await api.GetPictureAsync(songs.First().Id, px: 300);
Console.WriteLine($"封面链接：{pictureUrl}");
```

#### 依赖注入（ASP.NET Core）

```csharp
using MusicAPI.Extensions;
using MusicAPI.Abstractions;

// 注册服务
builder.Services.AddNeteaseApi();

// 在控制器或服务中使用
public class MusicController : ControllerBase
{
    private readonly IMusicApi musicApi;

    public MusicController(IMusicApi musicApi)
    {
        this.musicApi = musicApi;
    }

    [HttpGet("search")]
    public async Task<IActionResult> Search(string keyword, int limit = 10)
    {
        var results = await musicApi.SearchAsync(keyword, limit: limit);
        return Ok(results);
    }

    [HttpGet("song/{id}")]
    public async Task<IActionResult> GetSong(string id)
    {
        var song = await musicApi.GetSongAsync(id);
        return Ok(song);
    }
}
```

### 高级用法

#### 自定义请求头

```csharp
using var api = new NeteaseApi();

// 设置自定义 Headers
api.CustomHeaders = new Dictionary<string, string>
{
    ["User-Agent"] = "...",
    ["Cookie"] = "..."
};

var songs = await api.SearchAsync("星光就在前方");
```

#### 使用外部 HttpClient

```csharp
// 如果需要自己管理 HttpClient
var httpClient = new HttpClient
{
    Timeout = TimeSpan.FromSeconds(60)
};

using var api = new NeteaseApi(httpClient);
var songs = await api.SearchAsync("裙摆与向日葵花");
```

## 🧪 测试

```powershell
# 运行所有测试
dotnet test

# 运行测试并显示详细输出
dotnet test --logger "console;verbosity=detailed"

# 运行特定测试
dotnet test --filter "FullyQualifiedName~NeteaseApiTests"
```

## 🤝 贡献

### 贡献者

- **[electronix](https://github.com/samyycX)** - 项目原始贡献者

## 📄 许可证

本项目采用 [MIT](LICENSE) 许可证

## 🔗 相关项目

- [metowolf/Meting](https://github.com/metowolf/Meting)
- [ELDment/Meting-Fixed](https://github.com/ELDment/Meting-Fixed)

---

**关键词**: 网易云音乐API, Netease Cloud Music API, .NET Music API, C# Music Library, 音乐搜索, 歌词获取, 歌曲直链
