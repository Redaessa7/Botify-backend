namespace Botify.Application.Services.Merchants.Requests
{
    public class CreateUserProfileRequest
    {
        public CreateUserProfileRequest(
            string firstName,
            string lastName,
            string email,
            string? bio,
            string? profileImageUrl,
            string? language)
        {
            FirstName = firstName;
            LastName = lastName;
            Email = email;
            Bio = bio;
            ProfileImageUrl = profileImageUrl;
            Language = language;
        }

        public string FirstName { get; }
        public string LastName { get; }
        public string Email { get; }  
        public string? Bio { get; }
        public string? ProfileImageUrl { get; }
        public string? Language { get; }
    }
}
