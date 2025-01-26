using Newtonsoft.Json;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace MusicAPI;
public class NeteaseEncryptedResult
{
	public required string url { get; set; }
	public required Dictionary<string, string> body { get; set; }
}

public class NeteaseAPI
{
	~NeteaseAPI()
	{
		if (HttpClient != null)
			HttpClient.Dispose();
	}
	private HttpClient HttpClient = new();


	public Dictionary<string, string> Headers = new();

	private Dictionary<string, string> GetHeaders()
	{
		//string cookieString = string.Join("; ", Cookies.Select(cookie => $"{cookie.Key}={cookie.Value}"));
		Dictionary<string, string> keyValues = new Dictionary<string, string> {
						{ "Referer", "https://music.163.com/" },
						{ "Cookie", "appver=8.2.30; os=iPhone OS; osver=15.0; EVNSM=1.0.0; buildver=2206; channel=distribution; machineid=iPhone13.3" },
						{ "User-Agent", "Mozilla/5.0 (iPhone; CPU iPhone OS 15_0 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Mobile/15E148 CloudMusic/0.1.1 NeteaseMusic/8.2.30" },
						{ "X-Real-IP", HttpManager.GetRandomIP() },
						{ "Accept", "*/*" },
						{ "Accept-Language", "zh-CN,zh;q=0.8,gl;q=0.6,zh-TW;q=0.4" },
						{ "Connection", "keep-alive" },
						{ "Content-Type", "application/x-www-form-urlencoded" }
					};

		foreach (var header in Headers)
		{
			keyValues.TryAdd(header.Key, header.Value);
			if (keyValues.ContainsKey(header.Key))
				keyValues[header.Key] = header.Value;
			//Console.WriteLine($"{keyValues[header.Key]}: {keyValues[header.Value]}");
		}
		return keyValues;
	}

	public async Task<List<Song>?> Search(string keyword, int type = 1, int limit = 30, int page = 1)
	{
		Dictionary<string, object> apiRequest = new Dictionary<string, object> {
				{ "url", "http://music.163.com/api/cloudsearch/pc" },
				{ "body", new Dictionary<string, object> {
						{ "s", string.Concat(keyword.Select(c => $@"\u{(int)c:X4}")).ToLower() },
						{ "type", type },
						{ "limit", limit },
						{ "total", "true" },
						{ "offset", (page > 0 && limit > 0) ? (page - 1) * limit : 0 }
					}
				}
			};
		NeteaseEncryptedResult encryptedRequest = NeteaseCrypto.NetEaseAESCBC(apiRequest);


		string? jsonResult = await HttpClient.RequestAsync("POST", encryptedRequest.url, encryptedRequest.body, GetHeaders());
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		List<Song> searchResult = new();
		foreach (dynamic? jsonData in json!.result!.songs)
		{
			if (jsonData == null)
				continue;
			var artists = new List<string>();

			foreach (dynamic? artist in jsonData!.ar)
			{
				artists.Add((string)artist.name);
			}
			Song singleResult = new Song
			{
				Id = jsonData!.id,
				Name = jsonData!.name,
				Artists = artists,
				AlbumName = jsonData!.al!.name,
				LyricId = jsonData!.id
			};
			searchResult.Add(singleResult);
		}
		;

		// singleResult["id"] = jsonData!.id;
		// singleResult["name"] = jsonData!.name;
		// singleResult["artist"] = artistResult;
		// singleResult["album"] = jsonData!.al!.name;
		// singleResult["pic_id"] = jsonData!.al!.pic;
		// singleResult["url_id"] = jsonData!.id;
		// singleResult["lrc_id"] = jsonData!.id;

		return searchResult;
	}

