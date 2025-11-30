using eCommerce.UserService.Application.Commands;
using FluentValidation;

namespace eCommerce.UserService.Application.Validators;

/// <summary>
/// Validator for CreateUserCommand.
/// </summary>
public class CreateUserCommandValidator : AbstractValidator<CreateUserCommand>
{
    public CreateUserCommandValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .WithMessage("Name is required.")
            .MaximumLength(255)
            .WithMessage("Name must not exceed 255 characters.");

        RuleFor(x => x.Email)
            .NotEmpty()
            .WithMessage("Email is required.")
            .MaximumLength(255)
            .WithMessage("Email must not exceed 255 characters.")
            .EmailAddress()
            .WithMessage("Email must be a valid email address.");
    }
}
