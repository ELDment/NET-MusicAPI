using HttpManager;
using Microsoft.VisualBasic.FileIO;
using Newtonsoft.Json;
using System;
using System.Net;

namespace MusicAPI {
	public class MusicAPI {
		private string? platform = string.Empty;
		private string[] platforms = new string[] { "NetEase", "TenCent" };
		private HttpRequest httpClient;
		private HttpCookie httpCookie;
		private NetEase.NetEaseAPI? netEase;
		private TenCent.TenCentAPI? tenCent;

		public MusicAPI(string platform = "NetEase") {
			if (!platforms.Contains(platform))
				throw new ArgumentException("不支持的平台");
			
			this.platform = platform;
			httpClient = new HttpRequest();
			httpCookie = new HttpCookie();

			switch (this.platform) {
				case "NetEase": {
					netEase = new NetEase.NetEaseAPI();
					break;
				}
				case "TenCent": {
					tenCent = new TenCent.TenCentAPI();
					break;
				}
			}
		}

		public void Cookie(Dictionary<string, string> keyValues) {
			httpCookie.SetCookie(this.platform ?? " ", keyValues);
			return;
		}

		public async Task<string> Search(string keyword, Dictionary<string, object>? option = null) {
			try {
				List<object> searchResult = new List<object>();
				List<string> artistResult = new List<string>();
				Dictionary<string, object> singleResult = new Dictionary<string, object>();

				searchResult.Clear();
				switch (this.platform) {
					case "NetEase": {
						HttpPackage package = netEase!.Search(keyword, (option is null ? new Dictionary<string, object>() : option));
						string? jsonReasult = await httpClient.SendPost(package.Url, package.Parameters, httpCookie.GetCookie("NetEase"));
						dynamic? json = JsonConvert.DeserializeObject(jsonReasult ?? "{}");
						if (string.IsNullOrWhiteSpace(jsonReasult) || json == null)
							break;

						foreach (dynamic? jsonData in json!.result!.songs) {
							if (jsonData == null)
								continue;
							artistResult = new List<string>();
							singleResult = new Dictionary<string, object>();

							singleResult["id"] = jsonData!.id;
							singleResult["name"] = jsonData!.name;
							foreach (dynamic? artist in jsonData!.ar) {
								artistResult.Add(artist.name!.ToString());
							}
							singleResult["artist"] = artistResult;
							singleResult["album"] = jsonData!.al!.name;
							singleResult["pic_id"] = jsonData!.al!.pic;
							singleResult["url_id"] = jsonData!.id;
							singleResult["lrc_id"] = jsonData!.id;
							searchResult.Add(singleResult);
						}
						break;
					}
				}

				return JsonConvert.SerializeObject(searchResult, Formatting.Indented) ?? string.Empty;
			} catch {
				return string.Empty;
			}
		}

		public async Task<string> Song(long id) {
			try {
				Dictionary<string, object> singleResult = new Dictionary<string, object>();
				List<string> artistResult = new List<string>();

				switch (this.platform) {
					case "NetEase": {
						HttpPackage package = netEase!.Song(id);
						string? jsonReasult = await httpClient.SendPost(package.Url, package.Parameters, httpCookie.GetCookie("NetEase"));
						dynamic? json = JsonConvert.DeserializeObject(jsonReasult ?? "{}");
						if (string.IsNullOrWhiteSpace(jsonReasult) || json == null)
							break;

						foreach (dynamic? jsonData in json!.songs) {
							if (jsonData == null)
								continue;

							singleResult = new Dictionary<string, object> ();
							singleResult["id"] = jsonData!.id;
							singleResult["name"] = jsonData!.name;
							foreach (dynamic? artist in jsonData!.ar) {
								artistResult.Add(artist.name!.ToString());
							}
							singleResult["artist"] = artistResult;
							singleResult["album"] = jsonData!.al!.name;
							singleResult["pic_id"] = jsonData!.al!.pic;
							singleResult["url_id"] = jsonData!.id;
							singleResult["lrc_id"] = jsonData!.id;
						}
						break;
					}
				}

				return JsonConvert.SerializeObject(singleResult, Formatting.Indented) ?? string.Empty;
			} catch {
				return string.Empty;
			}
		}

		public async Task<string> Url(long id, int br = 320) {
			try {
				Dictionary<string, object> singleResult = new Dictionary<string, object>();

				switch (this.platform) {
					case "NetEase": {
						HttpPackage package = netEase!.Url(id);
						string? jsonReasult = await httpClient.SendPost(package.Url, package.Parameters, httpCookie.GetCookie("NetEase"));
						dynamic? json = JsonConvert.DeserializeObject(jsonReasult ?? "{}");
						if (string.IsNullOrWhiteSpace(jsonReasult) || json == null)
							break;

						foreach (dynamic? jsonData in json!.data) {
							singleResult["url"] = jsonData!.url;
							singleResult["br"] = (double)(jsonData!.br / 1000.0);
							singleResult["size"] = (double)(jsonData!.size / 1024.0 / 1024.0);
							singleResult["md5"] = jsonData!.md5;
							singleResult["encodeType"] = jsonData!.encodeType;
							singleResult["time"] = (double)(jsonData!.time / 1000.0);
						}
						break;
					}
				}

				return JsonConvert.SerializeObject(singleResult, Formatting.Indented) ?? string.Empty;
			} catch {
				return string.Empty;
			}
		}

		public async Task<string> Lyric(long id) {
			try {
				Dictionary<string, object> singleResult = new Dictionary<string, object>();
				string? contentLrc = string.Empty;
				string? contentKLrc = string.Empty;
				string? contentTLrc = string.Empty;

				switch (this.platform) {
					case "NetEase": {
						HttpPackage package = netEase!.Lyric(id);
						string? jsonReasult = await httpClient.SendPost(package.Url, package.Parameters, httpCookie.GetCookie("NetEase"));
						dynamic? json = JsonConvert.DeserializeObject(jsonReasult ?? "{}");
						if (string.IsNullOrWhiteSpace(jsonReasult) || json == null)
							break;

						contentLrc = json!.lrc!.lyric!.ToString();
						contentKLrc = json!.klyric!.lyric?.ToString() ?? string.Empty;
						contentTLrc = json!.tlyric!.lyric?.ToString() ?? string.Empty;
						singleResult["Lrc"] = contentLrc;
						singleResult["K"] = contentKLrc;
						singleResult["T"] = contentTLrc;

						break;
					}
				}

				return JsonConvert.SerializeObject(singleResult, Formatting.Indented) ?? string.Empty;
			} catch {
				return string.Empty;
			}
		}
	}
}