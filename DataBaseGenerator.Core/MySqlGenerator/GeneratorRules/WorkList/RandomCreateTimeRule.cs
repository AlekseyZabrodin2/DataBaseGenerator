using System;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList
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
