using eCommerce.Mediator.Validation;
using FluentValidation;

namespace eCommerce.Mediator.Commands;

/// <summary>
/// Decorates the command bus to add validation before command execution.
/// </summary>
public class ValidatingCommandBus : ICommandBus
{
    private readonly ICommandBus _inner;
    private readonly IServiceProvider _serviceProvider;

    public ValidatingCommandBus(ICommandBus inner, IServiceProvider serviceProvider)
    {
        _inner = inner;
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> SendAsync<TResult>(ICommand<TResult> command)
    {
        var commandType = command.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(commandType);
        
        var validator = _serviceProvider.GetService(validatorType) as IValidator;
        
        if (validator != null)
        {
            var context = new ValidationContext<object>(command);
            var validationResult = await validator.ValidateAsync(context);
            
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).ToArray());
                
                throw new RequestValidationException(errors);
            }
        }

        return await _inner.SendAsync(command);
    }
}
