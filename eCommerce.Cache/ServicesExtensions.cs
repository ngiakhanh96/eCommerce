using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Cache;

public static class ServicesExtensions
{
    extension(IServiceCollection serviceCollection)
    {
        public IServiceCollection AddInMemoryCacheProvider()
        {
            serviceCollection.AddSingleton<InMemoryCacheService>();
            return serviceCollection;
        }
    }
}