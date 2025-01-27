using Newtonsoft.Json;
using System.Text.RegularExpressions;

namespace MusicAPI;
public class TencentAPI {
	~TencentAPI() {
		if (HttpClient != null)
			HttpClient.Dispose();
	}
	private HttpClient HttpClient = new();


	public Dictionary<string, string> Headers = new();

	private Dictionary<string, string> GetHeaders() {
		Dictionary<string, string> keyValues = new Dictionary<string, string> {
						{ "Referer", "http://y.qq.com" },
						{ "Cookie", "pgv_pvi=22038528; pgv_si=s3156287488; pgv_pvid=5535248600; yplayer_open=1; ts_last=y.qq.com/portal/player.html; ts_uid=4847550686; yq_index=0; qqmusic_fromtag=66; player_exist=1" },
						{ "User-Agent", "QQ%E9%9F%B3%E4%B9%90/54409 CFNetwork/901.1 Darwin/17.6.0 (x86_64)" },
						{ "Accept", "*/*" },
						{ "Accept-Language", "zh-CN,zh;q=0.8,gl;q=0.6,zh-TW;q=0.4" },
						{ "Connection", "keep-alive" },
						{ "Content-Type", "application/x-www-form-urlencoded" }
					};

		foreach (var header in Headers) {
			keyValues.TryAdd(header.Key, header.Value);
			if (keyValues.ContainsKey(header.Key))
				keyValues[header.Key] = header.Value;
			//Console.WriteLine($"{keyValues[header.Key]}: {keyValues[header.Value]}");
		}
		return keyValues;
	}

	public async Task<List<Song>?> Search(string keyword, int type = 1, int limit = 30, int page = 1) {
		Dictionary<string, string> requestParams = new Dictionary<string, string> {
						{ "w", keyword },
						{ "p", page.ToString() },
						{ "n", limit.ToString() },
						{ "aggr", "1" },
						{ "lossless", "1" },
						{ "cr", "1" },
						{ "new_json", "1" },
						{ "format", "json" }
		};

		string? jsonResult = await HttpClient.RequestAsync("GET", "https://c.y.qq.com/soso/fcgi-bin/client_search_cp", requestParams, GetHeaders());

		//Console.WriteLine(jsonResult);
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		//Console.WriteLine("1");
		List<Song> searchResult = new();
		foreach (dynamic? jsonData in json!.data!.song!.list) {
			if (jsonData == null)
				continue;
			var artists = new List<string>();

			foreach (dynamic? artist in jsonData!.singer) {
				if (artist == null)
					continue;
				artists.Add((string)artist.name);
			}

			Song singleResult = new Song
			{
				Id = jsonData!.mid,
				Name = jsonData!.name,
				Artists = artists,
				AlbumName = jsonData!.album!.title,
				LyricId = jsonData!.mid
			};
			searchResult.Add(singleResult);
		};

		return searchResult;

	}

	public async Task<Song?> GetSong(string id) {
		Dictionary<string, string> requestParams = new Dictionary<string, string> {
				{ "format", "json" },
				{ "platform", "yqq" },
				{ "songmid", id }
		};

		string? jsonResult = await HttpClient.RequestAsync("GET", "https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg", requestParams, GetHeaders());

		//Console.WriteLine(jsonResult);
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		foreach (dynamic? jsonData in json!.data) {
			if (jsonData == null)
				continue;

			List<string> artists = new List<string>();

			foreach (dynamic? artist in jsonData!.singer) {
				if (artist.name == null)
					continue;
				artists.Add((string)artist.name ?? "");
			}

			string? pid = jsonData!.album!.pmid;
			return new Song {
				Id = jsonData!.mid,
				Name = jsonData!.name,
				Artists = artists,
				AlbumName = jsonData!.album!.title,
				LyricId = jsonData!.mid,
				Picture = string.IsNullOrWhiteSpace(pid) ? string.Empty : pid
			};
		}

		return null;
	}

	public async Task<SongResource?> GetSongResource(string id, int br = 320) {
		Dictionary<string, string> requestParams = new Dictionary<string, string> {
				{ "format", "json" },
				{ "platform", "yqq" },
				{ "songmid", id }
		};

		string? jsonResult = await HttpClient.RequestAsync("GET", "https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg", requestParams, GetHeaders());
		//Console.WriteLine(jsonResult);
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		Dictionary<string, object>? songResource = await TencentUrl(jsonResult!, br);
		if (songResource == null)
			return null;

		return new SongResource {
			Size = (double)((int)songResource["size"] / 1024.0 / 1024.0),
			Br = br,
			Duration = 0.0,
			Type = string.IsNullOrWhiteSpace((string)songResource["fileName"]) ? string.Empty : (
				((string)songResource["fileName"]).EndsWith("mp3", StringComparison.OrdinalIgnoreCase) ? "mp3" : (
					((string)songResource["fileName"]).EndsWith("m4a", StringComparison.OrdinalIgnoreCase) ? "m4a" : (
						((string)songResource["fileName"]).EndsWith("flac", StringComparison.OrdinalIgnoreCase) ? "flac" : string.Empty
					)
				)
			),
			Url = string.IsNullOrWhiteSpace((string)songResource["url"]) ? string.Empty : (string)songResource["url"]
		};
	}

