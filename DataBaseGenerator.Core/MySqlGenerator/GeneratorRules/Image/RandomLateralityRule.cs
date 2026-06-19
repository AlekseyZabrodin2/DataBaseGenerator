using System;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Image
{
    public class RandomLateralityRule : IGeneratorRule<string>
    {
        private static readonly Random _random = new Random();

        private static readonly string[] _laterality =
        {
            "Right",
            "Left",
            "Both",
            "Not Applicable",
            "Unknown"
        };

        public string Generate()
        {
            var laterality = _laterality[_random.Next(0, _laterality.Length)];

            return laterality;
        }
    }
}
