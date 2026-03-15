using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedIdentityAndMerchant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    DECLARE @MerchantUserId NVARCHAR(450) = 'U2222222-2222-2222-2222-222222222222';
    DECLARE @MerchantProfileId UNIQUEIDENTIFIER = NEWID();
    DECLARE @MerchantId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';

    -- Create Identity User
    IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = @MerchantUserId)
        INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
        VALUES (@MerchantUserId, 'merchant@botify.com', 'MERCHANT@BOTIFY.COM', 'merchant@botify.com', 'MERCHANT@BOTIFY.COM', 1, 'AQAAAAIAAYagAAAAEJ...', NEWID(), NEWID(), 0, 0, 1, 0);

    -- Create Profile
    IF NOT EXISTS (SELECT 1 FROM UserProfiles WHERE IdentityUserId = @MerchantUserId)
        INSERT INTO UserProfiles (Id, IdentityUserId, FirstName, LastName, Email, Bio, [Language], CreatedAtUtc, IsDeleted, LastModifiedUtc)
        VALUES (@MerchantProfileId, @MerchantUserId, 'Mohammed', 'Fares', 'merchant@botify.com', 'Expert in AI-driven retail solutions.', 'ar', GETUTCDATE(), 0, GETUTCDATE());

    -- Create Merchant
    IF NOT EXISTS (SELECT 1 FROM Merchants WHERE Id = @MerchantId)
        INSERT INTO Merchants (Id, Status, UserProfileId, Preferences, CreatedAtUtc, IsDeleted, LastModifiedUtc)
        VALUES (@MerchantId, 1, @MerchantProfileId, 0, GETUTCDATE(), 0, GETUTCDATE());
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
