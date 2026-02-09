using System.Diagnostics;
using eCommerce.Aop;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;

namespace eCommerce.Logging;

public static class WebApplicationBuilderExtensions
{
    extension(WebApplicationBuilder webApplicationBuilder)
    {
        public WebApplicationBuilder WithLogProxy(ActivitySource activitySource)
        {
            webApplicationBuilder.Services.AddScoped<ActivitySource>(svp => activitySource);
            return webApplicationBuilder.WithAttributeAndProxy(typeof(LogAttribute), typeof(LogProxy));
        }
    }
}