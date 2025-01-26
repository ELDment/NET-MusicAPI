using MusicAPI;
using Newtonsoft.Json;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
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


	public async Task<Lyric?> GetLyric(string id) {
		Dictionary<string, string> requestParams = new Dictionary<string, string> {
				{ "format", "json" },
				{ "platform", "yqq" },
				{ "songid", id }
		};
		
		string? jsonResult = await HttpClient.RequestAsync("GET", "https://c.y.qq.com/v8/fcg-bin/fcg_play_single_song.fcg", requestParams, GetHeaders());
		Console.WriteLine(jsonResult);
		//dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		//if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
		//	return null;
		//Regex regex = new Regex(@"\[(\d+:\d+.*)\]", RegexOptions.Compiled);
		//return new Lyric {
		//	HasTimeInfo = regex.IsMatch(json!.lrc!.lyric!.ToString()),
		//	OriginalLyric = json!.lrc!.lyric!.ToString(),
		//	TranslatedLyric = json!.tlyric?.lyric?.ToString() ?? string.Empty
		//};

		return null;
	}
}