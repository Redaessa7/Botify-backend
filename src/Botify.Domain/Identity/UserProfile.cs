using Botify.Domain.Models;
using Botify.Domain.ResultPattern;

namespace Botify.Domain.Identity;

public sealed class UserProfile : AuditableEntity
{
    public string IdentityUserId { get; private set; } = null!;
    public string FirstName { get; private set; } = null!;
    public string LastName { get; private set; } = null!;
    public string Email { get; private set; } = null!;
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
        string email,
        string? bio,
        string? profileImageUrl,
        string? language,
        string? timeZone
    ) : base(id)
    {
        IdentityUserId = identityUserId;
        FirstName = firstName;
        LastName = lastName;
        Email = email;
        Bio = bio;
        Language = language;
        TimeZone = timeZone;
        ProfileImageUrl = profileImageUrl;
    }

    public static Result<UserProfile> Create(
        Guid id,
        string identityUserId,
        string firstName,
        string lastName,
        string email,
        string? bio = null,
        string? profileImageUrl = null,
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

        if (string.IsNullOrWhiteSpace(email))
            return UserProfileErrors.EmailRequired;

        var profile = new UserProfile(
            id,
            identityUserId,
            firstName,
            lastName,
            email,
            bio,
            profileImageUrl ?? string.Empty,
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

    public Result<Success> UpdateEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
            return UserProfileErrors.EmailRequired;

        Email = email;
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
