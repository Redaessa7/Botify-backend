using Microsoft.AspNetCore.Http;

namespace Botify.Application.CrossCutting.Requests;

public class RegisterRequest
{
    public required string Email { get; set; }
    public required string Password { get; set; }
    public required string Phone { get; set; }
    public required string FirstName { get; set; }
    public required string LastName { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }
    public string? Language { get; set; }
}
