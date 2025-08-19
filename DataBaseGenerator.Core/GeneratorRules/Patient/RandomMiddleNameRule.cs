using System;
using System.Collections.Generic;

namespace DataBaseGenerator.Core.GeneratorRules.Patient
{
    public sealed class RandomMiddleNameRule : IGeneratorRule<string>
    {
        private readonly Random _random = new();

        private static readonly IDictionary<int, string> _russianMiddlename = new Dictionary<int, string>
        {
            {0, "Дмитреевич"},
            {1, "Аркадьевич"},
            {2, "Иваныч"},
            {3, "Семенович"},
            {4, "Вольфович"},
            {5, "Ильич"},
            {6, "Михайлович"},
            {7, "Григорьевич"},
            {8, "Борисович"},
            {9, "Юрьевич"}
        };

        private static readonly IDictionary<int, string> _englishMiddleNames = new Dictionary<int, string>
        {
            {0, "John"},
            {1, "Michael"},
            {2, "Rose"},
            {3, "Grace"},
            {4, "Lee"},
            {5, "James"},
            {6, "Edward"},
            {7, "Elizabeth"},
            {8, "Marie"},
            {9, "Ann"}
        };

        private static readonly IDictionary<int, string> _chineseMiddleNames = new Dictionary<int, string>
        {
            {0, "晓明"},
            {1, "玉兰"},
            {2, "志强"},
            {3, "婷婷"},
            {4, "建华"},
            {5, "伟强"},
            {6, "芳丽"},
            {7, "国华"},
            {8, "建国"},
            {9, "美玲"}
        };


        public string Generate() => GenerateRussian();

        public string GenerateRussian()
        {
            return _russianMiddlename[_random.Next(0, _russianMiddlename.Count)];
        }

        public string GenerateEnglish()
        {
            return _englishMiddleNames[_random.Next(0, _englishMiddleNames.Count)];
        }

        public string GenerateChinese()
        {
            return _chineseMiddleNames[_random.Next(0, _chineseMiddleNames.Count)];
        }

        public string GenerateMiddleNames(PatientGeneratorDto patientGenerator)
        {
            var generatorMiddleNames = new List<Func<string>>();

            if (patientGenerator.NamesRusGeneratorRule)
                generatorMiddleNames.Add(GenerateRussian);

            if (patientGenerator.NamesEngGeneratorRule)
                generatorMiddleNames.Add(GenerateEnglish);

            if (patientGenerator.NamesChinaGeneratorRule)
                generatorMiddleNames.Add(GenerateChinese);

            if (generatorMiddleNames.Count == 0)
                return Generate();

            var middleName = generatorMiddleNames[_random.Next(generatorMiddleNames.Count)];
            return middleName();
        }

        public override string ToString()
        {
            return $"{Generate()}";
        }
    }
}
