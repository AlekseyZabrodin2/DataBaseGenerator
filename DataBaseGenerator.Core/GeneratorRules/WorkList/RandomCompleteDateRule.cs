﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseGenerator.Core.GeneratorRules.WorkList
{
    public sealed class RandomCompleteDateRule : IGeneratorRule<DateTime>
    {
        public DateTime Generate()
        {
            return new DateTime(1980, 12, 3);
        }
    }
}
