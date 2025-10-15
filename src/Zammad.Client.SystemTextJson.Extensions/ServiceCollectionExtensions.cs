using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zammad.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IHttpClientBuilder AddZammadClient(
        this IServiceCollection services,
        Action<ZammadOptions> configureOptions
    )
    {
        services.Configure(configureOptions);
        return services.AddHttpClient();
    }

    public static IHttpClientBuilder AddZammadClient(this IServiceCollection services, IConfigurationSection config)
    {
        services.Configure<ZammadOptions>(config);
        return services.AddHttpClient();
    }

    private static IHttpClientBuilder AddHttpClient(this IServiceCollection services) =>
        services.AddHttpClient<IZammadClient, ZammadClient>();
}
