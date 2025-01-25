namespace MusicAPI;

public interface IMusicAPI
{

  public Task<List<Song>?> Search(string keyword, ISearchParamBuilder? builder = null);

  public Task<Song?> GetSong(string id);

  public Task<SongResource?> GetSongResource(string id, IFetchResourceParamBuilder? builder = null);

  public Task<Lyric?> GetLyric(string id);

}
