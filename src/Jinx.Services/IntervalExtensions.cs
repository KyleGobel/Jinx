using System;

namespace Jinx.Services
{
    public static class IntervalExtensions
    {
        public static TimeSpan ToTimespan(this long interval)
        {
            return TimeSpan.FromMilliseconds(interval);
        }
    }
}