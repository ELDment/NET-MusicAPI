namespace MusicAPI.Abstractions;

public interface IMusicApi
{
    /// <summary>
    /// Search for songs by keyword
    /// </summary>
    /// <param name="keyword">Search keyword</param>
    /// <param name="type">Search type (default: 1)</param>
    /// <param name="limit">Maximum number of results (default: 30)</param>
    /// <param name="page">Page number (default: 1)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>List of songs matching the search criteria</returns>
    Task<IReadOnlyList<Song>?> SearchAsync(string keyword, int type = 1, int limit = 30, int page = 1, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get detailed information about a song
    /// </summary>
    /// <param name="id">Song identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Song details or null if not found</returns>
    Task<Song?> GetSongAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get streaming resource for a song
    /// </summary>
    /// <param name="id">Song identifier</param>
    /// <param name="br">Bitrate in kbps (default: 320)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Song resource or null if not available</returns>
    Task<SongResource?> GetSongResourceAsync(string id, int br = 320, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get lyrics for a song
    /// </summary>
    /// <param name="id">Song identifier</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Lyric information or null if not available</returns>
    Task<Lyric?> GetLyricAsync(string id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get picture URL for a song
    /// </summary>
    /// <param name="id">Song identifier</param>
    /// <param name="px">Picture size in pixels (default: 300)</param>
    /// <param name="cancellationToken">Cancellation token</param>
    /// <returns>Picture URL or null if not available</returns>
    Task<string?> GetPictureAsync(string id, int px = 300, CancellationToken cancellationToken = default);
}