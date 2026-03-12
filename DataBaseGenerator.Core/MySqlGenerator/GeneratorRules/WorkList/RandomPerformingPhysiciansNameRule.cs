using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList
{
    public sealed class RandomPerformingPhysiciansNameRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _doctorPerformer = new Dictionary<int, string>
        {
            {0, "Боткин С.П."},
            {1, "Павлов И.П."},
            {2, "Юдин С.С."},
            {3, "Филатов В.П."},
            {4, "Войно-Ясенецкий В.Ф."}
        };

        public string Generate()
        {
            var random = new Random();

            var doctorPerformer = _doctorPerformer[random.Next(0, _doctorPerformer.Count)];

            return doctorPerformer;
        }
    }
}
