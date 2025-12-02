using Botify.API.Extensions;
using Botify.Application.Common.Interfaces;
using Botify.Application.Requests;
using Botify.Domain.Identity;
using Botify.Domain.Models.Merchants;
using Botify.Domain.ResultPattern;
using FluentValidation;

namespace Botify.API.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator(IFileService fileService)
    {
        // IdentityUser
        RuleFor(x => x.Email)
            .RequiredField(UserProfileErrors.EmailRequired)
            .EmailAddress()
                .WithMessage(UserProfileErrors.EmailInvalid.Description)
                .WithErrorCode(UserProfileErrors.EmailInvalid.Code)
                .WithState(_ => UserProfileErrors.EmailInvalid);

        RuleFor(x => x.Password)
            .RequiredField(UserProfileErrors.PasswordRequired)
            .MinimumLength(6)
                .WithMessage(UserProfileErrors.PasswordTooShort.Description)
                .WithErrorCode(UserProfileErrors.PasswordTooShort.Code)
                .WithState(_ => UserProfileErrors.PasswordTooShort);

        RuleFor(x => x.Phone)
            .RequiredField(UserProfileErrors.PhoneRequired)
            .Matches(@"^\+?[0-9]{8,15}$")
                .WithMessage(UserProfileErrors.PhoneInvalid.Description)
                .WithErrorCode(UserProfileErrors.PhoneInvalid.Code)
                .WithState(_ => UserProfileErrors.PhoneInvalid);

        // Profile
        RuleFor(x => x.FirstName)
            .RequiredField(UserProfileErrors.FirstNameRequired)
            .Length(2, 50)
                .WithMessage(UserProfileErrors.FirstNameLengthInvalid.Description)
                .WithErrorCode(UserProfileErrors.FirstNameLengthInvalid.Code)
                .WithState(_ => UserProfileErrors.FirstNameLengthInvalid);

        RuleFor(x => x.LastName)
            .RequiredField(UserProfileErrors.LastNameRequired)
            .Length(2, 50)
                .WithMessage(UserProfileErrors.LastNameLengthInvalid.Description)
                .WithErrorCode(UserProfileErrors.LastNameLengthInvalid.Code)
                .WithState(_ => UserProfileErrors.LastNameLengthInvalid);

        RuleFor(x => x.Language)
            .Must(lang => string.IsNullOrEmpty(lang) || lang!.Length <= 5)
                .WithMessage(UserProfileErrors.LanguageTooLong.Description)
                .WithErrorCode(UserProfileErrors.LanguageTooLong.Code)
                .WithState(_ => UserProfileErrors.LanguageTooLong);

        // Merchant
        RuleFor(x => x.MerchantName)
            .RequiredField(MerchantErrors.MerchantNameRequired)
            .MaximumLength(100)
                .WithMessage(MerchantErrors.MerchantNameTooLong.Description)
                .WithErrorCode(MerchantErrors.MerchantNameTooLong.Code)
                .WithState(_ => MerchantErrors.MerchantNameTooLong);

        RuleFor(x => x.Industry)
            .RequiredField(MerchantErrors.IndustryRequired)
            .MaximumLength(100)
                .WithMessage(MerchantErrors.IndustryTooLong.Description)
                .WithErrorCode(MerchantErrors.IndustryTooLong.Code)
                .WithState(_ => MerchantErrors.IndustryTooLong);
        // File validation
        RuleFor(x => x.Logo)
            .Must(file => file == null || fileService.ValidateFile(file.FileName, file.ContentType, file.Length))
                .WithMessage(MerchantErrors.LogoUrlInvalid.Description)
                .WithErrorCode(MerchantErrors.LogoUrlInvalid.Code)
                .WithState(_ => MerchantErrors.LogoUrlInvalid);

        RuleFor(x => x.ProfileImage)
            .Must(file => file == null || fileService.ValidateFile(file.FileName, file.ContentType, file.Length))
                .WithMessage(UserProfileErrors.ProfileImageUrlInvalid.Description)
                .WithErrorCode(UserProfileErrors.ProfileImageUrlInvalid.Code)
                .WithState(_ => UserProfileErrors.ProfileImageUrlInvalid);
    }
}
