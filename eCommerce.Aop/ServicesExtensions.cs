using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Aop;

public static class ServicesExtensions
{
    extension(IServiceCollection services)
    {
        public IServiceCollection AddProxyFromAssemblies(Type attributeType, Type proxyType)
        {
            var replacedRegistrations = new List<(Type svcType, Type implType, ServiceLifetime svcLifetime)>();

            foreach (var svc in services)
            {
                if (svc.ImplementationType != null)
                {
                    if (svc.ServiceType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Any(
                            m => m.GetCustomAttributes(attributeType, false).FirstOrDefault() != null) ||
                        svc.ImplementationType.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance).Any(
                            m => m.GetCustomAttributes(attributeType, false).FirstOrDefault() != null))
                    {
                        replacedRegistrations.Add((svc.ServiceType, svc.ImplementationType, svc.Lifetime));
                    }
                }
            }

            foreach (var registration in replacedRegistrations)
            {
                if (registration.svcType.Name != registration.implType.Name)
                {
                    //TODO Add support for delegate scenario
                    services.AddProxy(proxyType, registration.svcType, registration.implType, registration.svcLifetime);
                }
                else
                {
                    //DispatchProxy does not support class proxying
                }
            }

            return services;
        }
    }

    public static IServiceCollection AddProxy<TAttribute, TImplementation, TProxy, TInterface>(
        this IServiceCollection serviceCollection, ServiceLifetime serviceLifetime)
        where TAttribute : Attribute
        where TImplementation : class
        where TProxy : BaseDispatchProxy<TAttribute>
    {
        return serviceCollection.AddProxy(typeof(TProxy), typeof(TInterface), typeof(TImplementation), serviceLifetime);
    }

    public static IServiceCollection AddProxy
        (this IServiceCollection services, Type proxyType, Type interfaceType, Type implementationType, ServiceLifetime serviceLifetime)
    {
        switch (serviceLifetime)
        {
            // This registers the underlying class
            case ServiceLifetime.Transient:
                services.AddTransient(implementationType);
                services.AddTransient(interfaceType, serviceProvider =>
                    ProxyFactory(serviceProvider, proxyType, interfaceType, implementationType));
                break;
            case ServiceLifetime.Scoped:
                services.AddScoped(implementationType);
                services.AddScoped(interfaceType, serviceProvider =>
                    ProxyFactory(serviceProvider, proxyType, interfaceType, implementationType));
                break;
            case ServiceLifetime.Singleton:
            default:
                services.AddSingleton(implementationType);
                services.AddSingleton(interfaceType, serviceProvider =>
                    ProxyFactory(serviceProvider, proxyType, interfaceType, implementationType));
                break;
        }

        return services;

        static object ProxyFactory(IServiceProvider serviceProvider, Type proxyType, Type interfaceType, Type implementationType)
        {
            var proxy = DispatchProxy.Create(interfaceType, proxyType);
            var actual = serviceProvider
                .GetRequiredService(implementationType);

            var setDecoratedMethod = proxyType.GetMethod(
                nameof(BaseDispatchProxy<>.SetProxied), 
                BindingFlags.Instance | BindingFlags.Public);
            setDecoratedMethod?.Invoke(proxy, [actual]);

            var setServiceProviderMethod = proxyType.GetMethod(
                nameof(BaseDispatchProxy<>.SetServiceProvider), 
                BindingFlags.Instance | BindingFlags.Public);
            setServiceProviderMethod?.Invoke(proxy, [serviceProvider]);

            return proxy;
        }
    }
}