using eCommerce.OrderService.Application.Commands;
using FluentValidation;

namespace eCommerce.OrderService.Application.Validators;

/// <summary>
/// Validator for CreateOrderCommand.
/// </summary>
public class CreateOrderCommandValidator : AbstractValidator<CreateOrderCommand>
{
    public CreateOrderCommandValidator()
    {
        RuleFor(x => x.UserId)
            .NotEmpty()
            .WithMessage("UserId is required.");

        RuleFor(x => x.Product)
            .NotEmpty()
            .WithMessage("Product is required.")
            .MaximumLength(255)
            .WithMessage("Product must not exceed 255 characters.");

        RuleFor(x => x.Quantity)
            .GreaterThan(0)
            .WithMessage("Quantity must be greater than 0.");

        RuleFor(x => x.Price)
            .GreaterThan(0)
            .WithMessage("Price must be greater than 0.");
    }
}
