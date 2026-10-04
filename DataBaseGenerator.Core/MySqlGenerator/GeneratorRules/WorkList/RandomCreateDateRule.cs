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
            var maxSecondsBack = 7 * 24 * 60 * 60;
            return DateTime.Now.AddSeconds(-Random.Shared.Next(0, maxSecondsBack + 1));
        }
    }
}
