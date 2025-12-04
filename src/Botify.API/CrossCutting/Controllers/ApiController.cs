using Botify.Domain.ResultPattern;
using Microsoft.AspNetCore.Mvc;

namespace Botify.API.CrossCutting.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    protected ActionResult Problem(List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Problem();
        }

        // If all are validation errors, return 400 with the first code in detail
        if (errors.All(error => error.Type == ErrorKind.Validation))
        {
            return ValidationProblem(errors);
        }

        // Otherwise return the first error
        return Problem(errors[0]);
    }

    private ObjectResult Problem(Error error)
    {
        var statusCode = error.Type switch
        {
            ErrorKind.Conflict => StatusCodes.Status409Conflict,
            ErrorKind.Validation => StatusCodes.Status400BadRequest,
            ErrorKind.NotFound => StatusCodes.Status404NotFound,
            ErrorKind.Unauthorized => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        var problemDetails = new ProblemDetails
        {
            Status = statusCode,
            Title = error.Description,
            Detail = error.Code, 
            Instance = HttpContext.Request.Path
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = statusCode
        };
    }

    private ActionResult ValidationProblem(List<Error> errors)
    {
        var firstError = errors.First();

        var problemDetails = new ProblemDetails
        {
            Title = "Validation error occurred.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Detail = firstError.Code, // ✅ only ErrorCode in detail
            Instance = HttpContext.Request.Path
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }


}