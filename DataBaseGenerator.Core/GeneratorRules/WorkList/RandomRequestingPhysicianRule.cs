﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DataBaseGenerator.Core.GeneratorRules.WorkList
{
    public sealed class RandomRequestingPhysicianRule : IGeneratorRule<string>
    {
        public string Generate()
        {
            return "FFFF";
        }
    }
}
