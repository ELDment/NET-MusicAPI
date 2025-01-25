using Newtonsoft.Json;
using System.Net;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace MusicAPI;

public static class HttpManager
{
	public static async Task<string?> SendPost(this HttpClient client, string? url, Dictionary<string, string>? parameters, Dictionary<string, string> cookie)
	{
		if (parameters == null || parameters.Count == 0)
		{
			return string.Empty;
		}
		var content = new FormUrlEncodedContent((IEnumerable<KeyValuePair<string, string>>)parameters);

		if (string.IsNullOrWhiteSpace(url))
			return string.Empty;

		string responseBody = string.Empty;
		using (HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, url))
		{
			request.Content = content;
			foreach (var header in cookie)
			{
				//Console.WriteLine($"{header.Key}: {header.Value}");
				request.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}
			HttpResponseMessage response = await client.SendAsync(request);
			responseBody = await response.Content.ReadAsStringAsync();
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