using Newtonsoft.Json;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace HttpManager {

	public struct HttpPackage {
		public string? Url { get; set; }
		public Dictionary<string, string>? Parameters { get; set; }
		
		public HttpPackage(string? url = null, Dictionary<string, string>? parameters = null) {
			this.Url = url;
			this.Parameters = parameters;
		}
	}

	public class HttpRequest {
		private string platform;
		private HttpClient httpClient;
		

		public HttpRequest(string platform = "NetEase") {
			this.platform = platform;
			this.httpClient = new HttpClient();
		}

		~HttpRequest() {
			if (this.httpClient != null)
				this.httpClient.Dispose();
		}

		public async Task<string?> SendPost(string? url, Dictionary<string, string>? parameters, Dictionary<string, string> cookie) {
			if (parameters == null || parameters.Count == 0) {
				return string.Empty;
			}
			var content = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)parameters);

			if (string.IsNullOrWhiteSpace(url))
				return string.Empty;

			string responseBody = string.Empty;
			using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)) {
				request.Content = content;
				foreach (var header in cookie) {
					//Console.WriteLine($"{header.Key}: {header.Value}");
					request.Headers.TryAddWithoutValidation(header.Key, header.Value);
				}
				HttpResponseMessage response = await this.httpClient.SendAsync(request);
				responseBody = await response.Content.ReadAsStringAsync();
			}

			return responseBody;
		}

			
	}

	public class HttpCookie {
		private Dictionary<string, Dictionary<string, string>> keyValuesList;

		public HttpCookie() {
			this.keyValuesList = new Dictionary<string, Dictionary<string, string>>();
		}

		public void SetCookie(string platform, Dictionary<string, string> keyValues) {
			this.keyValuesList.TryAdd(platform, new Dictionary<string, string>());
			this.keyValuesList[platform].Clear();
			this.keyValuesList[platform] = keyValues;
			return;
		}

		public Dictionary<string, string> GetCookie(string platform) {
			Dictionary<string, string> keyValues = new Dictionary<string, string>();
			switch (platform) {
				case "NetEase": {
					keyValues = new Dictionary<string, string> {
						{ "Referer", "https://music.163.com/" },
						{ "Cookie", "appver=8.2.30; os=iPhone OS; osver=15.0; EVNSM=1.0.0; buildver=2206; channel=distribution; machineid=iPhone13.3" },
						{ "User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 CloudMusic/0.1.1 NeteaseMusic/8.2.30" },
						{ "X-Real-IP", GetRandomIP() },
						{ "Accept", "*/*" },
						{ "Accept-Language", "zh-CN,zh;q=0.8,gl;q=0.6,zh-TW;q=0.4" },
						{ "Connection", "keep-alive" },
						{ "Content-Type", "application/x-www-form-urlencoded" }
					};
					break;
				}
			}

			foreach (var keyValue in this.keyValuesList[platform]) {
				keyValues.TryAdd(keyValue.Key, keyValue.Value);
				if (this.keyValuesList[platform].ContainsKey(keyValue.Key))
					keyValues[keyValue.Key] = keyValue.Value;
			}
			return keyValues;
		}

		private string GetRandomIP() {
			Random random = new Random();
			int ip = random.Next(1884815360, 1884890111);
			return new IPAddress(ip).ToString();
		}
	}
}