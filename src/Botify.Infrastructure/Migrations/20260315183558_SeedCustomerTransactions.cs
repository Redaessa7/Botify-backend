using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Botify.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SeedCustomerTransactions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
    -- 1. الإعدادات الأساسية والمعرفات
    DECLARE @MerchantId UNIQUEIDENTIFIER = '11111111-1111-1111-1111-111111111111';
    DECLARE @BotId UNIQUEIDENTIFIER = '55555555-5555-5555-5555-555555555555';

    -- معرفات العملاء
    DECLARE @C1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @C2 UNIQUEIDENTIFIER = NEWID();
    DECLARE @C3 UNIQUEIDENTIFIER = NEWID();
    DECLARE @C4 UNIQUEIDENTIFIER = NEWID();
    DECLARE @C5 UNIQUEIDENTIFIER = NEWID();

    -- معرفات المنتجات لضمان ترابط البيانات (Relational Integrity)
    DECLARE @P1 UNIQUEIDENTIFIER = NEWID(); 
    DECLARE @P2 UNIQUEIDENTIFIER = NEWID(); 
    DECLARE @P3 UNIQUEIDENTIFIER = NEWID(); 
    DECLARE @P4 UNIQUEIDENTIFIER = NEWID(); 
    DECLARE @P5 UNIQUEIDENTIFIER = NEWID(); 

    -- 2. إدخال المنتجات (تم إضافة الوصف لضمان عدم وجود NULL)
    INSERT INTO Products (Id, MerchantId, Name, Price, Category, StockQuantity, [Description], ImageUrl, IsActive, CreatedAtUtc, IsDeleted, LastModifiedUtc)
    VALUES 
    (@P1, @MerchantId, 'Ultra Phone Case', 15.00, 'Accessories', 100, 'Premium shockproof crystal clear case', 'https://placehold.co/400?text=Case', 1, GETUTCDATE(), 0, GETUTCDATE()),
    (@P2, @MerchantId, 'Wireless Earbuds Pro', 55.00, 'Audio', 45, 'High-quality wireless earbuds with noise cancellation', 'https://placehold.co/400?text=Earbuds', 1, GETUTCDATE(), 0, GETUTCDATE()),
    (@P3, @MerchantId, 'Smart Watch Series X', 120.00, 'Wearables', 30, 'Advanced fitness and health tracking', 'https://placehold.co/400?text=Watch', 1, GETUTCDATE(), 0, GETUTCDATE()),
    (@P4, @MerchantId, 'Noise Cancelling Headphones', 199.00, 'Audio', 10, 'Professional studio grade sound', 'https://placehold.co/400?text=Headphones', 1, GETUTCDATE(), 0, GETUTCDATE()),
    (@P5, @MerchantId, 'Precision Gaming Mouse', 45.00, 'PC Peripherals', 50, 'High precision 16000 DPI gaming mouse', 'https://placehold.co/400?text=Mouse', 1, GETUTCDATE(), 0, GETUTCDATE());

    -- 3. إضافة العملاء
    INSERT INTO Customers (Id, MerchantId, Uuid, Name, Email, PhoneNumber, CreatedAtUtc, IsDeleted, LastModifiedUtc) VALUES 
    (@C1, @MerchantId, 'CH-9001', 'Ahmed Ali', 'ahmed@email.com', '+967711111111', GETUTCDATE(), 0, GETUTCDATE()),
    (@C2, @MerchantId, 'CH-9002', 'Sara Mansour', 'sara@email.com', '+967722222222', GETUTCDATE(), 0, GETUTCDATE()),
    (@C3, @MerchantId, 'CH-9003', 'Khaled Hassan', 'khaled@email.com', '+967733333333', GETUTCDATE(), 0, GETUTCDATE()),
    (@C4, @MerchantId, 'CH-9004', 'Reem Saleh', 'reem@email.com', '+967744444444', GETUTCDATE(), 0, GETUTCDATE()),
    (@C5, @MerchantId, 'CH-9005', 'Faisal Omar', 'faisal@email.com', '+967755555555', GETUTCDATE(), 0, GETUTCDATE());

    -- 4. إضافة الطلبات (تم إضافة CustomerContact, Platform, Currency لضمان عدم وجود NULL)
