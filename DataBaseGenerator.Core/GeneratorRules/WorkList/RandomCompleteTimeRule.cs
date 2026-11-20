using System;
using DataBaseGenerator.Core.GeneratorRules;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.GeneratorRules.WorkList
{
    public sealed class RandomCompleteTimeRule : IGeneratorRule<TimeSpan>
    {
        public TimeSpan Generate()
        {
            return new TimeSpan(15, 15,15);
        }
    }
}
