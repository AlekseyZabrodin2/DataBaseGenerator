using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList
{
    public sealed class RandomStationAeTitleRule : IGeneratorRule<string>
    {
        public RandomStationAeTitleRule(string aeTitle)
        {
            AeTitle = aeTitle;
        }

        public string AeTitle { get; set; }


        public string Generate()
        {
            return AeTitle;
        }
    }
}
