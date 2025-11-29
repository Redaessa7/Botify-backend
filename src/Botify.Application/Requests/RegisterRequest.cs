using Botify.Domain.Identity.Enums;

namespace Botify.Application.Requests;

public class RegisterRequest
{
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string Password { get; set; } = default!;
    public Roles Role { get; set; } = Roles.User;
    public string? ShopName { get; set; } 
}
