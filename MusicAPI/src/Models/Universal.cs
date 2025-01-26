using Newtonsoft.Json;

namespace MusicAPI;


public record Song
{
  public required string    Id { get; set; }
  public required string    Name { get; set; }
  public required List<string> Artists { get; set; }
  public required string    AlbumName { get; set; }
  public required string    LyricId { get; set; }
  public string?            Picture { get; set; } = string.Empty;
}

public record SongResource
{
  public required string    Url { get; set; }
  public required string    Type { get; set; }
  public required int       Br { get; set; }
  public double             Size { get; set; }
  public double             Duration { get; set; }
}

public record Artist
{
  public required string    Name { get; set; }
  public string?            Id { get; set; }
}

public record Album
{
  public required string    Name { get; set; }
  public required string    Pic { get; set; }
}

public record Lyric
{
  public required bool      HasTimeInfo { get; set; }
  public required string    OriginalLyric { get; set; }
  public string?            TranslatedLyric { get; set; }
}