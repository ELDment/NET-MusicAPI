using System.Web;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using MusicAPI.Abstractions;

namespace MusicAPI.Tencent;

public sealed partial class TencentApi : IMusicApi, IDisposable
{
    private readonly HttpClient httpClient;
    private readonly bool disposedHttpClient;
    private volatile bool disposed;

    [GeneratedRegex(@"\[(\d+:\d+.*)\]", RegexOptions.Compiled)]
    private static partial Regex TimeInfoRegex();

    public IReadOnlyDictionary<string, string>? CustomHeaders { get; set; }

    public TencentApi() : this(null)
    {
    }

    public TencentApi(HttpClient? client)
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
            ["Referer"] = "http://y.qq.com",
            ["Cookie"] = "pgv_pvi=22038528; pgv_si=s3156287488; pgv_pvid=5535248600; yplayer_open=1; ts_last=y.qq.com/portal/player.html; ts_uid=4847550686; yq_index=0; qqmusic_fromtag=66; player_exist=1",
            ["User-Agent"] = "QQ%E9%9F%B3%E4%B9%90/54409 CFNetwork/901.1 Darwin/17.6.0 (x86_64)",
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
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyword);

        var url = "https://c.y.qq.com/soso/fcgi-bin/client_search_cp";
        var parameters = new Dictionary<string, string>
        {
            ["format"] = "json",
            ["p"] = page.ToString(),
            ["n"] = limit.ToString(),
            ["w"] = keyword,
            ["aggr"] = "1",
            ["lossless"] = "1",
            ["cr"] = "1",
            ["new_json"] = "1"
        };

        var response = await httpClient.SendMusicApiRequestAsync(HttpMethod.Get, url, parameters, BuildHeaders(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        var json = JsonNode.Parse(response);
        var songs = json?["data"]?["song"]?["list"]?.AsArray();
        if (songs == null)
        {
            return null;
        }

        var results = new List<Song>(songs.Count);

        foreach (var songNode in songs)
        {
            if (songNode != null)
            {
                var artists = songNode["singer"]?.AsArray()
                    .Select(a => a?["name"]?.GetValue<string>())
                    .Where(name => !string.IsNullOrWhiteSpace(name))
                    .Cast<string>()
                    .ToList() ?? [];

                var song = new Song
                {
                    Id = songNode["mid"]?.GetValue<string>() ?? string.Empty,
                    Name = songNode["name"]?.GetValue<string>() ?? string.Empty,
                    Artists = artists,
                    AlbumName = songNode["album"]?["title"]?.GetValue<string>() ?? string.Empty,
                    LyricId = songNode["mid"]?.GetValue<string>() ?? string.Empty
                };

                results.Add(song);
            }
        }

        return results;
    }

    public async Task<Song?> GetSongAsync(string id, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var url = "https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg";
        var parameters = new Dictionary<string, string>
        {
            ["songmid"] = id,
            ["platform"] = "yqq",
            ["format"] = "json"
        };

        var response = await httpClient.SendMusicApiRequestAsync(HttpMethod.Get, url, parameters, BuildHeaders(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        var json = JsonNode.Parse(response);
        var songNode = json?["data"]?[0];
        if (songNode == null)
        {
            return null;
        }

        var artists = songNode["singer"]?.AsArray()
            .Select(a => a?["name"]?.GetValue<string>())
            .Where(name => !string.IsNullOrWhiteSpace(name))
            .Cast<string>()
            .ToList() ?? [];

        var albumMid = songNode["album"]?["mid"]?.GetValue<string>() ?? string.Empty;

        return new Song
        {
            Id = songNode["mid"]?.GetValue<string>() ?? string.Empty,
            Name = songNode["name"]?.GetValue<string>() ?? string.Empty,
            Artists = artists,
            AlbumName = songNode["album"]?["title"]?.GetValue<string>() ?? string.Empty,
            LyricId = songNode["mid"]?.GetValue<string>() ?? string.Empty,
            Picture = !string.IsNullOrEmpty(albumMid) ? $"https://y.gtimg.cn/music/photo_new/T002R300x300M000{albumMid}.jpg" : string.Empty
        };
    }

    public async Task<SongResource?> GetSongResourceAsync(string id, int br = 320, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var guid = "10000";
        var uin = "0";

        var qualityMap = new[]
        {
            new { Br = 999, Prefix = "F000", Ext = ".flac", Name = "flac" },
            new { Br = 320, Prefix = "M800", Ext = ".mp3", Name = "320" },
            new { Br = 192, Prefix = "C600", Ext = ".m4a", Name = "aac_192" },
            new { Br = 128, Prefix = "M500", Ext = ".mp3", Name = "128" },
            new { Br = 96, Prefix = "C400", Ext = ".m4a", Name = "aac_96" },
            new { Br = 48, Prefix = "C200", Ext = ".m4a", Name = "aac_48" }
        };

        // Build request payload for single quality level
        foreach (var quality in qualityMap.Where(q => q.Br <= br).OrderByDescending(q => q.Br))
        {
            // Reference: https://github.com/Suxiaoqinx/tencent_url
            // !!! IMPORTANT: filename format is {prefix}{songmid}{songmid}{ext}
            var filename = $"{quality.Prefix}{id}{id}{quality.Ext}";

            var payload = new
            {
                req_1 = new
                {
                    module = "vkey.GetVkeyServer",
                    method = "CgiGetVkey",
                    param = new
                    {
                        filename = new[] { filename },
                        guid = guid,
                        songmid = new[] { id },
                        songtype = new[] { 0 },
                        uin = uin,
                        loginflag = 1,
                        platform = "20"
                    }
                },
                loginUin = uin,
                comm = new
                {
                    uin = uin,
                    format = "json",
                    ct = 24,
                    cv = 0
                }
            };

            var vkeyUrl = "https://u.y.qq.com/cgi-bin/musicu.fcg";
            var jsonPayload = JsonSerializer.Serialize(payload);

            var headers = BuildHeaders();
            headers["Content-Type"] = "application/json";

            using var request = new HttpRequestMessage(HttpMethod.Post, vkeyUrl)
            {
                Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json")
            };

            foreach (var (key, value) in headers)
            {
                request.Headers.TryAddWithoutValidation(key, value);
            }

            using var response = await httpClient.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                continue;
            }

            var responseContent = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
            if (string.IsNullOrWhiteSpace(responseContent))
            {
                continue;
            }

            var vkeyJson = JsonNode.Parse(responseContent);
            var midurlinfo = vkeyJson?["req_1"]?["data"]?["midurlinfo"]?.AsArray();
            var sip = vkeyJson?["req_1"]?["data"]?["sip"]?.AsArray();

            if (midurlinfo == null || midurlinfo.Count == 0 || sip == null || sip.Count == 0)
            {
                continue;
            }

            var purl = midurlinfo[0]?["purl"]?.GetValue<string>();
            if (string.IsNullOrEmpty(purl))
            {
                // This quality not available, try next one
                continue;
            }

            var sipUrl = sip[1]?.GetValue<string>() ?? sip[0]?.GetValue<string>() ?? string.Empty;
            var fullUrl = (sipUrl + purl).Replace("http://", "https://");

            return new SongResource
            {
                Url = fullUrl,
                Type = quality.Ext.TrimStart('.'),
                Br = quality.Br,
                Size = 0,
                Duration = 0
            };
        }

        return null;
    }

    public async Task<Lyric?> GetLyricAsync(string id, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var url = "https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg";
        var parameters = new Dictionary<string, string>
        {
            ["songmid"] = id,
            ["g_tk"] = "5381"
        };

        var response = await httpClient.SendMusicApiRequestAsync(HttpMethod.Get, url, parameters, BuildHeaders(), cancellationToken).ConfigureAwait(false);
        if (string.IsNullOrWhiteSpace(response))
        {
            return null;
        }

        // Remove JSONP wrapper: MusicJsonCallback(...)
        var jsonStr = response.Trim();
        if (jsonStr.StartsWith("MusicJsonCallback("))
        {
            jsonStr = jsonStr[18..^1];
        }

        var json = JsonNode.Parse(jsonStr);
        var lyricBase64 = json?["lyric"]?.GetValue<string>();
        var transBase64 = json?["trans"]?.GetValue<string>();

        if (string.IsNullOrWhiteSpace(lyricBase64))
        {
            return null;
        }

        var originalLyric = DecodeBase64AndHtmlEntities(lyricBase64);
        var translatedLyric = !string.IsNullOrWhiteSpace(transBase64) ? DecodeBase64AndHtmlEntities(transBase64) : string.Empty;

        return new Lyric
        {
            HasTimeInfo = TimeInfoRegex().IsMatch(originalLyric),
            OriginalLyric = originalLyric,
            TranslatedLyric = translatedLyric
        };
    }

    public async Task<string?> GetPictureAsync(string id, int px = 300, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);

        var song = await GetSongAsync(id, cancellationToken).ConfigureAwait(false);
        if (song == null || string.IsNullOrWhiteSpace(song.Picture))
        {
            return string.Empty;
        }

        return song.Picture.Replace("300x300", $"{px}x{px}");
    }

    private static string DecodeBase64AndHtmlEntities(string base64String)
    {
        var bytes = Convert.FromBase64String(base64String);
        var text = Encoding.UTF8.GetString(bytes);

        return HttpUtility.HtmlDecode(text);
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
}