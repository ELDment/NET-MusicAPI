
namespace MusicAPI;

/// <summary>
/// Represents a music song with metadata
/// </summary>
public sealed record Song
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required IReadOnlyList<string> Artists { get; init; }
    public required string AlbumName { get; init; }
    public required string LyricId { get; init; }
    public string Picture { get; init; } = string.Empty;
}

/// <summary>
/// Represents a song resource with streaming information
/// </summary>
public sealed record SongResource
{
    public required string Url { get; init; }
    public required string Type { get; init; }
    public required int Br { get; init; }
    public double Size { get; init; }
    public double Duration { get; init; }
}

/// <summary>
/// Represents an artist
/// </summary>
public sealed record Artist
{
    public required string Name { get; init; }
    public string? Id { get; init; }
}

/// <summary>
/// Represents an album with cover art
/// </summary>
public sealed record Album
{
    public required string Name { get; init; }
    public required string Pic { get; init; }
}

/// <summary>
/// Represents song lyrics with optional translation
/// </summary>
public sealed record Lyric
{
    public required bool HasTimeInfo { get; init; }
    public required string OriginalLyric { get; init; }
    public string TranslatedLyric { get; init; } = string.Empty;
}