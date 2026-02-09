using Microsoft.AspNetCore.Builder;

namespace eCommerce.Aop;

public static class WebApplicationBuilderExtensions
{
    private static readonly List<(Type attributeType, Type proxyType)> AttributeAndProxyTypeList = [];

    extension(WebApplicationBuilder webApplicationBuilder)
    {
        public WebApplicationBuilder WithAttributeAndProxy<TAttribute, TProxy>()
            where TAttribute : Attribute
            where TProxy : BaseDispatchProxy<TAttribute>
        {
            return webApplicationBuilder.WithAttributeAndProxy(typeof(TAttribute), typeof(TProxy));
        }

        public WebApplicationBuilder WithAttributeAndProxy(Type attributeType, 
            Type proxyType)
        {
            AttributeAndProxyTypeList.Add((attributeType, proxyType));
            return webApplicationBuilder;
        }

        public WebApplication BuildWithProxies()
        {
            foreach (var (attributeType, proxyType) in AttributeAndProxyTypeList)
            {
                webApplicationBuilder.Services.AddProxyFromAssemblies(attributeType, proxyType);
            }
            return webApplicationBuilder.Build();
        }
    }
}