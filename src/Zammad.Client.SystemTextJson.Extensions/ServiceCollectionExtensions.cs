using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zammad.Client.SystemTextJson.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZammadClient(
        this IServiceCollection collection,
        Action<ZammadOptions> configureOptions
    )
    {
        collection.AddHttpClient<ZammadClient>();
        collection.Configure(configureOptions);
        return collection;
    }

    public static IServiceCollection AddZammadClient(this IServiceCollection collection, IConfigurationSection config)
    {
        collection.AddHttpClient<ZammadClient>();
        collection.Configure<ZammadOptions>(config);
        return collection;
    }
}
