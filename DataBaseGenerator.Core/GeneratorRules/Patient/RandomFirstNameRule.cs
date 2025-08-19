using System;
using System.Collections.Generic;

namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public sealed class RandomFirstNameRule : IGeneratorRule<string>
    {
        private readonly Random _random = new();

        private static readonly IDictionary<int, string> _russianName = new Dictionary<int, string>
        {
            {0, "Андрей"},
            {1, "Борис"},
            {2, "Виталик"},
            {3, "Георгий"},
            {4, "Дмитрий"},
            {5, "Егор"},
            {6, "Сергей"},
            {7, "Ярик"},
            {8, "Павел"},
            {9, "Марк"}
        };

        private static readonly IDictionary<int, string> _englishNames = new Dictionary<int, string>
        {
            {0, "John"},
            {1, "Michael"},
            {2, "William"},
            {3, "James"},
            {4, "Robert"},
            {5, "Emily"},
            {6, "Sarah"},
            {7, "Jessica"},
            {8, "Ashley"},
            {9, "Amanda"}
        };

        private static readonly IDictionary<int, string> _chineseNames = new Dictionary<int, string> 
        {
            {0, "伟"},
            {1, "芳"},
            {2, "娜"},
            {3, "敏"},
            {4, "静"},
            {5, "秀英"},
            {6, "丽"},
            {7, "强"},
            {8, "磊"},
            {9, "洋"}
        };


        public string Generate() => GenerateRussian();

        public string GenerateRussian()
        {
            return _russianName[_random.Next(0, _russianName.Count)];
        }

        public string GenerateEnglish()
        {
            return _englishNames[_random.Next(0, _englishNames.Count)];
        }

        public string GenerateChinese()
        {
            return _chineseNames[_random.Next(0, _chineseNames.Count)];
        }

        public string GenerateFirstName(PatientGeneratorDto patientGenerator)
        {
            var generatorFirstName = new List<Func<string>>();

            if (patientGenerator.NamesRusGeneratorRule)
                generatorFirstName.Add(GenerateRussian);

            if (patientGenerator.NamesEngGeneratorRule)
                generatorFirstName.Add(GenerateEnglish);

            if (patientGenerator.NamesChinaGeneratorRule)
                generatorFirstName.Add(GenerateChinese);

            if (generatorFirstName.Count == 0)
                return Generate();

            var firstName = generatorFirstName[_random.Next(generatorFirstName.Count)];
            return firstName();
        }

        public override string ToString()
        {
            return $"{Generate()}";
        }
    }
}
