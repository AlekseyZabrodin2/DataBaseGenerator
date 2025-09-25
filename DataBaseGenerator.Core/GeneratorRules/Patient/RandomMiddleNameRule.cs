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

        private static readonly IDictionary<int, string> _longRusValues = new Dictionary<int, string>
        {
            {0, "Владимирович-Александрович"},
            {1, "Всеволодович-Святославович"},
            {2, "Ростиславович-Мстиславович"},
            {3, "Вячеславович-Святославович"},
            {4, "Ярославович-Владимирович"},
            {5, "Радимирович-Любомирович"},
            {6, "Святозарович-Борисович"},
            {7, "Святославович-Ростиславович"},
            {8, "Мстиславович-Всеволодович"},
            {9, "Михайлович-Девяткович"}
        };

        private static readonly IDictionary<int, string> _longEngValues = new Dictionary<int, string>
        {
            {0, "Gregor-Stewart"},
            {1, "Chesterton-Harrison"},
            {2, "Bromley-Harrison"},
            {3, "Wainwright-Thorndike-Longman"},
            {4, "Flannagan-MacDonald"},
            {5, "MacAlister-Campbel"},
            {6, "Brompton-Chesterfield-Wright"},
            {7, "Wainwright-MacGregor"},
            {8, "Goldsmith-Thorndike"},
            {9, "Northampton-Harrison"}
        };

        private static readonly IDictionary<int, string> _longChinValues = new Dictionary<int, string>
        {
            {0, "阿列坎素奈斯里卡素"},
            {1, "黄石金谟斯杜林"},
            {2, "奈斯里卡素夫"},
            {3, "吾勃阿列坎素奈"},
            {4, "阿列古拉勃尔谟斯"},
            {5, "黄石金谟斯杜林"},
            {6, "张谟斯王李赵钱"},
            {7, "司马黄石金谟斯"},
            {8, "欧阳家谟斯永德"},
            {9, "谟斯公羊王孙李"}
        };

        private static readonly IDictionary<int, string> _rusEmptyValues = new Dictionary<int, string>
        {
            {0, ""},
            {1, "Аркадьевич"},
            {2, ""},
            {3, "Семенович"},
            {4, ""},
            {5, "Ильич"},
            {6, ""},
            {7, "Григорьевич"},
            {8, ""},
            {9, "Юрьевич"}
        };

        private static readonly IDictionary<int, string> _engEmptyValues = new Dictionary<int, string>
        {
            {0, "John"},
            {1, ""},
            {2, "Rose"},
            {3, ""},
            {4, "Lee"},
            {5, ""},
            {6, "Edward"},
            {7, ""},
            {8, "Marie"},
            {9, ""}
        };

        private static readonly IDictionary<int, string> _chinEmptyValues = new Dictionary<int, string>
        {
            {0, "晓明"},
            {1, ""},
            {2, "志强"},
            {3, ""},
            {4, "建华"},
            {5, ""},
            {6, "芳丽"},
            {7, ""},
            {8, "建国"},
            {9, ""}
        };

        private static readonly IDictionary<int, string> _specialChars = new Dictionary<int, string>
        {
            {0, "°′″‰↵↔↕↔©®™•…"},
            {1, ".,!?:;...—-()"},
            {2, "'{}[]@#$&*~`|"},
            {3, "/_-^§¶°′″€$£"},
            {4, "“«»♥♦♣♠♂♀☯"},
            {5, "↔+.×÷=><±√%@#"},
            {6, "$&*~`|‰↵↔↕"},
            {7, "/_-^§¶°′″€$£¥₽"},
            {8, "„“«»♥♦♣♠♂♀☯"},
            {9, "¥₽©®™•…„☠°′″"}
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

        public string GenerateLongName(PatientGeneratorDto patientGenerator)
        {
            if (patientGenerator.NamesEngGeneratorRule)
                return _longEngValues[_random.Next(0, _longEngValues.Count)];

            if (patientGenerator.NamesChinaGeneratorRule)
                return _longChinValues[_random.Next(0, _longChinValues.Count)];

            return _longRusValues[_random.Next(0, _longRusValues.Count)];
        }

        public string GenerateEmptyValues(PatientGeneratorDto patientGenerator)
        {
            if (patientGenerator.NamesEngGeneratorRule)
                return _engEmptyValues[_random.Next(0, _engEmptyValues.Count)];

            if (patientGenerator.NamesChinaGeneratorRule)
                return _chinEmptyValues[_random.Next(0, _chinEmptyValues.Count)];

            return _rusEmptyValues[_random.Next(0, _rusEmptyValues.Count)];
        }

        public string GenerateSpecialChars()
        {
            return _specialChars[_random.Next(0, _specialChars.Count)];
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

            if (patientGenerator.EmptyStringsGeneratorRule)
                generatorMiddleNames.Add(() => GenerateEmptyValues(patientGenerator));

            if (patientGenerator.LongValuesGeneratorRule)
                generatorMiddleNames.Add(() => GenerateLongName(patientGenerator));

            if (patientGenerator.SpecialCharsGeneratorRule)
                generatorMiddleNames.Add(GenerateSpecialChars);

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
