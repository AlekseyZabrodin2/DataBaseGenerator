using System;

namespace DataBaseGenerator.Core.GeneratorRules.Study
{
    public sealed class RangeStudyDateTimeRule : IGeneratorRule<DateTime>
    {
        public RangeStudyDateTimeRule(DateTime minimumDateTime, DateTime maximumDateTime, TimeSpan minimumTime, TimeSpan maximumTime)
        {
            if (maximumDateTime.TimeOfDay == TimeSpan.Zero)
                maximumDateTime = maximumDateTime.Date.AddDays(1).AddTicks(-1);

            if (minimumDateTime.Date > maximumDateTime.Date)
            {
                throw new ArgumentException("Minimum date and time must be less then maximum date and time");
            }

            if (maximumTime <= minimumTime)
            {
                throw new ArgumentException("Minimum time must be less than maximum time");
            }

            MinimumDateTime = minimumDateTime;
            MaximumDateTime = maximumDateTime;
            MinimumTime = minimumTime;
            MaximumTime = maximumTime;
        }

        public DateTime MinimumDateTime { get; }

        public DateTime MaximumDateTime { get; }

        public TimeSpan MinimumTime { get; }

        public TimeSpan MaximumTime { get; }

        public DateTime Generate()
        {
            var random = new Random();
            var dayCount = (MaximumDateTime.Date - MinimumDateTime.Date).Days;
            var date = MinimumDateTime.Date.AddDays(random.Next(0, dayCount + 1));
            var offsetTicks = (long)(random.NextDouble() * (MaximumTime - MinimumTime).Ticks);

            return date.Add(MinimumTime).AddTicks(offsetTicks);
        }
    }
}
