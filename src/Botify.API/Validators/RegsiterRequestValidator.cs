using Botify.Application.Requests;
using FluentValidation;

namespace Botify.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email address.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        // Conditional validation: ShopName required if Role is Merchant
        When(x => x.Role == Domain.Identity.Enums.Roles.Merchant, () =>
        {
            RuleFor(x => x.ShopName)
                .NotEmpty().WithMessage("Shop name is required for merchants.")
                .Length(2, 100).WithMessage("Shop name must be between 2 and 100 characters.");
        });
    }
}