	public async Task<Song?> GetSong(string id)
	{
		Dictionary<string, object> apiRequest = new Dictionary<string, object> {
				{ "url", "http://music.163.com/api/v3/song/detail/" },
				{ "body", new Dictionary<string, object> {
						{ "c", $"[{{\"id\":'{id}',\"v\":0}}]" }
					}
				}
			};
		NeteaseEncryptedResult encryptedRequest = NeteaseCrypto.NetEaseAESCBC(apiRequest);

		string? jsonResult = await HttpClient.RequestAsync("POST", encryptedRequest.url, encryptedRequest.body, GetHeaders());
		//Console.WriteLine(jsonResult);
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		foreach (dynamic? jsonData in json!.songs)
		{
			if (jsonData == null)
				continue;

			List<string> artists = new List<string>();

			foreach (dynamic? artist in jsonData!.ar) {
				if (artist.name == null)
					continue;
				artists.Add((string)artist.name ?? "");
			}
			
			string? url = jsonData!.al!.picUrl;
			return new Song {
				Id = jsonData!.id,
				Name = jsonData!.name,
				Artists = artists,
				AlbumName = jsonData!.al!.name,
				LyricId = jsonData!.id,
				Picture = string.IsNullOrWhiteSpace(url) ? string.Empty : url
			};
		}
		return null;
	}

	public async Task<SongResource?> GetSongResource(string id, int br = 320)
	{
		Dictionary<string, object> apiRequest = new Dictionary<string, object> {
				{ "url", "http://music.163.com/api/song/enhance/player/url" },
				{ "body", new Dictionary<string, object> {
						{ "ids", $"[{id}]" },
						{ "br", br * 1000 }
					}
				}
			};
		NeteaseEncryptedResult encryptedRequest = NeteaseCrypto.NetEaseAESCBC(apiRequest);

		string? jsonResult = await HttpClient.RequestAsync("POST", encryptedRequest.url, encryptedRequest.body, GetHeaders());
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		var data = json!.data[0];
		return new SongResource
		{
			Url = data!.url,
			Type = data!.type,
			Br = (int)(data!.br / 1000.0),
			Size = data!.size,
			Duration = (double)(data!.time / 1000.0)
		};

	}

	public async Task<Lyric?> GetLyric(string id)
	{
		Dictionary<string, object> apiRequest = new Dictionary<string, object> {
				{ "url", "http://music.163.com/api/song/lyric" },
				{ "body", new Dictionary<string, object> {
						{ "id", id },
						{ "os", "linux" },
						{ "lv", -1 },
						{ "kv", -1 },
						{ "tv", -1 }
					}
				}
			};
		NeteaseEncryptedResult encryptedRequest = NeteaseCrypto.NetEaseAESCBC(apiRequest);

		string? jsonResult = await HttpClient.RequestAsync("POST", encryptedRequest.url, encryptedRequest.body, GetHeaders());
		//Console.WriteLine(jsonResult);
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;
		Regex regex = new Regex(@"\[(\d+:\d+.*)\]", RegexOptions.Compiled);
		return new Lyric
		{
			HasTimeInfo = regex.IsMatch(json!.lrc!.lyric!.ToString()),
			OriginalLyric = json!.lrc!.lyric!.ToString(),
			TranslatedLyric = json!.tlyric?.lyric?.ToString() ?? string.Empty
		};
	}

	public async Task<string?> GetPicture(string id, int px = 400) {
		Song? songInfo = await GetSong(id);
		if (songInfo == null)
			return string.Empty;
		string? url = songInfo.Picture;
		if (string.IsNullOrWhiteSpace(url))
			return string.Empty;

		return $"{url}?param={px}y{px}";
	}
}


public static class NeteaseCrypto
{
	private static readonly string Modulus = "157794750267131502212476817800345498121872783333389747424011531025366277535262539913701806290766479189477533597854989606803194253978660329941980786072432806427833685472618792592200595694346872951301770580765135349259590167490536138082469680638514416594216629258349130257685001248172188325316586707301643237607";
	private static readonly string PubKey = "65537";
	private static readonly string Nonce = "0CoJUm6Qyw8W8jud";
	private static readonly string IV = "0102030405060708";

