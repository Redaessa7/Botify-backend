using System;

namespace Botify.Domain.Models.Merchants.ValueObjects
{
    public record DayOpeningHours
    {
        public TimeOnly Open { get; init; }
        public TimeOnly Close { get; init; }
        public bool IsOpen { get; init; }

        private DayOpeningHours() { }
        public static DayOpeningHours Create(TimeOnly open, TimeOnly close, bool isOpen)
        {
            return new DayOpeningHours
            {
                Open = open,
                Close = close,
                IsOpen = isOpen
            };
        }
    }

    public record OpeningHours
    {
        public DayOpeningHours? Monday { get; init; } 
        public DayOpeningHours? Tuesday { get; init; } 
        public DayOpeningHours? Wednesday { get; init; } 
        public DayOpeningHours? Thursday { get; init; } 
        public DayOpeningHours? Friday { get; init; }
        public DayOpeningHours? Saturday { get; init; } 
        public DayOpeningHours? Sunday { get; init; } 
        private OpeningHours() { }

        public static OpeningHours Create(
            DayOpeningHours monday,
            DayOpeningHours tuesday,
            DayOpeningHours wednesday,
            DayOpeningHours thursday,
            DayOpeningHours friday,
            DayOpeningHours saturday,
            DayOpeningHours sunday)
        {
            return new OpeningHours
            {
                Monday = monday,
                Tuesday = tuesday,
                Wednesday = wednesday,
                Thursday = thursday,
                Friday = friday,
                Saturday = saturday,
                Sunday = sunday
            };
        }
    }
}
