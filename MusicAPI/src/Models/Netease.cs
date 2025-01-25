namespace MusicAPI;

public class NeteaseEncryptedResult
{
  public required string url { get; set; }
  public required Dictionary<string, string> body { get; set; }
}

public class NeteaseSong : Song
{
  public required string id { get; set; }
  public required string name { get; set; }
  public required List<Artist> artists { get; set; }
  public required Album album { get; set; }

  public override string GetId()
  {
    return this.id;
  }

  public override string GetName()
  {
    return this.name;
  }

  public override List<Artist> GetArtists()
  {
    return this.artists;
  }

  public override Album GetAlbum()
  {
    return this.album;
  }

  public override async Task<SongResource?> GetResource()
  {
    return await Api.GetSongResource(GetId());
  }

  public override async Task<Lyric?> GetLyric()
  {
    return await Api.GetLyric(GetId());
  }
}

public class NeteaseSearchParam : ISearchParam
{
  public int type { get; set; } = 1;
  public int limit { get; set; } = 30;
  public int page { get; set; } = 1;
}

public class NeteaseFetchResourceParam : IFetchResourceParam
{
  public int br { get; set; } = 320;
}

public class NeteaseSearchParamBuilder : ISearchParamBuilder
{
  private NeteaseSearchParam param = new();

  public NeteaseSearchParamBuilder type(int type)
  {
    param.type = type;
    return this;
  }

  public NeteaseSearchParamBuilder limit(int limit)
  {
    param.limit = limit;
    return this;
  }


  public NeteaseSearchParamBuilder page(int page)
  {
    param.page = page;
    return this;
  }

  public ISearchParam Build()
  {
    return param;
  }
}

public class NeteaseFetchResourceParamBuilder : IFetchResourceParamBuilder
{
  private NeteaseFetchResourceParam param = new();

  public NeteaseFetchResourceParamBuilder br(int br)
  {
    param.br = br;
    return this;
  }

  public IFetchResourceParam Build()
  {
    return param;
  }
}

