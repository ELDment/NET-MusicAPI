using System.Text;
using System.Numerics;
using System.Text.Json;
using System.Security.Cryptography;

namespace MusicAPI.Netease;

internal static class NeteaseCryptoService
{
    private const string Modulus = "157794750267131502212476817800345498121872783333389747424011531025366277535262539913701806290766479189477533597854989606803194253978660329941980786072432806427833685472618792592200595694346872951301770580765135349259590167490536138082469680638514416594216629258349130257685001248172188325316586707301643237607";
    private const string PubKey = "65537";
    private const string Nonce = "0CoJUm6Qyw8W8jud";
    private const string IV = "0102030405060708";

    /// <summary>
    /// Encrypts API request using AES-CBC encryption
    /// </summary>
    public static NeteaseEncryptedResult EncryptRequest(string url, object body)
    {
        var skey = GenerateRandomHex(16);

        var bodyJson = JsonSerializer.Serialize(body);
        var encryptedBody = EncryptAes(bodyJson.Replace("\\\\", "\\"), Nonce, IV);
        encryptedBody = EncryptAes(encryptedBody, skey, IV);

        var encryptedSkey = EncryptSecretKey(skey, PubKey, Modulus);

        return new NeteaseEncryptedResult(url.Replace("/api/", "/weapi/"), new Dictionary<string, string>
        {
            ["params"] = encryptedBody,
            ["encSecKey"] = encryptedSkey.ToLowerInvariant()
        });
    }

    private static string GenerateRandomHex(int length)
    {
        Span<byte> bytes = stackalloc byte[length / 2];
        RandomNumberGenerator.Fill(bytes);
        return Convert.ToHexString(bytes).ToLowerInvariant();
    }

    private static string EncryptAes(string plainText, string key, string iv)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(plainText);
        ArgumentException.ThrowIfNullOrWhiteSpace(key);
        ArgumentException.ThrowIfNullOrWhiteSpace(iv);

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var ivBytes = Encoding.UTF8.GetBytes(iv);

        if (keyBytes.Length != 16)
        {
            throw new ArgumentException("AES-128-CBC key must be 16 bytes", nameof(key));
        }

        if (ivBytes.Length != 16)
        {
            throw new ArgumentException("AES-128-CBC IV must be 16 bytes", nameof(iv));
        }

        using var aes = Aes.Create();
        aes.Key = keyBytes;
        aes.IV = ivBytes;
        aes.Mode = CipherMode.CBC;
        aes.Padding = PaddingMode.PKCS7;

        using var encryptor = aes.CreateEncryptor();
        var plainBytes = Encoding.UTF8.GetBytes(plainText);
        var encryptedBytes = encryptor.TransformFinalBlock(plainBytes, 0, plainBytes.Length);

        return Convert.ToBase64String(encryptedBytes);
    }

    private static string EncryptSecretKey(string skey, string pubkey, string modulus)
    {
        var reversedKey = ReverseString(skey);
        var hexKey = ConvertToHex(reversedKey);
        var decimalKey = HexToDecimal(hexKey);
        var encrypted = ModularExponentiation(decimalKey, pubkey, modulus);

        var result = BigInteger.Parse(encrypted).ToString("X").ToLowerInvariant();

        // Remove leading character if length is 257
        if (result.Length == 257)
        {
            result = result[1..];
        }

        return result.PadLeft(256, '0');
    }

    private static string ReverseString(string input)
    {
        Span<char> chars = stackalloc char[input.Length];
        input.AsSpan().CopyTo(chars);
        chars.Reverse();
        return new string(chars);
    }

    private static string ConvertToHex(string input)
    {
        var builder = new StringBuilder(input.Length * 2);
        foreach (var c in input)
        {
            builder.Append(((int)c).ToString("X2"));
        }
        return builder.ToString();
    }

    private static string HexToDecimal(string hex)
    {
        BigInteger result = 0;
        var length = hex.Length;

        for (var i = 0; i < length; i++)
        {
            var digitValue = HexCharToValue(hex[i]);
            var power = BigInteger.Pow(16, length - i - 1);
            result += digitValue * power;
        }

        return result.ToString();
    }

    private static string ModularExponentiation(string baseValue, string exponent, string modulus)
    {
        var baseInt = BigInteger.Parse(baseValue);
        var expInt = BigInteger.Parse(exponent);
        var modInt = BigInteger.Parse(modulus);

        return BigInteger.ModPow(baseInt, expInt, modInt).ToString();
    }

    private static BigInteger HexCharToValue(char hexChar) => hexChar switch
    {
        >= '0' and <= '9' => hexChar - '0',
        >= 'a' and <= 'f' => 10 + hexChar - 'a',
        >= 'A' and <= 'F' => 10 + hexChar - 'A',
        _ => throw new ArgumentException($"Invalid hex character: {hexChar}", nameof(hexChar))
    };
}