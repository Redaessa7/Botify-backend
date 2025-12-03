using Botify.API.Extensions;
using Botify.Application.Services.Merchants;
using Botify.Domain.Models.Merchants.Bots;
using Botify.Domain.Models.Merchants.ValueObjects;
using FluentValidation;
using  Botify.Application.Services.Merchants.Requests;

namespace Botify.API.Validators;

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
            .Must(p => BotPlatform.From(p).IsSuccess)
                .WithMessage(BotErrors.PlatformInvalid.Description)
                .WithErrorCode(BotErrors.PlatformInvalid.Code)
                .WithState(_ => BotErrors.PlatformInvalid);
    }
}
