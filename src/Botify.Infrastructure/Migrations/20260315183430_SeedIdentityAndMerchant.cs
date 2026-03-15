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
    DECLARE @RoleId NVARCHAR(450) = 'R3333333-3333-3333-3333-333333333333';

    -- 1. إنشاء دور التاجر (AspNetRoles)
    IF NOT EXISTS (SELECT 1 FROM AspNetRoles WHERE Id = @RoleId)
        INSERT INTO AspNetRoles (Id, Name, NormalizedName, ConcurrencyStamp)
        VALUES (@RoleId, 'Merchant', 'MERCHANT', NEWID());

    -- 2. إنشاء مستخدم الهوية (AspNetUsers)
    IF NOT EXISTS (SELECT 1 FROM AspNetUsers WHERE Id = @MerchantUserId)
        INSERT INTO AspNetUsers (Id, UserName, NormalizedUserName, Email, NormalizedEmail, EmailConfirmed, PasswordHash, SecurityStamp, ConcurrencyStamp, PhoneNumberConfirmed, TwoFactorEnabled, LockoutEnabled, AccessFailedCount)
        VALUES (@MerchantUserId, 'merchant@botify.com', 'MERCHANT@BOTIFY.COM', 'merchant@botify.com', 'MERCHANT@BOTIFY.COM', 1, 'AQAAAAIAAYagAAAAEJ...', NEWID(), NEWID(), 0, 0, 1, 0);

    -- 3. ربط المستخدم بالدور (AspNetUserRoles)
    IF NOT EXISTS (SELECT 1 FROM AspNetUserRoles WHERE UserId = @MerchantUserId AND RoleId = @RoleId)
        INSERT INTO AspNetUserRoles (UserId, RoleId)
        VALUES (@MerchantUserId, @RoleId);

    -- 4. إنشاء الملف الشخصي (UserProfiles)
    IF NOT EXISTS (SELECT 1 FROM UserProfiles WHERE IdentityUserId = @MerchantUserId)
        INSERT INTO UserProfiles (Id, IdentityUserId, FirstName, LastName, Email, Bio, [Language], CreatedAtUtc, IsDeleted, LastModifiedUtc)
        VALUES (@MerchantProfileId, @MerchantUserId, 'Mohammed', 'Fares', 'merchant@botify.com', 'Expert in AI-driven retail solutions.', 'ar', GETUTCDATE(), 0, GETUTCDATE());

    -- 5. إنشاء التاجر (Merchants)
    IF NOT EXISTS (SELECT 1 FROM Merchants WHERE Id = @MerchantId)
        INSERT INTO Merchants (Id, Status, UserProfileId, Preferences, CreatedAtUtc, IsDeleted, LastModifiedUtc)
        VALUES (@MerchantId, 1, @MerchantProfileId, 0, GETUTCDATE(), 0, GETUTCDATE());

    -- 6. إنشاء اشتراك التاجر (MerchantSubscriptions)
    IF NOT EXISTS (SELECT 1 FROM MerchantSubscriptions WHERE MerchantId = @MerchantId)
        INSERT INTO MerchantSubscriptions (MerchantId, CurrentPlan, ExpiryDate)
        VALUES (@MerchantId, 0, DATEADD(YEAR, 1, GETUTCDATE()));
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DELETE FROM AspNetUserRoles WHERE UserId = 'U2222222-2222-2222-2222-222222222222';
            DELETE FROM MerchantSubscriptions WHERE MerchantId = '11111111-1111-1111-1111-111111111111';
            DELETE FROM Merchants WHERE Id = '11111111-1111-1111-1111-111111111111';
            DELETE FROM UserProfiles WHERE Email = 'merchant@botify.com';
            DELETE FROM AspNetUsers WHERE Id = 'U2222222-2222-2222-2222-222222222222';
            DELETE FROM AspNetRoles WHERE Name = 'Merchant';
            ");
        }
    }
}