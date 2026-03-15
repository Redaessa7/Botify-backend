using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedStoreAssets : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    -- 1. تعريف المعرفات الأساسية
    DECLARE @MerchantId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
    DECLARE @BotId UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';

    -- 2. إعدادات المتجر (Shop Settings) - تم تحديثها لتشمل كافة الأعمدة المطلوبة
    IF NOT EXISTS (SELECT 1 FROM ShopSettings WHERE MerchantId = @MerchantId)
    BEGIN
        INSERT INTO ShopSettings (
            [Id], [MerchantId], [Name], [IndustryName], [Currency], [Language], [Address], 
            [BusinessEmail], [BusinessPhoneNumber], [LogoImageUrl], [CoverImageUrl], [TaxRate], [Description], 
            [Timezone], [DeliveryEnabled], [PickupEnabled], [MinimumOrder], [DeliveryFee], [IsOpen],
            -- روابط التواصل الاجتماعي
            [SocialMediaLinks_Whatsapp], [SocialMediaLinks_Website], [SocialMediaLinks_Facebook], 
            [SocialMediaLinks_Twitter], [SocialMediaLinks_Instagram], [SocialMediaLinks_LinkedIn],
            -- ساعات العمل (OpeningHours)
            [OpeningHours_Monday_Open], [OpeningHours_Monday_Close], [OpeningHours_Monday_IsOpen],
            [OpeningHours_Tuesday_Open], [OpeningHours_Tuesday_Close], [OpeningHours_Tuesday_IsOpen],
            [OpeningHours_Wednesday_Open], [OpeningHours_Wednesday_Close], [OpeningHours_Wednesday_IsOpen],
            [OpeningHours_Thursday_Open], [OpeningHours_Thursday_Close], [OpeningHours_Thursday_IsOpen],
            [OpeningHours_Friday_Open], [OpeningHours_Friday_Close], [OpeningHours_Friday_IsOpen],
            [OpeningHours_Saturday_Open], [OpeningHours_Saturday_Close], [OpeningHours_Saturday_IsOpen],
            [OpeningHours_Sunday_Open], [OpeningHours_Sunday_Close], [OpeningHours_Sunday_IsOpen],
            -- حقول التدقيق
            [IsDeleted], [CreatedAtUtc], [LastModifiedUtc]
        ) VALUES (
            NEWID(), @MerchantId, 'Botify Tech Store', 'Electronics', 'USD', 'en', '123 AI Street, Tech City',
            'sales@botify.com', '+967700000000', 'https://placehold.co/200', 'https://placehold.co/1200x400', 0.05, 
            'Premium AI-powered gadgets and electronics.', 'UTC', 
            1, -- DeliveryEnabled
            1, -- PickupEnabled
            10.00, 5.00, 1,
            -- Social Links
            'https://wa.me/967700000000', 'https://botify.com', 'https://fb.com/botify', 
            'https://twitter.com/botify', 'https://instgr.am/botify', 'https://linkedin.com/botify',
            -- Hours: 08:00 to 22:00
            '08:00:00', '22:00:00', 1, '08:00:00', '22:00:00', 1, '08:00:00', '22:00:00', 1,
            '08:00:00', '22:00:00', 1, '14:00:00', '23:00:00', 1, '09:00:00', '23:00:00', 1,
            '00:00:00', '00:00:00', 0, -- Sunday Closed
            0, GETUTCDATE(), GETUTCDATE());
    END

    -- 3. إعداد البوت (Bot Setup)
    IF NOT EXISTS (SELECT 1 FROM Bots WHERE Id = @BotId)
    BEGIN
        INSERT INTO Bots (Id, MerchantId, Name, Token, Platform, Secret, WebhookUrl, Active, IsPrimary, PlatformVerified, CreatedAtUtc, IsDeleted, LastModifiedUtc)
        VALUES (@BotId, @MerchantId, 'Botify Assistant', 'bot-token-xyz', 'Telegram', 'secret-key', 'https://api.botify.com/webhook', 1, 1, 1, GETUTCDATE(), 0, GETUTCDATE());
    END

-- 7. إضافة إحصائيات البوت لمدة شهر كامل (30 يوماً)
    DECLARE @Counter INT = 0;
    WHILE @Counter < 30
    BEGIN
        INSERT INTO BotDailyStats (BotId, [Date], MessageCount, OrderCount)
        VALUES (
            @BotId, 
            CAST(DATEADD(DAY, -@Counter, GETUTCDATE()) AS DATE), -- يطرح الأيام يوماً بيوم
            ABS(CHECKSUM(NEWID()) % 50) + 10,  -- توليد عدد رسائل عشوائي بين 10 و 60
            ABS(CHECKSUM(NEWID()) % 5) + 1     -- توليد عدد طلبات عشوائي بين 1 و 6
        );
        SET @Counter = @Counter + 1;
    END
    ");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DELETE FROM BotDailyStats;
            DELETE FROM Bots WHERE Id = '55555555-5555-5555-5555-555555555555';
            DELETE FROM ShopSettings WHERE MerchantId = '11111111-1111-1111-1111-111111111111';
            ");
        }
    }
}