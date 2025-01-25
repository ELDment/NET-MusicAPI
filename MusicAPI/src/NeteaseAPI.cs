using Newtonsoft.Json;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace MusicAPI;
public class NeteaseAPI : IMusicAPI {

	public Dictionary<string, string> Headers = new();

	private HttpClient HttpClient = new();

	private Dictionary<string, string> GetHeader() {
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

		foreach (var header in Headers) {
			keyValues.TryAdd(header.Key, header.Value);
			if (keyValues.ContainsKey(header.Key))
				keyValues[header.Key] = header.Value;
			//Console.WriteLine($"{keyValues[header.Key]}: {keyValues[header.Value]}");
		}
		return keyValues;
	}

	public async Task<List<Song>?> Search(string keyword, ISearchParamBuilder? builder = null) {
		builder = builder ?? new NeteaseSearchParamBuilder();
		NeteaseSearchParam param = (NeteaseSearchParam)builder.Build();
		Dictionary<string, object> apiRequest = new Dictionary<string, object> {
				{ "url", "http://music.163.com/api/cloudsearch/pc" },
				{ "body", new Dictionary<string, object> {
						{ "s", string.Concat(keyword.Select(c => $@"\u{(int)c:X4}")).ToLower() },
						{ "type", param.type },
						{ "limit", param.limit },
						{ "total", "true" },
						{ "offset", (param.page > 0 && param.limit > 0) ? (param.page - 1) * param.limit : 0 }
					}
				}
			};
		NeteaseEncryptedResult encryptedRequest = NeteaseCrypto.NetEaseAESCBC(apiRequest);

		string? jsonResult = await HttpClient.SendPost(encryptedRequest.url, encryptedRequest.body, GetHeader());
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		List<Song> searchResult = new();
		foreach (dynamic? jsonData in json!.result!.songs) {
			if (jsonData == null)
				continue;
			var artists = new List<Artist>();

			foreach (dynamic? artist in jsonData!.ar) {
				artists.Add(new Artist {
					id = artist.id,
					name = artist.name
				});
			}
			Song singleResult = new NeteaseSong
			{
				Api = this,
				id = jsonData!.id,
				name = jsonData!.name,
				artists = artists,
				album = new Album
				{
					id = jsonData!.al!.id,
					name = jsonData!.al!.name,
					pic = jsonData!.al!.pic
				},
			};

			// singleResult["id"] = jsonData!.id;
			// singleResult["name"] = jsonData!.name;
			// singleResult["artist"] = artistResult;
			// singleResult["album"] = jsonData!.al!.name;
			// singleResult["pic_id"] = jsonData!.al!.pic;
			// singleResult["url_id"] = jsonData!.id;
			// singleResult["lrc_id"] = jsonData!.id;
			searchResult.Add(singleResult);
		}

		return searchResult;
	}

	public async Task<Song?> GetSong(string id) {
		Dictionary<string, object> apiRequest = new Dictionary<string, object> {
				{ "url", "http://music.163.com/api/v3/song/detail/" },
				{ "body", new Dictionary<string, object> {
						{ "c", $"[{{\"id\":'{id}',\"v\":0}}]" }
					}
				}
			};
		NeteaseEncryptedResult encryptedRequest = NeteaseCrypto.NetEaseAESCBC(apiRequest);

		string? jsonReasult = await HttpClient.SendPost(encryptedRequest.url, encryptedRequest.body, GetHeader());
		dynamic? json = JsonConvert.DeserializeObject(jsonReasult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonReasult) || json == null)
			return null;

		foreach (dynamic? jsonData in json!.songs) {
			if (jsonData == null)
				continue;

			List<Artist> artists = new List<Artist>();
			foreach (dynamic? artist in jsonData!.ar) {
				artists.Add(new Artist {
					id = artist.id,
					name = artist.name
				});
			}
			return new NeteaseSong {
				Api = this,
				id = jsonData!.id,
				name = jsonData!.name,
				artists = artists,
				album = new Album {
					id = jsonData!.al!.id,
					name = jsonData!.al!.name,
					pic = jsonData!.al!.pic
				},
			};
			// {
			// 	artistResult.Add(artist.name!.ToString());
			// }
			// singleResult["artist"] = artistResult;
			// singleResult["album"] = jsonData!.al!.name;
			// singleResult["pic_id"] = jsonData!.al!.pic;
			// singleResult["url_id"] = jsonData!.id;
			// singleResult["lrc_id"] = jsonData!.id;
		}
		return null;
	}

	public async Task<SongResource?> GetSongResource(string id, IFetchResourceParamBuilder? builder = null) {
		builder = builder ?? new NeteaseFetchResourceParamBuilder();
		NeteaseFetchResourceParam param = (NeteaseFetchResourceParam)builder.Build();
		Dictionary<string, object> apiRequest = new Dictionary<string, object> {
				{ "url", "http://music.163.com/api/song/enhance/player/url" },
				{ "body", new Dictionary<string, object> {
						{ "ids", $"[{id}]" },
						{ "br", param.br * 1000 }
					}
				}
			};
		NeteaseEncryptedResult encryptedRequest = NeteaseCrypto.NetEaseAESCBC(apiRequest);

		string? jsonResult = await HttpClient.SendPost(encryptedRequest.url, encryptedRequest.body, GetHeader());
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		var data = json!.data[0];
		Console.WriteLine(data);
		SongResource resource = new SongResource
		{
			url = data!.url,
			type = data!.type,
			// br = (double)(json!.data!.br / 1000.0),
			size = data!.size,
			// md5 = json!.data!.md5,
			// encodeType = json!.data!.encodeType,
			time = (double)(data!.time / 1000.0)
		};

		// foreach (dynamic? jsonData in json!.data)
		// {
		// 	singleResult["url"] = jsonData!.url;
		// 	singleResult["br"] = (double)(jsonData!.br / 1000.0);
		// 	singleResult["size"] = (double)(jsonData!.size / 1024.0 / 1024.0);
		// 	singleResult["md5"] = jsonData!.md5;
		// 	singleResult["encodeType"] = jsonData!.encodeType;
		// 	singleResult["time"] = (double)(jsonData!.time / 1000.0);
		// }
		return resource;
	}

	public async Task<Lyric?> GetLyric(string id) {
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

		string? jsonResult = await HttpClient.SendPost(encryptedRequest.url, encryptedRequest.body, GetHeader());
		dynamic? json = JsonConvert.DeserializeObject(jsonResult ?? "{}");
		if (string.IsNullOrWhiteSpace(jsonResult) || json == null)
			return null;

		Lyric lyric = new Lyric
		{
			id = id,
			originalLyric = json!.lrc!.lyric!.ToString(),
			translatedLyric = json!.tlyric!.lyric?.ToString() ?? string.Empty
		};

		return lyric;
		// contentTLrc = json!.tlyric!.lyric?.ToString() ?? string.Empty;
		// singleResult["Lrc"] = contentLrc;
		// singleResult["K"] = contentKLrc;
		// singleResult["T"] = contentTLrc;

		// break;
	}
}


