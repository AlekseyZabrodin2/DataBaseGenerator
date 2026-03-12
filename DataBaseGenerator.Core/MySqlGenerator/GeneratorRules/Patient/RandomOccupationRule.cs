using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Patient
{
    public sealed class RandomOccupationRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _patientOccupation = new Dictionary<int, string>
        {
            {0, "Антиквар"},
            {1, "Археолог"},
            {2, "Бортинженер"},
            {3, "Вальщик леса"},
            {4, "Географ"},
            {5, "Декоратор"},
            {6, "Животновод"},
            {7, "Инженер сети"},
            {8, "Кассир"},
            {9, "Матрос"}

        };

        public string Generate()
        {
            var random = new Random();

            var occupation = _patientOccupation[random.Next(0, _patientOccupation.Count)];

            return occupation;
        }


        public override string ToString()
        {
            return $"{Generate()}";
        }
    }
}
