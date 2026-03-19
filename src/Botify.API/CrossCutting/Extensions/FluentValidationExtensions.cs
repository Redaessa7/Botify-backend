using Botify.Domain.ResultPattern;
using FluentValidation;

namespace Botify.API.CrossCutting.Extensions
{
    public static class FluentValidationExtensions
    {
        public static IRuleBuilderOptions<T, string> RequiredField<T>(
            this IRuleBuilder<T, string> rule,
            Error error
        )
        {
            return rule
                .NotNull()
                    .WithMessage(error.Description)
                    .WithErrorCode(error.Code)
                    .WithState(_ => error)
                .NotEmpty()
                    .WithMessage(error.Description)
                    .WithErrorCode(error.Code)
                    .WithState(_ => error);
        }
    }
}
