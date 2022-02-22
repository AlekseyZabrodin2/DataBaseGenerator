﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseGenerator.Core.GeneratorRules.WorkList
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
