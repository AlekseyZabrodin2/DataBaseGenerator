using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList
{
    public sealed class RandomStudyDescriptionRule : IGeneratorRule<string>
    {
        private static IDictionary<int, string> _studyDescription = new Dictionary<int, string>
        {
            {0, "Снимок органов брюшной полости"},
            {1, "Запись органов малого таза"},
            {2, "Фотка грудной клетки"},
            {3, "КТ костей черепа"},
            {4, "Ортопантомография"}
        };

        public string Generate()
        {
            var random = new Random();

            var studyDescription = _studyDescription[random.Next(0, _studyDescription.Count)];

            return studyDescription;
        }
    }
}
