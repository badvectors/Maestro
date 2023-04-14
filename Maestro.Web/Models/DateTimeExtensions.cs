using System;

namespace Maestro.Web.Models
{
    public static class DateTimeExtensions
    {
        public static DateTime RoundToTicks(this DateTime target, long ticks) => new DateTime((target.Ticks + ticks / 2) / ticks * ticks, target.Kind);
        public static DateTime RoundUpToTicks(this DateTime target, long ticks) => new DateTime((target.Ticks + ticks - 1) / ticks * ticks, target.Kind);
        public static DateTime RoundDownToTicks(this DateTime target, long ticks) => new DateTime(target.Ticks / ticks * ticks, target.Kind);

        public static DateTime Round(this DateTime target, TimeSpan round) => RoundToTicks(target, round.Ticks);
        public static DateTime RoundUp(this DateTime target, TimeSpan round) => RoundUpToTicks(target, round.Ticks);
        public static DateTime RoundDown(this DateTime target, TimeSpan round) => RoundDownToTicks(target, round.Ticks);

        public static DateTime RoundToMinutes(this DateTime target, int minutes = 1) => RoundToTicks(target, minutes * TimeSpan.TicksPerMinute);
        public static DateTime RoundUpToMinutes(this DateTime target, int minutes = 1) => RoundUpToTicks(target, minutes * TimeSpan.TicksPerMinute);
        public static DateTime RoundDownToMinutes(this DateTime target, int minutes = 1) => RoundDownToTicks(target, minutes * TimeSpan.TicksPerMinute);

        public static DateTime RoundToHours(this DateTime target, int hours = 1) => RoundToTicks(target, hours * TimeSpan.TicksPerHour);
        public static DateTime RoundUpToHours(this DateTime target, int hours = 1) => RoundUpToTicks(target, hours * TimeSpan.TicksPerHour);
        public static DateTime RoundDownToHours(this DateTime target, int hours = 1) => RoundDownToTicks(target, hours * TimeSpan.TicksPerHour);

        public static DateTime RoundToDays(this DateTime target, int days = 1) => RoundToTicks(target, days * TimeSpan.TicksPerDay);
        public static DateTime RoundUpToDays(this DateTime target, int days = 1) => RoundUpToTicks(target, days * TimeSpan.TicksPerDay);
        public static DateTime RoundDownToDays(this DateTime target, int days = 1) => RoundDownToTicks(target, days * TimeSpan.TicksPerDay);
    }
}
