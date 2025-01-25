using Newtonsoft.Json;

namespace MusicAPI;

public interface ISearchParam
{

}
public interface ISearchParamBuilder
{
  public ISearchParam Build();
}

public interface IFetchResourceParam
{
}

public interface IFetchResourceParamBuilder
{
  public IFetchResourceParam Build();
}


public abstract class Song
{
  public required IMusicAPI Api { get; init; }
  public abstract string GetId();
  public abstract string GetName();
  public abstract Task<SongResource?> GetResource();
  public abstract List<Artist> GetArtists();
  public abstract Album GetAlbum();
  public abstract Task<Lyric?> GetLyric();

  public string ToDebugString()
  {
    return $"id: {GetId()}\n name: {GetName()}\n artists: {string.Join(", ", GetArtists().Select(artist => artist.ToDebugString()))}\n album: {GetAlbum().ToDebugString()}\n resource: {GetResource().GetAwaiter().GetResult()?.ToDebugString()}\n lyric: {GetLyric().GetAwaiter().GetResult()?.ToDebugString()}";
  }
}

public record SongResource
{
  public required string url { get; set; }

  public required string type { get; set; }
  public required double size { get; set; }
  public required double time { get; set; }



  public string ToDebugString()
  {
    return $"url: {url}, type: {type}, size: {size}, time: {time}";
  }
}

public record Artist
{
  public required string id { get; set; }
  public required string name { get; set; }

  public string ToDebugString()
  {
    return $"id: {id}, name: {name}";
  }
}

public record Album
{
  public required string id { get; set; }
  public required string name { get; set; }
  public required string pic { get; set; }

  public string ToDebugString()
  {
    return $"id: {id}, name: {name}, pic: {pic}";
  }
}

public record Lyric
{
  public required string id { get; set; }
  public required string originalLyric { get; set; }
  public required string translatedLyric { get; set; }

  public string ToDebugString()
  {
    return $"id: {id}, originalLyric: {originalLyric}, translatedLyric: {translatedLyric}";
  }
}
