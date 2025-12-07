using Botify.Application.Services.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.ValueObjects;
using FluentValidation;
using Botify.Application.Services.Merchants.Requests;
using Botify.API.CrossCutting.Extensions;

namespace Botify.API.CrossCutting.Validators;

public class CreateBotRequestValidator : AbstractValidator<CreateBotRequest>
{
    public CreateBotRequestValidator()
    {
        RuleFor(x => x.Name)
            .RequiredField(BotErrors.NameRequired)
            .MaximumLength(100)
                .WithMessage(BotErrors.NameTooLong.Description)
                .WithErrorCode(BotErrors.NameTooLong.Code)
                .WithState(_ => BotErrors.NameTooLong);

        RuleFor(x => x.ApiToken)
            .RequiredField(BotErrors.TokenRequired);

        RuleFor(x => x.Platform)
            .RequiredField(BotErrors.PlatformRequired)
            .Must(p => BotPlatformExtensions.From(p).IsSuccess)
                .WithMessage(BotErrors.PlatformInvalid.Description)
                .WithErrorCode(BotErrors.PlatformInvalid.Code)
                .WithState(_ => BotErrors.PlatformInvalid);
    }
}