	public async Task<Lyric?> GetLyric(string id) {
		Dictionary<string, string> requestParams = new Dictionary<string, string> {
				{ "songmid", id },
				{ "g_tk", "5381" }
		};

		string? jsonResult = await HttpClient.RequestAsync("GET", "https://c.y.qq.com/lyric/fcgi-bin/fcg_query_lyric_new.fcg", requestParams, GetHeaders());
		//Console.WriteLine(jsonResult);
		if (string.IsNullOrWhiteSpace(jsonResult))
			return null;

		Match match = Regex.Match(jsonResult!, @"MusicJsonCallback\((.*?)\)");
		if (!match.Success)
			return null;

		string? content = match.Groups[1].Value;
		dynamic? json = JsonConvert.DeserializeObject(content ?? "{}");
		if (json == null)
			return null;

		string? originalLyric = json!.lyric!.ToString(), translatedLyric = json!.trans?.ToString();
		if (string.IsNullOrWhiteSpace(originalLyric))
			return null;

		originalLyric = System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(originalLyric));

		Regex regex = new Regex(@"\[(\d+:\d+.*)\]", RegexOptions.Compiled);
		return new Lyric {
			HasTimeInfo = regex.IsMatch(originalLyric),
			OriginalLyric = originalLyric,
			TranslatedLyric = String.IsNullOrWhiteSpace(translatedLyric) ? string.Empty : System.Text.Encoding.UTF8.GetString(Convert.FromBase64String(translatedLyric))
		};
	}

	public async Task<string?> GetPicture(string id, int px = 300) {
		Song? songInfo = await GetSong(id);
		if (songInfo == null)
			return string.Empty;
		string? pid = songInfo.Picture;
		if (string.IsNullOrWhiteSpace(pid))
			return string.Empty;

		return $"https://y.qq.com/music/photo_new/T002R{FindClosestValue(px)}x{FindClosestValue(px)}M000{pid}.jpg?max_age=2592000";
	}

	private static int FindClosestValue(int target) {
		int[] array = { 58, 68, 300, 500 };
		int left = 0;
		int right = array.Length - 1;
		int closestValue = array[0];

		while (left <= right) {
			int mid = left + (right - left) / 2;

			if (array[mid] == target)
				return array[mid];

			if (Math.Abs(array[mid] - target) < Math.Abs(closestValue - target))
				closestValue = array[mid];

			if (array[mid] < target)
				left = mid + 1;
			else
				right = mid - 1;
		}

		return closestValue;
	}

	private async Task<Dictionary<string, object>?> TencentUrl(string result, int br) {
		var data = JsonConvert.DeserializeObject<Dictionary<string, object>>(result);
		var random = new Random();
		var guid = random.Next(0, 100000) * 1000000L + random.Next(0, 100000);

		var types = new List<Tuple<string, int, string, string>>(){
			new Tuple<string, int, string, string>("size_flac", 999, "F000", "flac"),
			new Tuple<string, int, string, string>("size_320mp3", 320, "M800", "mp3"),
			new Tuple<string, int, string, string>("size_192aac", 192, "C600", "m4a"),
			new Tuple<string, int, string, string>("size_128mp3", 128, "M500", "mp3"),
			new Tuple<string, int, string, string>("size_96aac", 96, "C400", "m4a"),
			new Tuple<string, int, string, string>("size_48aac", 48, "C200", "m4a"),
			new Tuple<string, int, string, string>("size_24aac", 24, "C100", "m4a")
		};

		var match = Regex.Match(GetHeaders()["Cookie"], @"uin=(\d+)");
		string uin = match.Success ? match.Groups[1].Value : "0";

		var songData = (data!["data"] as Newtonsoft.Json.Linq.JArray)?[0];
		if (songData == null)
			return null;

		var songmid = songData["mid"]!.ToString();
		var songtype = songData["type"]!.ToString();
		var mediaMid = songData["file"]!["media_mid"]!.ToString();


		var payload = new {
			req_0 = new {
				module = "vkey.GetVkeyServer",
				method = "CgiGetVkey",
				param = new {
					guid = guid.ToString(),
					songmid = new List<string> { songmid },
					filename = types.Select(type => type.Item3 + mediaMid + "." + type.Item4).ToList(),
					songtype = new List<int> { int.Parse(songtype) },
					uin = uin,
					loginflag = 1,
					platform = "20"
				}
			}
		};

		var requestParams = new Dictionary<string, string> {
			{ "format", "json" },
			{ "platform", "yqq.json" },
			{ "needNewCode", "0" },
			{ "data", JsonConvert.SerializeObject(payload) }
		};

		string? responseContent = await HttpClient.RequestAsync("GET", "https://u.y.qq.com/cgi-bin/musicu.fcg", requestParams, GetHeaders());
		if (string.IsNullOrWhiteSpace(responseContent))
			return null;

		var jsonResponse = JsonConvert.DeserializeObject<Dictionary<string, object>>(responseContent);
		var vkeys = jsonResponse?["req_0"] as Newtonsoft.Json.Linq.JObject;
		var midurlinfo = vkeys?["data"]?["midurlinfo"] as Newtonsoft.Json.Linq.JArray;

		if (midurlinfo == null)
			return null;

		Dictionary<string, object> songResource = new Dictionary<string, object> {
			{ "url", string.Empty },
			{ "fileName", string.Empty },
			{ "size", 0 }
		};


		foreach (var (type, index) in types.Select((value, index) => (value, index))) {
			var vkey = midurlinfo[index]?["vkey"]?.ToString();
			if (!string.IsNullOrEmpty(vkey) && int.Parse(songData["file"]![type.Item1]!.ToString()) <= br) {
				songResource["url"] = vkeys!["data"]!["sip"]![0]!.ToString() + midurlinfo[index]!["purl"]!.ToString();
				songResource["fileName"] = midurlinfo[index]!["filename"]!.ToString();
				songResource["size"] = songData["file"]![type.Item1]!.ToString();
				break;
			}
		}

		return songResource;
	}
}