using eCommerce.OrderService.Application.Queries;
using FluentValidation;

namespace eCommerce.OrderService.Application.Validators;

/// <summary>
/// Validator for GetOrderQuery.
/// </summary>
public class GetOrderQueryValidator : AbstractValidator<GetOrderQuery>
{
    public GetOrderQueryValidator()
    {
        RuleFor(x => x.OrderId)
            .NotEmpty()
            .WithMessage("OrderId is required.");
    }
}
