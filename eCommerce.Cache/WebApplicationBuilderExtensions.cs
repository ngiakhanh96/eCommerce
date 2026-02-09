using eCommerce.Aop;
using Microsoft.AspNetCore.Builder;

namespace eCommerce.Cache;

public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder webApplicationBuilder)
    {
        public WebApplicationBuilder WithCacheProxy()
        {
            return webApplicationBuilder.WithAttributeAndProxy(typeof(CacheableAttribute), typeof(CacheProxy));
        }
    }
}