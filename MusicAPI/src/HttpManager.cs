using Newtonsoft.Json;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Web;

namespace MusicAPI;

public static class HttpManager
{
	public static async Task<string?> RequestAsync(this HttpClient client, string method, string? url, Dictionary<string, string>? parameters, Dictionary<string, string> headers)
	{
		string responseBody = string.Empty;
		if (string.IsNullOrWhiteSpace(url))
			return string.Empty;
		if (parameters == null || parameters.Count == 0)
			return string.Empty;

		switch (method) {
			case "POST": {
				using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url)) {
					request.Content = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)parameters);
					foreach (var header in headers) {
						//Console.WriteLine($"{header.Key}: {header.Value}");
						request.Headers.TryAddWithoutValidation(header.Key, header.Value);
					}
					HttpResponseMessage response = await client.SendAsync(request);
					responseBody = await response.Content.ReadAsStringAsync();
				}
				break;
			}

			case "GET": {
				var queryString = HttpUtility.ParseQueryString(string.Empty);
				foreach (var parameter in parameters) {
					queryString[parameter.Key] = parameter.Value;
				}
				var urlWithQuery = $"{url}?{queryString.ToString()}";

				using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Get, urlWithQuery)) {
					foreach (var header in headers) {
						//Console.WriteLine($"{header.Key}: {header.Value}");
						request.Headers.TryAddWithoutValidation(header.Key, header.Value);
					}
					HttpResponseMessage response = await client.SendAsync(request);
					responseBody = await response.Content.ReadAsStringAsync();
				}
				break;
			}
		}

		return responseBody;
	}


	public static string GetRandomIP()
	{
		Random random = new Random();
		int ip = random.Next(1884815360, 1884890111);
		return new IPAddress(ip).ToString();
	}
}