-- 4. إضافة الطلبات (Orders) مع العناوين وكافة الحقول الإلزامية
    DECLARE @O1 UNIQUEIDENTIFIER = NEWID();
    DECLARE @O2 UNIQUEIDENTIFIER = NEWID();
    DECLARE @O3 UNIQUEIDENTIFIER = NEWID();
    DECLARE @O4 UNIQUEIDENTIFIER = NEWID();
    DECLARE @O5 UNIQUEIDENTIFIER = NEWID();

    INSERT INTO Orders (
        Id, MerchantId, CustomerId, Uuid, CustomerName, CustomerContact, 
        Platform, TotalAmount, Currency, Status, DeliveryAddress, 
        CreatedAtUtc, IsDeleted, LastModifiedUtc
    ) VALUES 
    (@O1, @MerchantId, @C1, 'ORD-5001', 'Ahmed Ali', '+967711111111', 'Telegram', 70.00, 'USD', 'Pending', 'Yemen, Sanaa, Hadda St.', GETUTCDATE(), 0, GETUTCDATE()),
    (@O2, @MerchantId, @C2, 'ORD-5002', 'Sara Mansour', '+967722222222', 'Telegram', 120.00, 'USD', 'Processing', 'Yemen, Aden, Crater', GETUTCDATE(), 0, GETUTCDATE()),
    (@O3, @MerchantId, @C3, 'ORD-5003', 'Khaled Hassan', '+967733333333', 'Telegram', 15.00, 'USD', 'Shipped', 'Yemen, Taiz, Jamal St.', GETUTCDATE(), 0, GETUTCDATE()),
    (@O4, @MerchantId, @C4, 'ORD-5004', 'Reem Saleh', '+967744444444', 'Telegram', 254.00, 'USD', 'Delivered', 'Yemen, Mukalla, Al-Kornish', GETUTCDATE(), 0, GETUTCDATE()),
    (@O5, @MerchantId, @C5, 'ORD-5005', 'Faisal Omar', '+967755555555', 'Telegram', 45.00, 'USD', 'Pending', 'Yemen, Ibb, City Center', GETUTCDATE(), 0, GETUTCDATE());
    -- 5. إضافة تفاصيل الطلبات (Order Items) - متوافق مع الجدول بدون عمود Id
    INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice) VALUES 
    (@O1, @P1, 'Ultra Phone Case', 1, 15.00),
    (@O1, @P2, 'Wireless Earbuds Pro', 1, 55.00),
    (@O2, @P3, 'Smart Watch Series X', 1, 120.00),
    (@O3, @P1, 'Ultra Phone Case', 1, 15.00),
    (@O4, @P4, 'Noise Cancelling Headphones', 1, 199.00),
    (@O4, @P2, 'Wireless Earbuds Pro', 1, 55.00),
    (@O5, @P5, 'Precision Gaming Mouse', 1, 45.00);

    -- 6. إضافة طلبات الدعم (Support Requests)
    INSERT INTO SupportRequests (
        [Id], [MerchantId], [BotId], [CustomerId], [BotName], [Platform], 
        [PlatformContact], [CustomerName], [Type], [Reason], [Message], 
        [IsResolved], [ResolvedAt], [CreatedAtUtc], [IsDeleted], [LastModifiedUtc]
    ) VALUES 
    (NEWID(), @MerchantId, @BotId, @C1, 'Botify Assistant', 'Telegram', '967711', 'Ahmed Ali', 'General', 0, 'Is my order on the way?', 1, GETUTCDATE(), GETUTCDATE(), 0, GETUTCDATE()),
    (NEWID(), @MerchantId, @BotId, @C2, 'Botify Assistant', 'Telegram', '967722', 'Sara Mansour', 'Technical', 2, 'Can I change my delivery address?', 0, NULL, GETUTCDATE(), 0, GETUTCDATE()),
    (NEWID(), @MerchantId, @BotId, @C3, 'Botify Assistant', 'Telegram', '967733', 'Khaled Hassan', 'General', 0, 'Do you have the phone case in Blue?', 1, GETUTCDATE(), GETUTCDATE(), 0, GETUTCDATE()),
    (NEWID(), @MerchantId, @BotId, @C4, 'Botify Assistant', 'Telegram', '967744', 'Reem Saleh', 'General', 0, 'The headphones quality is amazing, thank you!', 1, GETUTCDATE(), GETUTCDATE(), 0, GETUTCDATE()),
    (NEWID(), @MerchantId, @BotId, @C5, 'Botify Assistant', 'Telegram', '967755', 'Faisal Omar', 'Order', 1, 'I received the wrong mouse model.', 0, NULL, GETUTCDATE(), 0, GETUTCDATE());
    ");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(@"
            DELETE FROM OrderItems WHERE OrderId IN (SELECT Id FROM Orders WHERE MerchantId = '11111111-1111-1111-1111-111111111111');
            DELETE FROM SupportRequests WHERE MerchantId = '11111111-1111-1111-1111-111111111111';
            DELETE FROM Orders WHERE MerchantId = '11111111-1111-1111-1111-111111111111';
            DELETE FROM Customers WHERE MerchantId = '11111111-1111-1111-1111-111111111111';
            DELETE FROM Products WHERE MerchantId = '11111111-1111-1111-1111-111111111111';
            ");
        }
    }
}