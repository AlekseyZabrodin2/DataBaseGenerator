﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseGenerator.Core.GeneratorRules.WorkList
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
