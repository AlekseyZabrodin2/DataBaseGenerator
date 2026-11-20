using System;

namespace DataBaseGenerator.Core.GeneratorRules.Study
{
    public sealed class RandomEffectiveDoseRule : IGeneratorRule<double>
    {
        private static readonly Random _random = new Random();

        private const double MinDose = 0.001;
        private const double MaxDose = 25.0;

        public double Generate()
        {
            var dose = Math.Round(_random.NextDouble() * (MaxDose - MinDose) + MinDose, 3);
            return dose;
        }
    }
}
