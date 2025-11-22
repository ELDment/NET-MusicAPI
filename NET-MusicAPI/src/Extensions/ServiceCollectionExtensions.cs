using Microsoft.Extensions.DependencyInjection;
using MusicAPI.Netease;
using MusicAPI.Tencent;
using MusicAPI.Spotify;
using MusicAPI.Abstractions;

namespace MusicAPI.Extensions;

public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds Netease Cloud Music API services to the service collection
    /// </summary>
    public static IServiceCollection AddNeteaseApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient<IMusicApi, NeteaseApi>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(8);
        });

        return services;
    }

    /// <summary>
    /// Adds Netease Cloud Music API as a singleton with self-managed HttpClient
    /// </summary>
    public static IServiceCollection AddNeteaseApiSingleton(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IMusicApi>(sp => new NeteaseApi());

        return services;
    }

    /// <summary>
    /// Adds Tencent QQ Music API services to the service collection
    /// </summary>
    public static IServiceCollection AddTencentApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient<IMusicApi, TencentApi>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(8);
        });

        return services;
    }

    /// <summary>
    /// Adds Tencent QQ Music API as a singleton with self-managed HttpClient
    /// </summary>
    public static IServiceCollection AddTencentApiSingleton(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IMusicApi>(sp => new TencentApi());

        return services;
    }

    /// <summary>
    /// Adds Spotify Music API services (Spotify metadata + YouTube audio search)
    /// </summary>
    public static IServiceCollection AddSpotifyApi(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddHttpClient<IMusicApi, SpotifyApi>(client =>
        {
            client.Timeout = TimeSpan.FromSeconds(10);
        });

        return services;
    }

    /// <summary>
    /// Adds Spotify Music API as a singleton
    /// </summary>
    public static IServiceCollection AddSpotifyApiSingleton(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IMusicApi>(sp => new SpotifyApi());

        return services;
    }
}