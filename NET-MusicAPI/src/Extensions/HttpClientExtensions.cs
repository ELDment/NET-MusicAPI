using System.Net;
using System.Web;

namespace MusicAPI;

public static class HttpClientExtensions
{
    /// <summary>
    /// Sends an HTTP request with custom headers and parameters
    /// </summary>
    public static async Task<string?> SendMusicApiRequestAsync(this HttpClient client, HttpMethod method, string? url, IReadOnlyDictionary<string, string>? parameters, IReadOnlyDictionary<string, string> headers, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(url))
        {
            return string.Empty;
        }

        using var request = CreateRequest(method, url, parameters);

        foreach (var (key, value) in headers)
        {
            request.Headers.TryAddWithoutValidation(key, value);
        }

        using var response = await client.SendAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
    }

    private static HttpRequestMessage CreateRequest(HttpMethod method, string url, IReadOnlyDictionary<string, string>? parameters)
    {
        if (method == HttpMethod.Post)
        {
            var request = new HttpRequestMessage(HttpMethod.Post, url);
            if (parameters.Any())
            {
                request.Content = new FormUrlEncodedContent(parameters);
            }
            return request;
        }

        // GET request
        var urlWithQuery = BuildUrlWithQuery(url, parameters);
        return new HttpRequestMessage(HttpMethod.Get, urlWithQuery);
    }

    private static string BuildUrlWithQuery(string url, IReadOnlyDictionary<string, string>? parameters)
    {
        if (!parameters.Any())
        {
            return url;
        }

        var queryString = HttpUtility.ParseQueryString(string.Empty);
        foreach (var (key, value) in parameters)
        {
            queryString[key] = value;
        }

        return $"{url}?{queryString}";
    }

    /// <summary>
    /// Generates a random IP address in the range 112.80.0.0 - 112.95.255.255
    /// </summary>
    public static string GenerateRandomIp()
    {
        var ip = Random.Shared.Next(1884815360, 1884890111);
        return new IPAddress(ip).ToString();
    }
}