using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Patient
{
    public sealed class RandomAddressRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _patientAddress = new Dictionary<int, string>
        {
            {0, "Минск"},
            {1, "Гомель"},
            {2, "Витебск"},
            {3, "Могилёв"},
            {4, "Гродно"},
            {5, "Брест"},
            {6, "Барановичи"},
            {7, "Борисов"},
            {8, "Мозырь"},
            {9, "Солигорск"}

        };

        public string Generate()
        {
            var random = new Random();

            var address = _patientAddress[random.Next(0, _patientAddress.Count)];

            return address;
        }

        public override string ToString()
        {
            return $"{Generate()}";
        }
    }
}
