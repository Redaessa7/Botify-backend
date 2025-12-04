namespace Botify.Application.Services.Merchants.Requests
{
    public class CreateUserProfileRequest
    {
        public CreateUserProfileRequest(string firstName, string lastName, string? bio, string? profileImageUrl, string? language)
        {
            FirstName = firstName;
            LastName = lastName;
            Bio = bio;
            ProfileImageUrl = profileImageUrl;
            Language = language;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public string? Bio { get; }
        public string? ProfileImageUrl { get; }
        public string? Language { get; }
    }

}
