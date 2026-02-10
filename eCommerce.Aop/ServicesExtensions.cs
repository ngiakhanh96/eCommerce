using Microsoft.Extensions.DependencyInjection;
using System.Collections.Concurrent;
using System.Linq.Expressions;
using System.Reflection;

namespace eCommerce.Aop;

public static class ServicesExtensions
{
    // Cache delegates per proxy type to avoid repeated reflection
    private static readonly ConcurrentDictionary<Type, (Action<object, object> SetProxied, Action<object, IServiceProvider> SetServiceProvider)> ProxyMethodCache = new();
    
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
    }

    private static object ProxyFactory(IServiceProvider serviceProvider, Type proxyType, Type interfaceType, Type implementationType)
    {
        var proxy = DispatchProxy.Create(interfaceType, proxyType);
        var actual = serviceProvider.GetRequiredService(implementationType);

        // Get or create cached delegates for this proxy type
        var (setProxied, setServiceProvider) = 
            ProxyMethodCache.GetOrAdd(proxyType, CreateProxyDelegates);

        setProxied(proxy, actual);
        setServiceProvider(proxy, serviceProvider);

        return proxy;
    }

    private static (Action<object, object> SetProxied, Action<object, IServiceProvider> SetServiceProvider) CreateProxyDelegates(Type proxyType)
    {
        // Cache SetProxied delegate
        var setProxiedMethod = proxyType.GetMethod(
            nameof(BaseDispatchProxy<>.SetProxied),
            BindingFlags.Instance | BindingFlags.Public);

        var setProxiedDelegate = CreateSetProxiedDelegate(setProxiedMethod);

        // Cache SetServiceProvider delegate
        var setServiceProviderMethod = proxyType.GetMethod(
            nameof(BaseDispatchProxy<>.SetServiceProvider),
            BindingFlags.Instance | BindingFlags.Public);

        var setServiceProviderDelegate = CreateSetServiceProviderDelegate(setServiceProviderMethod);

        return (setProxiedDelegate, setServiceProviderDelegate);

        static Action<object, object> CreateSetProxiedDelegate(MethodInfo method)
        {
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var argParam = Expression.Parameter(typeof(object), "arg");
            var call = Expression.Call(Expression.Convert(instanceParam, method.DeclaringType!), method, argParam);
            return Expression.Lambda<Action<object, object>>(call, instanceParam, argParam).Compile();
        }

        static Action<object, IServiceProvider> CreateSetServiceProviderDelegate(MethodInfo method)
        {
            var instanceParam = Expression.Parameter(typeof(object), "instance");
            var argParam = Expression.Parameter(typeof(IServiceProvider), "arg");
            var call = Expression.Call(Expression.Convert(instanceParam, method.DeclaringType!), method, argParam);
            return Expression.Lambda<Action<object, IServiceProvider>>(call, instanceParam, argParam).Compile();
        }
    }
}