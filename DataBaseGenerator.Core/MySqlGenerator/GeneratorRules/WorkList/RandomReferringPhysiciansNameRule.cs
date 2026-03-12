using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList
{
    public sealed class RandomReferringPhysiciansNameRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _nameSentDoctor = new Dictionary<int, string>
        {
            {0, "Доктор Айболит"},
            {1, "Гааз Ф.П."},
            {2, "Захарьин Г.А."},
            {3, "Пирогов Н.И."},
            {4, "Склифосовский Н.В."}
        };

        public string Generate()
        {
            var random = new Random();

            var nameSentDoctor = _nameSentDoctor[random.Next(0, _nameSentDoctor.Count)];

            return nameSentDoctor;
        }
    }
}
