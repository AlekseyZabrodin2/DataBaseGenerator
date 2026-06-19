using System;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Study
{
    public sealed class RandomStudyDateTimeRule : IGeneratorRule<DateTime>
    {
        private static readonly Random _random = new Random();

        public DateTime Generate()
        {
            var now = DateTime.Now;
            
            var daysBack = _random.Next(0, 30);
            
            var hours = _random.Next(0, 24);
            var minutes = _random.Next(0, 60);
            var seconds = _random.Next(0, 60);

            var date = now
                .AddDays(-daysBack)
                .AddHours(-hours)
                .AddMinutes(-minutes)
                .AddSeconds(-seconds);

            return date;
        }
    }
}
