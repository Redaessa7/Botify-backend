using Botify.Domain.Models.Merchants.ValueObjects;
using System;
using System.Collections.Generic;
using System.Text;

namespace Botify.Application.Features.Merchants.Requests
{

        /// <summary>
        /// Request DTO for creating or updating a shop.
        /// Mirrors ShopDto contract.
        /// </summary>
        public sealed class SetShopRequest
        {
            public string ShopName { get; set; } = default!;
            public string? IndustryName { get; set; }
            public string Currency { get; set; } = default!;
            public string Language { get; set; } = default!;
            public string Address { get; set; } = default!;
            public string? BusinessEmail { get; set; }
            public string? BusinessPhoneNumber { get; set; }
            public SocialMediaLinks? SocialMediaLinks { get; set; }
            public decimal? TaxRate { get; set; }
            public string? Description { get; set; }
            public string? LogoImageUrl { get; set; }
            public string? CoverImageUrl { get; set; }
            public string? Timezone { get; set; }
            public bool DeliveryEnabled { get; set; }
            public bool PickupEnabled { get; set; }
            public decimal? MinimumOrder { get; set; }
            public decimal? DeliveryFee { get; set; }
            public bool IsOpen { get; set; }
            public OpeningHoursDto? OpeningHours { get; set; }
        }
    /// <summary>
    /// DTO for binding OpeningHours from JSON requests.
    /// </summary>
    public sealed class OpeningHoursDto
    {
        public DayOpeningHoursDto? Monday { get; set; }
        public DayOpeningHoursDto? Tuesday { get; set; }
        public DayOpeningHoursDto? Wednesday { get; set; }
        public DayOpeningHoursDto? Thursday { get; set; }
        public DayOpeningHoursDto? Friday { get; set; }
        public DayOpeningHoursDto? Saturday { get; set; }
        public DayOpeningHoursDto? Sunday { get; set; }
    }

    /// <summary>
    /// DTO for binding a single day's opening hours.
    /// </summary>
    public sealed class DayOpeningHoursDto
    {
        public string? Open { get; set; }   // e.g. "09:00"
        public string? Close { get; set; }  // e.g. "17:00"
        public bool IsOpen { get; set; }
    }
}
