using System.Security.Claims;
using Botify.Application.Responses;
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

    public UserInfoResponse UserInfo()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var email = User.FindFirst(ClaimTypes.Email)?.Value;
        var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToList();

        return new UserInfoResponse
        {
            Id = userId!,
            Email = email!,
            Roles = roles,
        };
    }
}