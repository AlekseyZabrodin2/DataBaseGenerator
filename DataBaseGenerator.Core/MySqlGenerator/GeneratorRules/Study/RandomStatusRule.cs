using System;
using System.Collections.Generic;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Study
{
    public sealed class RandomStatusRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _studyStatus = new Dictionary<int, string>
        {
            {0, "Completed"},
            {1, "InProgress"},
            {2, "Scheduled"},
            {3, "Cancelled"},
            {4, "Pending"}
        };

        public string Generate()
        {
            var random = new Random();

            var address = _studyStatus[random.Next(0, _studyStatus.Count)];

            return address;
        }
    }
}
