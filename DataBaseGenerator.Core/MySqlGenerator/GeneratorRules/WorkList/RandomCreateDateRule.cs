using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList
{
    public sealed class RandomCreateDateRule : IGeneratorRule<DateTime>
    {
        public DateTime Generate()
        {
            DateTime dateNow = DateTime.Today;

            return dateNow;
        }
    }
}
