using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Aop;

public static class ServicesExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddProxyFromAssemblies(Type attributeType, Type proxyType)
        {
            services.Scan(x =>
            {
                var entryAssembly = Assembly.GetEntryAssembly();
                var referencedAssemblies = entryAssembly.GetReferencedAssemblies().Select(Assembly.Load);
                var assemblies = new List<Assembly> { entryAssembly }.Concat(referencedAssemblies);

                x.FromAssemblies(assemblies)
                    .AddClasses(classes => classes.AssignableTo(proxyType))
                    .AsSelf()
                    .WithSingletonLifetime();
            });
            var replacedRegistrations = new List<(ServiceDescriptor?, Type, Type)>();

            foreach (var svc in services)
            {
                if (svc.ImplementationType != null)
                {
                    if (svc.ServiceType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Any(
                            m => m.GetCustomAttributes(attributeType, false).FirstOrDefault() != null) ||
                        svc.ImplementationType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Any(
                            m => m.GetCustomAttributes(attributeType, false).FirstOrDefault() != null))
                    {
                        var serviceDescriptor = services.FirstOrDefault(descriptor => 
                            descriptor.ImplementationType == svc.ImplementationType && 
                            descriptor.ServiceType == svc.ServiceType);
                        replacedRegistrations.Add((serviceDescriptor, svc.ServiceType, svc.ImplementationType));
                    }
                }
            }

            foreach (var registration in replacedRegistrations)
            {
                if (registration.Item2 != null && registration.Item2.Name != registration.Item3.Name)
                {
                    //TODO Check service lifetime as well
                    //TODO Add support for delegate scenario
                    services.AddProxiedScope(registration.Item2, registration.Item3, proxyType);
                }
                else
                {
                    //DispatchProxy does not support class proxying
                }
            }

            return services;
        }
    }

    public static IServiceCollection AddProxiedScope<TAttribute, TImplementation, TProxy>(
        this IServiceCollection serviceCollection)
        where TAttribute : Attribute
        where TImplementation : class, TAttribute
        where TProxy : BaseDispatchProxy<TAttribute>
    {
        return serviceCollection.AddProxiedScope(typeof(TAttribute), typeof(TImplementation), typeof(TProxy));
    }

    public static IServiceCollection AddProxiedScope
        (this IServiceCollection services, Type attribute, Type implementation, Type proxyType)
    {
        services.AddScoped(implementation);
        // This registers the underlying class
        services.AddScoped(attribute, serviceProvider =>
        {
            // if proxy type is CacheProxy<T> and interface is IWeatherForecastService
            // then make closed type CacheProxy<IWeatherForecastService>
            var closedProxyType = proxyType.IsGenericTypeDefinition
                ? proxyType.MakeGenericType(attribute)
                : proxyType;
            var proxy = DispatchProxy.Create(attribute, closedProxyType);
            var actual = serviceProvider
                .GetRequiredService(implementation);

            var setDecoratedMethod = closedProxyType.GetMethod(
                nameof(BaseDispatchProxy<>.SetProxied), 
                BindingFlags.Instance | BindingFlags.Public);
            setDecoratedMethod?.Invoke(proxy, [actual]);

            var setServiceProviderMethod = closedProxyType.GetMethod(
                nameof(BaseDispatchProxy<>.SetServiceProvider), 
                BindingFlags.Instance | BindingFlags.Public);
            setServiceProviderMethod?.Invoke(proxy, [serviceProvider]);

            return proxy;
        });
        return services;
    }
}