using eCommerce.UserService.Application.Queries;
using FluentValidation;

namespace eCommerce.UserService.Application.Validators;

/// <summary>
/// Validator for GetUserQuery.
/// </summary>
public class GetUserQueryValidator : AbstractValidator<GetUserQuery>
{
    public GetUserQueryValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");
    }
}
