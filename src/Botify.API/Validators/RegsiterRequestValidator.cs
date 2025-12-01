using Botify.Application.Requests;
using FluentValidation;

namespace Botify.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        // IdentityUser
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email is required.")
            .EmailAddress().WithMessage("Invalid email format.");

        RuleFor(x => x.Password)
            .NotEmpty().WithMessage("Password is required.")
            .MinimumLength(6).WithMessage("Password must be at least 6 characters.");

        RuleFor(x => x.Phone)
            .NotEmpty().WithMessage("Phone number is required.")
            .Matches(@"^\+?[0-9]{8,15}$")
            .WithMessage("Invalid phone number format.");

        // Profile
        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("First name is required.")
            .Length(2, 50).WithMessage("First name must be between 2 and 50 characters.");

        RuleFor(x => x.LastName)
            .NotEmpty().WithMessage("Last name is required.")
            .Length(2, 50).WithMessage("Last name must be between 2 and 50 characters.");

        RuleFor(x => x.Language)
            .Must(lang => string.IsNullOrEmpty(lang) || lang!.Length <= 5)
            .WithMessage("Language code cannot exceed 5 characters.");

        // Merchant
        RuleFor(x => x.MerchantName)
            .NotEmpty().WithMessage("Merchant name is required.")
            .MaximumLength(100).WithMessage("Merchant name cannot exceed 100 characters.");

        RuleFor(x => x.Industry)
            .NotEmpty().WithMessage("Industry is required.")
            .MaximumLength(100).WithMessage("Industry cannot exceed 100 characters.");

        RuleFor(x => x.LogoUrl)
            .Must(url => string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("LogoUrl must be a valid URL.");

        RuleFor(x => x.ProfileImageUrl)
            .Must(url => string.IsNullOrEmpty(url) || Uri.IsWellFormedUriString(url, UriKind.Absolute))
            .WithMessage("ProfileImageUrl must be a valid URL.");
    }
}
