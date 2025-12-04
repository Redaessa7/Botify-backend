namespace Botify.Application.CrossCutting.Responses;

public class UserInfoResponse
{
    public string Id { get; set; } = null!;
    public string Email { get; set; } = null!;
    public List<string> Roles { get; set; } = new();
}
