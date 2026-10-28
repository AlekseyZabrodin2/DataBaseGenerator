using System;
using DataBaseGenerator.Core.GeneratorRules;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.GeneratorRules.WorkList
{
    public sealed class OrderIdWorklistRule : IGeneratorRule<int, int>
    {
        public int Generate(int parameter)
        {
            var iDWorklist = parameter;

            return iDWorklist;
        }
    }
}
