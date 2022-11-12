namespace Agora.Shared.Extensions
{
    public static class DateTimeExtensions
    {
        public static DateTime OverrideStartDate(this DateTime endDate, DateTime currentDate, (DayOfWeek Weekday, TimeSpan Time)[] overrides, TimeSpan duration)
        {
            var index = Array.FindIndex(overrides, x => x.Weekday == endDate.DayOfWeek && x.Time == endDate.TimeOfDay);
            var (Weekday, Time) = index == 0 ? overrides.Last() : overrides[index - 1];
            var nextSchedule = currentDate.Next(Weekday, Time);
            var earliestTime = currentDate < nextSchedule ? currentDate : nextSchedule;
            var desiredTime = endDate.Subtract(duration);

            return desiredTime > earliestTime ? desiredTime : earliestTime;
        }

        public static DateTime OverrideEndDate(this DateTime currentDateTime, (DayOfWeek Weekday, TimeSpan Time)[] overrides)
        {
            foreach (var (Weekday, Time) in overrides)
            {
                if (currentDateTime.DayOfWeek == Weekday && currentDateTime.TimeOfDay < Time)
                    return currentDateTime.WithTime(Time);

                if (currentDateTime.DayOfWeek < Weekday)
                    return currentDateTime.Next(Weekday, Time);
            }

            return currentDateTime.Next(overrides[0].Weekday, overrides[0].Time);
        }

        public static DateTime WithTime(this DateTime dateTime, TimeSpan time)
        {
            return new DateTime(dateTime.Year, dateTime.Month, dateTime.Day).Add(time);
        }

        public static DateTime Next(this DateTime from, DayOfWeek dayOfTheWeek, TimeSpan time)
        {
            var date = from.Date.AddDays(1);
            var days = ((int)dayOfTheWeek - (int)date.DayOfWeek + 7) % 7;
            return date.AddDays(days).Add(time);
        }
    }
}
