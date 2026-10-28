using System;

namespace DataBaseGenerator.Core.GeneratorRules.Series
{
    public sealed class RangeSeriesDateTimeRule : IGeneratorRule<DateTime>
    {

        public RangeSeriesDateTimeRule(DateTime startDateTime, DateTime endDateTime)
        {
            MinSeriesTime = startDateTime;
            MaxSeriesTime = endDateTime;
        }

        public DateTime MaxSeriesTime { get; }
        public DateTime MinSeriesTime { get; }

        public DateTime Generate()
        {
            var start = MinSeriesTime.TimeOfDay;
            var end = MaxSeriesTime.TimeOfDay;
            if (end <= start)
            {
                throw new ArgumentException("Minimum time must be less than maximum time");
            }

            var random = new Random();
            var offsetTicks = (long)(random.NextDouble() * (end - start).Ticks);

            return MinSeriesTime.Date.Add(start).AddTicks(offsetTicks);
        }
    }
}
