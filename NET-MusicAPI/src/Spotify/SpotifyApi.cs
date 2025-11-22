using System.Web;
using SpotifyAPI.Web;
using MusicAPI.Abstractions;

namespace MusicAPI.Spotify;

/// <summary>
/// Spotify + YouTube downloader implementation
/// Gets metadata from Spotify and generates YouTube search URLs
/// Note: This returns YouTube search URLs, not direct download links
/// </summary>
public sealed class SpotifyApi : IMusicApi, IDisposable
{
    private readonly string? spotifyClientId;
    private readonly string? spotifyClientSecret;
    private SpotifyClient? spotifyClient;
    private readonly SemaphoreSlim authLock = new(1, 1);
    private volatile bool disposed;

    public IReadOnlyDictionary<string, string>? CustomHeaders { get; set; }

    public SpotifyApi(string? spotifyClientId = null, string? spotifyClientSecret = null)
    {
        this.spotifyClientId = spotifyClientId;
        this.spotifyClientSecret = spotifyClientSecret;
    }

    public SpotifyApi(HttpClient httpClient, string? spotifyClientId = null, string? spotifyClientSecret = null) : this(spotifyClientId, spotifyClientSecret)
    {
        // SpotifyAPI.Web manages its own HttpClient
    }

    public async Task<IReadOnlyList<Song>?> SearchAsync(string keyword, int type = 1, int limit = 30, int page = 1, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(keyword);

        var client = await GetSpotifyClientAsync(cancellationToken).ConfigureAwait(false);
        var offset = (page - 1) * limit;

        var searchRequest = new SearchRequest(SearchRequest.Types.Track, keyword)
        {
            Limit = limit,
            Offset = offset
        };

        var searchResponse = await client.Search.Item(searchRequest, cancellationToken).ConfigureAwait(false);
        if (searchResponse?.Tracks?.Items == null || searchResponse.Tracks.Items.Count == 0)
        {
            return null;
        }

        var results = new List<Song>();
        foreach (var track in searchResponse.Tracks.Items)
        {
            if (track == null)
            {
                continue;
            }

            results.Add(new Song
            {
                Id = track.Id,
                Name = track.Name,
                Artists = track.Artists.Select(a => a.Name).ToList(),
                AlbumName = track.Album.Name,
                Picture = track.Album.Images.FirstOrDefault()?.Url ?? string.Empty,
                LyricId = string.Empty
            });
        }

        return results;
    }

    public async Task<Song?> GetSongAsync(string id, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var client = await GetSpotifyClientAsync(cancellationToken).ConfigureAwait(false);
        var track = await client.Tracks.Get(id, cancellationToken).ConfigureAwait(false);

        if (track == null)
        {
            return null;
        }

        return new Song
        {
            Id = track.Id,
            Name = track.Name,
            Artists = track.Artists.Select(a => a.Name).ToList(),
            AlbumName = track.Album.Name,
            Picture = track.Album.Images.FirstOrDefault()?.Url ?? string.Empty,
            LyricId = string.Empty
        };
    }

    public async Task<SongResource?> GetSongResourceAsync(string id, int br = 320, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        // Get Spotify metadata
        var client = await GetSpotifyClientAsync(cancellationToken).ConfigureAwait(false);
        var track = await client.Tracks.Get(id, cancellationToken).ConfigureAwait(false);
        if (track == null)
        {
            return null;
        }

        var artists = string.Join(" ", track.Artists.Select(a => a.Name));
        var searchQuery = $"{artists} - {track.Name}";

        // Generate YouTube search URL
        var youtubeSearchUrl = GenerateYouTubeSearchUrl(searchQuery);

        return new SongResource
        {
            Url = youtubeSearchUrl,
            Type = "youtube-search",
            Br = 128, // YouTube typically 128-256kbps
            Size = 0,
            Duration = track.DurationMs / 1000
        };
    }

    public Task<Lyric?> GetLyricAsync(string id, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        // TODO:
        // Spotify Web API doesn't provide lyrics
        // Could integrate with Genius/Musixmatch like spotdl does
        return Task.FromResult<Lyric?>(null);
    }

    public async Task<string?> GetPictureAsync(string id, int px = 300, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(disposed, this);
        ArgumentException.ThrowIfNullOrWhiteSpace(id);

        var song = await GetSongAsync(id, cancellationToken).ConfigureAwait(false);
        return song?.Picture;
    }

    public void Dispose()
    {
        if (disposed)
        {
            return;
        }
        disposed = true;

        authLock.Dispose();
        GC.SuppressFinalize(this);
    }

    private async Task<SpotifyClient> GetSpotifyClientAsync(CancellationToken cancellationToken = default)
    {
        if (spotifyClient != null)
        {
            return spotifyClient;
        }

        await authLock.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (spotifyClient != null)
            {
                return spotifyClient;
            }

            var id = spotifyClientId ?? CustomHeaders?.GetValueOrDefault("SpotifyClientId") ?? Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_ID");
            var secret = spotifyClientSecret ?? CustomHeaders?.GetValueOrDefault("SpotifyClientSecret") ?? Environment.GetEnvironmentVariable("SPOTIFY_CLIENT_SECRET");

            if (string.IsNullOrEmpty(id) || string.IsNullOrEmpty(secret))
            {
                throw new InvalidOperationException("Spotify Client ID and Secret are required.");
            }

            var config = SpotifyClientConfig.CreateDefault();
            var request = new ClientCredentialsRequest(id, secret);
            var response = await new OAuthClient(config).RequestToken(request, cancellationToken).ConfigureAwait(false);

            spotifyClient = new SpotifyClient(config.WithToken(response.AccessToken));
            return spotifyClient;
        }
        finally
        {
            authLock.Release();
        }
    }

    private static string GenerateYouTubeSearchUrl(string query)
    {
        var encodedQuery = HttpUtility.UrlEncode(query);
        return $"https://www.youtube.com/results?search_query={encodedQuery}";
    }
}