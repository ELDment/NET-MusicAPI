using Newtonsoft.Json;

namespace MusicAPI;
public class SpotifyAPI {
	//https://developer.spotify.com/dashboard
	public SpotifyAPI(string ClientID, string ClientSecret) {
		this.ClientID = ClientID;
		this.ClientSecret = ClientSecret;
	}

	~SpotifyAPI() {
		if (HttpClient != null)
			HttpClient.Dispose();
	}
	private HttpClient HttpClient = new();
	private string ClientID, ClientSecret;
	private string? Token = string.Empty;
	private long expiredTime = -1;

	public Dictionary<string, string> Headers = new();

	private Dictionary<string, string> GetHeaders() {
		Dictionary<string, string> keyValues = new Dictionary<string, string> {
						{ "Referer", "https://developer.spotify.com" },
						{ "User-Agent", "Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/122.0.6261.95 Safari/537.36" },
						{ "Accept", "*/*" },
						{ "Accept-Language", "zh-CN,zh;q=0.8,gl;q=0.6,zh-TW;q=0.4" },
						{ "Connection", "keep-alive" },
						{ "Content-Type", "application/x-www-form-urlencoded" },
						{ "Authorization", this.Token ?? "" }
					};

		return keyValues;
	}

	public async Task<List<Song>?> Search(string keyword, int type = 1, int limit = 30, int page = 1) {
		if (!(await this.GetToken()))
			return null;

		Dictionary<string, string> requestParams = new Dictionary<string, string> {
				{ "q", keyword },
				{ "type", "track" },
				{ "limit", $"{limit}" }
		};

		string? jsonResult = await HttpClient.RequestAsync("GET", "https://api.spotify.com/v1/search", requestParams, GetHeaders());

		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		//Console.WriteLine("1");
		List<Song> searchResult = new();
		foreach (dynamic? jsonData in json!.tracks!.items) {
			if (jsonData == null)
				continue;
			var artists = new List<string>();

			foreach (dynamic? artist in jsonData!.artists) {
				if (artist == null)
					continue;
				artists.Add((string)artist.name);
			}

			Song singleResult = new Song
			{
				Id = jsonData!.id,
				Name = jsonData!.name,
				Artists = artists,
				AlbumName = jsonData!.album!.name,
				LyricId = jsonData!.id
			};
			searchResult.Add(singleResult);
		};

		return searchResult;
	}

	public async Task<Song?> GetSong(string id) {
		if (!(await this.GetToken()))
			return null;

		//Console.WriteLine(id);
		Dictionary<string, string> requestParams = new();
		string? jsonResult = await HttpClient.RequestAsync("GET", $"https://api.spotify.com/v1/tracks/{id}", requestParams, GetHeaders());

		//Console.WriteLine(jsonResult);
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		Dictionary<string, object> albumImage = new();
		List<Dictionary<string, object>> albumImages = new();
		List<string> artists = new();

		foreach (dynamic? artist in json!.artists) {
			if (artist.name == null)
				continue;
			artists.Add((string)artist.name ?? "");
		}

		foreach (dynamic? image in json!.album!.images) {
			if (image?.url == null)
				continue;
			albumImage = new();
			albumImage.Add("url", (string)image!.url!);
			albumImage.Add("height", (int)image!.height!);
			albumImage.Add("width", (int)image!.width!);
			albumImages.Add(albumImage);
		}

		string? pid = (albumImages.Count > 0) ? JsonConvert.SerializeObject(albumImages) : string.Empty;
		return new Song {
			Id = json!.id,
			Name = json!.name,
			Artists = artists,
			AlbumName = json!.album!.name,
			LyricId = json!.id,
			Picture = string.IsNullOrWhiteSpace(pid) ? string.Empty : pid
		};
	}

	private async Task<bool> GetToken() {
		long timeStamp = DateTimeOffset.Now.ToUnixTimeSeconds();
		if (expiredTime > timeStamp + 1)
			return true;

		Dictionary<string, string> requestParams = new Dictionary<string, string> {
				{ "grant_type", "client_credentials" },
				{ "client_id", this.ClientID },
				{ "client_secret", this.ClientSecret }
		};

		string? result = await HttpClient.RequestAsync("POST", "https://accounts.spotify.com/api/token", requestParams, GetHeaders());
		if (string.IsNullOrWhiteSpace(result))
			return false;

		dynamic? json = JsonConvert.DeserializeObject(result ?? "{}");
		if (json == null)
			return false;

		string? access_token = json?.access_token ?? string.Empty, token_type = json?.token_type ?? string.Empty;
		int expires_in = json?.expires_in ?? -1;
		if (expires_in <= 0 || string.IsNullOrWhiteSpace(access_token) || string.IsNullOrWhiteSpace(token_type))
			return false;

		expiredTime = timeStamp + expires_in;
		//Bearer 1POdFZRZbvb...qqillRxMr2z'
		this.Token = $"{token_type} {access_token}";
		return true;
	}
}