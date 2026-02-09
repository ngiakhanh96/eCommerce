using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Cache;

public static class ServicesExtensions
{
    public static IServiceCollection AddInMemoryCacheProvider(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<InMemoryCacheService>();
        return serviceCollection;
    }
}