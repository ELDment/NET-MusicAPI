using Newtonsoft.Json;

namespace MusicAPI;


public record Song
{
  public required string Id { get; set; }
  public required string Name { get; set; }
  public List<string> Artists { get; set; }
  public required string AlbumName { get; set; }
  public required string PicId { get; set; }
  public required string LyricId { get; set; }

}

public record SongResource
{
  public required string Url { get; set; }
  public required string Type { get; set; }
  public required int Br { get; set; }
  public required double Size { get; set; }
  public required double Duration { get; set; }
}

public record Artist
{
  public required string Name { get; set; }
  public required string Id { get; set; }
}

public record Album
{
  public required string Name { get; set; }
  public required string Pic { get; set; }
}

public record Lyric
{
  public required bool HasTimeInfo { get; set; }
  public required string OriginalLyric { get; set; }
  public required string TranslatedLyric { get; set; }
}