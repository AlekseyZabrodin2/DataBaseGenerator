﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
