using Botify.Application.Services.Merchants;
using Botify.Domain.Models.Merchants.ValueObjects;
using FluentValidation;
using Botify.Application.Services.Merchants.Requests;
using Botify.API.CrossCutting.Extensions;
using Botify.Domain.Models.Bots;

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

    }
}
