using eCommerce.Mediator.Commands;
using eCommerce.Mediator.Queries;
using eCommerce.Mediator.Validation;
using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using System.Reflection;

namespace eCommerce.Mediator.Extensions;

/// <summary>
/// Provides extension methods for registering mediator-related services with an <see cref="IServiceCollection"/>.
/// </summary>
/// <remarks>These extension methods simplify the setup of command and query bus infrastructure in dependency
/// injection containers. Use these methods to add mediator patterns to your application's service collection during
/// startup configuration.
/// </remarks>
public static class Extensions
{
    /// <summary>
    /// Adds the mediator services without validation.
    /// </summary>
    public static TServiceCollection AddMediator<TServiceCollection>(this TServiceCollection service) where TServiceCollection : IServiceCollection
    {
        service.AddScoped<ICommandBus, CommandBus>();
        service.AddScoped<IQueryBus, QueryBus>();
        return service;
    }

    /// <summary>
    /// Adds the mediator services with FluentValidation support.
    /// Validators are automatically discovered from the provided assembly.
    /// </summary>
    public static TServiceCollection AddMediatorWithValidation<TServiceCollection>(
        this TServiceCollection service, 
        Assembly validatorsAssembly) where TServiceCollection : IServiceCollection
    {
        // Register validators from the assembly
        service.AddValidatorsFromAssembly(validatorsAssembly);

        // Register the base command/query buses
        service.AddScoped<CommandBus>();
        service.AddScoped<QueryBus>();

        // Register validating decorators
        service.AddScoped<ICommandBus>(sp => 
            new ValidatingCommandBus(sp.GetRequiredService<CommandBus>(), sp));
        service.AddScoped<IQueryBus>(sp => 
            new ValidatingQueryBus(sp.GetRequiredService<QueryBus>(), sp));

        return service;
    }
}
