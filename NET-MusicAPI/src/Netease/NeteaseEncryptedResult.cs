namespace MusicAPI.Netease;

internal sealed record NeteaseEncryptedResult(string Url, IReadOnlyDictionary<string, string> Body)
{
    public override string ToString()
    {
        var bodyParams = string.Join("&", Body.Select(kvp => $"{kvp.Key}={kvp.Value}"));
        return $"{Url}?{bodyParams}";
    }
}