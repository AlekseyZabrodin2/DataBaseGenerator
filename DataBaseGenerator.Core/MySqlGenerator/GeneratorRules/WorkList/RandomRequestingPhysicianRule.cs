using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList
{
    public sealed class RandomRequestingPhysicianRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _requestingDoctor = new Dictionary<int, string>
        {
            {0, "Илизаров Г.А."},
            {1, "Бехтерев В.М."},
            {2, "Биша М.Ф."},
            {3, "Важинский Ф.И."},
            {4, "Гаврилов Е.И."}
        };

        public string Generate()
        {
            var random = new Random();

            var requestingDoctor = _requestingDoctor[random.Next(0, _requestingDoctor.Count)];

            return requestingDoctor;
        }
    }
}
