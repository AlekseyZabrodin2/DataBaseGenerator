using System;
using System.Collections.Generic;

namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public sealed class RandomLastNameRule : IGeneratorRule<string>
    {
        private readonly Random _random = new();

        private static readonly IDictionary<int, string> _russianLastName = new Dictionary<int, string>
        {
            {0, "Покровский"},
            {1, "Лебединский"},
            {2, "Дубов"},
            {3, "Голицын"},
            {4, "Селиверстов"},
            {5, "Сибирцев"},
            {6, "Дунаевский"},
            {7, "Ржевский"},
            {8, "Чацкий"},
            {9, "Ростов"}
        };

        private static readonly IDictionary<int, string> _englishLastNames = new Dictionary<int, string>
        {
            {0, "Smith"},
            {1, "Johnson"},
            {2, "Williams"},
            {3, "Brown"},
            {4, "Jones"},
            {5, "Miller"},
            {6, "Davis"},
            {7, "Garcia"},
            {8, "Rodriguez"},
            {9, "Wilson"}
        };

        private static readonly IDictionary<int, string> _chineseLastNames = new Dictionary<int, string>
        {
            {0, "王 (Wang)"},
            {1, "李 (Li)"},
            {2, "张 (Zhang)"},
            {3, "刘 (Liu)"},
            {4, "陈 (Chen)"},
            {5, "杨 (Yang)"},
            {6, "赵 (Zhao)"},
            {7, "黄 (Huang)"},
            {8, "周 (Zhou)"},
            {9, "吴 (Wu)"}
        };

        public string Generate() => GenerateRussian();

        public string GenerateRussian()
        {
            return _russianLastName[_random.Next(0, _russianLastName.Count)];
        }

        public string GenerateEnglish()
        {
            return _englishLastNames[_random.Next(0, _englishLastNames.Count)];
        }

        public string GenerateChinese()
        {
            return _chineseLastNames[_random.Next(0, _chineseLastNames.Count)];
        }

        public string GenerateLastName(PatientGeneratorDto patientGenerator)
        {
            var generatorLastNames = new List<Func<string>>();

            if (patientGenerator.NamesRusGeneratorRule)
                generatorLastNames.Add(GenerateRussian);

            if (patientGenerator.NamesEngGeneratorRule)
                generatorLastNames.Add(GenerateEnglish);

            if (patientGenerator.NamesChinaGeneratorRule)
                generatorLastNames.Add(GenerateChinese);

            if (generatorLastNames.Count == 0)
                return Generate();

            var lastName = generatorLastNames[_random.Next(generatorLastNames.Count)];
            return lastName();
        }

        public override string ToString()
        {
            return $"{Generate()}";
        }
    }
}
