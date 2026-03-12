using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.Patient
{
    public sealed class RandomSexRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _patientSex = new Dictionary<int, string>
        {
            {0, "M"},
            {1, "F"},
            {2, "O"},
        };

        public string Generate()
        {
            var random = new Random();

            var patientSex = _patientSex[random.Next(0, _patientSex.Count)];

            return patientSex;
        }

        public override string ToString()
        {
            return $"{Generate()}";
        }
    }
}
