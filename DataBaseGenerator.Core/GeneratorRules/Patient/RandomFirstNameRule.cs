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

        private static readonly IDictionary<int, string> _longRusValues = new Dictionary<int, string>
        {
            {0, "Берёза-Белоствольная"},
            {1, "Принцесса-Даниэлла"},
            {2, "Абдурахмангаджи"},
            {3, "Матвей-Радуга"},
            {4, "Николай-Никита-Нил"},
            {5, "Дмитрий-Аметист"},
            {6, "Заря-Заряница"},
            {7, "София-Солнышко"},
            {8, "Аврора-Златокудрая"},
            {9, "София-Виктория"}
        };

        private static readonly IDictionary<int, string> _longEngValues = new Dictionary<int, string>
        {
            {0, "MacGregor-Campbell-Stewart"},
            {1, "Chesterton-Harrison-Wright"},
            {2, "Bromley-Davenport-Harrison"},
            {3, "Wainwright-Thorndike-Longman"},
            {4, "Fitzgerald-Flannagan-MacDonald"},
            {5, "Riverside-Chesterton"},
            {6, "Kingsley-Harrison"},
            {7, "Silverstone-Wright"},
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
            {0, "Андрей"},
            {1, ""},
            {2, "Виталик"},
            {3, ""},
            {4, "Дмитрий"},
            {5, ""},
            {6, "Сергей"},
            {7, ""},
            {8, "Павел"},
            {9, ""}
        };

        private static readonly IDictionary<int, string> _engEmptyValues = new Dictionary<int, string>
        {
            {0, ""},
            {1, "Michael"},
            {2, ""},
            {3, "James"},
            {4, ""},
            {5, "Emily"},
            {6, ""},
            {7, "Jessica"},
            {8, ""},
            {9, "Amanda"}
        };

        private static readonly IDictionary<int, string> _chinEmptyValues = new Dictionary<int, string>
        {
            {0, ""},
            {1, "芳"},
            {2, ""},
            {3, "敏"},
            {4, ""},
            {5, "秀英"},
            {6, ""},
            {7, "强"},
            {8, ""},
            {9, "洋"}
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

        public string GenerateLongFirstName(PatientGeneratorDto patientGenerator)
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

        public string GenerateFirstName(PatientGeneratorDto patientGenerator)
        {
            var generatorFirstName = new List<Func<string>>();

            if (patientGenerator.NamesRusGeneratorRule)
                generatorFirstName.Add(GenerateRussian);

            if (patientGenerator.NamesEngGeneratorRule)
                generatorFirstName.Add(GenerateEnglish);

            if (patientGenerator.NamesChinaGeneratorRule)
                generatorFirstName.Add(GenerateChinese);

            if (patientGenerator.EmptyStringsGeneratorRule)
                generatorFirstName.Add(GenerateEmptyValues(patientGenerator).ToString);

            if (patientGenerator.LongValuesGeneratorRule)
                generatorFirstName.Add(GenerateLongFirstName(patientGenerator).ToString);

            if (patientGenerator.SpecialCharsGeneratorRule)
                generatorFirstName.Add(GenerateSpecialChars);

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
