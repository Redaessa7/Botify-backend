using Botify.API.CrossCutting.Extensions;
using Botify.Application.Services.Merchants;
using Botify.Application.Services.Merchants.Requests;
using Botify.Domain.Models.Merchants.Shops;
using Botify.Domain.Models.Merchants.ValueObjects;
using FluentValidation;
using Microsoft.AspNetCore.Http;

namespace Botify.API.CrossCutting.Validators;

public class AssignShopRequestValidator : AbstractValidator<AssignShopRequest>
{
    private static readonly string[] AllowedImageContentTypes = new[] { "image/jpeg", "image/png", "image/webp" };
    private const long MaxImageBytes = 5 * 1024 * 1024; // 5 MB

    public AssignShopRequestValidator()
    {
        RuleFor(x => x.Name)
            .RequiredField(ShopErrors.NameRequired)
            .MaximumLength(200)
                .WithMessage(ShopErrors.NameTooLong.Description)
                .WithErrorCode(ShopErrors.NameTooLong.Code)
                .WithState(_ => ShopErrors.NameTooLong);

        RuleFor(x => x.Currency)
            .RequiredField(ShopErrors.CurrencyRequired)
            .Matches("^[A-Za-z]{3}$")
                .WithMessage(ShopErrors.CurrencyInvalid.Description)
                .WithErrorCode(ShopErrors.CurrencyInvalid.Code)
                .WithState(_ => ShopErrors.CurrencyInvalid);

        RuleFor(x => x.Language)
            .RequiredField(ShopErrors.LanguageRequired)
            .Length(2, 8)
                .WithMessage(ShopErrors.LanguageInvalid.Description)
                .WithErrorCode(ShopErrors.LanguageInvalid.Code)
                .WithState(_ => ShopErrors.LanguageInvalid);

        RuleFor(x => x.Address)
            .RequiredField(ShopErrors.AddressRequired)
            .MaximumLength(1000)
                .WithMessage(ShopErrors.AddressTooLong.Description)
                .WithErrorCode(ShopErrors.AddressTooLong.Code)
                .WithState(_ => ShopErrors.AddressTooLong);

        RuleFor(x => x.SocialMediaLinks)
            .Cascade(CascadeMode.Stop)
            .Must(BeValidSocialLinks)
                .When(x => x.SocialMediaLinks is not null)
                .WithMessage(ShopErrors.SocialMediaLinksInvalid.Description)
                .WithErrorCode(ShopErrors.SocialMediaLinksInvalid.Code)
                .WithState(_ => ShopErrors.SocialMediaLinksInvalid);

        RuleFor(x => x.Image)
            .Cascade(CascadeMode.Stop)
            .Must(BeValidImage)
                .When(x => x.Image is not null)
                .WithMessage(ShopErrors.ImageInvalid.Description)
                .WithErrorCode(ShopErrors.ImageInvalid.Code)
                .WithState(_ => ShopErrors.ImageInvalid);

        RuleFor(x => x.TaxRate)
            .Cascade(CascadeMode.Stop)
            .InclusiveBetween(0m, 1m)
                .When(x => x.TaxRate.HasValue)
                .WithMessage(ShopErrors.TaxRateInvalid.Description)
                .WithErrorCode(ShopErrors.TaxRateInvalid.Code)
                .WithState(_ => ShopErrors.TaxRateInvalid);
    }

    private static bool BeValidSocialLinks(SocialMediaLinks? links)
    {
        if (links is null) return true;

       

        return true;
    }

    private static bool BeValidImage(IFormFile? file)
    {
        if (file is null) return true;

        if (file.Length <= 0 || file.Length > MaxImageBytes) return false;

        if (string.IsNullOrWhiteSpace(file.ContentType)) return false;

        return AllowedImageContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase);
    }
}
