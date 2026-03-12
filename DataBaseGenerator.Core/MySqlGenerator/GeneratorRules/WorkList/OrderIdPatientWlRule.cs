using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DataBaseGenerator.Core.MySqlGenerator;

namespace DataBaseGenerator.Core.MySqlGenerator.GeneratorRules.WorkList
{
    public sealed class OrderIdPatientWlRule : IGeneratorRule<int, int>
    {
        public int Generate(int parameter)
        {
            
                var iDWlPatient = parameter;

                return iDWlPatient;
            
        }

        //public override string ToString()
        //{
        //    return $"{Generate()}";
        //}
    }
}
