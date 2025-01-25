namespace MusicAPI;

public class NeteaseEncryptedResult
{
  public required string url { get; set; }
  public required Dictionary<string, string> body { get; set; }
}