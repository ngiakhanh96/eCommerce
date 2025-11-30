using eCommerce.UserService.Application.Queries;
using FluentValidation;

namespace eCommerce.UserService.Application.Validators;

/// <summary>
/// Validator for GetUserOrdersQuery.
/// </summary>
public class GetUserOrdersQueryValidator : AbstractValidator<GetUserOrdersQuery>
{
    public GetUserOrdersQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
