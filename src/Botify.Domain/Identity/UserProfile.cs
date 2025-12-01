using Botify.Domain.Models;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Identity;

public sealed class UserProfile : AuditableEntity
{
    public string IdentityUserId { get; private set; } = null!;

    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string? ProfileImageUrl { get; private set; }
    public string? Bio { get; private set; }
    public string? Language { get; private set; }
    public string? TimeZone { get; private set; }

    private UserProfile() { }

    private UserProfile(
        Guid id,
        string identityUserId,
        string firstName,
        string lastName,
        string? bio,
        string? language,
        string? timeZone
    ) : base(id)
    {
        IdentityUserId = identityUserId;
        FirstName = firstName;
        LastName = lastName;
        Bio = bio;
        Language = language;
        TimeZone = timeZone;
    }

    public static Result<UserProfile> Create(
        Guid id,
        string identityUserId,
        string firstName,
        string lastName,
        string? bio = null,
        string? language = null,
        string? timeZone = null
    )
    {
        if (string.IsNullOrWhiteSpace(identityUserId))
            return UserProfileErrors.IdentityUserIdRequired;

        if (string.IsNullOrWhiteSpace(firstName))
            return UserProfileErrors.FirstNameRequired;

        if (string.IsNullOrWhiteSpace(lastName))
            return UserProfileErrors.LastNameRequired;

        var profile = new UserProfile(
            id,
            identityUserId,
            firstName,
            lastName,
            bio,
            language,
            timeZone
        );

        return profile;
    }

    public Result<Success> UpdateName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName))
            return UserProfileErrors.FirstNameRequired;

        if (string.IsNullOrWhiteSpace(lastName))
            return UserProfileErrors.LastNameRequired;

        FirstName = firstName;
        LastName = lastName;
        

        return Result.Success;
    }

    public Result<Success> UpdateBio(string? bio)
    {
        Bio = bio?.Trim();
        
        return Result.Success;
    }

    public Result<Success> UpdateProfileImage(string? imageUrl)
    {
        ProfileImageUrl = imageUrl;
        
        return Result.Success;
    }

    public Result<Success> UpdateLanguage(string? language)
    {
        Language = language;
        
        return Result.Success;
    }

    public Result<Success> UpdateTimeZone(string? timeZone)
    {
        TimeZone = timeZone;
        
        return Result.Success;
    }
}
