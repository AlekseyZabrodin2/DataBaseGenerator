using System;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Study
{
    public sealed class RandomAccessionNumberRule : IGeneratorRule<string>
    {
        private static readonly Random _random = new Random();

        public string Generate()
        {
            var number = _random.Next(100000, 999999).ToString();
            return number;
        }
    }
}
