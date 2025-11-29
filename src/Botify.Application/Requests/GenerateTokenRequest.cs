namespace Botify.Application.Requests;

public class GenerateTokenRequest
{
    public Guid UserId { get; set; }
     public string? Id { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Email { get; set; }
    public List<string> Roles { get; set; } = [];
    public string? OldRefreshToken { get; set; }

}