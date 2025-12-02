using Botify.Domain.ResultPattern;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;

namespace Botify.API.Controllers;

[ApiController]
public class ApiController : ControllerBase
{
    protected ActionResult Problem(List<Error> errors)
    {
        if (errors.Count is 0)
        {
            return Problem();
        }

        return errors.All(error => error.Type == ErrorKind.Validation) ? ValidationProblem(errors) : Problem(errors[0]);
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

        return Problem(statusCode: statusCode, title: error.Description, detail: error.Code);
    }
    private ActionResult ValidationProblem(List<Error> errors)
    {
        var errorDictionary = new Dictionary<string, string[]>();

        foreach (var error in errors)
        {
            if (!errorDictionary.ContainsKey(error.Code))
            {
                errorDictionary[error.Code] = Array.Empty<string>();
            }

            errorDictionary[error.Code] = errorDictionary[error.Code].Append(error.Description).ToArray();
        }

        var problemDetails = new ValidationProblemDetails(errorDictionary)
        {
            Title = "One or more validation errors occurred.",
            Status = StatusCodes.Status400BadRequest,
            Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
            Instance = HttpContext.Request.Path
        };

        return new ObjectResult(problemDetails)
        {
            StatusCode = problemDetails.Status
        };
    }

    //private ActionResult ValidationProblem(List<Error> errors)
    //{
    //    var modelStateDictionary = new ModelStateDictionary();

    //    errors.ForEach(error => modelStateDictionary.AddModelError(error.Code, error.Description));

    //    return ValidationProblem(modelStateDictionary);
    //}
}