using Botify.Application.Features.Merchants.Requests;
using Botify.Application.Features.Merchants.Responses;
using Botify.Domain.Models.Merchants;
using Botify.Domain.Models.Merchants.ValueObjects;

namespace Botify.Application.Services.Merchants
{
    public static class MerchantMappingExtensions
    {
        public static OpeningHours ToDomain(this OpeningHoursDto dto)
        {
            return OpeningHours.Create(
                dto.Monday != null ? DayOpeningHours.Create(TimeOnly.Parse(dto.Monday.Open!), TimeOnly.Parse(dto.Monday.Close!), dto.Monday.IsOpen) : DayOpeningHours.Create(new TimeOnly(0, 0), new TimeOnly(0, 0), false),
                dto.Tuesday != null ? DayOpeningHours.Create(TimeOnly.Parse(dto.Tuesday.Open!), TimeOnly.Parse(dto.Tuesday.Close!), dto.Tuesday.IsOpen) : DayOpeningHours.Create(new TimeOnly(0, 0), new TimeOnly(0, 0), false),
                dto.Wednesday != null ? DayOpeningHours.Create(TimeOnly.Parse(dto.Wednesday.Open!), TimeOnly.Parse(dto.Wednesday.Close!), dto.Wednesday.IsOpen) : DayOpeningHours.Create(new TimeOnly(0, 0), new TimeOnly(0, 0), false),
                dto.Thursday != null ? DayOpeningHours.Create(TimeOnly.Parse(dto.Thursday.Open!), TimeOnly.Parse(dto.Thursday.Close!), dto.Thursday.IsOpen) : DayOpeningHours.Create(new TimeOnly(0, 0), new TimeOnly(0, 0), false),
                dto.Friday != null ? DayOpeningHours.Create(TimeOnly.Parse(dto.Friday.Open!), TimeOnly.Parse(dto.Friday.Close!), dto.Friday.IsOpen) : DayOpeningHours.Create(new TimeOnly(0, 0), new TimeOnly(0, 0), false),
                dto.Saturday != null ? DayOpeningHours.Create(TimeOnly.Parse(dto.Saturday.Open!), TimeOnly.Parse(dto.Saturday.Close!), dto.Saturday.IsOpen) : DayOpeningHours.Create(new TimeOnly(0, 0), new TimeOnly(0, 0), false),
                dto.Sunday != null ? DayOpeningHours.Create(TimeOnly.Parse(dto.Sunday.Open!), TimeOnly.Parse(dto.Sunday.Close!), dto.Sunday.IsOpen) : DayOpeningHours.Create(new TimeOnly(0, 0), new TimeOnly(0, 0), false)
            );
        }
        public static CurrentMerchantResponse ToCurrentResponse(this Merchant merchant)
        {
            return new CurrentMerchantResponse
            {
                merchantId = merchant.Id,
                Status = merchant.Status,
                VerifiedAt = merchant.VerifiedAt,

                FirstName = merchant.Profile.FirstName,
                LastName = merchant.Profile.LastName,
                Email = merchant.Profile.Email,
                ProfileImageUrl = merchant.Profile.ProfileImageUrl,
                Bio = merchant.Profile.Bio,
                Language = merchant.Profile.Language,

                CurrentPlan = merchant.Subscription.CurrentPlan,
                ExpiryDate = merchant.Subscription.ExpiryDate
            };
        }
    }
}