public static class NeteaseCrypto {
	private static readonly string Modulus = "157794750267131502212476817800345498121872783333389747424011531025366277535262539913701806290766479189477533597854989606803194253978660329941980786072432806427833685472618792592200595694346872951301770580765135349259590167490536138082469680638514416594216629258349130257685001248172188325316586707301643237607";
	private static readonly string PubKey = "65537";
	private static readonly string Nonce = "0CoJUm6Qyw8W8jud";
	private static readonly string IV = "0102030405060708";

	public static NeteaseEncryptedResult NetEaseAESCBC(Dictionary<string, object> api) {
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

	private static string GetRandomHex(int length) {
		using (var rng = RandomNumberGenerator.Create()) {
			byte[] bytes = new byte[length / 2];
			rng.GetBytes(bytes);
			return BitConverter.ToString(bytes).Replace("-", "").ToLower();
		}
	}

	private static string EncryptAES(string plainText, string key, string iv) {
		byte[] keyBytes = Encoding.UTF8.GetBytes(key);
		byte[] ivBytes = Encoding.UTF8.GetBytes(iv);

		if (keyBytes.Length != 16)
			throw new ArgumentException("AES-128-CBC的KEY必须为16字节长");
		if (ivBytes.Length != 16)
			throw new ArgumentException("AES-128-CBC的IV必须为16字节长");

		using (Aes aes = Aes.Create()) {
			aes.Key = keyBytes;
			aes.IV = ivBytes;
			aes.Mode = CipherMode.CBC;
			aes.Padding = PaddingMode.PKCS7;

			ICryptoTransform encryptor = aes.CreateEncryptor(aes.Key, aes.IV);
			byte[] plainBytes = Encoding.UTF8.GetBytes(plainText);
			using (MemoryStream ms = new MemoryStream()) {
				using (CryptoStream cs = new CryptoStream(ms, encryptor, CryptoStreamMode.Write)) {
					cs.Write(plainBytes, 0, plainBytes.Length);
					cs.FlushFinalBlock();
				}
				byte[] encryptedBytes = ms.ToArray();
				return Convert.ToBase64String(encryptedBytes);
			}
		}
	}

	private static string SecretKey(string skey, string pubkey, string modulus) {
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

	private static string ReverseString(string input) {
		char[] charArray = input.ToCharArray();
		Array.Reverse(charArray);
		return new string(charArray);
	}

	private static string bchexdec(string hex) {
		BigInteger result = 0;
		int length = hex.Length;
		for (int i = 0; i < length; i++) {
			char hexChar = hex[i];
			BigInteger digitValue = HexCharToValue(hexChar);
			BigInteger power = BigInteger.Pow(16, length - i - 1);
			result += digitValue * power;
		}

		return result.ToString();
	}

	private static string str2hex(string input) {
		StringBuilder hexBuilder = new StringBuilder();
		foreach (char c in input) {
			string hexCode = ((int)c).ToString("X2");
			hexBuilder.Append(hexCode);
		}
		return hexBuilder.ToString();
	}

	private static string bcpowmod(string input, string pubkey, string modulus) {
		BigInteger skeyBigInt = BigInteger.Parse(input);
		BigInteger pubkeyBigInt = BigInteger.Parse(pubkey);
		BigInteger modulusBigInt = BigInteger.Parse(modulus);
		BigInteger result = BigInteger.ModPow(skeyBigInt, pubkeyBigInt, modulusBigInt);

		return result.ToString();
	}

	private static BigInteger HexCharToValue(char hexChar) {
		if (hexChar >= '0' && hexChar <= '9') {
			return hexChar - '0';
		} else if (hexChar >= 'a' && hexChar <= 'f') {
			return 10 + hexChar - 'a';
		} else if (hexChar >= 'A' && hexChar <= 'F') {
			return 10 + hexChar - 'A';
		} else {
			throw new ArgumentException($"无效HEX字符: {hexChar}");
		}
	}
}