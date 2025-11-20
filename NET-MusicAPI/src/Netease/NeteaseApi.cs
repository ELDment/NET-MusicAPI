using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MusicAPI.Abstractions;

namespace MusicAPI.Netease;

public sealed partial class NeteaseApi : IMusicApi, IDisposable
{
    private readonly HttpClient httpClient;
    private readonly bool disposedHttpClient;
    private volatile bool disposed;

    [GeneratedRegex(@"\[(\d+:\d+.*)\]", RegexOptions.Compiled)]
    private static partial Regex TimeInfoRegex();

    public IReadOnlyDictionary<string, string>? CustomHeaders { get; set; }

    public NeteaseApi() : this(null)
    {
    }

    public NeteaseApi(HttpClient? client)
    {
        if (client == null)
        {
            this.httpClient = new HttpClient
            {
                Timeout = TimeSpan.FromSeconds(8)
            };
            this.disposedHttpClient = true;
        }
        else
        {
            this.httpClient = client;
            this.disposedHttpClient = false;
        }
    }

    private Dictionary<string, string> BuildHeaders()
    {
        var headers = new Dictionary<string, string>
        {
            ["Referer"] = "https://music.163.com/",
            ["Cookie"] = "appver=8.2.30; os=iPhone OS; osver=15.0; EVNSM=1.0.0; buildver=2206; channel=distribution; machineid=iPhone13.3",
            ["User-Agent"] = "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 CloudMusic/0.1.1 NeteaseMusic/8.2.30",
            ["X-Real-IP"] = HttpClientExtensions.GenerateRandomIp(),
            ["Accept"] = "*/*",
            ["Accept-Language"] = "zh-CN,zh;q=0.8,gl;q=0.6,zh-TW;q=0.4",
            ["Connection"] = "keep-alive",
            ["Content-Type"] = "application/x-www-form-urlencoded"
        };

        if (CustomHeaders != null)
        {
            foreach (var (key, value) in CustomHeaders)
            {
                headers[key] = value;
            }
        }

        return headers;
    }

    public async Task<IReadOnlyList<Song>?> SearchAsync(string keyword, int type = 1, int limit = 30, int page = 1, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(keyword);

        var requestBody = new
        {
            s = string.Concat(keyword.Select(c => $@"\u{(int)c:x4}")),
            type,
            limit,
            total = "true",
            offset = page > 0 && limit > 0 ? (page - 1) * limit : 0
        };

        var encrypted = NeteaseCryptoService.EncryptRequest("http://music.163.com/api/cloudsearch/pc", requestBody);
        var response = await httpClient.SendMusicApiRequestAsync(HttpMethod.Post, encrypted.Url, encrypted.Body, BuildHeaders(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        var json = JsonNode.Parse(response);
        var songs = json?["result"]?["songs"]?.AsArray();
        if (songs == null)
        {
            return null;
        }

        var results = new List<Song>(songs.Count);

        foreach (var songNode in songs)
        {
            if (songNode != null)
            {
                var artists = songNode["ar"]?.AsArray()
                .Select(a => a?["name"]?.GetValue<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Cast<string>()
                .ToList() ?? [];

                var song = new Song
                {
                    Id = songNode["id"]?.GetValue<long>().ToString() ?? string.Empty,
                    Name = songNode["name"]?.GetValue<string>() ?? string.Empty,
                    Artists = artists,
                    AlbumName = songNode["al"]?["name"]?.GetValue<string>() ?? string.Empty,
                    LyricId = songNode["id"]?.GetValue<long>().ToString() ?? string.Empty
                };

                results.Add(song);
            }
        }

        return results;
    }

    public async Task<Song?> GetSongAsync(string id, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var requestBody = new
        {
            c = $"[{{\"id\":'{id}',\"v\":0}}]"
        };

        var encrypted = NeteaseCryptoService.EncryptRequest("http://music.163.com/api/v3/song/detail/", requestBody);

        var response = await httpClient.SendMusicApiRequestAsync(HttpMethod.Post, encrypted.Url, encrypted.Body, BuildHeaders(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        var json = JsonNode.Parse(response);
        var songNode = json?["songs"]?[0];
        if (songNode == null)
        {
            return null;
        }

        var artists = songNode["ar"]?.AsArray()
            .Select(a => a?["name"]?.GetValue<string>())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToList() ?? [];

        return new Song
        {
            Id = songNode["id"]?.GetValue<long>().ToString() ?? string.Empty,
            Name = songNode["name"]?.GetValue<string>() ?? string.Empty,
            Artists = artists,
            AlbumName = songNode["al"]?["name"]?.GetValue<string>() ?? string.Empty,
            LyricId = songNode["id"]?.GetValue<long>().ToString() ?? string.Empty,
            Picture = songNode["al"]?["picUrl"]?.GetValue<string>() ?? string.Empty
        };
    }

    public async Task<SongResource?> GetSongResourceAsync(string id, int br = 320, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var requestBody = new
        {
            ids = $"[{id}]",
            br = br * 1000
        };

        var encrypted = NeteaseCryptoService.EncryptRequest("http://music.163.com/api/song/enhance/player/url", requestBody);

        var response = await httpClient.SendMusicApiRequestAsync(HttpMethod.Post, encrypted.Url, encrypted.Body, BuildHeaders(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        var json = JsonNode.Parse(response);
        var data = json?["data"]?[0];
        if (data == null)
        {
            return null;
        }

        return new SongResource
        {
            Url = data["url"]?.GetValue<string>() ?? string.Empty,
            Type = data["type"]?.GetValue<string>() ?? string.Empty,
            Br = (int)(data["br"]?.GetValue<long>() ?? 0) / 1000,
            Size = (data["size"]?.GetValue<long>() ?? 0) / 1024.0 / 1024.0,
            Duration = (data["time"]?.GetValue<long>() ?? 0) / 1000.0
        };
    }

    public async Task<Lyric?> GetLyricAsync(string id, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var requestBody = new
        {
            id,
            os = "linux",
            lv = -1,
            kv = -1,
            tv = -1
        };

        var encrypted = NeteaseCryptoService.EncryptRequest("http://music.163.com/api/song/lyric", requestBody);

        var response = await httpClient.SendMusicApiRequestAsync(HttpMethod.Post, encrypted.Url, encrypted.Body, BuildHeaders(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        var json = JsonNode.Parse(response);
        var originalLyric = json?["lrc"]?["lyric"]?.GetValue<string>() ?? string.Empty;
        var translatedLyric = json?["tlyric"]?["lyric"]?.GetValue<string>() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(originalLyric))
        {
            return null;
        }

        return new Lyric
        {
            HasTimeInfo = TimeInfoRegex().IsMatch(originalLyric),
            OriginalLyric = originalLyric,
            TranslatedLyric = translatedLyric
        };
    }

    public async Task<string?> GetPictureAsync(string id, int px = 300, CancellationToken cancellationToken = default)
    {
        ThrowIfDisposed();
        var song = await GetSongAsync(id, cancellationToken).ConfigureAwait(false);

        if (song == null || string.IsNullOrWhiteSpace(song.Picture))
        {
            return string.Empty;
        }

        return $"{song.Picture}?param={px}y{px}";
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }

        disposed = true;

        if (disposedHttpClient)
        {
            httpClient?.Dispose();
        }

        GC.SuppressFinalize(this);
    }

    private void ThrowIfDisposed()
    {
        if (disposed)
        {
            throw new ObjectDisposedException(nameof(NeteaseApi));
        }
    }
}