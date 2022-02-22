﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseGenerator.Core.GeneratorRules.WorkList
{
    public sealed class RandomModalityRule : IGeneratorRule< string>
    {
        public RandomModalityRule(string modality)
        {
            Modality = modality;
        }

        public string Modality { get; set; }

        public string Generate()
        {
            return Modality;
        }

        public override string ToString()
        {
            return $"{Generate()}";
        }
    }
}