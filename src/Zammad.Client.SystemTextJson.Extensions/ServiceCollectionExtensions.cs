using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Zammad.Client.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddZammadClient(
        this IServiceCollection services,
        Action<ZammadOptions> configureOptions
    )
    {
        services.AddServices();
        services.Configure(configureOptions);
        return services;
    }

    public static IServiceCollection AddZammadClient(this IServiceCollection services, IConfigurationSection config)
    {
        services.AddServices();
        services.Configure<ZammadOptions>(config);
        return services;
    }

    private static void AddServices(this IServiceCollection services) =>
        services.AddHttpClient<IZammadClient, ZammadClient>();
}
