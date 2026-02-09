using System.Diagnostics;
using System.Reflection;
using eCommerce.Aop;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace eCommerce.Logging;
public class LogProxy : BaseDispatchProxy<LogAttribute>
{
    private ILogger _logger;
    private ActivitySource _activitySource;

    protected override void InitServices(MethodInfo? targetMethod, object?[]? args)
    {
        _logger = _serviceProvider.GetRequiredService<ILogger>();
        _activitySource = _serviceProvider.GetRequiredService<ActivitySource>();
    }

    protected override object InvokeSyncInternal(
        MethodInfo targetMethod,
        LogAttribute attribute,
        object?[]? args)
    {
        var parentId = Activity.Current?.Id;
        using var activity = _activitySource.StartActivity(
            attribute.CustomMethodLogName ?? targetMethod.Name, 
            ActivityKind.Internal, 
            parentId);
        var result = targetMethod.Invoke(_proxied, args);
        return result;
    }

    protected override async Task InvokeAsyncWithoutResultInternal(
        MethodInfo targetMethod,
        LogAttribute attribute,
        object?[]? args)
    {
        var parentId = Activity.Current?.Id;
        using var activity = _activitySource.StartActivity(
            attribute.CustomMethodLogName ?? targetMethod.Name, 
            ActivityKind.Internal, 
            parentId);
        await (Task) targetMethod.Invoke(_proxied, args)!;
    }

    protected override async Task<TResult> InvokeAsyncWithResultInternal<TResult>(
        MethodInfo targetMethod,
        LogAttribute attribute,
        object?[]? args)
    {
        var parentId = Activity.Current?.Id;
        using var activity = _activitySource.StartActivity(
            attribute.CustomMethodLogName ?? targetMethod.Name, 
            ActivityKind.Internal, 
            parentId);
        TResult result;
        result = await (Task<TResult>) targetMethod.Invoke(_proxied, args)!;
        return result;
    }
}
