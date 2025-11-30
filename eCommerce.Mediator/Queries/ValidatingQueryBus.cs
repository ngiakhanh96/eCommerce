using eCommerce.Mediator.Validation;
using FluentValidation;

namespace eCommerce.Mediator.Queries;

/// <summary>
/// Decorates the query bus to add validation before query execution.
/// </summary>
public class ValidatingQueryBus : IQueryBus
{
    private readonly IQueryBus _inner;
    private readonly IServiceProvider _serviceProvider;

    public ValidatingQueryBus(IQueryBus inner, IServiceProvider serviceProvider)
    {
        _inner = inner;
        _serviceProvider = serviceProvider;
    }

    public async Task<TResult> SendAsync<TResult>(IQuery<TResult> query)
    {
        var queryType = query.GetType();
        var validatorType = typeof(IValidator<>).MakeGenericType(queryType);
        
        var validator = _serviceProvider.GetService(validatorType) as IValidator;
        
        if (validator != null)
        {
            var context = new ValidationContext<object>(query);
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

        return await _inner.SendAsync(query);
    }
}
