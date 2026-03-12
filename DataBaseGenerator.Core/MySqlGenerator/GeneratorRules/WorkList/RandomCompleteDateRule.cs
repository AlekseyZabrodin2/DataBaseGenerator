using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList
{
    public sealed class RandomCompleteDateRule : IGeneratorRule<DateTime>
    {
        public DateTime Generate()
        {
            return new DateTime(1980, 12, 3);
        }
    }
}
