using System;
using System.Collections.Generic;

namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public sealed class RandomSexRule : IGeneratorRule<string>
    {
        public RandomSexRule(string getGender = null)
        {
            SetGender = getGender;
        }

        public string SetGender { get; }

        private static IList<string> _patientSex = new List<string> { "M", "F", "O" };

        public string Generate()
        {
            var random = new Random();

            if (SetGender != null)
            {
                if (SetGender == "M")
                {
                    return _patientSex[0];
                }
                else if (SetGender == "F")
                {
                    return _patientSex[1];
                }
                else if (SetGender == "O")
                {
                    return _patientSex[2];
                }
            }

            var selectGender = _patientSex[random.Next(_patientSex.Count)];

            return selectGender;
        }
    }
}