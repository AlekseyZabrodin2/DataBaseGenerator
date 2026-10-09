using System;
using System.Collections.Generic;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Study
{
    public sealed class RandomStatusRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _studyStatus = new Dictionary<int, string>
        {
            {0, "Pending"},
            {1, "Sending"},
            {2, "Delivered"},
            {3, "Failed"},
            {4, "NotRequired"}
        };

        public string Generate()
        {
            var random = new Random();

            var address = _studyStatus[random.Next(0, _studyStatus.Count)];

            return address;
        }
    }
}