	public static NeteaseEncryptedResult NetEaseAESCBC(Dictionary<string, object> api)
	{
		string skey = GetRandomHex(16) ?? "B3v3kH4vRPWRJFfH";

		string body = JsonConvert.SerializeObject(api["body"]);
		body = EncryptAES(body.Replace("\\\\", "\\"), Nonce, IV);
		body = EncryptAES(body, skey, IV);

		string encryptedSkey = SecretKey(skey, PubKey, Modulus);

		NeteaseEncryptedResult encryptedResult = new NeteaseEncryptedResult
		{
			url = api["url"].ToString()!.Replace("/api/", "/weapi/"),
			body = new Dictionary<string, string> {
				{ "params", body },
				{ "encSecKey", encryptedSkey.ToLower() }
			}
		};

		return encryptedResult;
	}

	private static string GetRandomHex(int length)
	{
		using (var rng = RandomNumberGenerator.Create())
		{
			byte[] bytes = new byte[length / 2];
			rng.GetBytes(bytes);
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}
	}

	private static string EncryptAES(string plainText, string key, string iv)
	{
		byte[] keyBytes = Encoding.UTF8.GetBytes(key);
		byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

		if (keyBytes.Length != 16)
			throw new ArgumentException("AES-128-CBC的KEY必须为16字节长");
		if (ivBytes.Length != 16)
			throw new ArgumentException("AES-128-CBC的IV必须为16字节长");

		using (Aes aes = Aes.Create())
		{
			aes.Key = keyBytes;
			aes.IV = ivBytes;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
			byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
			using (MemoryStream ms = new MemoryStream())
			{
				using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write))
				{
					cs.Write(plainBytes, 0, plainBytes.Length);
					cs.FlushFinalBlock();
				}
				byte[] encryptedBytes = ms.ToArray();
				return Convert.ToBase64String(encryptedBytes);
			}
		}
	}

	private static string SecretKey(string skey, string pubkey, string modulus)
	{
		skey = ReverseString(skey);
		skey = bchexdec(str2hex(skey));
		skey = bcpowmod(skey, pubkey, modulus);

		BigInteger bigIntValue = BigInteger.Parse(skey);
		skey = bigIntValue.ToString("X").ToLower();
		if (skey.Length == 257)
			skey = skey.Substring(1);

		skey = skey.PadLeft(256, '0');
		return skey;
	}

	private static string ReverseString(string input)
	{
		char[] charArray = input.ToCharArray();
		Array.Reverse(charArray);
		return new string(charArray);
	}

	private static string bchexdec(string hex)
	{
		BigInteger result = 0;
		int length = hex.Length;
		for (int i = 0; i < length; i++)
		{
			char hexChar = hex[i];
			BigInteger digitValue = HexCharToValue(hexChar);
			BigInteger power = BigInteger.Pow(16, length - i - 1);
			result += digitValue * power;
		}

		return result.ToString();
	}

	private static string str2hex(string input)
	{
		StringBuilder hexBuilder = new StringBuilder();
		foreach (char c in input)
		{
			string hexCode = ((int)c).ToString("X2");
			hexBuilder.Append(hexCode);
		}
		return hexBuilder.ToString();
	}

	private static string bcpowmod(string input, string pubkey, string modulus)
	{
		BigInteger skeyBigInt = BigInteger.Parse(input);
		BigInteger pubkeyBigInt = BigInteger.Parse(pubkey);
		BigInteger modulusBigInt = BigInteger.Parse(modulus);
		BigInteger result = BigInteger.ModPow(skeyBigInt, pubkeyBigInt, modulusBigInt);

		return result.ToString();
	}

	private static BigInteger HexCharToValue(char hexChar)
	{
		if (hexChar >= '0' && hexChar <= '9')
		{
			return hexChar - '0';
		}
		else if (hexChar >= 'a' && hexChar <= 'f')
		{
			return 10 + hexChar - 'a';
		}
		else if (hexChar >= 'A' && hexChar <= 'F')
		{
			return 10 + hexChar - 'A';
		}
		else
		{
			throw new ArgumentException($"无效HEX字符: {hexChar}");
		}
	}
}