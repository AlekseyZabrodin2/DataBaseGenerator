using System;

namespace DataBaseGenerator.Core.GeneratorRules.WorkList
{
    public sealed class RandomCreateTimeRule : IGeneratorRule<TimeSpan>
    {
        public TimeSpan Generate()
        {
            TimeSpan timeNow = DateTime.Now.TimeOfDay;

            return timeNow;
        }
    }
}
