using Botify.Domain.ResultPattern;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace Botify.API.Filters;

public class FluentValidationFilter : IAsyncActionFilter
{
    private readonly IServiceProvider _serviceProvider;

    public FluentValidationFilter(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
    {
        var errors = new List<Error>();

        foreach (var arg in context.ActionArguments.Values)
        {
            if (arg is null) continue;

            // Resolve the correct validator for the argument type
            var validatorType = typeof(IValidator<>).MakeGenericType(arg.GetType());
            var validator = _serviceProvider.GetService(validatorType) as IValidator;
            if (validator is null) continue;

            var result = await validator.ValidateAsync(new ValidationContext<object>(arg));
            if (!result.IsValid)
            {
                errors.AddRange(result.Errors.Select(f =>
                    f.CustomState is Error error
                        ? error
                        : Error.Validation(f.ErrorCode ?? f.PropertyName, f.ErrorMessage)
                ));
            }
        }

        if (errors.Count > 0)
        {
            var errorDictionary = errors
                .GroupBy(e => e.Code)
                .ToDictionary(g => g.Key, g => g.Select(e => e.Description).ToArray());

            var problemDetails = new ValidationProblemDetails(errorDictionary)
            {
                Title = "One or more validation errors occurred.",
                Status = StatusCodes.Status400BadRequest,
                Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                Instance = context.HttpContext.Request.Path
            };

            context.Result = new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            };
            return;
        }

        await next();
    }
}
