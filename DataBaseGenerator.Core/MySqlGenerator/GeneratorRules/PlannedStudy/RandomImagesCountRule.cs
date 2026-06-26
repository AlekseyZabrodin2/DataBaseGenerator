using System;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.PlannedStudy
{
    public sealed class RandomImagesCountRule : IGeneratorRule<int>
    {
        private static readonly Random _random = new Random();


        public int Generate()
        {
            return _random.Next(1, 10);
        }
    }
}
