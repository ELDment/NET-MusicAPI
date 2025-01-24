using HttpManager;
using Newtonsoft.Json;
using System.Numerics;
using System.Security.Cryptography;
using System.Text;

namespace NetEase {
	public class NetEaseAPI {
		private NetEaseCrypto crypto;

		public NetEaseAPI() {
			this.crypto = new NetEaseCrypto();
		}

		public HttpPackage Search(string keyword, Dictionary<string, object> option) {
			keyword = keyword.Trim();
			if (string.IsNullOrWhiteSpace(keyword)) {
				return new HttpPackage();
			}

			Dictionary<string, object> apiRequest = new Dictionary<string, object> {
				{ "url", "http://music.163.com/api/cloudsearch/pc" },
				{ "body", new Dictionary<string, object> {
						{ "s", string.Concat(keyword.Select(c => $@"\u{(int)c:X4}")).ToLower() },
						{ "type", option.ContainsKey("type") ? int.Parse(option["type"].ToString() ?? "1") : 1 },
						{ "limit", option.ContainsKey("limit") ? int.Parse(option["limit"].ToString() ?? "30") : 30 },
						{ "total", "true" },
						{ "offset", ((option.ContainsKey("page") && option.ContainsKey("limit")) ? (int.Parse(option["page"].ToString() ?? "2") - 1) * int.Parse(option["limit"].ToString() ?? "30") : 0) }
					}
				}
			};
			Dictionary<string, object> encryptedRequest = this.crypto.NetEaseAESCBC(apiRequest);

			return new HttpPackage(encryptedRequest["url"]!.ToString(), (Dictionary<string, string>)encryptedRequest["body"]);
		}

		public HttpPackage Song(long id) {
			Dictionary<string, object> apiRequest = new Dictionary<string, object> {
				{ "url", "http://music.163.com/api/v3/song/detail/" },
				{ "body", new Dictionary<string, object> {
						{ "c", $"[{{\"id\":'{id}',\"v\":0}}]" }
					}
				}
			};
			Dictionary<string, object> encryptedRequest = this.crypto.NetEaseAESCBC(apiRequest);

			return new HttpPackage(encryptedRequest["url"]!.ToString(), (Dictionary<string, string>)encryptedRequest["body"]);
		}

		public HttpPackage Url(long id, int br = 320) {
			Dictionary<string, object> apiRequest = new Dictionary<string, object> {
				{ "url", "http://music.163.com/api/song/enhance/player/url" },
				{ "body", new Dictionary<string, object> {
						{ "ids", $"[{id}]" },
						{ "br", br * 1000 }
					}
				}
			};
			Dictionary<string, object> encryptedRequest = this.crypto.NetEaseAESCBC(apiRequest);

			return new HttpPackage(encryptedRequest["url"]!.ToString(), (Dictionary<string, string>)encryptedRequest["body"]);
		}

		public HttpPackage Lyric(long id) {
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
			Dictionary<string, object> encryptedRequest = this.crypto.NetEaseAESCBC(apiRequest);

			return new HttpPackage(encryptedRequest["url"]!.ToString(), (Dictionary<string, string>)encryptedRequest["body"]);
		}
	}


	public class NetEaseCrypto {
		private static readonly string Modulus = "157794750267131502212476817800345498121872783333389747424011531025366277535262539913701806290766479189477533597854989606803194253978660329941980786072432806427833685472618792592200595694346872951301770580765135349259590167490536138082469680638514416594216629258349130257685001248172188325316586707301643237607";
		private static readonly string PubKey = "65537";
		private static readonly string Nonce = "0CoJUm6Qyw8W8jud";
		private static readonly string IV = "0102030405060708";

		public Dictionary<string, object> NetEaseAESCBC(Dictionary<string, object> api) {
			string skey = GetRandomHex(16) ?? "B3v3kH4vRPWRJFfH";

			string body = JsonConvert.SerializeObject(api["body"]);
			body = EncryptAES(body.Replace("\\\\", "\\"), Nonce, IV);
			body = EncryptAES(body, skey, IV);

			string encryptedSkey = SecretKey(skey, PubKey, Modulus);

			api["url"] = api["url"].ToString()!.Replace("/api/", "/weapi/");
			api["body"] = new Dictionary<string, string> {
				{ "params", body },
				{ "encSecKey", encryptedSkey.ToLower() }
			};

			return api;
		}

		private string GetRandomHex(int length) {
			using (var rng = RandomNumberGenerator.Create()) {
				byte[] bytes = new byte[length / 2];
				rng.GetBytes(bytes);
				return BitConverter.ToString(bytes).Replace("-", "").ToLower();
			}
		}

		private string EncryptAES(string plainText, string key, string iv) {
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

		private string SecretKey(string skey, string pubkey, string modulus) {
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

		private string ReverseString(string input) {
			char[] charArray = input.ToCharArray();
			Array.Reverse(charArray);
			return new string(charArray);
		}

		private string bchexdec(string hex) {
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

		private string str2hex(string input) {
			StringBuilder hexBuilder = new StringBuilder();
			foreach (char c in input) {
				string hexCode = ((int)c).ToString("X2");
				hexBuilder.Append(hexCode);
			}
			return hexBuilder.ToString();
		}

		private string bcpowmod(string input, string pubkey, string modulus) {
			BigInteger skeyBigInt = BigInteger.Parse(input);
			BigInteger pubkeyBigInt = BigInteger.Parse(pubkey);
			BigInteger modulusBigInt = BigInteger.Parse(modulus);
			BigInteger result = BigInteger.ModPow(skeyBigInt, pubkeyBigInt, modulusBigInt);

			return result.ToString();
		}

		private BigInteger HexCharToValue(char hexChar) {
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
}