using System;

namespace Botify.Domain.Models.Merchants.Enums
{
    /// <summary>
    /// Quick actions available for merchants and bots.
    /// Supports bitwise combination for multiple actions.
    /// </summary>
    [Flags]
    public enum QuickActions
    {
        None = 0,

        // Bot creation
        CreateBot = 1 << 0,
        SetUpMessagingBot = 1 << 1,

        // Orders
        ViewOrders = 1 << 2,
        CheckRecentOrders = 1 << 3,

        // Customer service
        AnswerQuestions = 1 << 4,
        RespondToCustomers = 1 << 5,

        // Products
        AddProduct = 1 << 6,
        ExpandCatalog = 1 << 7,

        // AI configuration
        AISettings = 1 << 8,
        ConfigureAIBehavior = 1 << 9,

        // Notifications
        Notifications = 1 << 10,
        ManageAlerts = 1 << 11,

        // Reporting
        ViewReports = 1 << 12,
        AnalyticsDashboard = 1 << 13,

        // Services
        CustomServices = 1 << 14,
        ManageEscalations = 1 << 15
    }
}
