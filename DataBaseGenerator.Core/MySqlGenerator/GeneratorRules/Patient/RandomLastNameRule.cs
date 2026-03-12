using System;
using System.Collections.Generic;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Patient
{
    public sealed class RandomLastNameRule : IGeneratorRule<string>
    {
        private readonly Random _random = new();

        private static readonly IDictionary<int, string> _russianLastName = new Dictionary<int, string>
        {
            {0, "Андреев"},
            {1, "Белов"},
            {2, "Волков"},
            {3, "Громов"},
            {4, "Дроздов"},
            {5, "Ершов"},
            {6, "Жданов"},
            {7, "Зверев"},
            {8, "Ильин"},
            {9, "Крылов"},
            {10, "Лазарев"},
            {11, "Морозов"},
            {12, "Наумов"},
            {13, "Озеров"},
            {14, "Павлов"},
            {15, "Рыбаков"},
            {16, "Сергеев"},
            {17, "Тарасов"},
            {18, "Уваров"},
            {19, "Фролов"},
            {20, "Хохлов"},
            {21, "Царев"},
            {22, "Чистяков"},
            {23, "Шубин"},
            {24, "Щеглов"},
            {25, "Эрдман"},
            {26, "Юсупов"},
            {27, "Яшин"}
        };

        private static readonly IDictionary<int, string> _englishLastNames = new Dictionary<int, string>
        {
            {0, "Anderson"},
            {1, "Brown"},
            {2, "Clark"},
            {3, "Davis"},
            {4, "Evans"},
            {5, "Foster"},
            {6, "Garcia"},
            {7, "Harris"},
            {8, "Johnson"},
            {9, "King"},
            {10, "Lewis"},
            {11, "Miller"},
            {12, "Nelson"},
            {13, "Parker"},
            {14, "Roberts"},
            {15, "Smith"},
            {16, "Taylor"},
            {17, "Walker"},
            {18, "Young"},
            {19, "Adams"},
            {20, "Baker"},
            {21, "Campbell"},
            {22, "Edwards"},
            {23, "Flores"},
            {24, "Green"},
            {25, "Hill"},
            {26, "Jackson"},
            {27, "Kelly"},
            {28, "Long"},
            {29, "Mitchell"},
            {30, "Nguyen"},
            {31, "Phillips"},
            {32, "Rivera"},
            {33, "Scott"},
            {34, "Thomas"},
            {35, "White"},
            {36, "Xu"},
            {37, "Yoshida"},
            {38, "Zhang"}
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

        private static readonly IDictionary<int, string> _longRusValues = new Dictionary<int, string>
        {
            {0, "Смирнов-Петров-Васильев"},
            {1, "Михайлов-Сергеев-Иванов"},
            {2, "Александров-Николаев"},
            {3, "Владимиров-Константинов"},
            {4, "Семёнов-Прохоров-Белов"},
            {5, "Андреев-Михайлов-Соколов"},
            {6, "Орлов-Лебедев-Воробьёв"},
            {7, "Павлов-Морозов-Никитин"},
            {8, "Королёв-Александров"},
            {9, "Волков-Соколов-Петров"}
        };

        private static readonly IDictionary<int, string> _longEngValues = new Dictionary<int, string>
        {
            {0, "Featherstonhaugh"},
            {1, "MacGregor-Campbell"},
            {2, "Chesterton-Harrison"},
            {3, "MacAlister-MacDonald"},
            {4, "Longman-Chesterfield"},
            {5, "Fitzgerald-Flannagan"},
            {6, "Brompton-Chesterfield"},
            {7, "MacAlister-Campbell"},
            {8, "Wainwright-Chesterfield"},
            {9, "MacDonald-MacGregor"}
        };

        private static readonly IDictionary<int, string> _longChinValues = new Dictionary<int, string>
        {
            {0, "司马黄石金谟斯"},
            {1, "欧阳家谟斯永德"},
            {2, "谟斯公羊王孙李"},
            {3, "张谟斯王李赵钱"},
            {4, "黄石金谟斯杜林"},
            {5, "阿列古拉勃尔谟斯"},
            {6, "吾勃阿列坎素奈"},
            {7, "奈斯里卡素夫"},
            {8, "古拉勃尔谟斯吾勃阿"},
            {9, "阿列坎素奈斯里卡素"}
        };

        private static readonly IDictionary<int, string> _rusEmptyValues = new Dictionary<int, string>
        {
            {0, "Покровский"},
            {1, ""},
            {2, "Дубов"},
            {3, ""},
            {4, "Селиверстов"},
            {5, ""},
            {6, "Дунаевский"},
            {7, ""},
            {8, "Чацкий"},
            {9, ""}
        };

        private static readonly IDictionary<int, string> _engEmptyValues = new Dictionary<int, string>
        {
            {0, "Smith"},
            {1, ""},
            {2, "Williams"},
            {3, ""},
            {4, "Jones"},
            {5, ""},
            {6, "Davis"},
            {7, ""},
            {8, "Rodriguez"},
            {9, ""}
        };

        private static readonly IDictionary<int, string> _chinEmptyValues = new Dictionary<int, string>
        {
            {0, "王 (Wang)"},
            {1, ""},
            {2, "张 (Zhang)"},
            {3, ""},
            {4, "陈 (Chen)"},
            {5, ""},
            {6, "赵 (Zhao)"},
            {7, ""},
            {8, "周 (Zhou)"},
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

        public string GenerateLongLastName(PatientGeneratorDto patientGenerator)
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

        public string GenerateLastName(PatientGeneratorDto patientGenerator)
        {
            var generatorLastNames = new List<Func<string>>();

            if (patientGenerator.NamesRusGeneratorRule)
                generatorLastNames.Add(GenerateRussian);

            if (patientGenerator.NamesEngGeneratorRule)
                generatorLastNames.Add(GenerateEnglish);

            if (patientGenerator.NamesChinaGeneratorRule)
                generatorLastNames.Add(GenerateChinese);

            if (patientGenerator.EmptyStringsGeneratorRule)
                generatorLastNames.Add(() => GenerateEmptyValues(patientGenerator));

            if (patientGenerator.LongValuesGeneratorRule)
                generatorLastNames.Add(() => GenerateLongLastName(patientGenerator));

            if (patientGenerator.SpecialCharsGeneratorRule)
                generatorLastNames.Add(GenerateSpecialChars);

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
