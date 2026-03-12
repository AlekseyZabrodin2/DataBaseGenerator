using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Patient
{
    public sealed class RandomAddInfoRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _patientAddInformation = new Dictionary<int, string>
        {
            {0, "Жалоб нет"},
            {1, "Все хорошо"},
            {2, "Нога вроде болит"},
            {3, "Что-то с головой"},
            {4, "Вроде рука"},
            {5, "Палец болит"},
            {6, "Пятку натёр"},
            {7, "Ноготь сломал"},
            {8, "Локоть болит"},
            {9, "Мизинчик ударил"}

        };

        public string Generate()
        {
            var random = new Random();

            var address = _patientAddInformation[random.Next(0, _patientAddInformation.Count)];

            return address;
        }

        public override string ToString()
        {
            return $"{Generate()}";
        }
    }
}
